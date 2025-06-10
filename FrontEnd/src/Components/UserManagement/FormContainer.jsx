import React, {useState, useEffect} from 'react';
import {Container, Form} from 'react-bootstrap';
import {toast} from 'react-toastify';
import {useLocation, useNavigate} from 'react-router-dom';
import FormInputs from './FormInputs';
import FormActions from './FormActions';
import {INITIAL_FORM_VALUES} from './constants';
import {styles} from './styles';
import axios from '../../api/axios';

const FormContainer = ({clientId, prevPath}) => {
  const location = useLocation();
  const navigate = useNavigate();
  const [formData, setFormData] = useState(INITIAL_FORM_VALUES);
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);

  // اگر prevPath وجود داشت، از آن استفاده کن، وگرنه مسیر پیش‌فرض
  const from = prevPath || location.state?.from || '/Clients';

  useEffect(() => {
    if (clientId) {
      const fetchClientData = async () => {
        try {
          const response = await axios.get(`/api/ClientEndpoints/Clients/${clientId}`);
          const clientData = response.data;
          setInitialFormData(clientData);
        } catch (error) {
          console.error('Error fetching client data:', error);
          toast.error('خطا در دریافت اطلاعات کاربر');
        }
      };
      fetchClientData();
    }
  }, [clientId]);

  const setInitialFormData = (clientData) => {
    setFormData({
      firstName: clientData.firstName || '',
      lastName: clientData.lastName || '',
      nationalCode: clientData.nationalCode || '',
      phoneNumber: clientData.phoneNumber || '',
      userRole: clientData.roles || [],
    });
  };

  const validateNationalCode = (code) => /^\d{10}$/.test(code);
  const validatePhoneNumber = (phone) => /^09\d{9}$/.test(phone);

  const validateForm = () => {
    const newErrors = {};

    if (!formData.firstName?.trim()) {
      newErrors.firstName = 'لطفا نام را وارد نمایید';
    }

    if (!formData.lastName?.trim()) {
      newErrors.lastName = 'لطفا نام خانوادگی را وارد نمایید';
    }

    if (!validateNationalCode(formData.nationalCode) && formData.nationalCode?.trim()) {
      newErrors.nationalCode = 'کد ملی باید 10 رقم باشد';
    }

    if (!validatePhoneNumber(formData.phoneNumber) && formData.phoneNumber?.trim()) {
      newErrors.phoneNumber = 'شماره تلفن معتبر وارد نمایید';
    }

    if (!Array.isArray(formData.userRole) || formData.userRole.length === 0) {
      newErrors.userRole = 'لطفا حداقل یک نقش را انتخاب کنید';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (validateForm()) {
      setLoading(true);
      const toastId = toast.loading('در حال ثبت اطلاعات...', {
        position: 'bottom-right',
        rtl: true,
      });

      try {
        const userData = {
          FirstName: formData.firstName,
          LastName: formData.lastName,
          PhoneNumber: formData.phoneNumber,
          NationalCode: formData.nationalCode,
          Roles: formData.userRole,
          ...(clientId && {UserId: clientId}),
        };

        if (clientId) {
          // Update existing user
          await axios.put(`/api/AuthEndpoints/updateProfile/${clientId}`, userData);
          toast.update(toastId, {
            render: 'اطلاعات کاربر با موفقیت بروزرسانی شد',
            type: 'success',
            isLoading: false,
            autoClose: 3000,
            closeButton: true,
          });
        } else {
          // Create new user
          await axios.post('/api/AuthEndpoints/register', userData);
          toast.update(toastId, {
            render: 'ثبت نام با موفقیت انجام شد',
            type: 'success',
            isLoading: false,
            autoClose: 3000,
            closeButton: true,
          });
        }

        // Navigate back to previous page after success
        navigate(from);
      } catch (error) {
        console.error('Operation failed:', error);
        toast.update(toastId, {
          render: typeof error.response?.data === 'string' ? error.response.data : error.response?.data?.message || 'خطا در ثبت اطلاعات',
          type: 'error',
          isLoading: false,
          autoClose: 3000,
          closeButton: true,
        });
        setErrors((prev) => ({
          ...prev,
          submit: typeof error.response?.data === 'string' ? error.response.data : error.response?.data?.message || 'خطا در ثبت اطلاعات',
        }));
      } finally {
        setLoading(false);
      }
    }
  };

  const handleReset = () => {
    if (clientId) {
      navigate(from);
    } else {
      setFormData(INITIAL_FORM_VALUES);
      setErrors({});
    }
  };
  const handleInputChange = (field, value) => {
    setFormData((prev) => ({...prev, [field]: value}));

    if (value === '') {
      setFormData((prev) => ({...prev, [field]: value}));
      return;
    }
    // پاک کردن خطای مربوط به این فیلد وقتی کاربر شروع به تایپ می‌کند
    if (errors[field]) {
      setErrors((prev) => ({...prev, [field]: ''}));
    }

    if (field === 'nationalCode' || field === 'phoneNumber') {
      value = value.replace(/\D/g, ''); // حذف همه کاراکترهای غیر عددی
    }

    if (field === 'firstName' || field === 'lastName') {
      value = value.replace(/[^آ-یa-zA-Z\s]/g, ''); // فقط حروف فارسی، انگلیسی و فاصله
    }
  };
  return (
    <Form onSubmit={handleSubmit}>
      <FormInputs formData={formData} setFormData={setFormData} errors={errors} handleInputChange={handleInputChange} />
      <FormActions onReset={handleReset} loading={loading} submitError={errors.submit} />
    </Form>
  );
};

export default FormContainer;
