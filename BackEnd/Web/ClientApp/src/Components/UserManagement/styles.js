export const styles = {
  container: {
    background: '#f8f9fa',
    minHeight: '100vh',
  },
  formContainer: {
    backgroundColor: '#fff',
    borderRadius: '12px',
    boxShadow: '0 10px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)',
    padding: '24px',
  },
  buttonPrimary: {
    background: 'linear-gradient(135deg, #3b82f6, #2563eb)',
    border: 'none',
    transition: 'all 0.3s ease',
  },
  buttonSecondary: {
    background: 'linear-gradient(135deg, #f3f4f6, #e5e7eb)',
    border: 'none',
    color: '#374151',
    transition: 'all 0.3s ease',
  },
  inputFocusEffect: {
    transition: 'all 0.2s ease',
    borderRadius: '8px',
  },
  inputError: {
    borderColor: '#ef4444',
  },
  errorMessage: {
    color: '#ef4444',
    fontSize: '0.8rem',
    marginTop: '0.25rem',
  },
  successModalIcon: {
    width: '64px',
    height: '64px',
    backgroundColor: '#e0f2f1',
    borderRadius: '50%',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: '16px',
  },
};
