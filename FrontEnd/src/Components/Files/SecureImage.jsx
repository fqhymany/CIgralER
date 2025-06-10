// src/components/SecureImage.jsx
import React, {useState, useEffect, useRef} from 'react';
import {Spinner} from 'react-bootstrap';
import secureFileApi from '../../api/secureFileApi';
import {useAuth} from '../../contexts/AuthContext';

const SecureImage = ({fileId, alt = 'تصویر', className = '', width, height, onError, placeholderSrc, ...props}) => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [imageLoaded, setImageLoaded] = useState(false);
  const [imageData, setImageData] = useState(null);
  const canvasRef = useRef(null);
  const tempImageRef = useRef(null);
  const {refreshSession} = useAuth();

  // جلوگیری از باز شدن منوی راست‌کلیک
  const handleContextMenu = (e) => {
    e.preventDefault();
    return false;
  };

  // تنظیم event listener برای ریفرش جلسه کاربر (همیشه روی canvas موجود)
  useEffect(() => {
    const resetTimer = () => {
      refreshSession();
    };
    const canvas = canvasRef.current;
    if (canvas) {
      canvas.addEventListener('mouseenter', resetTimer);
      canvas.addEventListener('click', resetTimer);
    }
    return () => {
      if (canvas) {
        canvas.removeEventListener('mouseenter', resetTimer);
        canvas.removeEventListener('click', resetTimer);
      }
    };
  }, [refreshSession, imageLoaded]);

  // useEffect مخصوص رسم تصویر روی canvas
  useEffect(() => {
    if (imageData && canvasRef.current) {
      const renderImage = () => {
        try {
          const canvas = canvasRef.current;
          if (!canvas) {
            console.error('Canvas became null during render attempt');
            return false;
          }

          const img = imageData.img;

          const ctx = canvas.getContext('2d');
          if (!ctx) {
            throw new Error('Could not get canvas context');
          }

          // تعیین ابعاد canvas؛ اگر یکی از ابعاد ارائه شده نباشد نسبت ابعاد تنظیم می‌شود
          let canvasWidth = width || img.width;
          let canvasHeight = height || img.height;
          if (width && !height) {
            const aspectRatio = img.width / img.height;
            canvasHeight = width / aspectRatio;
          } else if (height && !width) {
            const aspectRatio = img.width / img.height;
            canvasWidth = height * aspectRatio;
          }

          // تنظیم ابعاد داخلی canvas (attributes) به طوری که محتویات حفظ شود
          canvas.width = canvasWidth;
          canvas.height = canvasHeight;

          ctx.clearRect(0, 0, canvas.width, canvas.height);
          ctx.drawImage(img, 0, 0, canvasWidth, canvasHeight);
          return true;
        } catch (err) {
          console.error('Error drawing image on canvas:', err);
          setError('خطا در رسم تصویر روی canvas');
          setLoading(false);
          if (onError) onError(err);
          return false;
        }
      };

      const success = renderImage();
      if (success) {
        setImageLoaded(true);
        setLoading(false);
        refreshSession();
      } else {
        setTimeout(() => {
          const retrySuccess = renderImage();
          if (retrySuccess) {
            setImageLoaded(true);
            setLoading(false);
            refreshSession();
          }
        }, 100);
      }
    }
  }, [imageData, width, height, refreshSession, onError]);

  // useEffect برای بارگذاری تصویر از سرور
  useEffect(() => {
    if (!fileId) {
      setLoading(false);
      setError('شناسه فایل مشخص نشده است');
      console.error('fileId is not provided.');
      return;
    }

    let isActive = true; // flag برای جلوگیری از بروزرسانی state پس از unmount

    const loadImage = async () => {
      try {
        if (!isActive) return;
        setLoading(true);
        setError(null);
        setImageLoaded(false);

        if (tempImageRef.current) {
          tempImageRef.current.onload = null;
          tempImageRef.current.onerror = null;
          tempImageRef.current = null;
        }

        const result = await secureFileApi.generateSecureAccessUrl(fileId, 10);
        const tokenUrl = `${window.location.origin}/api/files/access/${result.accessToken}`;

        const response = await fetch(tokenUrl);
        if (!response.ok) {
          throw new Error(`خطای دریافت تصویر: ${response.status}`);
        }

        const blobData = await response.blob();

        // تبدیل Blob به یک شیء با MIME type صحیح
        const imageBlob = new Blob([blobData], {type: 'image/jpeg'});

        const blobUrl = URL.createObjectURL(imageBlob);

        const img = new Image();
        img.crossOrigin = 'anonymous';
        tempImageRef.current = img;

        img.onload = () => {
          if (!isActive) return;
          setImageData({
            img: img,
            width: img.width,
            height: img.height,
          });
          URL.revokeObjectURL(blobUrl);
          tempImageRef.current = null;
        };

        img.onerror = (err) => {
          if (!isActive) return;
          console.error('Error loading image on canvas:', err);
          setError('خطا در بارگذاری تصویر');
          setLoading(false);
          URL.revokeObjectURL(blobUrl);
          tempImageRef.current = null;
          if (onError) onError(err);
        };

        img.src = blobUrl;
      } catch (err) {
        if (!isActive) return;
        console.error('Error in loadImage:', err);
        setError(err.message || 'خطا در بارگذاری تصویر');
        setLoading(false);
        if (onError) onError(err);
      }
    };

    loadImage();

    const expiryTimer = setTimeout(() => {
      loadImage();
    }, 9 * 60 * 1000);

    return () => {
      isActive = false;
      clearTimeout(expiryTimer);
      if (tempImageRef.current) {
        tempImageRef.current.onload = null;
        tempImageRef.current.onerror = null;
        tempImageRef.current = null;
      }
    };
  }, [fileId, onError, width, height, refreshSession]);

  // در صورت خطا، نمایش پیام یا تصویر جایگزین
  if (error) {
    console.error('Error state reached:', error);
    return placeholderSrc ? (
      <img src={placeholderSrc} alt={alt} className={`secure-image-error ${className}`} style={{width, height, objectFit: 'cover'}} {...props} />
    ) : (
      <div
        className={`secure-image-error ${className}`}
        style={{
          width: width || '100px',
          height: height || '100px',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          backgroundColor: '#f8f9fa',
          border: '1px solid #dee2e6',
          borderRadius: '4px',
          color: '#dc3545',
          fontSize: '12px',
          textAlign: 'center',
          padding: '8px',
        }}
      >
        خطا در بارگذاری تصویر
      </div>
    );
  }

  // همیشه یک canvas ثابت render می‌کنیم و در صورت loading، overlay اسپینر نمایش می‌دهیم
  return (
    <div
      className={`secure-canvas-wrapper ${className}`}
      style={{
        position: 'relative',
        width: width || '100px',
        height: height || '100px',
      }}
    >
      {loading && (
        <div
          className="secure-image-loading-overlay"
          style={{
            position: 'absolute',
            top: 0,
            left: 0,
            width: '100%',
            height: '100%',
            backgroundColor: 'rgba(248,249,250,0.7)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            zIndex: 2,
          }}
        >
          <Spinner animation="border" size="sm" />
        </div>
      )}
      <canvas
        ref={canvasRef}
        style={{
          width: '100%',
          height: '100%',
          objectFit: 'cover',
          borderRadius: props.style?.borderRadius || '0',
        }}
        onContextMenu={handleContextMenu}
        role="img"
        aria-label={alt}
        tabIndex={props.tabIndex || 0}
      />
    </div>
  );
};

export default SecureImage;
