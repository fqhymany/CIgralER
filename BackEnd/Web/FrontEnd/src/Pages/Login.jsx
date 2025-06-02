import React, {useState} from 'react';
import {useNavigate} from 'react-router-dom';
import {useAuth} from '../contexts/AuthContext';
import {Container, Row, Col, Form, Button, Alert} from 'react-bootstrap';
import '../assets/styles/LoginStyle.css';
import {RiUserLine} from 'react-icons/ri';
import {CiLock} from 'react-icons/ci';
import {RiEyeLine, RiEyeOffLine} from 'react-icons/ri';
import Logo from '../assets/img/Dadvik_logoV.svg';

export default function Login() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [availableRegions, setAvailableRegions] = useState(null);
  const [showPassword, setShowPassword] = useState(false);
  const navigate = useNavigate();
  const {login, selectRegion} = useAuth();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const result = await login({username, password});
      if (result.requiresRegionSelection) {
        setAvailableRegions(result.availableRegions);
      } else {
        navigate('/');
      }
    } catch (err) {
      setError(err.message || 'خطا در ورود به سیستم');
    } finally {
      setLoading(false);
    }
  };

  const handleRegionSelect = async (regionId) => {
    try {
      await selectRegion(regionId);
      // اگر تغییر مسیر صورت نگرفت، به صفحه خانه هدایت کن
      navigate('/');
    } catch (err) {
      setError(err.message || 'خطا در انتخاب ناحیه');
    }
  };

  if (availableRegions) {
    return (
      <Container fluid className="min-vh-100 d-flex align-items-center justify-content-center bg-light">
        <Row className="w-100 justify-content-center">
          <Col xs={12} sm={8} md={6} lg={4}>
            <div className="logoImg text-center mb-5">
              <img src={Logo} alt="لوگو" className="w-75" />
            </div>
            <div className="text-center mb-2">
              <h5 className="fw-bold">انتخاب ناحیه</h5>
            </div>
            <div className="p-4 border rounded bg-white shadow-sm">
              {error && <Alert variant="danger">{error}</Alert>}
              {availableRegions.map((region) => (
                <Button key={region.id} onClick={() => handleRegionSelect(region.id)} className="w-100 mb-2 custom-region-btn">
                  {region.name}
                </Button>
              ))}
            </div>
          </Col>
        </Row>
      </Container>
    );
  }

  return (
    <Container fluid className="min-vh-100 d-flex align-items-center justify-content-center bg-light">
      <Row className="w-100 justify-content-center">
        <Col xs={12} sm={8} md={6} lg={4}>
          <div className="logoImgDiv text-center mb-5">
            <img src={Logo} alt="لوگو" className="w-75" />
          </div>
          <div className="text-center mb-2">
            <h3 className="fw-bold login-header">ورود به حساب کاربری</h3>
          </div>
          <Form onSubmit={handleSubmit} className="p-4 login-form">
            {error && <Alert variant="danger">{error}</Alert>}
            <div className="inputDiv mb-3">
              <RiUserLine size={25} color="#5c647b" />
              <Form.Control type="text" placeholder="نام کاربری" value={username} onChange={(e) => setUsername(e.target.value)} required className="login-input" />
            </div>
            <div className="inputDiv mb-3 position-relative" style={{direction: 'rtl'}}>
              <CiLock size={25} color="#5c647b" />
              <Form.Control type={showPassword ? 'text' : 'password'} placeholder="رمز عبور" value={password} onChange={(e) => setPassword(e.target.value)} required className="login-input pr-5" />
              <span
                onClick={() => setShowPassword((prev) => !prev)}
                style={{position: 'absolute', left: '10px', top: '50%', transform: 'translateY(-50%)', cursor: 'pointer', zIndex: 2, color: '#5c647b'}}
                tabIndex={0}
                aria-label={showPassword ? 'مخفی کردن رمز' : 'نمایش رمز'}
              >
                {showPassword ? <RiEyeOffLine size={22} /> : <RiEyeLine size={22} />}
              </span>
            </div>
            <div className="forgetRememberDiv d-flex justify-content-between align-items-center mb-3">
              <Form.Check type="checkbox" label="مرا به خاطر بسپار" className="remember-me" />
              <Button variant="link" className="p-0 forgot-password" onClick={() => navigate('/ForgotPassword')}>
                فراموشی رمز عبور
              </Button>
            </div>
            <div className="loginBtnDiv mb-3">
              <Button type="submit" className="w-100 submit-btn" disabled={loading}>
                {loading ? 'در حال ورود...' : 'ورود'}
              </Button>
            </div>
          </Form>
        </Col>
      </Row>
    </Container>
  );
}
