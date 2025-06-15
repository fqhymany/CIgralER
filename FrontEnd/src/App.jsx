// App.jsx

import React, {Suspense} from 'react';
import {Route, Routes} from 'react-router-dom';
import AppRoutes from './AppRoutes';
import {Layout} from './layouts/Layout';
import {LoginLayout} from './layouts/LoginLayout';
import {useAuth} from './contexts/AuthContext';
import Login from './Pages/Login';
import {ForgotPassword} from './Pages/ForgotPassword';
import {ToastContainer} from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import './assets/custom.css';
import './assets/Font.css';
import './assets/styles/CustomForms.css';
import LiveChatWidget from './components/Chat/LiveChatWidget';
import AgentDashboard from './components/Chat/AgentDashboard';
import ChatRoom from './components/Chat/Chat';
import ChatList from './components/Chat/ChatRoomList';
import ProtectedRoute from './components/ProtectedRoute';
import { isAgent } from './utils/isAgent';

const renderRoutes = (routes) =>
  routes.map((route, index) => {
    const {children, element, requireAuth = true, layoutType = 'default', ...rest} = route;

    let wrappedElement;

    switch (layoutType) {
      case 'admin':
        wrappedElement = element;
        break;
      case 'none': // No layout wrapper
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
  const { isLoading, user } = useAuth();

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center min-vh-100">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  // Check if the current user is an agent
  const showLiveChatWidget = !isAgent(user);

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

          {/* Chat routes */}
          <Route
            path="/chats"
            element={
              <ProtectedRoute>
                <ChatList />
              </ProtectedRoute>
            }
          />
          <Route
            path="/chat/:roomId"
            element={
              <ProtectedRoute>
                <ChatRoom />
              </ProtectedRoute>
            }
          />
          <Route
            path="/agent"
            element={
              <ProtectedRoute requiredRole="Agent">
                <AgentDashboard />
              </ProtectedRoute>
            }
          />

          {renderRoutes(AppRoutes)}
        </Routes>
        {showLiveChatWidget && <LiveChatWidget />}
        <ToastContainer rtl={true} />
      </Suspense>
    </>
  );
}
