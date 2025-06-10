// src/components/SecureFileUploader.jsx
import React, {useState, useRef, useEffect} from 'react';
import {Form, Container, Card, Button, ProgressBar, Alert, Spinner} from 'react-bootstrap';
import 'bootstrap/dist/css/bootstrap.min.css';
import styles from '../../assets/styles/FileUploader.module.css';
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
} from '@fortawesome/free-solid-svg-icons';

const SecureFileUploader = ({
  onUploadComplete,
  caseId,
  area,
  maxFileSize = 100, // in MB
  acceptedFileTypes = null,
  multiple = true,
  rtl = true,
  onClose,
  caseTitle = ''  // Adding default value if no title is provided
}) => {
  const [files, setFiles] = useState([]);
  const [uploading, setUploading] = useState(false);
  const [uploadComplete, setUploadComplete] = useState(false);
  const [dragActive, setDragActive] = useState(false);
  const [error, setError] = useState(null);
  const {refreshSession} = useAuth();

  const fileInputRef = useRef(null);
  const dropzoneRef = useRef(null);

  // src/components/SecureFileUploader.jsx
  const [isMobile, setIsMobile] = useState(false);

  useEffect(() => {
    const onResize = () => setIsMobile(window.innerWidth <= 768);
    onResize();
    window.addEventListener('resize', onResize);
    return () => window.removeEventListener('resize', onResize);
  }, []);

  useEffect(() => {
    const onPaste = (e) => {
      if (e.clipboardData && e.clipboardData.files.length) {
        handleFiles(Array.from(e.clipboardData.files));
      }
    };
    document.addEventListener('paste', onPaste);
    return () => document.removeEventListener('paste', onPaste);
  }, []);

  // Set RTL direction based on prop
  useEffect(() => {
    document.documentElement.dir = rtl ? 'rtl' : 'ltr';
  }, [rtl]);

  // Reset inactivity timer on user interaction
  useEffect(() => {
    const resetTimer = () => refreshSession();
    document.addEventListener('mousemove', resetTimer);
    document.addEventListener('keypress', resetTimer);

    return () => {
      document.removeEventListener('mousemove', resetTimer);
      document.removeEventListener('keypress', resetTimer);
    };
  }, [refreshSession]);

  // Handle drag events
  const handleDrag = (e) => {
    e.preventDefault();
    e.stopPropagation();

    if (e.type === 'dragenter' || e.type === 'dragover') {
      setDragActive(true);
    } else if (e.type === 'dragleave') {
      setDragActive(false);
    }
  };

  // Handle dropped files
  const handleDrop = (e) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);

    const droppedFiles = Array.from(e.dataTransfer.files);
    handleFiles(droppedFiles);
  };

  // Handle file selection via input
  const handleFileSelect = (e) => {
    const selectedFiles = Array.from(e.target.files);
    handleFiles(selectedFiles);
  };

  // Handle files (validation and adding to state)
  const handleFiles = (newFiles) => {
    setError(null);
    setUploadComplete(false); // Reset upload status when adding new files

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
          updatedFiles.push({
            file,
            id: `file-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
            progress: 0,
            status: 'pending',
            uploadedFileId: null,
            customName: '',
            extension: file.name.split('.').pop().toLowerCase(),
            showError: false,
          });
        }
      });

      return updatedFiles;
    });
  };

  // Remove a file
  const removeFile = (fileId) => {
    setFiles((prevFiles) => prevFiles.filter((file) => file.id !== fileId));
    // Reset file input so the same file can be selected again
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  // Upload all files
  const uploadFiles = async () => {
    if (files.length === 0 || uploading || uploadComplete) return;

    const hasEmptyNames = files.some((f) => f.status === 'pending' && !f.customName.trim());
    if (hasEmptyNames) {
      setError('لطفاً برای تمام فایل‌ها نام وارد کنید');
      setFiles((prevFiles) => prevFiles.map((f) => (f.status === 'pending' && !f.customName.trim() ? {...f, showError: true} : f)));
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

        // Perform actual upload with encryption
        const fileName = fileObj.customName.trim() + '.' + fileObj.extension;
        const response = await secureFileApi.uploadFile(fileObj.file, caseId, area, fileName);
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
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  // Get upload button text and icon
  const getUploadButtonContent = () => {
    if (uploading) {
      return (
        <>
          <FontAwesomeIcon icon={faSpinner} spin className="ms-2" />
          <span>در حال آپلود...</span>
        </>
      );
    } else if (uploadComplete) {
      return (
        <>
          <FontAwesomeIcon icon={faCheck} className="ms-2" />
          <span>آپلود کامل شد</span>
        </>
      );
    } else {
      return (
        <>
          <FontAwesomeIcon icon={faUpload} className="ms-2" />
          <span>آپلود فایل‌ها</span>
        </>
      );
    }
  };

  // Get file status component
  const getFileStatusComponent = (fileObj) => {
    const {status, progress, error, id} = fileObj;

    if (status === 'error') {
      return (
        <div className="mt-2 w-100">
          <Alert variant="danger" className="p-1 mt-1 mb-0">
            <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
            <small>{error || 'خطا در آپلود'}</small>
          </Alert>
          <Button variant="warning" size="sm" onClick={() => retryUpload(id)} className="mt-2">
            <VscDebugRestart />
          </Button>
        </div>
      );
    }

    if (status === 'uploading') {
      return (
        <div className="mt-2 w-100">
          <ProgressBar animated now={progress} variant="primary" style={{height: '0.5rem'}} />
          <div className="d-flex justify-content-end">
            <small className="text-muted">{`${Math.round(progress)}%`}</small>
          </div>
        </div>
      );
    }

    if (status === 'completed') {
      return (
        <div className="mt-2 w-100">
          <ProgressBar now={100} variant="success" style={{height: '0.5rem'}} />
          <div className="d-flex justify-content-end">
            <small className="text-success">آپلود کامل شد!</small>
          </div>
        </div>
      );
    }

    return null;
  };

  const retryUpload = (fileId) => {
    setFiles((prevFiles) => prevFiles.map((file) => (file.id === fileId ? {...file, status: 'pending', progress: 0, error: null} : file)));
  };

  return (
    <Container className="py-4">
      <Card className={styles['secure-file-uploader-card']}>
        <Card.Body className="p-4">
          <div className="position-absolute top-0 end-0 p-2">
            <Button variant="link" className={`text-secondary p-0 ${styles['close-button']}`} onClick={onClose}>
              <FontAwesomeIcon icon={faTimes} size="lg" />
            </Button>
          </div>

          <h2 className="mb-2 h4">{caseTitle}</h2>
          <p className="text-muted mb-4">فایل‌های مورد نظر خود را با امنیت بالا آپلود کنید. فایل‌ها به صورت رمزنگاری شده ذخیره می‌شوند.</p>

          {/* Error message */}
          {error && (
            <Alert variant="danger" onClose={() => setError(null)} dismissible>
              {error}
            </Alert>
          )}

          {/* Dropzone */}
          <div
            ref={dropzoneRef}
            className={`${styles.dropzone} rounded p-4 mb-4 text-center ${dragActive ? styles.active : ''}`}
            {...(!isMobile && {
              onDragEnter: handleDrag,
              onDragOver: handleDrag,
              onDragLeave: handleDrag,
              onDrop: handleDrop,
            })}
            onClick={() => fileInputRef.current.click()}
          >
            <div className="d-flex flex-column align-items-center py-4">
              <p className="mb-1 fw-medium">فایل‌ها را اینجا رها کنید یا برای انتخاب کلیک کنید</p>
              <p className="text-muted small mb-3">
                یا میتوانید فایل‌ها را از کامپیوتر کپی کرده و با <kbd>Ctrl+V</kbd> اینجا بچسبانید
              </p>
              <input type="file" ref={fileInputRef} className={styles['file-input']} multiple={multiple} onChange={handleFileSelect} accept={acceptedFileTypes ? acceptedFileTypes.join(',') : undefined} />
              <Button
                variant="primary"
                onClick={(e) => {
                  e.stopPropagation();
                  fileInputRef.current.click();
                }}
                size="sm"
              >
                انتخاب فایل‌ها
              </Button>
            </div>
          </div>

          {/* File List */}
          {files.length > 0 && (
            <div className={`mb-4 ${styles['file-list']}`}>
              {files.map((fileObj) => {
                const {file, id, status} = fileObj;
                const {icon, color} = getFileIcon(file);

                return (
                  <div key={id} className={`${styles['file-item']} rounded ${styles[status]} border d-flex align-items-center justify-content-between`}>
                    <div className="d-flex align-items-center flex-grow-1 min-width-0">
                      <div className={`me-3 ${color}`}>
                        <FontAwesomeIcon icon={icon} size="lg" />
                      </div>
                      <div className="overflow-hidden w-100">
                        <div className="d-flex align-items-center w-100">
                          <div className="flex-grow-1 m-2">
                            <Form.Control
                              className="fs-7 p-2"
                              type="text"
                              placeholder=" نام فایل (اجباری) "
                              value={fileObj.customName}
                              onChange={(e) => {
                                const newName = e.target.value;
                                setFiles((prevFiles) => prevFiles.map((f) => (f.id === fileObj.id ? {...f, customName: newName} : f)));
                              }}
                              isInvalid={status === 'pending' && !fileObj.customName.trim() && fileObj.showError}
                              disabled={status !== 'pending'}
                              required
                            />
                            {status === 'pending' && !fileObj.customName.trim() && fileObj.showError && 
                              <Form.Control.Feedback type="invalid">
                                نام فایل اجباری است
                              </Form.Control.Feedback>
                            }
                          </div>
                          <div className="ms-2 d-flex flex-column align-items-end">
                            <span className="text-muted">.{fileObj.extension}</span>
                            <small className="text-muted">{formatFileSize(file.size)}</small>
                          </div>
                        </div>
                        {getFileStatusComponent(fileObj)}
                      </div>
                    </div>
                    {status === 'pending' && !uploading && (
                      <Button
                        variant="link"
                        className={`text-danger p-0 ms-2 ${styles['file-action']}`}
                        onClick={(e) => {
                          e.stopPropagation();
                          removeFile(id);
                        }}
                      >
                        <FontAwesomeIcon icon={faTimes} />
                      </Button>
                    )}
                  </div>
                );
              })}
            </div>
          )}

          {/* Upload Button */}
          <div className="d-flex justify-content-start">
            <Button
              variant="success"
              disabled={files.length === 0 || uploading || uploadComplete || !files.some((f) => f.status === 'pending') || files.some((f) => f.status === 'pending' && !f.customName.trim())}
              onClick={uploadFiles}
            >
              {getUploadButtonContent()}
            </Button>
          </div>
        </Card.Body>
      </Card>
    </Container>
  );
};

export default SecureFileUploader;
