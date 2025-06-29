import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { Container, Row, Col, Spinner } from 'react-bootstrap';

export default function Logout() {
  const { logout } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    const performLogout = async () => {
      await logout();
      navigate('/Home/login');
    };

    performLogout();
  }, [logout, navigate]);

  return (
    <Container fluid className="min-vh-100 d-flex align-items-center justify-content-center bg-light">
      <Row>
        <Col className="text-center">
          <Spinner animation="border" variant="primary" />
          <p className="mt-3">در حال خروج از حساب کاربری...</p>
        </Col>
      </Row>
    </Container>
  );
}
