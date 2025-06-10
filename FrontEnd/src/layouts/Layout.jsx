import React from 'react';
import {Container} from 'reactstrap';
import {Row, Col} from 'react-bootstrap';
import Header from '../components/Header/Header';
import MainNavbar from '../components/Navbar/MainNavbar';
import Aside from '../components/Aside/Aside';
import QuickAccess from '../components/QuickAccess/QuickAccess';
import Breadcrumbs from '../components/BreadCrumb/Breadcrumbs';
import {Outlet, useOutlet} from 'react-router-dom';
export const Layout = (props) => {
  const outlet = useOutlet();
  if (outlet) {
    return outlet;
  }
  return (
    <>
      <Header />
      <div className="container-fluid gx-0 row flex-grow-1">
        <MainNavbar />

        <Container tag="main" className="flex-grow-1 col-12 col-md-8 mb-2 mb-md-4 py-3 pt-md-2 pb-md-4 px-0 main-container-position">
          <div className="position-relative m-2 mt-md-0 m-md-3 shadow rounded border" style={{minHeight: '100%'}}>
            <Row className="m-0">
              <Col className="pt-2">
                <Breadcrumbs />
              </Col>
            </Row>
            {props.children}
          </div>
        </Container>
        <Aside />
        <QuickAccess />
      </div>
    </>
  );
};
