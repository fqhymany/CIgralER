// FrontEnd/src/hooks/useMobileNavigation.js
import {useEffect} from 'react';
import {useNavigate, useLocation} from 'react-router-dom';

export const useMobileNavigation = (onBackPress) => {
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    // Handle Android back button
    const handlePopState = (event) => {
      if (window.innerWidth <= 768) {
        // Mobile only
        event.preventDefault();

        // If custom handler provided, use it
        if (onBackPress) {
          onBackPress();
        } else {
          // Default behavior - go to chat list
          if (location.pathname.includes('/chat/')) {
            navigate('/chats');
          } else {
            navigate(-1);
          }
        }
      }
    };

    // Push a new state when component mounts
    window.history.pushState({page: 'chat'}, '');

    window.addEventListener('popstate', handlePopState);

    return () => {
      window.removeEventListener('popstate', handlePopState);
    };
  }, [navigate, location, onBackPress]);
};
