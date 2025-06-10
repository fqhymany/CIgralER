import React, {useState, useRef, useEffect} from 'react';
import {Form, Container, Card, Button, ProgressBar, Alert, Spinner} from 'react-bootstrap';
import 'bootstrap/dist/css/bootstrap.min.css';
import styles from '../../assets/styles/MobileFileUploader.module.css';
import secureFileApi from '../../api/secureFileApi';
import {useAuth} from '../../contexts/AuthContext';
import {VscDebugRestart} from 'react-icons/vsc';

// Icons
import {FontAwesomeIcon} from '@fortawesome/react-fontawesome';
import {
  faCloudUploadAlt,
  faImage,
  faVideo,
  faMusic,
  faFilePdf,
  faFileWord,
  faFileExcel,
  faFilePowerpoint,
  faFileArchive,
  faFileAlt,
  faFile,
  faTimes,
  faUpload,
  faSpinner,
  faCheck,
  faExclamationTriangle,
  faCamera,
  faPlus,
} from '@fortawesome/free-solid-svg-icons';

const MobileSecureFileUploader = ({
  onUploadComplete,
  caseId,
  caseTitle,
  area,
  maxFileSize = 100, // in MB
  acceptedFileTypes = null,
  multiple = true,
  rtl = true,
  onClose,
}) => {
  const [files, setFiles] = useState([]);
  const [uploading, setUploading] = useState(false);
  const [uploadComplete, setUploadComplete] = useState(false);
  const [error, setError] = useState(null);
  const [activeFileId, setActiveFileId] = useState(null);
  const [renameErrors, setRenameErrors] = useState({});
  const {refreshSession} = useAuth();

  const fileInputRef = useRef(null);
  const cameraInputRef = useRef(null);

  // Set RTL direction based on prop
  useEffect(() => {
    document.documentElement.dir = rtl ? 'rtl' : 'ltr';
  }, [rtl]);

  useEffect(() => {
    if (activeFileId) {
      const inputElement = document.querySelector(`#file-name-${activeFileId}`);
      if (inputElement) {
        inputElement.focus();
      }
    }
  }, [activeFileId]);

  // Reset inactivity timer on user interaction
  useEffect(() => {
    const resetTimer = () => refreshSession();
    document.addEventListener('mousemove', resetTimer);
    document.addEventListener('keypress', resetTimer);
    document.addEventListener('touchstart', resetTimer);

    return () => {
      document.removeEventListener('mousemove', resetTimer);
      document.removeEventListener('keypress', resetTimer);
      document.removeEventListener('touchstart', resetTimer);
    };
  }, [refreshSession]);

  // Handle file selection via input
  const handleFileSelect = (e) => {
    const selectedFiles = Array.from(e.target.files);
    handleFiles(selectedFiles);
  };

  // Split filename from extension
  const splitFileName = (fullName) => {
    const lastDotIndex = fullName.lastIndexOf('.');
    if (lastDotIndex === -1) return {name: fullName, extension: ''};
    return {
      name: fullName.substring(0, lastDotIndex),
      extension: fullName.substring(lastDotIndex),
    };
  };

  // Handle files (validation and adding to state)
  const handleFiles = (newFiles) => {
    setError(null);
    setUploadComplete(false);

    // Validate file size
    const oversizedFiles = newFiles.filter((file) => file.size > maxFileSize * 1024 * 1024);
    if (oversizedFiles.length > 0) {
      setError(`فایل(های) انتخاب شده بزرگتر از حد مجاز ${maxFileSize} مگابایت هستند.`);
      return;
    }

    // Validate file types if specified
    if (acceptedFileTypes) {
      const invalidFiles = newFiles.filter((file) => {
        const fileType = file.type.split('/')[0];
        const fileExtension = file.name.split('.').pop().toLowerCase();
        return !acceptedFileTypes.includes(fileType) && !acceptedFileTypes.includes(fileExtension);
      });

      if (invalidFiles.length > 0) {
        setError(`فرمت فایل(های) انتخاب شده پشتیبانی نمی‌شود.`);
        return;
      }
    }

    // Add new files to state (prevent duplicates)
    setFiles((prevFiles) => {
      const updatedFiles = [...prevFiles];
      const existingFileNames = new Set(prevFiles.map((f) => `${f.file.name}-${f.file.size}`));

      newFiles.forEach((file) => {
        const fileIdentifier = `${file.name}-${file.size}`;

        if (!existingFileNames.has(fileIdentifier)) {
          const {name, extension} = splitFileName(file.name);

          const newFile = {
            file,
            id: `file-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
            progress: 0,
            status: 'pending',
            uploadedFileId: null,
            fileName: name,
            fileExtension: extension,
            customName: '', // Default empty to force renaming
            needsRename: true, // Mark file as needing rename
          };

          updatedFiles.push(newFile);

          // Set the first new file as active for rename focus
          if (!activeFileId && updatedFiles.length === prevFiles.length + 1) {
            setActiveFileId(newFile.id);
          }
        }
      });

      return updatedFiles;
    });
  };

  // Handle file name change
  const handleFileNameChange = (fileId, newName) => {
    setFiles((prevFiles) => prevFiles.map((file) => (file.id === fileId ? {...file, customName: newName, needsRename: newName.trim() === ''} : file)));

    // Clear error when user starts typing
    if (renameErrors[fileId]) {
      setRenameErrors((prev) => ({...prev, [fileId]: null}));
    }
  };

  // Remove a file
  const removeFile = (fileId) => {
    setFiles((prevFiles) => prevFiles.filter((file) => file.id !== fileId));
    // Reset file input so the same file can be selected again
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }

    // Clear any errors for this file
    if (renameErrors[fileId]) {
      const {[fileId]: _, ...rest} = renameErrors;
      setRenameErrors(rest);
    }

    // Reset active file if needed
    if (activeFileId === fileId) {
      const remainingFiles = files.filter((file) => file.id !== fileId);
      setActiveFileId(remainingFiles.length > 0 ? remainingFiles[0].id : null);
    }
  };

  // Validate all filenames before upload
  const validateFileNames = () => {
    const errors = {};
    let isValid = true;

    files.forEach((file) => {
      if (file.status === 'pending') {
        if (!file.customName || file.customName.trim() === '') {
          errors[file.id] = 'نام فایل الزامی است';
          isValid = false;
        }
      }
    });

    setRenameErrors(errors);
    return isValid;
  };

  // Upload all files
  const uploadFiles = async () => {
    if (files.length === 0 || uploading || uploadComplete) return;

    // First validate that all files have names
    if (!validateFileNames()) {
      setError('لطفاً برای همه فایل‌ها نام مناسب وارد کنید');
      return;
    }

    setUploading(true);
    setUploadComplete(false);
    setError(null);

    // Update status of all pending files
    setFiles((prevFiles) =>
      prevFiles.map((file) => ({
        ...file,
        status: file.status === 'pending' ? 'uploading' : file.status,
        progress: file.status === 'pending' ? 0 : file.progress,
      }))
    );

    // Upload only pending files
    const pendingFiles = files.filter((f) => f.status === 'pending');
    const uploadedFiles = [];
    let hasError = false;

    for (const fileObj of pendingFiles) {
      try {
        // Update progress to show activity
        setFiles((prevFiles) => prevFiles.map((file) => (file.id === fileObj.id ? {...file, progress: 10} : file)));

        // Create full filename with extension for upload
        const fullFileName = fileObj.customName + fileObj.fileExtension;

        // Perform actual upload with encryption
        const response = await secureFileApi.uploadFile(fileObj.file, caseId, area, fullFileName);

        // Update file status on success
        setFiles((prevFiles) =>
          prevFiles.map((file) =>
            file.id === fileObj.id
              ? {
                  ...file,
                  progress: 100,
                  status: 'completed',
                  uploadedFileId: response.fileId,
                }
              : file
          )
        );

        uploadedFiles.push({
          ...response,
          originalFile: fileObj.file,
        });
      } catch (err) {
        // Update file status on error
        setFiles((prevFiles) =>
          prevFiles.map((file) =>
            file.id === fileObj.id
              ? {
                  ...file,
                  progress: 0,
                  status: 'error',
                  error: err.message || 'خطا در آپلود',
                }
              : file
          )
        );
        hasError = true;
      }
    }

    setUploading(false);

    if (!hasError) {
      setUploadComplete(true);
    }

    if (uploadedFiles.length > 0 && onUploadComplete) {
      onUploadComplete(uploadedFiles);
    }
  };

  // Get appropriate icon for file type
  const getFileIcon = (file) => {
    const type = file.type.split('/')[0];
    const extension = file.name.split('.').pop().toLowerCase();

    const icons = {
      image: {icon: faImage, color: 'text-success'},
      video: {icon: faVideo, color: 'text-danger'},
      audio: {icon: faMusic, color: 'text-purple'},
      application: {
        pdf: {icon: faFilePdf, color: 'text-danger'},
        msword: {icon: faFileWord, color: 'text-primary'},
        'vnd.openxmlformats-officedocument.wordprocessingml.document': {icon: faFileWord, color: 'text-primary'},
        'vnd.ms-excel': {icon: faFileExcel, color: 'text-success'},
        'vnd.openxmlformats-officedocument.spreadsheetml.sheet': {icon: faFileExcel, color: 'text-success'},
        'vnd.ms-powerpoint': {icon: faFilePowerpoint, color: 'text-warning'},
        'vnd.openxmlformats-officedocument.presentationml.presentation': {icon: faFilePowerpoint, color: 'text-warning'},
        zip: {icon: faFileArchive, color: 'text-warning'},
        'x-rar-compressed': {icon: faFileArchive, color: 'text-warning'},
        'x-7z-compressed': {icon: faFileArchive, color: 'text-warning'},
        default: {icon: faFile, color: 'text-secondary'},
      },
      text: {icon: faFileAlt, color: 'text-info'},
      default: {icon: faFile, color: 'text-secondary'},
    };

    if (type === 'application') {
      for (const key in icons.application) {
        if (file.type.includes(key)) {
          return icons.application[key];
        }
      }
      return icons.application.default;
    }

    return icons[type] || icons.default;
  };

  // Format file size
  const formatFileSize = (bytes) => {
    if (bytes === 0) return '0 بایت';
    const k = 1024;
    const sizes = ['بایت', 'کیلوبایت', 'مگابایت', 'گیگابایت', 'ترابایت'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
  };

  // Get file status component
  const getFileStatusComponent = (fileObj) => {
    const {status, progress, error, id} = fileObj;

    if (status === 'error') {
      return (
        <div className={styles.fileStatus}>
          <Alert variant="danger" className={styles.fileAlert}>
            <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
            <small>{error || 'خطا در آپلود'}</small>
          </Alert>
          <Button variant="warning" size="sm" onClick={() => retryUpload(id)} className={styles.retryButton}>
            <VscDebugRestart />
            <span className={styles.buttonText}>تلاش مجدد</span>
          </Button>
        </div>
      );
    }

    if (status === 'uploading') {
      return (
        <div className={styles.fileStatus}>
          <ProgressBar animated now={progress} variant="primary" className={styles.progressBar} />
          <small className={styles.progressText}>{`${Math.round(progress)}%`}</small>
        </div>
      );
    }

    if (status === 'completed') {
      return (
        <div className={styles.fileStatus}>
          <ProgressBar now={100} variant="success" className={styles.progressBar} />
          <div className={styles.completeStatus}>
            <FontAwesomeIcon icon={faCheck} className={styles.successIcon} />
            <small className={styles.successText}>آپلود شد</small>
          </div>
        </div>
      );
    }

    return null;
  };

  const retryUpload = (fileId) => {
    setFiles((prevFiles) => prevFiles.map((file) => (file.id === fileId ? {...file, status: 'pending', progress: 0, error: null} : file)));
  };

  const openCamera = () => {
    if (cameraInputRef.current) {
      cameraInputRef.current.click();
    }
  };

  return (
    <div className={styles.mobileUploaderContainer}>
      <div className={styles.mobileUploaderHeader}>
        <h2 className={styles.mobileUploaderTitle}>{caseTitle}</h2>
        <button className={styles.closeButton} onClick={onClose}>
          <FontAwesomeIcon icon={faTimes} />
        </button>
      </div>

      {/* Error message */}
      {error && (
        <Alert variant="danger" className={styles.errorAlert} onClose={() => setError(null)} dismissible>
          {error}
        </Alert>
      )}

      {/* File selection buttons */}
      <div className={styles.fileSelectionArea}>
        <input type="file" ref={fileInputRef} className={styles.hiddenInput} multiple={multiple} onChange={handleFileSelect} accept={acceptedFileTypes ? acceptedFileTypes.join(',') : undefined} />

        <input type="file" ref={cameraInputRef} className={styles.hiddenInput} accept="image/*" capture="camera" onChange={handleFileSelect} />

        <div className={styles.selectionButtons}>
          <button className={`${styles.selectionButton} ${styles.fileButton}`} onClick={() => fileInputRef.current.click()}>
            <FontAwesomeIcon icon={faCloudUploadAlt} className={styles.buttonIcon} />
            <span>فایل‌ها</span>
          </button>

          <button className={`${styles.selectionButton} ${styles.cameraButton}`} onClick={openCamera}>
            <FontAwesomeIcon icon={faCamera} className={styles.buttonIcon} />
            <span>دوربین</span>
          </button>
        </div>
      </div>

      {/* File List */}
      {files.length > 0 && (
        <div className={styles.fileListContainer}>
          <h3 className={styles.fileListTitle}>فایل‌های انتخاب شده</h3>

          <div className={styles.fileList}>
            {files.map((fileObj) => {
              const {file, id, status, fileName, fileExtension, customName} = fileObj;
              const {icon, color} = getFileIcon(file);
              const isActive = id === activeFileId;
              const hasError = renameErrors[id] != null;

              return (
                <div key={id} className={`${styles.fileItem} ${isActive ? styles.activeFile : ''} ${hasError ? styles.errorFile : ''}`} onClick={() => setActiveFileId(id)}>
                  <div className={styles.fileItemHeader}>
                    <div className={`${styles.fileIcon} ${color}`}>
                      <FontAwesomeIcon icon={icon} />
                    </div>

                    <div className={styles.fileInfo}>
                      {status === 'completed' ? (
                        <span className={styles.completeFileName}>
                          {customName}
                          {fileExtension}
                        </span>
                      ) : (
                        <Form.Control
                          size="sm"
                          className={`${styles.fileNameInput} ${hasError ? styles.inputError : ''}`}
                          type="text"
                          id={`file-name-${id}`}
                          placeholder="نام فایل (اجباری)"
                          value={customName}
                          onChange={(e) => handleFileNameChange(id, e.target.value)}
                          disabled={status !== 'pending'}
                        />
                      )}

                      <div className={styles.fileMetaInfo}>
                        <span className={styles.fileSize}>{formatFileSize(file.size)}</span>
                        {status !== 'completed' && <span className={styles.fileExtension}>{fileExtension}</span>}
                      </div>

                      {hasError && <div className={styles.inputErrorMessage}>{renameErrors[id]}</div>}
                    </div>

                    {status === 'pending' && !uploading && (
                      <button
                        className={styles.removeButton}
                        onClick={(e) => {
                          e.stopPropagation();
                          removeFile(id);
                        }}
                      >
                        <FontAwesomeIcon icon={faTimes} />
                      </button>
                    )}
                  </div>

                  {getFileStatusComponent(fileObj)}
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* Add more files button */}
      {files.length > 0 && !uploading && !uploadComplete && (
        <button className={styles.addMoreButton} onClick={() => fileInputRef.current.click()}>
          <FontAwesomeIcon icon={faPlus} className={styles.plusIcon} />
          <span>افزودن فایل</span>
        </button>
      )}

      {/* Upload Button */}
      <div className={styles.uploadButtonContainer}>
        <Button variant={uploadComplete ? 'success' : 'primary'} className={styles.uploadButton} disabled={files.length === 0 || uploading || uploadComplete || !files.some((f) => f.status === 'pending')} onClick={uploadFiles}>
          {uploading ? (
            <>
              <FontAwesomeIcon icon={faSpinner} spin className="ms-2" />
              <span>در حال آپلود...</span>
            </>
          ) : uploadComplete ? (
            <>
              <FontAwesomeIcon icon={faCheck} className="ms-2" />
              <span>آپلود کامل شد</span>
            </>
          ) : (
            <>
              <FontAwesomeIcon icon={faUpload} className="ms-2" />
              <span>آپلود {files.length} فایل</span>
            </>
          )}
        </Button>
      </div>
    </div>
  );
};

export default MobileSecureFileUploader;
