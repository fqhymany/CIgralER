import React from 'react';
import {Navigate, useLocation} from 'react-router-dom';
import {useAuth} from '../contexts/AuthContext';

const ProtectedRoute = ({children, requiredRole}) => {
  const {user, isLoading} = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center min-vh-100">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  if (!user) {
    // ذخیره مسیر فعلی برای بازگشت بعد از لاگین
    return <Navigate to={`/login?returnUrl=${encodeURIComponent(location.pathname)}`} replace />;
  }

  if (requiredRole && !(user.roles?.includes(requiredRole) || user.roles?.includes('Admin'))) {
    return <Navigate to="/" replace />;
  }

  return children;
};

export default ProtectedRoute;
