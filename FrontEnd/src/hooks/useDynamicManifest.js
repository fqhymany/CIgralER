// hooks/useDynamicManifest.js
import {useEffect} from 'react';
import {useLocation} from 'react-router-dom';

const useDynamicManifest = () => {
  const location = useLocation();

  const generateManifest = () => {
    // اصلاح URL برای اطمینان از درستی مسیر
    const baseUrl = window.location.origin;
    const relativePath = '/home' + location.pathname + location.search;
    const startUrl = baseUrl + relativePath;

    // تعیین نام بر اساس صفحه
    let pageName = 'دادویک';
    let shortName = 'Dadvik';

    if (location.pathname.includes('/chat')) {
      pageName = 'دادویک - گفتگو';
      shortName = 'Chat';
    } else if (location.pathname.includes('/Cases')) {
      pageName = 'دادویک - پرونده‌ها';
      shortName = 'Cases';
    } else if (location.pathname.includes('/Reports')) {
      pageName = 'دادویک - گزارشات';
      shortName = 'Reports';
    }

    const manifest = {
      short_name: shortName,
      name: pageName,
      start_url: startUrl,
      scope: baseUrl + '/',
      display: 'standalone',
      theme_color: '#0DBA85',
      background_color: '#ffffff',
      orientation: 'portrait-primary',
      icons: [
        {
          src: '/favicon.ico',
          sizes: '64x64 32x32 24x24 16x16',
          type: 'image/x-icon',
        },
        {
          src: '/web-app-manifest-192x192.png',
          sizes: '192x192',
          type: 'image/png',
          purpose: 'maskable',
        },
        {
          src: '/web-app-manifest-512x512.png',
          sizes: '512x512',
          type: 'image/png',
          purpose: 'maskable',
        },
      ],
      shortcuts: [
        {
          name: 'گفتگو',
          short_name: 'Chat',
          description: 'مراجعه مستقیم به بخش گفتگو',
          url: '/Home/chat',
          icons: [
            {
              src: '/web-app-manifest-192x192.png',
              sizes: '192x192',
            },
          ],
        },
        {
          name: 'پرونده‌ها',
          short_name: 'Cases',
          description: 'مدیریت پرونده‌ها',
          url: '/Home/Cases',
          icons: [
            {
              src: '/web-app-manifest-192x192.png',
              sizes: '192x192',
            },
          ],
        },
      ],
    };

    // تولید blob URL برای manifest
    const manifestBlob = new Blob([JSON.stringify(manifest)], {
      type: 'application/manifest+json',
    });
    const manifestURL = URL.createObjectURL(manifestBlob);

    // حذف manifest قبلی
    const existingManifest = document.querySelector('link[rel="manifest"]');
    if (existingManifest) {
      existingManifest.remove();
    }

    // اضافه کردن manifest جدید
    const manifestLink = document.createElement('link');
    manifestLink.rel = 'manifest';
    manifestLink.href = manifestURL;
    document.head.appendChild(manifestLink);

    console.log('Dynamic manifest updated for:', startUrl);
  };

  useEffect(() => {
    // تأخیر کوتاه تا URL کاملاً update شود
    const timeoutId = setTimeout(generateManifest, 100);
    return () => {
      clearTimeout(timeoutId);
      // Clean up any remaining blob URLs
      const manifestLink = document.querySelector('link[rel="manifest"]');
      if (manifestLink && manifestLink.href.startsWith('blob:')) {
        URL.revokeObjectURL(manifestLink.href);
      }
    };
  }, [location.pathname, location.search]);

  return null;
};

export default useDynamicManifest;
