import React, {useState, useMemo, useEffect, useRef} from 'react';
import {Button, Form, Spinner, Alert, Row, Col} from 'react-bootstrap';
import {FontAwesomeIcon} from '@fortawesome/react-fontawesome';
import {faTrashAlt, faDownload, faEllipsisVertical, faFileUpload, faCheck, faSquare, faCheckSquare} from '@fortawesome/free-solid-svg-icons';
import secureFileApi from '../../api/secureFileApi';
import SecureFileDownload from './SecureFileDownload';
import FilePreview from './FilePreview';
import clsx from 'clsx';
import styles from './FileManagerMobile.module.css';
import MobileSecureFileUploader from './MobileSecureFileUploader';

const FileManagerMobile = ({caseId, area, caseTitle = '', readOnly = false, showUploader = true, onFileUpload, onFileDelete, fileCategories = [], maxFileSize = 50, acceptedFileTypes = null}) => {
  const [files, setFiles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [search, setSearch] = useState('');
  const [preview, setPreview] = useState(null);
  const [selected, setSelected] = useState(new Set());
  const [uploadVisible, setUploadVisible] = useState(false);
  const [selectAll, setSelectAll] = useState(false);
  const [menuOpen, setMenuOpen] = useState(null);

  const menuRef = useRef(null);

  const toggleMenu = (id, e) => {
    e.stopPropagation();
    setMenuOpen((prev) => (prev === id ? null : id));
  };

  const formatSize = (bytes) => {
    if (bytes < 1024) return bytes + ' B';
    const kb = bytes / 1024;
    if (kb < 1024) return Math.round(kb) + ' KB';
    const mb = kb / 1024;
    return mb.toFixed(1) + ' MB';
  };

  const fetchFiles = () => {
    setLoading(true);
    secureFileApi
      .getCaseFiles(caseId, area)
      .then((data) => setFiles(data))
      .catch((err) => setError(err))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    const handleClickOutside = (e) => {
      if (menuRef.current && !menuRef.current.contains(e.target)) {
        setMenuOpen(null);
      }
    };
    document.addEventListener('click', handleClickOutside);
    return () => document.removeEventListener('click', handleClickOutside);
  }, []);

  useEffect(() => {
    if (caseId && area) fetchFiles();
  }, [caseId, area]);

  const filtered = useMemo(() => {
    const term = search.trim().toLowerCase();
    return term ? files.filter((f) => f.fileName.toLowerCase().includes(term)) : files;
  }, [files, search]);

  const toggleSelect = (id, e) => {
    e.stopPropagation();
    setSelected((prev) => {
      const ns = new Set(prev);
      ns.has(id) ? ns.delete(id) : ns.add(id);
      return ns;
    });
  };

  const handleSelectAll = () => {
    if (selectAll) {
      setSelected(new Set());
    } else {
      setSelected(new Set(filtered.map((f) => f.fileId)));
    }
    setSelectAll(!selectAll);
  };

  useEffect(() => {
    setSelectAll(filtered.length > 0 && selected.size === filtered.length);
  }, [selected, filtered]);

  const bulkDelete = async () => {
    if (!selected.size) return;
    if (!window.confirm(`حذف ${selected.size} فایل؟`)) return;
    for (let id of selected) {
      await secureFileApi.deleteFile(id);
    }
    onFileDelete?.();
    setSelected(new Set());
    fetchFiles();
  };

  const handleDelete = async (id, e) => {
    e.stopPropagation();
    if (!window.confirm('آیا از حذف این فایل اطمینان دارید؟')) return;
    await secureFileApi.deleteFile(id);
    onFileDelete?.();
    fetchFiles();
  };

  const handleFileClick = (file, e) => {
    e.stopPropagation();
    setPreview({ ...file });
  };

  if (loading)
    return (
      <div className={styles.center}>
        <Spinner animation="border" />
        <span>در حال بارگذاری...</span>
      </div>
    );
  if (error)
    return (
      <Alert variant="danger">
        خطا در بارگذاری فایل‌ها
        <button onClick={fetchFiles} className="btn btn-sm btn-link">
          تلاش مجدد
        </button>
      </Alert>
    );

  return (
    <div className={styles.manager} dir="rtl">
      <div className={styles.header}>
        <div className={styles.searchRow}>
          <Form.Control size="sm" placeholder="جستجو..." value={search} onChange={(e) => setSearch(e.target.value)} className={styles.search} />

          <div className={styles.headerActions}>
            {showUploader && !readOnly && (
              <Button size="sm" variant={uploadVisible ? 'secondary' : 'primary'} className={styles.uploadBtn} onClick={() => setUploadVisible((v) => !v)}>
                <FontAwesomeIcon icon={uploadVisible ? faCheck : faFileUpload} className="ms-1" />
                {uploadVisible ? 'بستن' : 'افزودن'}
              </Button>
            )}
          </div>
        </div>
      </div>

      {uploadVisible && showUploader && !readOnly && (
        <>
          <div className={styles.uploaderBackdrop} onClick={() => setUploadVisible(false)} />
          <div className={styles.uploaderOverlay}>
            <MobileSecureFileUploader
              caseId={caseId}
              caseTitle={caseTitle}
              area={area}
              maxFileSize={maxFileSize}
              acceptedFileTypes={acceptedFileTypes}
              multiple
              onUploadComplete={(uploads) => {
                onFileUpload?.(uploads);
                setUploadVisible(false);
                fetchFiles();
              }}
              onClose={() => setUploadVisible(false)}
            />
          </div>
        </>
      )}

      {selected.size > 0 && (
        <div className={styles.selectAllRow}>
          <div className={styles.selectAllCheckbox} onClick={handleSelectAll}>
            <FontAwesomeIcon icon={selectAll ? faCheckSquare : faSquare} />
            <span className={styles.selectAllText}>{selectAll ? 'لغو انتخاب همه' : 'انتخاب همه'}</span>
          </div>
        </div>
      )}

      {selected.size > 0 && (
        <div className={styles.bulkBar}>
          <span>{selected.size} فایل انتخاب شده</span>
          <div className={styles.bulkActions}>
            <button onClick={bulkDelete} className={styles.iconButton}>
              <FontAwesomeIcon icon={faTrashAlt} />
            </button>
            <button className={styles.iconButton}>
              <FontAwesomeIcon icon={faDownload} />
            </button>
          </div>
        </div>
      )}

      <ul className={styles.list}>
        {filtered.length ? (
          filtered.map((file) => (
            <li key={file.fileId} className={clsx(styles.item, selected.has(file.fileId) && styles.selected)}>
              <div className={styles.checkbox} onClick={(e) => toggleSelect(file.fileId, e)}>
                <FontAwesomeIcon icon={selected.has(file.fileId) ? faCheckSquare : faSquare} />
              </div>
              <div className={styles.info} onClick={(e) => handleFileClick(file, e)}>
                <div className={styles.meta}>
                  <div className={styles.name} title={file.fileName}>
                    {file.fileName}
                  </div>
                  <div className={styles.details}>{new Intl.DateTimeFormat('fa-IR', {year: 'numeric', month: 'numeric', day: 'numeric'}).format(new Date(file.uploadedDate))}</div>
                </div>
              </div>
              {!readOnly && (
                <div className={styles.dropdown} ref={menuOpen === file.fileId ? menuRef : null}>
                  <button className={styles.toggle} onClick={(e) => toggleMenu(file.fileId, e)}>
                    <FontAwesomeIcon icon={faEllipsisVertical} />
                  </button>

                  {menuOpen === file.fileId && (
                    <div className={styles.dropdownMenu}>
                      <SecureFileDownload fileId={file.fileId} fileName={file.fileName} buttonText={' دانلود '} variant="icon" className={`${styles.dropdownItem} ${styles.successItem}`} />
                      <button variant="icon" className={`${styles.dropdownItem} ${styles.dangerItem}`} onClick={(e) => handleDelete(file.fileId, e)}>
                        <FontAwesomeIcon icon={faTrashAlt} className="ms-1" /> حذف
                      </button>
                    </div>
                  )}
                </div>
              )}
            </li>
          ))
        ) : (
          <li className={styles.empty}>فایلی وجود ندارد</li>
        )}
      </ul>

      {preview && (
        <FilePreview 
          file={preview} 
          onClose={() => setPreview(null)} 
          defaultFullscreen={true}
        />
      )}
    </div>
  );
};

export default FileManagerMobile;
