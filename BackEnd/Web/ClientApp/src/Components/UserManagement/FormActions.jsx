import React from 'react';
import {Button} from 'react-bootstrap';

const FormActions = ({onReset, loading, submitError}) => {
  return (
    <div className="py-3">
      {submitError && (
        <div className="alert alert-danger mb-3" role="alert">
          {submitError}
        </div>
      )}
      <Button type="submit" variant="primary" className="mx-auto d-block fs-7" disabled={loading}>
        {loading ? 'در حال ارسال...' : 'ثبت اطلاعات'}
      </Button>
    </div>
  );
};

export default FormActions;
