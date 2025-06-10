import React, {useState, useMemo, useEffect} from 'react';
import {Container, Row, Col, Table, Button, Form, Spinner, Alert, Dropdown} from 'react-bootstrap';
import {FontAwesomeIcon} from '@fortawesome/react-fontawesome';
import {faEye, faTrashAlt, faDownload, faEllipsisVertical, faFileUpload} from '@fortawesome/free-solid-svg-icons';
import secureFileApi from '../../api/secureFileApi';
import SecureFileUploader from './SecureFileUploader';
import SecureFileDownload from './SecureFileDownload';
import FilePreview from './FilePreview';
import clsx from 'clsx';
import styles from './FileManagerDesktop.module.css';

const FileManagerDesktop = ({caseId, caseTitle = '', area, readOnly = false, showUploader = true, onFileUpload, onFileDelete}) => {
  const [files, setFiles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [search, setSearch] = useState('');
  const [preview, setPreview] = useState(null);
  const [selected, setSelected] = useState(new Set());
  const [uploadVisible, setUploadVisible] = useState(false);

  const fetchFiles = () => {
    setLoading(true);
    secureFileApi
      .getCaseFiles(caseId, area)
      .then((data) => setFiles(data))
      .catch((err) => setError(err))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    if (caseId && area) fetchFiles();
  }, [caseId, area]);

  const filtered = useMemo(() => {
    const term = search.trim().toLowerCase();
    return term ? files.filter((f) => f.fileName.toLowerCase().includes(term)) : files;
  }, [files, search]);

  const toggleSelect = (id) => {
    setSelected((prev) => {
      const ns = new Set(prev);
      ns.has(id) ? ns.delete(id) : ns.add(id);
      return ns;
    });
  };

  const bulkDelete = async () => {
    if (!selected.size) return;
    if (!window.confirm(`حذف ${selected.size} فایل؟`)) return;
    for (let id of selected) await secureFileApi.deleteFile(id);
    onFileDelete?.();
    setSelected(new Set());
    fetchFiles();
  };

  const handleDelete = async (id) => {
    if (!window.confirm('آیا از حذف این فایل اطمینان دارید؟')) return;
    await secureFileApi.deleteFile(id);
    onFileDelete?.();
    fetchFiles();
  };

  const handleFileClick = (file) => {
    setPreview({ ...file });
  };

  if (loading) return <Spinner animation="border" />;
  if (error)
    return (
      <Alert variant="danger">
        خطا در بارگذاری
        <button onClick={fetchFiles} className="btn btn-sm btn-link">
          تلاش مجدد
        </button>
      </Alert>
    );

  return (
    <Container fluid className="p-3">
      <Row className="align-items-center mb-3">
        <Col>
          <Form.Control placeholder="جستجو..." value={search} onChange={(e) => setSearch(e.target.value)} />
        </Col>
        <Col xs="auto">
          {showUploader && !readOnly && (
            <Button size="sm" variant={uploadVisible ? 'secondary' : 'primary'} className="me-2" onClick={() => setUploadVisible((v) => !v)}>
              <FontAwesomeIcon icon={faFileUpload} /> افزودن
            </Button>
          )}
          {selected.size > 0 && (
            <>
              <Button size="sm" variant="danger" onClick={bulkDelete} className="me-2">
                <FontAwesomeIcon icon={faTrashAlt} /> حذف گروهی
              </Button>
              <Button size="sm" variant="success" className="me-2">
                <FontAwesomeIcon icon={faDownload} /> دانلود گروهی
              </Button>
            </>
          )}
        </Col>
      </Row>

      {uploadVisible && showUploader && !readOnly && (
        <div className={styles.modalOverlay}>
          <div className={styles.modalContent}>
            <SecureFileUploader
              caseId={caseId}
              caseTitle={caseTitle}
              area={area}
              maxFileSize={50}
              acceptedFileTypes={null}
              multiple
              onUploadComplete={(uploads) => {
                onFileUpload?.(uploads);
                setUploadVisible(false);
                fetchFiles();
              }}
              onClose={() => setUploadVisible(false)}
            />
          </div>
        </div>
      )}

      <Table hover responsive className={styles.table} dir="rtl">
        <thead>
          <tr>
            <th>
              <Form.Check
                checked={selected.size === filtered.length && filtered.length > 0}
                onChange={() => {
                  const all = new Set(selected);
                  if (all.size === filtered.length) all.clear();
                  else filtered.forEach((f) => all.add(f.fileId));
                  setSelected(all);
                }}
              />
            </th>
            <th>نام فایل</th>
            <th>تاریخ آپلود</th>
            <th>حجم</th>
            <th>اقدامات</th>
          </tr>
        </thead>
        <tbody>
          {filtered.length === 0 ? (
            <tr>
              <td colSpan="5" className="text-center">
                 فایلی وجود ندارد
              </td>
            </tr>
          ) : (
            filtered.map((file) => (
              <tr key={file.fileId} className={clsx(selected.has(file.fileId) && styles.selectedRow)}>
                <td>
                  <Form.Check checked={selected.has(file.fileId)} onChange={() => toggleSelect(file.fileId)} />
                </td>
                <td onClick={() => handleFileClick(file)} className={styles.fileName}>
                  {file.fileName}
                </td>
                <td>{new Date(file.uploadedDate).toLocaleString('fa-IR')}</td>
                <td>{(file.fileSize / 1024).toFixed(1)} کیلوبایت</td>
                <td className={styles.actionButtons}>
                  <Button variant="link" className={styles.iconButton} onClick={() => handleFileClick(file)}>
                    <FontAwesomeIcon icon={faEye} />
                  </Button>
                  <SecureFileDownload fileId={file.fileId} fileName={file.fileName} as={Button} variant="link" className={clsx(styles.iconButton, styles.downloadButton)}>
                    <FontAwesomeIcon icon={faDownload} />
                  </SecureFileDownload>
                  {!readOnly && (
                    <Button variant="link" className={clsx(styles.iconButton, styles.deleteButton)} onClick={() => handleDelete(file.fileId)}>
                      <FontAwesomeIcon icon={faTrashAlt} />
                    </Button>
                  )}
                </td>
              </tr>
            ))
          )}
        </tbody>
      </Table>

      {preview && (
        <FilePreview 
          file={preview} 
          onClose={() => setPreview(null)} 
          defaultFullscreen={true}
        />
      )}
    </Container>
  );
};

export default FileManagerDesktop;
