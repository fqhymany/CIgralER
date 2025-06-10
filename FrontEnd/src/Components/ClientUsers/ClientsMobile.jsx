import React, {useState} from 'react';
import {Container, Row, Col, Collapse, Table, Alert, Dropdown} from 'react-bootstrap';
import {toast} from 'react-toastify';
import Swal from 'sweetalert2';
import {IoEyeOutline, IoEllipsisVertical, IoTrashOutline} from 'react-icons/io5';
import {RiHistoryLine} from 'react-icons/ri';
import {GrPowerCycle} from 'react-icons/gr';
import {TfiUser} from 'react-icons/tfi';
import {PiPhoneLight, PiNotePencilLight} from 'react-icons/pi';
import {Link, useLocation} from 'react-router-dom';
import {convertNumbers} from '../../Utils/convertUtils.js';
import ClientCases from './ClientCases.jsx';
import ModalComponent from '../../components/QuickAccess/ModalComponent';
import {useMutation, useQueryClient} from '@tanstack/react-query';
import api from '../../api/axios';

const ClientBox = ({clientItem, index}) => {
  const location = useLocation();
  const [openPanel, setOpenPanel] = useState(null);
  const [showModal, setShowModal] = useState(false);
  const [modalTitle, setModalTitle] = useState('');
  const [modalContent, setModalContent] = useState('');
  const queryClient = useQueryClient();

  const deleteMutation = useMutation({
    mutationFn: async (clientId) => {
      const response = await api.post(`/api/AuthEndpoints/deleteUsers/${clientId}`);
      if (response.status < 200 || response.status >= 300) {
        throw new Error('خطا در حذف کاربر');
      }
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries(['Clients']);
      toast.success('کاربر با موفقیت حذف شد', {
        position: 'bottom-left',
        autoClose: 5000,
        hideProgressBar: false,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
        progress: undefined,
      });
    },
    onError: (error) => {
      toast.error('خطا در حذف کاربر: ' + error.message, {
        position: 'bottom-left',
        autoClose: 5000,
        hideProgressBar: false,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
        progress: undefined,
      });
    },
  });

  const handleDelete = (clientId) => {
    Swal.fire({
      title: 'آیا مطمئن هستید؟',
      text: 'این کاربر حذف خواهد شد',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'بله، حذف شود',
      cancelButtonText: 'انصراف',
      customClass: {
        container: 'swal-rtl',
        title: 'swal-title-rtl',
        content: 'swal-content-rtl',
      },
    }).then((result) => {
      if (result.isConfirmed) {
        deleteMutation.mutate(clientId);
      }
    });
  };

  const togglePanel = (panelName) => {
    setOpenPanel(openPanel === panelName ? null : panelName);
  };

  const handleTabClick = (tabName, id) => {
    const title = tabName === 'currentCases' ? 'پرونده های جاری' : 'پرونده های خاتمه یافته';
    const content = <ClientCases clientId={id} activeTab={tabName} />;
    handleShowModal(title, content);
  };

  const handleShowModal = (title, content) => {
    setModalTitle(title);
    setModalContent(content);
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
  };

  const CustomToggle = React.forwardRef(({children, onClick}, ref) => (
    <div
      ref={ref}
      onClick={(e) => {
        e.preventDefault();
        onClick(e);
      }}
      style={{cursor: 'pointer'}}
    >
      {children}
    </div>
  ));

  return (
    <>
      <div className="container px-3 pt-2 pb-1 mb-0 mt-4 m-2 client-card" style={{border: '1px solid #666', borderBottomLeftRadius: openPanel ? '0' : '0.375rem', borderBottomRightRadius: openPanel ? '0' : '0.375rem'}}>
        <div className="d-flex justify-content-between align-items-center">
          <div>
            <span className="fs-7" style={{color: '#b7bbc7'}}>
              ردیف:{' '}
            </span>
            <span className="fs-7" style={{color: '#464c62'}}>
              {convertNumbers(index + 1, 'farsi')}
            </span>
          </div>
          <span>
            <PiPhoneLight size={20} color="#b7bbc7" />
            {clientItem.phoneNumber ? convertNumbers(clientItem.phoneNumber, 'farsi') : ''}
          </span>
          <Dropdown>
            <Dropdown.Toggle as={CustomToggle} id={`dropdown-${clientItem.id}`}>
              <IoEllipsisVertical size={20} className="cursor-pointer" />
            </Dropdown.Toggle>
            <Dropdown.Menu align="end">
              <Dropdown.Item 
              as={Link} 
              to={`/UserManagement?clientId=${clientItem.id}`}>
                <PiNotePencilLight className="me-2" />
                ویرایش
              </Dropdown.Item>
              <Dropdown.Item onClick={() => handleDelete(clientItem.id)} className="text-danger">
                <IoTrashOutline className="me-2" />
                حذف
              </Dropdown.Item>
            </Dropdown.Menu>
          </Dropdown>
          <TfiUser size={20} color="orange" />
        </div>
        <div className="text-center fs-7 py-3" style={{borderBottom: '1px solid #ececec'}}>
          {clientItem.firstName || clientItem.firstName ? clientItem.firstName + ' ' + clientItem.lastName : 'بدون نام'}
        </div>
        <Table borderless className="mb-0">
          <tbody>
            <tr className="d-flex">
              <td className="col-4 text-center d-flex justify-content-center align-items-center" style={{padding: '2% 0', borderLeft: '1px solid #ececec'}}>
                <div className="d-flex justify-content-center align-items-center" role="button" onClick={() => togglePanel('details')}>
                  <IoEyeOutline className="fs-3" style={{color: '#06cef0'}} />
                  <span className="pe-1 pe-md-3 fs-7" style={{color: '#5c647b'}}>
                    اطلاعات
                  </span>
                </div>
              </td>
              <td className="col-4 text-center d-flex justify-content-center align-items-center" style={{padding: '2% 0', borderLeft: '1px solid #ececec'}}>
                <div className="d-flex justify-content-center align-items-center" role="button" onClick={() => handleTabClick('currentCases', clientItem.id)}>
                  <RiHistoryLine className="fs-3" style={{color: '#06cef0'}} />
                  <span className="pe-1 pe-md-3 fs-7" style={{color: '#5c647b'}}>
                    پرونده جاری
                  </span>
                </div>
              </td>
              <td className="col-4 text-center d-flex justify-content-center align-items-center" style={{padding: '2% 0'}}>
                <div className="d-flex justify-content-center align-items-center" role="button" onClick={() => handleTabClick('closedCases', clientItem.id)}>
                  <GrPowerCycle className="fs-3" style={{color: '#06cef0'}} />
                  <span className="pe-1 pe-md-3 fs-7" style={{color: '#5c647b'}}>
                    پرونده مختومه
                  </span>
                </div>
              </td>
            </tr>
          </tbody>
        </Table>
      </div>

      {/* نمایش اطلاعات موکل */}
      <Collapse in={openPanel === 'details'}>
        <Row className="w-100">
          <Col xs={12} className="rounded px-0">
            <Table bordered hover className="mb-0">
              <tbody>
                <tr className="align-middle">
                  <td className="col-12 py-0">
                    <Row className="my-2 px-2">
                      <Col className="col-12 my-2">
                        <span className="fw-bold">نام پدر: </span>
                        <span>{clientItem.fatherName}</span>
                      </Col>
                      <Col className="col-12 my-2">
                        <span className="fw-bold">تاریخ تولد: </span>
                        <span>{clientItem.birthDate || 'نامشخص'}</span>
                      </Col>
                      <Col className="col-12 my-2">
                        <span className="fw-bold">تلفن : </span>
                        <span>{clientItem.phoneNumber || 'نامشخص'}</span>
                      </Col>
                      <Col className="col-12 my-2">
                        <span className="fw-bold">آدرس: </span>
                        <span>{clientItem.address || 'نامشخص'}</span>
                      </Col>
                    </Row>
                  </td>
                </tr>
              </tbody>
            </Table>
          </Col>
        </Row>
      </Collapse>

      {/* نمایش مدال برای پرونده‌های جاری و مختومه */}
      <ModalComponent show={showModal} onHide={handleCloseModal} title={modalTitle} content={modalContent} />
    </>
  );
};

const ClientsMobile = ({clientsData}) => {
  return (
    <Container className="mt-2">
      <Row>
        <Col className="d-flex flex-column justify-content-center align-items-center px-1" md={12}>
          {clientsData ? clientsData.map((clientItem, index) => <ClientBox key={clientItem.id} clientItem={clientItem} index={index} />) : <Alert variant="warning">هیچ موکلی یافت نشد.</Alert>}
        </Col>
      </Row>
    </Container>
  );
};

export default ClientsMobile;
