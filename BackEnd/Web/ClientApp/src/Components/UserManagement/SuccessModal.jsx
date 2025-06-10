import React from 'react';
import { Modal, Button } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faCheck } from '@fortawesome/free-solid-svg-icons';
import { styles } from './styles';

const SuccessModal = ({ show, onClose }) => {
  return (
    <Modal 
      show={show} 
      onHide={onClose} 
      centered
      className="text-center"
    >
      <Modal.Body className="d-flex flex-column align-items-center py-5">
        <div style={styles.successModalIcon}>
          <FontAwesomeIcon icon={faCheck} className="text-success" size="2x" />
        </div>
        <h3 className="h5 mb-2">عملیات موفق</h3>
        <p className="text-muted mb-4">کاربر جدید با موفقیت در سیستم ثبت شد</p>
        <Button 
          variant="primary" 
          onClick={onClose}
          style={styles.buttonPrimary}
          className="px-4"
        >
          تایید
        </Button>
      </Modal.Body>
    </Modal>
  );
};

export default SuccessModal;