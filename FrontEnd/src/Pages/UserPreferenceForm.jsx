import React, {useState, useEffect} from 'react';
import {Form, Button, Spinner, Alert, Row, Col} from 'react-bootstrap';
import {useQuery, useMutation, useQueryClient} from '@tanstack/react-query';
import {toast} from 'react-toastify';
import api from '../api/axios';

const DISPLAY_MODES = [
  {value: 'Fixed', label: 'ثابت'},
  {value: 'Percent', label: 'درصدی'},
];

const UserPreferenceForm = () => {
  const queryClient = useQueryClient();
  const [selectedMode, setSelectedMode] = useState('');

  const {
    data: prefData,
    isLoading,
    isError,
    error,
  } = useQuery({
    queryKey: ['userPref', 'CaseFinance_DisplayMode'],
    queryFn: async () => {
      const res = await api.get(`/api/Preference/GetUserPreferencesByKey?Key=CaseFinance_DisplayMode`);
      return res.data;
    },
    retry: false,
  });

  useEffect(() => {
    if (prefData) {
      setSelectedMode(prefData);
    }
  }, [prefData]);

  const updateMutation = useMutation({
    mutationFn: async (mode) => {
      return api.post('/api/Preference/UpdateUserPreference', {
        key: 'CaseFinance_DisplayMode',
        value: mode,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ['userPref', 'CaseFinance_DisplayMode']});
      toast.success('تنظیمات با موفقیت به‌روز شد');
    },
    onError: () => {
      toast.error('خطا در به‌روزرسانی تنظیمات');
    },
  });

  const handleSubmit = (e) => {
    e.preventDefault();
    updateMutation.mutate(selectedMode);
  };

  if (isLoading) return <Spinner animation="border" />;
  if (isError) return <Alert variant="danger">{error.message}</Alert>;

  return (
    <Form onSubmit={handleSubmit} className="p-3">
      <Form.Group as={Row} controlId="displayMode">
        <Form.Label column sm={4} className="text-end">
          نحوه نمایش مبلغ:
        </Form.Label>
        <Col sm={8}>
          <Form.Select value={selectedMode} onChange={(e) => setSelectedMode(e.target.value)} size="sm">
            <option value="">انتخاب کنید</option>
            {DISPLAY_MODES.map((opt) => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))}
          </Form.Select>
        </Col>
      </Form.Group>
      <div className="text-center mt-3">
        <Button variant="primary" type="submit" disabled={updateMutation.isPending} size="sm">
          {updateMutation.isPending ? 'در حال ذخیره...' : 'ذخیره'}
        </Button>
      </div>
      {updateMutation.isError && (
        <Alert className="mt-2" variant="danger">
          خطا در به‌روزرسانی تنظیمات
        </Alert>
      )}
    </Form>
  );
};

export default UserPreferenceForm;
