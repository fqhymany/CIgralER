import React, {useEffect, useState} from 'react';
import {useQuery} from '@tanstack/react-query';
import api from '../../api/axios';
import {Row, Col, Table, Spinner, Alert} from 'react-bootstrap';
import {convertNumbers} from '../../Utils/convertUtils.js';
import {IoEyeOutline} from 'react-icons/io5';

const ClientCases = ({clientId, activeTab}) => {
  const fetchClientCases = async () => {
    const response = await api.get(`./api/ClientEndpoints/GetClientCasesByClientId?ClientId=${clientId}`);
    return response.data.clientCases;
  };

  const {
    data: clientCases,
    isLoading,
    isError,
    error,
  } = useQuery({
    queryKey: ['GetClientCasesByClientId', clientId],
    queryFn: fetchClientCases,
  });

  // فیلتر کردن پرونده‌ها بر اساس tab فعال
  const filteredClientCases = clientCases?.filter((caseItem) => (activeTab === 'currentCases' && caseItem.caseStatus.id !== 3) || (activeTab === 'closedCases' && caseItem.caseStatus.id === 3));

  const renderTableRows = () => {
    if (isLoading) return <Spinner animation="border" />;
    if (error instanceof Error) return <Alert variant="danger">Error: {error.message}</Alert>;
    return filteredClientCases && filteredClientCases.length > 0 ? (
      filteredClientCases.map((caseItem, index) => (
        <Row key={caseItem.id} className="center-all text-center border Mainbox gx-0 px-1" style={{minHeight: '40px', backgroundColor: index % 2 === 0 ? '#ffffff' : '#f0f0f0'}}>
          <Col md={1} xs={1} className="d-none d-lg-block">
            {convertNumbers(index + 1, 'farsi')}
          </Col>
          <Col md={2} xs={2}>
            {convertNumbers(caseItem.caseNumber, 'farsi')}
          </Col>
          <Col lg={3} sm={3} xs={3}>
            {caseItem.subjectType ? caseItem.predefinedSubject?.title || 'بدون عنوان' : caseItem.title || 'بدون عنوان'}
          </Col>
          <Col lg={2} sm={3} xs={2}>
            {caseItem.hearingStage?.name || 'نامشخص'}
          </Col>
          <Col md={1} xs={2}>
            {caseItem.clientRoleInCase?.title || 'نامشخص'}
          </Col>
          <Col lg={2} sm={3} xs={3}>
            {convertNumbers(caseItem?.nearestDeadline?.endDate, 'farsi') || 'نامشخص'}
          </Col>
          <Col md={1} xs={1} className="d-none d-lg-block">
            <IoEyeOutline className="fs-4 text-primary" role="button" />
          </Col>
        </Row>
      ))
    ) : (
      <Row>
        <Col className="text-center text-muted my-1">
          <span>هیچ پرونده‌ای یافت نشد</span>
        </Col>
      </Row>
    );
  };

  return (
    <Row>
      <Col md={12} style={{backgroundColor: '#E2E2E3'}} className="text-center border">
        <Row className="fs-6 fw-bold center-all text-center gx-0" style={{minHeight: '40px'}}>
          <Col md={1} xs={1} className="d-none d-lg-block">
            ردیف
          </Col>
          <Col md={2} xs={2}>
            شماره
          </Col>
          <Col lg={3} sm={3} xs={3}>
            موضوع
          </Col>
          <Col lg={2} sm={3} xs={2}>
            مرحله رسیدگی
          </Col>
          <Col md={1} xs={2}>
            نقش
          </Col>
          <Col lg={2} sm={3} xs={3}>
            مهلت
          </Col>
          <Col md={1} xs={1} className="d-none d-lg-block">
            جزئیات
          </Col>
        </Row>
      </Col>
      <Col md={12} className="gx-0">
        {renderTableRows()}
      </Col>
    </Row>
  );
};

export default ClientCases;
