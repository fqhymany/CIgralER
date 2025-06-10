import React, {useState, useEffect} from 'react';
import {Form, Row, Col} from 'react-bootstrap';
import {SelectBoxWithSearch} from '../../components/Search/SelectBoxWithSearch';
import {translateRole} from './roleUtils';
import {fetchAndTranslateRoles} from '../../components/UserManagement/roleService';

const FormInputs = ({formData, errors, handleInputChange}) => {
  const [roles, setRoles] = useState([]);
  const [initialSelectedRoles, setInitialSelectedRoles] = useState([]);
  const [showRoleValue, setShowRoleValue] = useState('');

  useEffect(() => {
    const fetchRoles = async () => {
      try {
        const formattedRoles = await fetchAndTranslateRoles();
        setRoles(formattedRoles);
        if (formData.userRole && formData.userRole.length > 0) {
          const selectedRoles = formData.userRole.map((role) => ({
            id: role,
            label: translateRole(role),
          }));
          setInitialSelectedRoles(selectedRoles);
          setShowRoleValue('True');
        }
      } catch (error) {
        console.error('Error fetching roles:', error);
        setRoles([]);
      }
    };
    fetchRoles();
  }, [formData.userRole]);

  return (
    <Row className="my-sm-3 my-2">
      <Form.Group as={Col} md={6} xs={12} className="mb-3">
        <Form.Floating className="text-start floating-input-container">
          <Form.Control
            type="text"
            placeholder="نام"
            value={formData.firstName}
            onChange={(e) => handleInputChange('firstName', e.target.value)}
            onKeyDown={(e) => {
              if (
                e.key === 'Backspace' ||
                e.key === 'Delete' ||
                e.key === 'ArrowLeft' ||
                e.key === 'ArrowRight' ||
                e.key === 'Home' ||
                e.key === 'End' ||
                e.key === 'Tab' ||
                (e.ctrlKey && ['a', 'c', 'v', 'z', 'y', 'x'].includes(e.key.toLowerCase()))
              ) {
                return; // اجازه بده کلیدهای کنترلی کار کنند
              }
              const regex = /^[آ-یa-zA-Z\s]$/;
              if (!regex.test(e.key)) {
                e.preventDefault();
              }
            }}
            onPaste={(e) => {
              e.preventDefault();
              const pastedText = e.clipboardData.getData('text');
              const sanitizedText = pastedText.replace(/[^آ-یa-zA-Z\s]/g, '');
              handleInputChange('firstName', formData.firstName + sanitizedText);
            }}
            isInvalid={!!errors.firstName}
          />
          <Form.Label>نام</Form.Label>
          <Form.Control.Feedback type="invalid">{errors.firstName}</Form.Control.Feedback>
        </Form.Floating>
      </Form.Group>

      <Form.Group as={Col} md={6} xs={12} className="mb-3">
        <Form.Floating className="text-start floating-input-container">
          <Form.Control
            type="text"
            placeholder="نام خانوادگی"
            value={formData.lastName}
            onChange={(e) => handleInputChange('lastName', e.target.value)}
            onKeyDown={(e) => {
              if (
                e.key === 'Backspace' ||
                e.key === 'Delete' ||
                e.key === 'ArrowLeft' ||
                e.key === 'ArrowRight' ||
                e.key === 'Home' ||
                e.key === 'End' ||
                e.key === 'Tab' ||
                (e.ctrlKey && ['a', 'c', 'v', 'z', 'y', 'x'].includes(e.key.toLowerCase()))
              ) {
                return;
              }
              const regex = /^[آ-یa-zA-Z\s]$/;
              if (!regex.test(e.key)) {
                e.preventDefault();
              }
            }}
            onPaste={(e) => {
              // پردازش متن کپی شده
              e.preventDefault();
              const pastedText = e.clipboardData.getData('text');
              const sanitizedText = pastedText.replace(/[^آ-یa-zA-Z\s]/g, '');
              handleInputChange('lastName', formData.lastName + sanitizedText);
            }}
            isInvalid={!!errors.lastName}
          />
          <Form.Label>نام خانوادگی</Form.Label>
          <Form.Control.Feedback type="invalid">{errors.lastName}</Form.Control.Feedback>
        </Form.Floating>
      </Form.Group>

      <Form.Group as={Col} md={6} xs={12} className="mb-3">
        <Form.Floating className="text-start floating-input-container">
          <Form.Control
            type="text"
            placeholder="کد ملی"
            value={formData.nationalCode}
            onChange={(e) => handleInputChange('nationalCode', e.target.value)}
            onKeyDown={(e) => {
              if (
                e.key === 'Backspace' ||
                e.key === 'Delete' ||
                e.key === 'ArrowLeft' ||
                e.key === 'ArrowRight' ||
                e.key === 'Home' ||
                e.key === 'End' ||
                e.key === 'Tab' ||
                (e.ctrlKey && ['a', 'c', 'v', 'z', 'y', 'x'].includes(e.key.toLowerCase()))
              ) {
                return;
              }
              const regex = /^[0-9]$/;
              if (!regex.test(e.key)) {
                e.preventDefault();
              }
            }}
            onPaste={(e) => {
              // پردازش متن کپی شده
              e.preventDefault();
              const pastedText = e.clipboardData.getData('text');
              const sanitizedText = pastedText.replace(/\D/g, '');
              handleInputChange('nationalCode', formData.nationalCode + sanitizedText);
            }}
            isInvalid={!!errors.nationalCode}
            maxLength={10}
          />
          <Form.Label>کد ملی</Form.Label>
          <Form.Control.Feedback type="invalid">{errors.nationalCode}</Form.Control.Feedback>
        </Form.Floating>
      </Form.Group>

      <Form.Group as={Col} md={6} xs={12} className="mb-3">
        <Form.Floating className="text-start floating-input-container">
          <Form.Control
            type="tel"
            placeholder="شماره تلفن"
            value={formData.phoneNumber}
            onChange={(e) => handleInputChange('phoneNumber', e.target.value)}
            onKeyDown={(e) => {
              if (
                e.key === 'Backspace' ||
                e.key === 'Delete' ||
                e.key === 'ArrowLeft' ||
                e.key === 'ArrowRight' ||
                e.key === 'Home' ||
                e.key === 'End' ||
                e.key === 'Tab' ||
                (e.ctrlKey && ['a', 'c', 'v', 'z', 'y', 'x'].includes(e.key.toLowerCase()))
              ) {
                return;
              }
              const regex = /^[0-9]$/;
              if (!regex.test(e.key)) {
                e.preventDefault();
              }
            }}
            onPaste={(e) => {
              // پردازش متن کپی شده
              e.preventDefault();
              const pastedText = e.clipboardData.getData('text');
              const sanitizedText = pastedText.replace(/\D/g, '');
              handleInputChange('phoneNumber', formData.phoneNumber + sanitizedText);
            }}
            isInvalid={!!errors.phoneNumber}
            maxLength={11}
          />
          <Form.Label>شماره تلفن</Form.Label>
          <Form.Control.Feedback type="invalid">{errors.phoneNumber}</Form.Control.Feedback>
        </Form.Floating>
      </Form.Group>

      <Form.Group as={Col} md={6} xs={12} className="mb-3">
        <SelectBoxWithSearch
          key="roles-select"
          options={roles}
          initialSelectedOptions={initialSelectedRoles}
          onSelectionChange={(selectedIds) => {
            handleInputChange('userRole', selectedIds);
            setShowRoleValue('True');
          }}
          theme="floatingLabel"
          baseValue="نقش کاربر"
          showValue={showRoleValue}
          isInvalid={!!errors.userRole}
        />
        {errors.userRole && <div className="invalid-feedback d-block">{errors.userRole}</div>}
      </Form.Group>
    </Row>
  );
};

export default FormInputs;
