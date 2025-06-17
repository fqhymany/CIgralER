// فایل: FrontEnd/src/hooks/useMobileNavigation.js
// محتوای کامل فایل را با این کد جایگزین کنید:

import {useEffect, useRef} from 'react';
import {useNavigate, useLocation} from 'react-router-dom';

export const useMobileNavigation = (onBackPress) => {
  const navigate = useNavigate();
  const location = useLocation();
  const isHandlingRef = useRef(false);
  const timeoutRef = useRef(null);

  useEffect(() => {
    // Only handle on mobile
    if (window.innerWidth > 768) return;

    const handlePopState = (event) => {
      // Prevent multiple rapid calls
      if (isHandlingRef.current) return;

      isHandlingRef.current = true;

      // Clear any existing timeout
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }

      // Reset the handling flag after a short delay
      timeoutRef.current = setTimeout(() => {
        isHandlingRef.current = false;
      }, 300);

      try {
        if (onBackPress) {
          onBackPress();
        } else {
          // Default behavior
          if (location.pathname.includes('/chat/')) {
            navigate('/chats', {replace: true});
          }
        }
      } catch (error) {
        console.warn('Navigation error:', error);
      }
    };

    // Add event listener
    window.addEventListener('popstate', handlePopState);

    return () => {
      window.removeEventListener('popstate', handlePopState);
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, [navigate, location.pathname, onBackPress]);
};
