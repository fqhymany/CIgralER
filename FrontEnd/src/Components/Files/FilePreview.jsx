// src/components/FilePreview.jsx
import React, {useState, useEffect, useCallback, useRef} from 'react';
import {Modal, Button, Spinner} from 'react-bootstrap';
import {FontAwesomeIcon} from '@fortawesome/react-fontawesome';
import {faDownload, faTimes, faSearchPlus, faSearchMinus, faExpand, faCompress, faArrowLeft, faArrowRight, faRedo, faUndo} from '@fortawesome/free-solid-svg-icons';
import secureFileApi from '../../api/secureFileApi';
import SecureFileDownload from './SecureFileDownload';
import {Document, Page, pdfjs} from 'react-pdf';
import 'react-pdf/dist/esm/Page/AnnotationLayer.css';
import 'react-pdf/dist/esm/Page/TextLayer.css';
import styles from './FilePreview.module.css';

// Set worker path for PDF.js - using HTTPS and a reliable version
if (typeof window !== 'undefined' && 'Worker' in window) {
  pdfjs.GlobalWorkerOptions.workerSrc = `/pdf.worker.min.mjs`;
}

// Alternative approach - create blob URL for worker if CDN fails

const FilePreview = ({file, onClose, defaultFullscreen = false}) => {
  // Common states
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [fullscreen, setFullscreen] = useState(defaultFullscreen);
  const [accessToken, setAccessToken] = useState(null);

  const containerRef = useRef(null);

  // وقتی کامپوننت مانت می‌شود، اگر defaultFullscreen فعال باشد، حالت تمام صفحه را فعال می‌کنیم
  useEffect(() => {
    if (defaultFullscreen) {
      setFullscreen(true);
    }
  }, [defaultFullscreen]);

  // PDF specific states
  const [numPages, setNumPages] = useState(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [scale, setScale] = useState(1.0);
  const [pdfRotation, setPdfRotation] = useState(0);
  const [pdfBlob, setPdfBlob] = useState(null);

  // Image specific states
  const [imageData, setImageData] = useState(null);
  const [imageZoom, setImageZoom] = useState(1);
  const [imagePosition, setImagePosition] = useState({x: 0, y: 0});
  const [isDragging, setIsDragging] = useState(false);
  const [initialTouchDistance, setInitialTouchDistance] = useState(null);
  const [initialZoom, setInitialZoom] = useState(1);

  // Refs
  const dragStart = useRef({x: 0, y: 0});
  const canvasRef = useRef(null);

  const getFileType = () => {
    if (!file || !file.fileName) return 'unknown';
    const extension = file.fileName.split('.').pop().toLowerCase();
    if (['jpg', 'jpeg', 'png', 'gif', 'bmp', 'webp', 'svg'].includes(extension)) return 'image';
    if (extension === 'pdf') return 'pdf';
    if (['txt', 'md', 'html', 'xml', 'json', 'csv'].includes(extension)) return 'text';
    if (['doc', 'docx', 'xls', 'xlsx', 'ppt', 'pptx'].includes(extension)) return 'office';
    return 'other';
  };

  const loadSecurePdf = async (token) => {
    try {

      const response = await fetch(`${window.location.origin}/api/files/access/${token}`, {
        method: 'GET',
        headers: {
          'Cache-Control': 'no-cache',
          Pragma: 'no-cache',
          Accept: 'application/pdf,application/octet-stream,*/*',
        },
        credentials: 'include',
        mode: 'cors',
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const contentType = response.headers.get('Content-Type');

      const contentLength = response.headers.get('Content-Length');

      try {
        const arrayBuffer = await response.arrayBuffer();

        const blob = new Blob([arrayBuffer], {type: contentType || 'application/pdf'});

        if (blob.size === 0) {
          throw new Error('فایل پی دی اف خالی است');
        }

        return blob;
      } catch (bufferErr) {
        console.error('Error with ArrayBuffer approach:', bufferErr);

        const blob = await response.blob();

        if (blob.size === 0) {
          throw new Error('فایل پی دی اف خالی است');
        }

        return blob;
      }
    } catch (err) {
      console.error('خطا در بارگیری PDF:', err);
      setError('خطا در بارگذاری فایل PDF');
      return null;
    }
  };

  useEffect(() => {
    let isMounted = true;

    const fetchSecureToken = async () => {
      try {
        if (!isMounted) return;

        setLoading(true);
        setError(null);
        const result = await secureFileApi.generateSecureAccessUrl(file.fileId, 10);

        if (!isMounted) return;
        setAccessToken(result.accessToken);

        const fileType = getFileType();

        if (fileType === 'image') {
          await loadSecureImage(result.accessToken);
        } else if (fileType === 'pdf') {
          const fileContent = await loadSecurePdf(result.accessToken);
          if (fileContent && isMounted) {
            setPdfBlob(fileContent);
            setLoading(false);
          } else if (isMounted) {
            setLoading(false);
          }
        } else {
          if (isMounted) setLoading(false);
        }
      } catch (err) {
        if (isMounted) {
          console.error('Error generating secure access:', err);
          setError('خطا در بارگذاری فایل. لطفا مجددا تلاش کنید.');
          setLoading(false);
        }
      }
    };

    if (file && file.fileId) {
      fetchSecureToken();
    }

    return () => {
      isMounted = false;
      if (imageData && imageData.objectUrl) {
        URL.revokeObjectURL(imageData.objectUrl);
      }
    };
  }, [file]);

  const loadSecureImage = async (token, returnBlob = false) => {
    try {
      const response = await fetch(`${window.location.origin}/api/files/access/${token}`, {
        method: 'GET',
        headers: {
          'Cache-Control': 'no-cache',
          Pragma: 'no-cache',
        },
      });
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const blob = await response.blob();

      if (returnBlob) {
        return blob;
      }

      const objectUrl = URL.createObjectURL(blob);

      const img = new Image();
      img.onload = () => {
        if (canvasRef.current) {
          drawImageOnCanvas(img);
        } else {
          setImageData({img, objectUrl});
        }
      };
      img.onload = () => {
        if (canvasRef.current) {
          drawImageOnCanvas(img);
        } else {
          setImageData({img, objectUrl});
        }
        setLoading(false);
      };
      img.onerror = (e) => {
        console.error('Error loading image:', e);
        setError('خطا در بارگذاری تصویر');
        setLoading(false);
      };
      img.src = objectUrl;
    } catch (err) {
      console.error('Error loading secure image:', err);
      setError('خطا در بارگذاری تصویر امن');
      setLoading(false);
    }
  };

  const drawImageOnCanvas = useCallback(
    (img) => {
      if (!canvasRef.current) return;

      const canvas = canvasRef.current;
      const ctx = canvas.getContext('2d');
      const container = containerRef.current;

      if (!container) return;

      const containerRect = container.getBoundingClientRect();
      canvas.width = containerRect.width;
      canvas.height = containerRect.height;

      const imgRatio = img.width / img.height;
      const containerRatio = containerRect.width / containerRect.height;

      let drawWidth, drawHeight;
      if (imgRatio > containerRatio) {
        drawWidth = containerRect.width;
        drawHeight = containerRect.width / imgRatio;
      } else {
        drawHeight = containerRect.height;
        drawWidth = containerRect.height * imgRatio;
      }

      const centerX = canvas.width / 2;
      const centerY = canvas.height / 2;

      const scaledWidth = drawWidth * imageZoom;
      const scaledHeight = drawHeight * imageZoom;

      const posX = centerX - scaledWidth / 2 + imagePosition.x;
      const posY = centerY - scaledHeight / 2 + imagePosition.y;

      ctx.clearRect(0, 0, canvas.width, canvas.height);

      ctx.drawImage(img, posX, posY, scaledWidth, scaledHeight);
    },
    [imagePosition, imageZoom]
  );

  useEffect(() => {
    if (imageData && imageData.img && canvasRef.current) {
      drawImageOnCanvas(imageData.img);
    }
  }, [imageData, imagePosition, imageZoom, fullscreen, drawImageOnCanvas]);

  useEffect(() => {
    const handleResize = () => {
      if (imageData && imageData.img && canvasRef.current) {
        drawImageOnCanvas(imageData.img);
      }
    };

    window.addEventListener('resize', handleResize);
    return () => {
      window.removeEventListener('resize', handleResize);
    };
  }, [imageData, drawImageOnCanvas]);

  const handleMouseDown = (e) => {
    if (getFileType() !== 'image') return;
    e.preventDefault();
    setIsDragging(true);
    dragStart.current = {
      x: e.clientX - imagePosition.x,
      y: e.clientY - imagePosition.y,
    };
  };

  const handleMouseMove = useCallback(
    (e) => {
      if (!isDragging) return;
      e.preventDefault();
      setImagePosition({
        x: e.clientX - dragStart.current.x,
        y: e.clientY - dragStart.current.y,
      });
    },
    [isDragging]
  );

  const handleMouseUp = () => {
    setIsDragging(false);
  };

  const handleTouchStart = (e) => {
    if (getFileType() !== 'image') return;

    if (e.touches.length === 1) {
      setIsDragging(true);
      dragStart.current = {
        x: e.touches[0].clientX - imagePosition.x,
        y: e.touches[0].clientY - imagePosition.y,
      };
    } else if (e.touches.length === 2) {
      const touch1 = e.touches[0];
      const touch2 = e.touches[1];
      const distance = Math.hypot(touch1.clientX - touch2.clientX, touch1.clientY - touch2.clientY);

      setInitialTouchDistance(distance);
      setInitialZoom(imageZoom);
    }
  };

  const handleTouchMove = useCallback(
    (e) => {
      e.preventDefault();

      if (e.touches.length === 1 && isDragging) {
        setImagePosition({
          x: e.touches[0].clientX - dragStart.current.x,
          y: e.touches[0].clientY - dragStart.current.y,
        });
      } else if (e.touches.length === 2 && initialTouchDistance !== null) {
        const touch1 = e.touches[0];
        const touch2 = e.touches[1];
        const currentDistance = Math.hypot(touch1.clientX - touch2.clientX, touch1.clientY - touch2.clientY);

        const scale = currentDistance / initialTouchDistance;
        const newZoom = Math.max(0.5, Math.min(5, initialZoom * scale));
        setImageZoom(newZoom);
      }
    },
    [isDragging, initialTouchDistance, initialZoom, imagePosition.x, imagePosition.y]
  );

  const handleTouchEnd = () => {
    setIsDragging(false);
    setInitialTouchDistance(null);
  };

  useEffect(() => {
    if (isDragging || initialTouchDistance !== null) {
      window.addEventListener('mousemove', handleMouseMove);
      window.addEventListener('mouseup', handleMouseUp);
      window.addEventListener('touchmove', handleTouchMove, {passive: false});
      window.addEventListener('touchend', handleTouchEnd);
    }

    return () => {
      window.removeEventListener('mousemove', handleMouseMove);
      window.removeEventListener('mouseup', handleMouseUp);
      window.removeEventListener('touchmove', handleTouchMove);
      window.removeEventListener('touchend', handleTouchEnd);
    };
  }, [isDragging, initialTouchDistance, handleMouseMove, handleTouchMove]);

  const handleZoom = (delta) => {
    if (getFileType() === 'pdf') {
      setScale((prev) => Math.max(0.5, Math.min(3, prev + delta)));
    } else if (getFileType() === 'image') {
      setImageZoom((prev) => Math.max(0.5, Math.min(5, prev + delta)));
    }
  };

  const resetView = () => {
    if (getFileType() === 'pdf') {
      setScale(1.0);
      setPdfRotation(0);
    } else if (getFileType() === 'image') {
      setImageZoom(1);
      setImagePosition({x: 0, y: 0});
    }
  };

  const rotatePdf = (delta) => {
    setPdfRotation((prev) => (prev + delta) % 360);
  };

  const [pdfFallbackMode, setPdfFallbackMode] = useState(false);
  const [pdfData, setPdfData] = useState(null);
  const canvasContainerRef = useRef(null);

  const fetchPdfForFallback = useCallback(async () => {
    if (!accessToken || pdfData) return;

    try {
      const response = await fetch(`${window.location.origin}/api/files/access/${accessToken}`);
      if (!response.ok) throw new Error(`HTTP error ${response.status}`);

      const arrayBuffer = await response.arrayBuffer();
      setPdfData(arrayBuffer);
    } catch (err) {
      console.error('Error fetching PDF for fallback:', err);
      setError('خطا در بارگذاری PDF برای نمایش جایگزین');
    }
  }, [accessToken, pdfData]);

  const renderPdfFallback = useCallback(async () => {
    if (!pdfData || !canvasContainerRef.current) return;

    try {
      const container = canvasContainerRef.current;
      while (container.firstChild) {
        container.removeChild(container.firstChild);
      }

      const loadingTask = pdfjs.getDocument({data: pdfData});
      const pdf = await loadingTask.promise;
      setNumPages(pdf.numPages);

      const page = await pdf.getPage(pageNumber);

      const viewport = page.getViewport({scale, rotation: pdfRotation});

      const canvas = document.createElement('canvas');
      const context = canvas.getContext('2d');
      canvas.width = viewport.width;
      canvas.height = viewport.height;
      canvas.style.display = 'block';
      canvas.style.margin = '0 auto';
      canvas.style.boxShadow = '0 4px 8px rgba(0, 0, 0, 0.3)';
      canvas.style.backgroundColor = 'white';
      canvas.style.borderRadius = '4px';

      container.appendChild(canvas);

      const renderContext = {
        canvasContext: context,
        viewport: viewport,
      };

      await page.render(renderContext).promise;
      setLoading(false);
    } catch (err) {
      console.error('Error in fallback PDF rendering:', err);
      setError('خطا در رندر PDF. لطفا مجددا تلاش کنید یا فایل را دانلود کنید.');
      setLoading(false);
    }
  }, [pdfData, pageNumber, scale, pdfRotation]);

  useEffect(() => {
    if (pdfFallbackMode && pdfData) {
      setLoading(true);
      renderPdfFallback();
    }
  }, [pdfFallbackMode, pdfData, pageNumber, scale, pdfRotation, renderPdfFallback]);

  useEffect(() => {
    if (pdfFallbackMode && !pdfData) {
      fetchPdfForFallback();
    }
  }, [pdfFallbackMode, pdfData, fetchPdfForFallback]);

  const renderPdfPreview = () => {
    if (!accessToken) return null;

    if (pdfFallbackMode) {
      return (
        <div className={`${styles.pdfFallbackPreview} ${!fullscreen ? styles.notFullscreen : ''}`}>
          {loading && (
            <div className="text-center p-5">
              <Spinner animation="border" />
              <p className="text-white mt-3">در حال بارگذاری PDF...</p>
            </div>
          )}

          {error && (
            <div className="text-center p-5">
              <p className="text-danger">{error}</p>
              <SecureFileDownload fileId={file.fileId} fileName={file.fileName} buttonText="دانلود فایل" variant="primary" />
            </div>
          )}

          <div
            ref={canvasContainerRef}
            style={{
              width: '100%',
              padding: '1rem',
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
            }}
          />
        </div>
      );
    }

    return (
      <div className={`${styles.pdfPreview} ${!fullscreen ? styles.notFullscreen : ''}`}>
        <Document
          file={pdfBlob}
          onLoadSuccess={({numPages}) => setNumPages(numPages)}
          onLoadError={(error) => {
            console.error('Error loading PDF:', error);
            setPdfFallbackMode(true);
          }}
          loading={
            <div className="text-center p-5">
              <Spinner animation="border" />
              <p className="text-white mt-3">در حال بارگذاری PDF...</p>
            </div>
          }
          error={
            <div className="text-center p-5">
              <p className="text-danger">خطا در بارگذاری فایل PDF. در حال تلاش با روش جایگزین...</p>
              <Spinner animation="border" />
            </div>
          }
        >
          {numPages && (
            <Page
              pageNumber={pageNumber}
              scale={scale}
              rotate={pdfRotation}
              renderAnnotationLayer={true}
              renderTextLayer={true}
              loading={
                <div className="text-center p-4">
                  <Spinner animation="border" size="sm" />
                </div>
              }
              className="mb-4"
              style={{
                backgroundColor: 'white',
                boxShadow: '0 4px 8px rgba(0, 0, 0, 0.3)',
                margin: '1rem 0',
                borderRadius: '4px',
              }}
            />
          )}
        </Document>
      </div>
    );
  };

  const renderImagePreview = () => {
    return (
      <div
        ref={containerRef}
        className={`${styles.imagePreview} ${!fullscreen ? styles.notFullscreen : ''} ${isDragging ? styles.dragging : ''}`}
        onMouseDown={handleMouseDown}
        onTouchStart={handleTouchStart}
      >
        <canvas
          ref={canvasRef}
          style={{
            maxWidth: '100%',
            maxHeight: '100%',
            display: 'block',
          }}
        />
      </div>
    );
  };

  const renderPreviewContent = () => {
    const fileType = getFileType();

    if (loading) {
      return (
        <div className="text-center p-5">
          <Spinner animation="border" />
          <p className="mt-3">در حال بارگذاری پیش‌نمایش...</p>
        </div>
      );
    }

    if (error) {
      return (
        <div className="text-center p-5 text-danger">
          <p>{error}</p>
          <Button variant="outline-primary" onClick={onClose}>
            بستن
          </Button>
        </div>
      );
    }

    switch (fileType) {
      case 'image':
        return renderImagePreview();
      case 'pdf':
        return renderPdfPreview();
      case 'text':
        return (
          <div className="text-center p-5">
            <p className="mb-4">متن‌های ساده را می‌توانید پس از دانلود مشاهده کنید.</p>
            <SecureFileDownload fileId={file.fileId} fileName={file.fileName} buttonText="دانلود برای مشاهده" variant="primary" size="lg" />
          </div>
        );
      default:
        return (
          <div className="text-center p-5">
            <p>پیش‌نمایش برای این نوع فایل در دسترس نیست.</p>
            <SecureFileDownload fileId={file.fileId} fileName={file.fileName} buttonText="دانلود فایل" variant="primary" size="lg" />
          </div>
        );
    }
  };

  const renderControls = () => {
    const fileType = getFileType();
    if (fileType !== 'image' && fileType !== 'pdf') return null;

    return (
      <div className={styles.controls}>
        <Button variant="link" className={styles.controlButton} onClick={() => handleZoom(-0.1)} title="کوچک‌نمایی">
          <FontAwesomeIcon icon={faSearchMinus} />
        </Button>

        <Button variant="link" className={styles.controlButton} onClick={() => handleZoom(0.1)} title="بزرگ‌نمایی">
          <FontAwesomeIcon icon={faSearchPlus} />
        </Button>

        <Button variant="link" className={styles.controlButton} onClick={resetView} title="بازنشانی نما">
          <FontAwesomeIcon icon={faRedo} />
        </Button>

        {fileType === 'pdf' && (
          <>
            {numPages > 1 && (
              <>
                <Button variant="link" className={styles.controlButton} onClick={() => setPageNumber((prev) => Math.max(1, prev - 1))} disabled={pageNumber <= 1}>
                  <FontAwesomeIcon icon={faArrowRight} />
                </Button>

                <span className={styles.pageInfo}>
                  {pageNumber} / {numPages}
                </span>

                <Button variant="link" className={styles.controlButton} onClick={() => setPageNumber((prev) => Math.min(numPages, prev + 1))} disabled={pageNumber >= numPages}>
                  <FontAwesomeIcon icon={faArrowLeft} />
                </Button>
              </>
            )}

            <Button variant="link" className={styles.controlButton} onClick={() => rotatePdf(-90)} title="چرخش به چپ">
              <FontAwesomeIcon icon={faUndo} />
            </Button>

            <Button variant="link" className={styles.controlButton} onClick={() => rotatePdf(90)} title="چرخش به راست">
              <FontAwesomeIcon icon={faRedo} />
            </Button>
          </>
        )}

        <Button variant="link" className={styles.controlButton} onClick={() => setFullscreen(!fullscreen)} title={fullscreen ? 'خروج از تمام صفحه' : 'نمایش تمام صفحه'}>
          <FontAwesomeIcon icon={fullscreen ? faCompress : faExpand} />
        </Button>
      </div>
    );
  };

  return (
    <Modal
      show={true}
      onHide={onClose}
      size="xl"
      fullscreen // همیشه fullscreen
      className={styles.fullscreenModal}
      backdrop="static"
      centered
    >
      <Modal.Header className="py-2">
        <Modal.Title dir="rtl">{file.fileName}</Modal.Title>
        <div className="ms-auto d-flex gap-2">
          <SecureFileDownload fileId={file.fileId} fileName={file.fileName} buttonText="دانلود" variant="outline-primary" size="sm" />
          <Button variant="outline-danger" size="sm" onClick={onClose}>
            <FontAwesomeIcon icon={faTimes} />
          </Button>
        </div>
      </Modal.Header>

      <Modal.Body className="p-0 position-relative">
        <div className={styles.previewContainer}>
          {renderPreviewContent()}
          {renderControls()}
        </div>
      </Modal.Body>
    </Modal>
  );
};

export default FilePreview;
