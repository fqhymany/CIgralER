// App.jsx

import React, {Suspense} from 'react';
import {Route, Routes, Navigate} from 'react-router-dom';
import AppRoutes from './AppRoutes';
import {Layout} from './layouts/Layout'; // Import main layout
import {LoginLayout} from './layouts/LoginLayout'; // Import login layout
import {useAuth} from './contexts/AuthContext';
import Login from './Pages/Login';
import {ForgotPassword} from './Pages/ForgotPassword';
import {ToastContainer} from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import './assets/custom.css';
import './assets/Font.css';
import './assets/styles/CustomForms.css';

// Protected Route component
const ProtectedRoute = ({children}) => {
  const {user, isLoading} = useAuth();

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
    // Save the attempted URL for redirecting after login
    return <Navigate to="/login" replace />;
  }

  return children;
};
const renderRoutes = (routes) =>
  routes.map((route, index) => {
    const {children, element, requireAuth = true, layoutType = 'default', ...rest} = route;

    let wrappedElement;

    // if (layoutType === 'admin') {
    //   wrappedElement = element;
    // } else {
    //   wrappedElement = <Layout>{element}</Layout>;
    // }

    switch (layoutType) {
      case 'admin':
        wrappedElement = element;
        break;
      case 'none': // حالت بدون Layout
        wrappedElement = element;
        break;
      default:
        wrappedElement = <Layout>{element}</Layout>;
    }

    return (
      <Route key={index} {...rest} element={requireAuth ? <ProtectedRoute>{wrappedElement}</ProtectedRoute> : wrappedElement}>
        {children && renderRoutes(children)}
      </Route>
    );
  });
export default function App() {
  const {isLoading} = useAuth();

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center min-vh-100">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  return (
    <>
      <Suspense
        fallback={
          <div className="d-flex justify-content-center align-items-center min-vh-100">
            <div className="spinner-border text-primary" role="status">
              <span className="visually-hidden">Loading...</span>
            </div>
          </div>
        }
      >
        <Routes>
          {/* Route for login page */}
          <Route
            path="/login"
            element={
              <LoginLayout>
                <Login />
              </LoginLayout>
            }
          />

          <Route
            path="/ForgotPassword"
            element={
              <LoginLayout>
                <ForgotPassword />
              </LoginLayout>
            }
          />

          {/* <Route path="/" element={<Navigate to="/" replace />} /> */}

          {renderRoutes(AppRoutes)}
        </Routes>
        <ToastContainer rtl={true} />
      </Suspense>
    </>
  );
}
