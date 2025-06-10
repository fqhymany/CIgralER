// src/components/SecureFileDownload.jsx
import React, {useState} from 'react';
import {Button, Spinner} from 'react-bootstrap';
import {FontAwesomeIcon} from '@fortawesome/react-fontawesome';
import {faDownload, faLink, faExclamationTriangle} from '@fortawesome/free-solid-svg-icons';
import secureFileApi from '../../api/secureFileApi';
import {useAuth} from '../../contexts/AuthContext';

const SecureFileDownload = ({fileId, fileName, buttonText, variant = 'primary', size = 'sm', className = '', useSecureLink = false, showSecureLinkOption = false, onSuccess, onError, buttonProps = {}}) => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [showLinkOptions, setShowLinkOptions] = useState(false);
  const {refreshSession} = useAuth();

  const handleDownload = async (e) => {
    e.preventDefault();
    refreshSession();

    try {
      setLoading(true);
      setError(null);

      if (useSecureLink) {
        // روش 1: استفاده از توکن موقت
        const result = await secureFileApi.generateSecureAccessUrl(fileId);
        // استفاده از مسیر دقیق API که در بک‌اند تعریف شده است
        const secureUrl = `${window.location.origin}/api/files/access/${result.accessToken}`;

        // باز کردن لینک در تب جدید
        window.open(secureUrl, '_blank');

        if (onSuccess) {
          onSuccess({type: 'secure-link', url: secureUrl});
        }
      } else {
        // روش 2: دانلود مستقیم با استفاده از Blob
        const blob = await secureFileApi.downloadFileAsBlob(fileId);

        // ایجاد یک URL موقت برای دانلود
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', fileName || 'file');
        link.setAttribute('target', '_blank');
        document.body.appendChild(link);
        link.click();

        // پاکسازی
        setTimeout(() => {
          document.body.removeChild(link);
          window.URL.revokeObjectURL(url); // آزادسازی حافظه
        }, 100);

        if (onSuccess) {
          onSuccess({type: 'direct-download', fileName});
        }
      }
    } catch (err) {
      console.error('Error downloading file:', err);
      setError(err.message || 'خطا در دانلود فایل');
      if (onError) onError(err);
    } finally {
      setLoading(false);
    }
  };

  const generateSecureLink = async () => {
    refreshSession();

    try {
      setLoading(true);
      setError(null);

      const result = await secureFileApi.generateSecureAccessUrl(fileId, 60); // 60 دقیقه اعتبار
      const secureUrl = `${window.location.origin}/api/files/access/${result.accessToken}`;

      // کپی لینک به کلیپ‌بورد
      await navigator.clipboard.writeText(secureUrl);

      alert('لینک موقت با موفقیت کپی شد. این لینک تا 60 دقیقه معتبر است.');

      if (onSuccess) {
        onSuccess({type: 'secure-link-copy', url: secureUrl});
      }
    } catch (err) {
      console.error('Error generating secure link:', err);
      setError(err.message || 'خطا در ایجاد لینک امن');
      if (onError) onError(err);
    } finally {
      setLoading(false);
      setShowLinkOptions(false);
    }
  };

  // نمایش خطا
  if (error) {
    return (
      <div className={`secure-file-download ${className}`}>
        <Button variant="outline-danger" size={size} disabled className="d-flex align-items-center">
          <FontAwesomeIcon icon={faExclamationTriangle} className="me-1" />
          <span className="small">خطا در دانلود</span>
        </Button>
        <small className="text-danger d-block mt-1">{error}</small>
      </div>
    );
  }

  // حالت نمایش گزینه‌های لینک امن
  if (showLinkOptions) {
    return (
      <div className={`secure-file-download-options ${className} d-flex flex-column gap-2`}>
        <Button variant="primary" size={size} onClick={handleDownload} disabled={loading} className="d-flex align-items-center justify-content-center" {...buttonProps}>
          <FontAwesomeIcon icon={faDownload} className="me-1" />
          <span>دانلود مستقیم</span>
        </Button>

        <Button variant="secondary" size={size} onClick={generateSecureLink} disabled={loading} className="d-flex align-items-center justify-content-center" {...buttonProps}>
          <FontAwesomeIcon icon={faLink} className="me-1" />
          <span>کپی لینک موقت</span>
        </Button>

        <Button variant="outline-secondary" size="sm" onClick={() => setShowLinkOptions(false)} className="mt-1">
          <small>انصراف</small>
        </Button>
      </div>
    );
  }

  // حالت عادی
  return (
    <div className={`secure-file-download ${className}`}>
      {showSecureLinkOption ? (
        <div className="d-flex gap-2">
          <Button variant={variant} size={size} onClick={handleDownload} disabled={loading} className={className} {...buttonProps}>
            {loading ? (
              <Spinner animation="border" size="sm" />
            ) : (
              <>
                <FontAwesomeIcon icon={faDownload} />
                <span>{buttonText}</span>
              </>
            )}
          </Button>

          <Button variant="outline-secondary" size={size} onClick={() => setShowLinkOptions(true)} disabled={loading} className="px-2" title="گزینه‌های دانلود">
            <FontAwesomeIcon icon={faLink} />
          </Button>
        </div>
      ) : (
        <Button variant={variant} size={size} onClick={handleDownload} title="دانلود فایل" disabled={loading} className={className} {...buttonProps}>
          {loading ? (
            <Spinner animation="border" size="sm" />
          ) : (
            <>
              <FontAwesomeIcon icon={faDownload} />
              <span>{buttonText}</span>
            </>
          )}
        </Button>
      )}
    </div>
  );
};

export default SecureFileDownload;
