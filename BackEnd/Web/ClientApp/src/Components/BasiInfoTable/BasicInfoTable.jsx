import React, {useState} from 'react';
import {useQuery, useMutation, useQueryClient} from '@tanstack/react-query';
import {Table, Button, Row, Col, Spinner, Alert} from 'react-bootstrap';
import Form from 'react-bootstrap/Form';
import {IoIosAdd, IoIosCheckmark, IoIosClose} from 'react-icons/io';
import {PiNotePencilLight} from 'react-icons/pi';
import api from '../../api/axios';
import {IoRemoveOutline, IoTrashBinOutline} from 'react-icons/io5';
import FloatingLabel from 'react-bootstrap/FloatingLabel';
import FloatingLabelSelect from '../Form/FloatingLabelSelect';
import {toast} from 'react-toastify';

// Generalized Component for handling data fetching, adding, editing and deleting basicInfo
const BasicInfoTable = ({queryKey, fetchData, addData, updateData, deleteData, title, tHeads, nameField, additionalFields, fieldType, fieldTypeName, customDisplayHead, customDisplay}) => {
  const queryClient = useQueryClient();

  // Fetch data using React Query's useQuery hook
  const {data, error, isLoading} = useQuery({
    queryKey: [queryKey],
    queryFn: fetchData,
  });

  // Initialize form data with required fields and additional fields
  const initialFormData = {
    id: '',
    ...nameField.reduce((acc, field) => ({...acc, [field]: ''}), {}),
    ...additionalFields,
  };

  // State to manage the form data and edit mode
  const [formData, setFormData] = useState(initialFormData);
  const [editMode, setEditMode] = useState(false); // To track if the form is in edit mode
  const [currentEditId, setCurrentEditId] = useState(null); // Track the ID of the item being edited

  // Mutation hooks for add, update, and delete operations
  const addMutation = useMutation({
    mutationFn: addData,
    onSuccess: () => {
      queryClient.invalidateQueries(queryKey); // Invalidate query after successful add
      toast.success(`${title} با موفقیت اضافه شد.`);
    },
    onError: (error) => {
      console.error(`Error adding ${title}:`, error.response?.data);
    },
  });

  const updateMutation = useMutation({
    mutationFn: updateData,
    onSuccess: () => {
      queryClient.invalidateQueries(queryKey); // پس از به‌روزرسانی موفق
      setEditMode(false); // خاموش کردن حالت ویرایش
      setCurrentEditId(null); // پاک کردن ID جاری
      toast.success(`${title} با موفقیت بروزرسانی شد.`);
    },
    onError: (error) => {
      console.error(`Error updating ${title}:`, error.response?.data);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: deleteData,
    onSuccess: () => {
      queryClient.invalidateQueries(queryKey); // Invalidate query after successful delete
      toast.success(`${title} با موفقیت حذف شد.`);
    },
    onError: (error) => {
      console.error(`Error deleting ${title}:`, error.response?.data);
    },
  });

  // Handle form submission for adding or updating data
  const handleSubmit = (e) => {
    e.preventDefault();

    const isFormValid = validateForm();

    if (!isFormValid) {
      return;
    }

    // Collect form data and add id when updating
    const newData = nameField.reduce((acc, field) => {
      acc[field] = formData[field] || ''; // Add field data to new data object
      return acc;
    }, {});

    newData[`${fieldType}Id`] = formData[`${fieldType}Id`] || ''; // Add the fieldTypeId if available

    // Add additional fields if they exist
    if (additionalFields) {
      Object.keys(additionalFields).forEach((key) => {
        if (key !== `${fieldType}s`) {
          newData[key] = formData[key] || '';
        }
      });
    }

    // Add `id` to newData if in edit mode
    if (editMode) {
      newData.id = currentEditId; // Add id to the data for updating
      updateMutation.mutate(newData); // ارسال `newData` به‌طور کامل برای به‌روزرسانی
    } else {
      addMutation.mutate(newData); // ارسال برای افزودن داده جدید
    }

    setFormData(initialFormData); // Reset form data after submission
  };

  // Handle editing of an existing item
  const handleEdit = (id, names, typeId) => {
    // Populate the form data with the selected item's information for editing
    const newFormData = nameField.reduce((acc, field, index) => {
      acc[field] = names[index] || ''; // Set each field's value
      return acc;
    }, {});

    setFormData({...newFormData, [`${fieldType}Id`]: typeId, ...additionalFields});
    setEditMode(true);
    setCurrentEditId(id); // Set the ID of the item being edited
  };

  // Handle deletion of an item
  const handleDelete = (id) => {
    deleteMutation.mutate(id);
  };

  // Form validation
  const validateForm = () => {
    let isValid = true;
    let errors = [];

    nameField.forEach((field) => {
      if (!formData[field].trim()) {
        isValid = false;
        errors.push(`${tHeads[nameField.indexOf(field)]} نمی‌تواند خالی باشد.`);
      }
    });

    if (additionalFields) {
      if (!formData[`${fieldType}Id`]) {
        isValid = false;
        errors.push(`لطفاً ${fieldType === 'caseType' ? 'نوع پرونده' : 'نوع دادگاه'} را انتخاب کنید.`);
      } else if (
        !additionalFields?.[`${fieldType}s`]?.some(
          (item) =>
            String(item?.id || '')
              .trim()
              .toLowerCase() ===
            String(formData[`${fieldType}Id`] || '')
              .trim()
              .toLowerCase()
        )
      ) {
        isValid = false;
        errors.push(` ${fieldType === 'caseType' ? 'نوع پرونده' : 'نوع دادگاه'} معتبر نیست.`);
      }
    }

    if (errors.length > 0) {
      errors.forEach((error) => toast.error(error));
    }

    return isValid;
  };

  // Loading and error handling
  if (isLoading) return <Spinner animation="border" />;
  if (error instanceof Error) return <Alert variant="danger">Errror: {error.message}</Alert>;

  return (
    <>
      {/* <h2 className="main-title-position p-3 z-2 bg-white fs-6">{title}</h2> */}
      <div className="position-relative px-0 py-3 d-flex justify-content-center align-items-center gap-5 flex-wrap rounded">
        <Row className="my-2 w-100">
          <Col className="p-0">
            <Form.Group as={Col} md={12} controlId={title} className=" border-top border-bottom p-3">
              <Row className="gx-0 position-relative w-100 flex-column flex-md-row align-items-end align-items-md-center gap-3">
                {nameField.map((field, index) => (
                  <Col key={index} className="w-100 p-0">
                    {/* <Form.Label>{tHeads[index]}:</Form.Label> */}
                    {/* <Form.Control type="text" placeholder={tHeads[index]} value={formData[field] || ''} onChange={(e) => setFormData({...formData, [field]: e.target.value})} /> */}
                    <Form.Floating className="text-start floating-input-container">
                      <Form.Control className="fs-7" type="text" placeholder="" value={formData[field] || ''} onChange={(e) => setFormData({...formData, [field]: e.target.value})} />
                      <label className="fs-7">{tHeads[index]}</label>
                    </Form.Floating>
                  </Col>
                ))}
                {additionalFields && (
                  <Col className="w-100 p-0">
                    {/* <Form.Control as="select" className="p-2" value={formData[`${fieldType}Id`] || ''} onChange={(e) => setFormData({...formData, [`${fieldType}Id`]: e.target.value})}>
                        <option value="">انتخاب {fieldType === 'caseType' ? 'نوع پرونده' : 'نوع دادگاه'}</option>
                        {additionalFields[`${fieldType}s`]?.map((type) => (
                          <option key={type.id} value={type.id}>
                            {type[fieldTypeName]}
                          </option>
                        ))}
                      </Form.Control> */}
                    {/* <FloatingLabel controlId="floatingSelect" label="Works with selects">
                        <Form.Select value={formData[`${fieldType}Id`] || ''} onChange={(e) => setFormData({...formData, [`${fieldType}Id`]: e.target.value})}>
                          <option value="">انتخاب {fieldType === 'caseType' ? 'نوع پرونده' : 'نوع دادگاه'}</option>
                          {additionalFields[`${fieldType}s`]?.map((type) => (
                            <option key={type.id} value={type.id}>
                              {type[fieldTypeName]}
                            </option>
                          ))}
                        </Form.Select>
                      </FloatingLabel> */}
                    <FloatingLabelSelect
                      value={formData[`${fieldType}Id`] || ''}
                      onChange={(e) => setFormData({...formData, [`${fieldType}Id`]: e.target.value})}
                      options={
                        additionalFields?.[`${fieldType}s`]?.map((type) => ({
                          value: type.id,
                          label: type[fieldTypeName],
                        })) || []
                      } // مقدار پیش‌فرض آرایه خالی
                      label={fieldType === 'caseType' ? 'نوع پرونده' : 'نوع دادگاه'}
                    />
                  </Col>
                )}
                <Col className="p-0 text-start flex-grow-0" style={{width: '60px'}}>
                  <span className="d-flex flex-row-reverse">
                    {editMode ? (
                      <>
                        <span
                          role="button"
                          onClick={() => {
                            setEditMode(false);
                            setFormData(initialFormData); // Reset the form
                          }}
                        >
                          <IoIosClose size={32} className="fs-3" style={{color: '#ff0000'}} />
                        </span>
                        <span role="button" onClick={handleSubmit}>
                          <IoIosCheckmark size={32} className="fs-3" style={{color: '#16399e'}} />
                        </span>
                      </>
                    ) : (
                      <span role="button" onClick={handleSubmit}>
                        <IoIosAdd size={32} className="fs-3" style={{color: '#16399e'}} />
                      </span>
                    )}
                  </span>
                </Col>
              </Row>
            </Form.Group>
            <Table striped border={1} hover>
              <thead className="text-center">
                <tr>
                  <th className="col-1">ردیف</th>
                  {customDisplayHead
                    ? customDisplayHead.map((customDisplayHead, index) => (
                        <th key={index} className="col-7 col-md-7">
                          {customDisplayHead}
                        </th>
                      ))
                    : tHeads.map((tHead, index) => (
                        <th key={index} className="col-7 col-md-7">
                          {tHead}
                        </th>
                      ))}
                  {additionalFields && <th className="col-3">{fieldType === 'caseType' ? 'نوع پرونده' : 'نوع دادگاه'}</th>}
                  <th className="col-1">عملیات</th>
                </tr>
              </thead>
              <tbody className="text-center">
                {data.map((item, index) => {
                  const type = additionalFields?.[`${fieldType}s`]?.find((type) => type.id === item[`${fieldType}Id`]);

                  return (
                    <tr key={item.id}>
                      <td>{index + 1}</td>

                      {customDisplay ? customDisplay.map((customDisplay, idx) => <td key={idx}>{item[customDisplay]}</td>) : nameField.map((field, idx) => <td key={idx}>{item[field]}</td>)}
                      {additionalFields && <td>{type ? type[fieldTypeName] : 'نامشخص'}</td>}
                      <td className="text-center d-flex justify-content-center align-items-center">
                        <Button
                          onClick={() =>
                            handleEdit(
                              item.id,
                              nameField.map((field) => item[field]),
                              item[`${fieldType}Id`]
                            )
                          }
                          className="bg-transparent border-0 p-2"
                        >
                          <PiNotePencilLight color="black" size={18} className="fs-6" />
                        </Button>
                        <Button onClick={() => handleDelete(item.id)} className="bg-transparent border-0 p-2">
                          <IoTrashBinOutline color="red" size={18} className="fs-6" />
                        </Button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </Table>
          </Col>
        </Row>
      </div>
    </>
  );
};

export default BasicInfoTable;
