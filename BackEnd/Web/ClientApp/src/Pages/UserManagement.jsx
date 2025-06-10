import React from 'react';
import FormContainer from '../components/UserManagement/FormContainer';
import {Container, Row, Col} from 'react-bootstrap';
import {useSearchParams} from 'react-router-dom';

const UserManagement = () => {
  const [searchParams] = useSearchParams();
  const clientId = searchParams.get('clientId');
  const prevPath = searchParams.get('prevPath');

  return (
    <Container>
      <Row>
        <Col md={12} className="mx-auto">
          <div className="container my-2 mt-4 position-relative rounded-3 p-4 border" style={{padding: '1.25rem'}}>
            <span className="fw-bold fs-6 position-absolute bg-white px-2" style={{top: '-12px', right: '20px'}}>
              {clientId ? 'ویرایش کاربر' : 'ثبت کاربر جدید'}
            </span>
            <FormContainer clientId={clientId} prevPath={prevPath} />
          </div>
        </Col>
      </Row>
    </Container>
  );
};

export default UserManagement;