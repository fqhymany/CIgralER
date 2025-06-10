import React, {useState, useEffect} from 'react';
import {Row, Col, Collapse, Table, Dropdown} from 'react-bootstrap';
import {toast} from 'react-toastify';
import Swal from 'sweetalert2';
import {IoEyeOutline, IoEllipsisVertical, IoTrashOutline} from 'react-icons/io5';
import {RiListSettingsLine, RiHistoryLine} from 'react-icons/ri';
import {GrPowerCycle} from 'react-icons/gr';
import {PiNotePencilLight} from 'react-icons/pi';
import {Link} from 'react-router-dom';
import {convertNumbers} from '../../Utils/convertUtils.js';
import '../../assets/styles/CustomTable.css';
import ModalComponent from '../../components/QuickAccess/ModalComponent';
import ClientCases from './ClientCases.jsx';
import {useMutation, useQueryClient} from '@tanstack/react-query';
import api from '../../api/axios';

const ClientsDesktop = React.memo(({clientsData}) => {
  const [showModal, setShowModal] = useState(false);
  const [modalTitle, setModalTitle] = useState('');
  const [modalContent, setModalContent] = useState('');
  const [clientId, setClientId] = useState(null);
  const [activeTab, setActiveTab] = useState('currentCases');
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
        position: 'bottom-right',
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
        position: 'bottom-right',
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
      text: 'کاربر حذف خواهد شد',
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

  const handleShowModal = (title, content) => {
    setModalTitle(title);
    setModalContent(content);
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
  };

  const handleTabClick = (tabName, id) => {
    const title = tabName === 'currentCases' ? 'پرونده های جاری' : 'پرونده های خاتمه یافته';
    const content = <ClientCases clientId={id} activeTab={tabName} />;
    handleShowModal(title, content);
  };

  const CaseRow = ({clientItem, index}) => {
    const [openPanel, setOpenPanel] = useState(null);

    const togglePanel = (panelName) => {
      setOpenPanel(openPanel === panelName ? null : panelName);
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
      <React.Fragment key={clientItem.id}>
        <Row className="center-all text-center border Mainbox w-100" style={{minHeight: '40px', backgroundColor: index % 2 === 0 ? '#ffffff' : '#f0f0f0'}}>
          <Col md={1}>{convertNumbers(index + 1, 'farsi')}</Col>
          <Col md={4}>
            {clientItem.firstName} {clientItem.lastName}
          </Col>
          <Col md={2}>
            <IoEyeOutline className="fs-4 text-primary" role="button" onClick={() => togglePanel('details')} />
          </Col>
          <Col md={2}>
            <RiHistoryLine className="fs-4 text-warning" role="button" onClick={() => handleTabClick('currentCases', clientItem.id)} />
          </Col>
          <Col md={2}>
            <GrPowerCycle className="fs-4 text-success" role="button" onClick={() => handleTabClick('closedCases', clientItem.id)} />
          </Col>
          <Col md={1}>
            <Dropdown>
              <Dropdown.Toggle as={CustomToggle} id={`dropdown-${clientItem.id}`}>
                <IoEllipsisVertical className="fs-4 cursor-pointer" />
              </Dropdown.Toggle>
              <Dropdown.Menu align="end">
                <Dropdown.Item as={Link} to={`/UserManagement?clientId=${clientItem.id}`}>
                  <PiNotePencilLight className="me-2" />
                  ویرایش
                </Dropdown.Item>
                <Dropdown.Item onClick={() => handleDelete(clientItem.id)} className="text-danger">
                  <IoTrashOutline className="me-2" />
                  حذف
                </Dropdown.Item>
              </Dropdown.Menu>
            </Dropdown>
          </Col>
        </Row>
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
      </React.Fragment>
    );
  };

  const renderTableRows = (clients) => {
    return clients && clients.length > 0 ? (
      clients.map((clientItem, index) => <CaseRow key={clientItem.id} clientItem={clientItem} index={index} />)
    ) : (
      <Row>
        <Col className="text-center text-muted my-1">
          <span>هیچ موکلی یافت نشد</span>
        </Col>
      </Row>
    );
  };

  return (
    <>
      <div className="p-2 w-100">
        <Row>
          <Col md={12} style={{backgroundColor: '#E2E2E3'}} className="text-center border">
            <Row className="fs-6 fw-bold center-all text-center" style={{minHeight: '40px'}}>
              <Col md={1}>ردیف</Col>
              <Col md={4}>نام و نام‌خانوادگی</Col>
              <Col md={2}>اطلاعات</Col>
              <Col md={2}>پرونده های جاری</Col>
              <Col md={2}>پرونده های مختومه</Col>
              <Col md={1}>عملیات</Col>
            </Row>
          </Col>
          <Col md={12} className="d-flex flex-column justify-content-center align-items-center px-0">
            {renderTableRows(clientsData)}
          </Col>
        </Row>
      </div>
      <ModalComponent show={showModal} onHide={handleCloseModal} title={modalTitle} content={modalContent} />
    </>
  );
});

export default ClientsDesktop;
