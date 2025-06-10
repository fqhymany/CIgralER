import React, {useState, useEffect} from 'react';
import {Outlet, useLocation} from 'react-router-dom';
import Sidebar from '../components/AdminPanel/Sidebar';
import Header from '../components/AdminPanel/Header';
import {Container} from 'react-bootstrap';

export const AdminLayout = () => {
  // در ابتدا بسته به اندازه صفحه، حالت اولیه را تنظیم کنید:
  const [sidebarOpen, setSidebarOpen] = useState(window.innerWidth >= 992);
  const [isMobile, setIsMobile] = useState(window.innerWidth < 992);
  const location = useLocation();

  // بستن خودکار سایدبار در حالت موبایل هنگام تغییر مسیر
  useEffect(() => {
    if (isMobile) {
      setSidebarOpen(false);
    }
  }, [location.pathname, isMobile]);

  // تشخیص تغییر سایز صفحه
  useEffect(() => {
    const handleResize = () => {
      const mobile = window.innerWidth < 992;
      setIsMobile(mobile);
      // در موبایل همیشه منو به صورت offcanvas باشد
      if (mobile) {
        setSidebarOpen(false);
      }
    };
    window.addEventListener('resize', handleResize);
    handleResize(); // چک کردن وضعیت اولیه
    return () => {
      window.removeEventListener('resize', handleResize);
    };
  }, []);

  const toggleSidebar = () => {
    setSidebarOpen((prev) => !prev);
  };

  return (
    <div className="d-flex h-100 position-relative overflow-hidden">
      <Sidebar isOpen={sidebarOpen} isMobile={isMobile} onClose={() => setSidebarOpen(false)} />
      <div className="flex-grow-1 d-flex flex-column overflow-hidden">
        <Header onToggleSidebar={toggleSidebar} isMobile={isMobile} sidebarOpen={sidebarOpen} />
        <Container fluid className="flex-grow-1 overflow-auto py-4">
          <Outlet />
        </Container>
      </div>
    </div>
  );
};
