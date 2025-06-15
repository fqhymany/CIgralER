import React, {useState, useEffect} from 'react';
import {Container, Row, Col, Card, Button, Form, Badge, ListGroup, Spinner, Alert, ButtonGroup, ProgressBar} from 'react-bootstrap';
import {MessageSquare, Clock, CheckCircle, XCircle, RefreshCw, Users, Activity, Phone, Mail, ArrowRight, Circle, AlertCircle} from 'lucide-react';
import api from '../../api/axios';

const AgentDashboard = () => {
  const [tickets, setTickets] = useState([]);
  const [selectedTicket, setSelectedTicket] = useState(null);
  const [agentStatus, setAgentStatus] = useState('Available');
  const [stats, setStats] = useState({
    activeChats: 0,
    resolvedToday: 0,
    avgResponseTime: '2m 15s',
    satisfaction: 4.5,
  });
  const [availableAgents, setAvailableAgents] = useState([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadTickets();
    loadAvailableAgents();
    const interval = setInterval(() => {
      loadTickets();
      loadAvailableAgents();
    }, 30000);

    return () => clearInterval(interval);
  }, []);

  const loadTickets = async () => {
    try {
      const response = await api.get('/api/support/tickets');
      const data = response.data;
      setTickets(data);
      setStats((prev) => ({...prev, activeChats: data.filter((t) => t.status < 2).length}));
    } catch (error) {
      console.error('Failed to load tickets:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const loadAvailableAgents = async () => {
    try {
      const response = await api.get('/api/support/agents/available');
      const data = response.data;
      setAvailableAgents(data);
    } catch (error) {
      console.error('Failed to load agents:', error);
    }
  };

  const updateStatus = async (newStatus) => {
    try {
      await api.post('/api/support/agent/status', {status: newStatus});
      setAgentStatus(newStatus);
    } catch (error) {
      console.error('Failed to update status:', error);
    }
  };

  const transferTicket = async (ticketId, newAgentId) => {
    try {
      await api.post(`/api/support/tickets/${ticketId}/transfer`, {
        newAgentId,
        reason: 'Manual transfer by agent',
      });
      loadTickets();
    } catch (error) {
      console.error('Failed to transfer ticket:', error);
    }
  };

  const closeTicket = async (ticketId, reason) => {
    try {
      await api.post(`/api/support/tickets/${ticketId}/close`, {reason});
      loadTickets();
    } catch (error) {
      console.error('Failed to close ticket:', error);
    }
  };

  const getStatusVariant = (status) => {
    switch (status) {
      case 0:
        return 'primary';
      case 1:
        return 'warning';
      case 2:
        return 'success';
      case 3:
        return 'secondary';
      case 4:
        return 'info';
      default:
        return 'secondary';
    }
  };

  const getStatusText = (status) => {
    const statuses = ['باز', 'در حال بررسی', 'حل شده', 'بسته شده', 'منتقل شده'];
    return statuses[status] || 'نامشخص';
  };

  const getAgentStatusColor = (status) => {
    switch (status) {
      case 'Available':
        return 'success';
      case 'Busy':
        return 'warning';
      case 'Away':
        return 'warning';
      case 'Offline':
        return 'secondary';
      default:
        return 'secondary';
    }
  };

  return (
    <Container fluid className="h-100 p-0">
      <Row className="h-100 g-0">
        {/* Sidebar */}
        <Col md={4} lg={3} className="h-100 bg-white border-end d-flex flex-column">
          {/* Agent Status */}
          <div className="p-3 border-bottom">
            <div className="d-flex justify-content-between align-items-center mb-3">
              <h5 className="mb-0">داشبورد پشتیبان</h5>
              <Button variant="light" size="sm" onClick={() => loadTickets()}>
                <RefreshCw size={16} />
              </Button>
            </div>

            <div className="d-flex align-items-center gap-2 mb-3">
              <Circle size={12} fill="currentColor" className={`text-${getAgentStatusColor(agentStatus)}`} />
              <Form.Select value={agentStatus} onChange={(e) => updateStatus(e.target.value)} size="sm">
                <option value="Available">در دسترس</option>
                <option value="Busy">مشغول</option>
                <option value="Away">غایب</option>
                <option value="Offline">آفلاین</option>
              </Form.Select>
            </div>

            {/* Stats */}
            <Row className="g-2">
              <Col xs={6}>
                <Card bg="primary" text="white" className="text-center">
                  <Card.Body className="p-2">
                    <h6 className="mb-0">{stats.activeChats}</h6>
                    <small>گفتگوهای فعال</small>
                  </Card.Body>
                </Card>
              </Col>
              <Col xs={6}>
                <Card bg="success" text="white" className="text-center">
                  <Card.Body className="p-2">
                    <h6 className="mb-0">{stats.resolvedToday}</h6>
                    <small>حل شده امروز</small>
                  </Card.Body>
                </Card>
              </Col>
              <Col xs={6}>
                <Card bg="warning" text="dark" className="text-center">
                  <Card.Body className="p-2">
                    <h6 className="mb-0">{stats.avgResponseTime}</h6>
                    <small>میانگین پاسخ</small>
                  </Card.Body>
                </Card>
              </Col>
              <Col xs={6}>
                <Card bg="info" text="white" className="text-center">
                  <Card.Body className="p-2">
                    <h6 className="mb-0">⭐ {stats.satisfaction}</h6>
                    <small>رضایت</small>
                  </Card.Body>
                </Card>
              </Col>
            </Row>
          </div>

          {/* Ticket List */}
          <div className="flex-fill overflow-auto p-3">
            <h6 className="text-muted mb-3">تیکت‌های پشتیبانی</h6>
            {isLoading ? (
              <div className="text-center py-5">
                <Spinner animation="border" variant="primary" />
              </div>
            ) : tickets.length === 0 ? (
              <Alert variant="info" className="text-center">
                تیکتی برای شما ثبت نشده است
              </Alert>
            ) : (
              <ListGroup>
                {tickets.map((ticket) => (
                  <ListGroup.Item key={ticket.id} action active={selectedTicket?.id === ticket.id} onClick={() => setSelectedTicket(ticket)} className="border rounded mb-2">
                    <div className="d-flex justify-content-between align-items-start">
                      <div className="flex-fill">
                        <div className="d-flex justify-content-between mb-1">
                          <strong>{ticket.requesterName}</strong>
                          <Badge bg={getStatusVariant(ticket.status)}>{getStatusText(ticket.status)}</Badge>
                        </div>

                        {ticket.lastMessage && <p className="text-muted small mb-1 text-truncate">{ticket.lastMessage.content}</p>}

                        <div className="d-flex justify-content-between">
                          <small className="text-muted">{new Date(ticket.created).toLocaleTimeString()}</small>
                          {ticket.unreadCount > 0 && (
                            <Badge bg="danger" pill>
                              {ticket.unreadCount}
                            </Badge>
                          )}
                        </div>
                      </div>
                    </div>
                  </ListGroup.Item>
                ))}
              </ListGroup>
            )}
          </div>

          {/* Available Agents */}
          <div className="p-3 border-top">
            <h6 className="text-muted mb-2">اپراتورهای در دسترس</h6>
            <div style={{maxHeight: '120px', overflowY: 'auto'}}>
              {availableAgents.map((agent) => (
                <div key={agent.id} className="d-flex justify-content-between align-items-center py-1">
                  <div className="d-flex align-items-center gap-2">
                    <Circle size={8} fill="currentColor" className={`text-${agent.agentStatus === 0 ? 'success' : agent.agentStatus === 1 ? 'warning' : 'warning'}`} />
                    <small>{agent.name}</small>
                  </div>
                  <small className="text-muted">
                    {agent.currentActiveChats}/{agent.maxConcurrentChats}
                  </small>
                </div>
              ))}
            </div>
          </div>
        </Col>

        {/* Main Content */}
        <Col md={8} lg={9} className="h-100 d-flex flex-column">
          {selectedTicket ? (
            <>
              {/* Ticket Header */}
              <Card className="rounded-0 border-0 border-bottom">
                <Card.Body className="d-flex justify-content-between align-items-center">
                  <div>
                    <h4 className="mb-1">{selectedTicket.requesterName}</h4>
                    <div className="d-flex gap-3 text-muted small">
                      {selectedTicket.requesterEmail && (
                        <span>
                          <Mail size={16} className="me-1" />
                          {selectedTicket.requesterEmail}
                        </span>
                      )}
                      <span>
                        <Clock size={16} className="me-1" />
                        {new Date(selectedTicket.created).toLocaleString()}
                      </span>
                    </div>
                  </div>

                  <ButtonGroup>
                    <Button variant="primary" onClick={() => window.open(`/chat/${selectedTicket.chatRoomId}`, '_blank')}>
                      <MessageSquare size={16} className="me-2" />
                      ورود به چت
                    </Button>

                    <Button
                      variant="outline-primary"
                      onClick={() => {
                        const agent = availableAgents[0];
                        if (agent) transferTicket(selectedTicket.id, agent.id);
                      }}
                    >
                      <ArrowRight size={16} className="me-2" />
                      انتقال
                    </Button>

                    {selectedTicket.status < 2 && (
                      <Button variant="success" onClick={() => closeTicket(selectedTicket.id, 'حل شده توسط اپراتور')}>
                        <CheckCircle size={16} className="me-2" />
                        بستن تیکت
                      </Button>
                    )}
                  </ButtonGroup>
                </Card.Body>
              </Card>

              {/* Ticket Details */}
              <div className="flex-fill p-4 overflow-auto">
                <Container>
                  <Row className="justify-content-center">
                    <Col lg={8}>
                      <Card className="mb-4">
                        <Card.Header>
                          <h5 className="mb-0">اطلاعات تیکت</h5>
                        </Card.Header>
                        <Card.Body>
                          <Row>
                            <Col sm={6} className="mb-3">
                              <dt className="text-muted">وضعیت</dt>
                              <dd>
                                <Badge bg={getStatusVariant(selectedTicket.status)} className="px-3">
                                  {getStatusText(selectedTicket.status)}
                                </Badge>
                              </dd>
                            </Col>
                            <Col sm={6} className="mb-3">
                              <dt className="text-muted">تاریخ ثبت</dt>
                              <dd>{new Date(selectedTicket.created).toLocaleString()}</dd>
                            </Col>
                            <Col sm={6} className="mb-3">
                              <dt className="text-muted">آخرین پیام</dt>
                              <dd>{selectedTicket.lastMessage ? new Date(selectedTicket.lastMessage.created).toLocaleString() : 'بدون پیام'}</dd>
                            </Col>
                            <Col sm={6} className="mb-3">
                              <dt className="text-muted">شناسه چت</dt>
                              <dd>#{selectedTicket.chatRoomId}</dd>
                            </Col>
                          </Row>
                        </Card.Body>
                      </Card>

                      <Alert variant="warning">
                        <Alert.Heading>
                          <AlertCircle size={20} className="me-2" />
                          اقدامات سریع
                        </Alert.Heading>
                        <p>برای مشاهده و پاسخ به پیام‌ها روی دکمه &quot;ورود به چت&quot; کلیک کنید.</p>
                        <ul className="mb-0">
                          <li>برای انتقال تیکت به اپراتور دیگر از دکمه انتقال استفاده کنید</li>
                          <li>در صورت حل مشکل، تیکت را ببندید</li>
                          <li>اطلاعات مشتری را در پنجره چت بررسی کنید</li>
                        </ul>
                      </Alert>
                    </Col>
                  </Row>
                </Container>
              </div>
            </>
          ) : (
            <div className="h-100 d-flex align-items-center justify-content-center">
              <div className="text-center text-muted">
                <MessageSquare size={64} className="mb-3" />
                <h4>تیکتی انتخاب نشده است</h4>
                <p>برای مشاهده جزئیات، یک تیکت را از لیست انتخاب کنید</p>
              </div>
            </div>
          )}
        </Col>
      </Row>
    </Container>
  );
};

export default AgentDashboard;
