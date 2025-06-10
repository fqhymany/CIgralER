import React from 'react';
import { Form, InputGroup } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { styles } from './styles';

const FormField = ({ 
  label, 
  id, 
  type = 'text', 
  placeholder, 
  value, 
  onChange, 
  error, 
  icon,
  maxLength,
  required = true,
  inputProps = {}
}) => {
  return (
    <Form.Group className="mb-4">
      <Form.Label htmlFor={id}>{label}</Form.Label>
      <InputGroup>
        <Form.Control
          type={type}
          id={id}
          name={id}
          value={value}
          onChange={onChange}
          placeholder={placeholder}
          maxLength={maxLength}
          required={required}
          style={{
            ...styles.inputFocusEffect,
            ...(error ? styles.inputError : {})
          }}
          {...inputProps}
        />
        <InputGroup.Text style={{ backgroundColor: '#f8f9fa', border: 'none' }}>
          <FontAwesomeIcon icon={icon} style={{ color: '#9ca3af' }} />
        </InputGroup.Text>
      </InputGroup>
      {error && <div style={styles.errorMessage}>{error}</div>}
    </Form.Group>
  );
};

export default FormField;