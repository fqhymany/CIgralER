import React from 'react';

export const LoginLayout = ({children}) => {
  return (
    <div className="login-layout">
      <div className="login-container">{children}</div>
    </div>
  );
};
