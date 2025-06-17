// FrontEnd/src/Components/Chat/Chat.jsx
// Complete Chat component with all fixes applied

import React, {useState, useEffect, useCallback} from 'react';
import {useParams, useNavigate, useLocation} from 'react-router-dom';
import {Container, Row, Col, Card, Button, Alert, Spinner} from 'react-bootstrap';
import {useChat} from '../../hooks/useChat';
import {useMobileNavigation} from '../../hooks/useMobileNavigation';
import ChatRoomList from './ChatRoomList';
import MessageList from './MessageList';
import MessageInput from './MessageInput';
import TypingIndicatorComponent from './TypingIndicatorComponent';
import ConnectionStatus from './ConnectionStatus';
import ForwardModal from './ForwardModal';
import './Chat.css';

const Chat = () => {
  const {roomId} = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const {
    rooms,
    currentRoom,
    messages,
    typingUsers,
    isConnected,
    isLoading,
    error,
    currentLoggedInUserId,
    isForwardModalVisible,
    hideForwardModal,
    setCurrentRoom,
    loadMessages,
    loadRooms,
    joinRoom,
    leaveRoom,
    markAllMessagesAsReadInRoom,
  } = useChat();

  const [showNewRoomModal, setShowNewRoomModal] = useState(false);
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 768);

  // Check if current room is a group chat
  const isCurrentRoomGroupChat = currentRoom?.isGroup || false;

  // Check if this is a support chat
  const isSupportChat = currentRoom?.chatRoomType === 2;

  // Extract support parameters from URL
  const urlParams = new URLSearchParams(location.search);
  const isFromSupport = urlParams.get('support') === 'true';
  const ticketId = urlParams.get('ticketId');

  // Mobile navigation handler
  const handleMobileBack = useCallback(() => {
    if (currentRoom) {
      setCurrentRoom(null);
      navigate('/chats', {replace: true});
    }
  }, [currentRoom, navigate, setCurrentRoom]);

  useMobileNavigation(handleMobileBack);

  // Handle window resize
  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth <= 768);
    };

    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);
  // Load initial data
  useEffect(() => {
    if (!rooms.length && !error) {
      loadRooms();
    }
  }, []); // Empty dependency array

  // Handle room selection from URL
  useEffect(() => {
    if (roomId && rooms.length > 0) {
      const room = rooms.find((r) => r.id === parseInt(roomId));
      if (room && (!currentRoom || currentRoom.id !== room.id)) {
        handleRoomSelect(room);
      }
    }
  }, [roomId, rooms, currentRoom]);

  // Room selection handler
  const handleRoomSelect = async (room) => {
    try {
      // Leave current room if exists
      if (currentRoom && currentRoom.id !== room.id) {
        await leaveRoom(currentRoom.id);
      }

      // Set new current room
      setCurrentRoom(room);

      // Join new room
      await joinRoom(room.id);

      // Load messages for the room
      await handleLoadMessages(room.id, 1, 20, false);

      // Mark messages as read
      markAllMessagesAsReadInRoom(room.id);

      // Update URL
      navigate(`/chat/${room.id}${isFromSupport ? `?support=true&ticketId=${ticketId}` : ''}`, {replace: true});
    } catch (error) {
      console.error('Error selecting room:', error);
    }
  };

  // Load messages handler
  const handleLoadMessages = async (roomId, page = 1, pageSize = 20, isLoadingMore = false) => {
    try {
      await loadMessages(roomId, page, pageSize, isLoadingMore);
    } catch (error) {
      console.error('Error loading messages:', error);
    }
  };

  // Handle connection errors
  if (error) {
    return (
      <Container fluid className="h-100 d-flex align-items-center justify-content-center">
        <Alert variant="danger" className="text-center">
          <h5>خطا در اتصال</h5>
          <p>{error}</p>
          <Button variant="outline-danger" onClick={() => window.location.reload()}>
            تلاش مجدد
          </Button>
        </Alert>
      </Container>
    );
  }

  // Loading state
  if (isLoading && rooms.length === 0 && !error) {
    return (
      <Container fluid className="h-100 d-flex align-items-center justify-content-center">
        <div className="text-center">
          <Spinner animation="border" variant="primary" />
          <p className="mt-3">در حال بارگذاری...</p>
        </div>
      </Container>
    );
  }

  return (
    <div className="chat-container">
      <Container fluid className="h-100 p-0">
        {isMobile ? (
          // Mobile Layout: Show either chat list or chat room
          !currentRoom ? (
            // Show chat list on mobile when no room is selected
            <Row className="h-100 gx-0">
              <Col xs={12} className="h-100 d-flex flex-column">
                <Card className="flex-grow-1 d-flex flex-column border-0 rounded-0">
                  <Card.Header className="d-flex justify-content-between align-items-center py-3 border-bottom">
                    <h5 className="mb-0">چت‌ها</h5>
                    <ConnectionStatus isConnected={isConnected} />
                  </Card.Header>
                  <Card.Body className="chat-list-container p-0 d-flex flex-column flex-grow-1">
                    <ChatRoomList rooms={rooms} currentRoom={currentRoom} onRoomSelect={handleRoomSelect} onNewRoom={() => setShowNewRoomModal(true)} isLoading={isLoading && rooms.length === 0 && !error} />
                  </Card.Body>
                </Card>
              </Col>
            </Row>
          ) : (
            // Show chat room on mobile when room is selected
            <Row className="h-100 gx-0">
              <Col xs={12} className="h-100 d-flex flex-column">
                <Card className="flex-grow-1 d-flex flex-column border-0 rounded-0" style={{overflow: 'hidden'}}>
                  <Card.Header className="py-2 border-bottom">
                    <div className="d-flex flex-row-reverse align-items-center justify-content-between">
                      <div className="d-flex align-items-center">
                        {/* Back button for mobile */}
                        <Button variant="link" size="sm" className="me-2 p-1" onClick={handleMobileBack}>
                          <i className="bi bi-arrow-right fs-5"></i>
                        </Button>

                        <div className="me-3">
                          {isSupportChat ? (
                            <div className="rounded-circle bg-warning text-white d-flex align-items-center justify-content-center" style={{width: '40px', height: '40px'}}>
                              <i className="bi bi-headset"></i>
                            </div>
                          ) : currentRoom.avatar ? (
                            <img src={currentRoom.avatar} alt={currentRoom.name} className="rounded-circle" style={{width: '40px', height: '40px'}} />
                          ) : (
                            <div className={`rounded-circle ${currentRoom.isGroup ? 'bg-success' : 'bg-primary'} text-white d-flex align-items-center justify-content-center`} style={{width: '40px', height: '40px'}}>
                              {currentRoom.isGroup ? <i className="bi bi-people-fill"></i> : currentRoom.name.charAt(0).toUpperCase()}
                            </div>
                          )}
                        </div>

                        <div className="flex-grow-1">
                          <h6 className="mb-0">{isSupportChat ? `پشتیبانی - ${currentRoom.name}` : currentRoom.name}</h6>
                          {currentRoom.description && <small className="text-muted">{currentRoom.description}</small>}
                          {isFromSupport && ticketId && <small className="text-info d-block">تیکت #{ticketId}</small>}
                        </div>
                      </div>

                      <div>
                        {typingUsers[currentRoom.id] && typingUsers[currentRoom.id].filter((u) => u.userId !== currentLoggedInUserId).length > 0 && (
                          <TypingIndicatorComponent users={typingUsers[currentRoom.id].filter((u) => u.userId !== currentLoggedInUserId)} />
                        )}
                      </div>
                    </div>
                  </Card.Header>

                  <Card.Body className="p-0 d-flex flex-column flex-grow-1" style={{overflowY: 'hidden'}}>
                    <MessageList
                      messages={messages[currentRoom.id]?.items || []}
                      typingUsers={typingUsers[currentRoom.id] || []}
                      isLoading={isLoading}
                      hasMoreMessages={messages[currentRoom.id]?.hasMore || false}
                      onLoadMoreMessages={() => {
                        if (messages[currentRoom.id]?.hasMore) {
                          const nextPage = (messages[currentRoom.id]?.currentPage || 0) + 1;
                          handleLoadMessages(currentRoom.id, nextPage, 20, true);
                        }
                      }}
                      isGroupChat={isCurrentRoomGroupChat}
                    />
                    <MessageInput roomId={currentRoom.id} />
                  </Card.Body>
                </Card>
              </Col>
            </Row>
          )
        ) : (
          // Desktop Layout: Side by side layout
          <Row className="h-100 flex-row-reverse gx-2">
            {/* Chat List Sidebar */}
            <Col lg={3} md={4} className="h-100 order-1 order-lg-0 d-flex flex-column">
              <Card className="flex-grow-1 d-flex flex-column">
                <Card.Header className="d-flex justify-content-between align-items-center py-2">
                  <h5 className="mb-0 fs-6">چت‌ها</h5>
                  <ConnectionStatus isConnected={isConnected} />
                </Card.Header>
                <Card.Body className="chat-list-container p-0 d-flex flex-column flex-grow-1">
                  <ChatRoomList rooms={rooms} currentRoom={currentRoom} onRoomSelect={handleRoomSelect} onNewRoom={() => setShowNewRoomModal(true)} isLoading={isLoading && rooms.length === 0} />
                </Card.Body>
              </Card>
            </Col>

            {/* Main Chat Area */}
            <Col lg={9} md={8} className="h-100 order-0 order-lg-1 d-flex flex-column">
              <Card className="flex-grow-1 d-flex flex-column" style={{overflow: 'hidden'}}>
                {currentRoom ? (
                  <>
                    <Card.Header className="py-2">
                      <div className="d-flex flex-row-reverse align-items-center justify-content-between">
                        <div className="d-flex align-items-center">
                          <div className="me-3">
                            {isSupportChat ? (
                              <div className="rounded-circle bg-warning text-white d-flex align-items-center justify-content-center" style={{width: '40px', height: '40px'}}>
                                <i className="bi bi-headset"></i>
                              </div>
                            ) : currentRoom.avatar ? (
                              <img src={currentRoom.avatar} alt={currentRoom.name} className="rounded-circle" style={{width: '40px', height: '40px'}} />
                            ) : (
                              <div className={`rounded-circle ${currentRoom.isGroup ? 'bg-success' : 'bg-primary'} text-white d-flex align-items-center justify-content-center`} style={{width: '40px', height: '40px'}}>
                                {currentRoom.isGroup ? <i className="bi bi-people-fill"></i> : currentRoom.name.charAt(0).toUpperCase()}
                              </div>
                            )}
                          </div>

                          <div>
                            <h6 className="mb-0">{isSupportChat ? `پشتیبانی - ${currentRoom.name}` : currentRoom.name}</h6>
                            {currentRoom.description && <small className="text-muted">{currentRoom.description}</small>}
                            {isFromSupport && ticketId && <small className="text-info d-block">تیکت #{ticketId}</small>}
                          </div>
                        </div>

                        <div>
                          {typingUsers[currentRoom.id] && typingUsers[currentRoom.id].filter((u) => u.userId !== currentLoggedInUserId).length > 0 && (
                            <TypingIndicatorComponent users={typingUsers[currentRoom.id].filter((u) => u.userId !== currentLoggedInUserId)} />
                          )}
                        </div>
                      </div>
                    </Card.Header>

                    <Card.Body className="p-0 d-flex flex-column flex-grow-1" style={{overflowY: 'hidden'}}>
                      <MessageList
                        messages={messages[currentRoom.id]?.items || []}
                        typingUsers={typingUsers[currentRoom.id] || []}
                        isLoading={isLoading}
                        hasMoreMessages={messages[currentRoom.id]?.hasMore || false}
                        onLoadMoreMessages={() => {
                          if (messages[currentRoom.id]?.hasMore) {
                            const nextPage = (messages[currentRoom.id]?.currentPage || 0) + 1;
                            handleLoadMessages(currentRoom.id, nextPage, 20, true);
                          }
                        }}
                        isGroupChat={isCurrentRoomGroupChat}
                      />
                      <MessageInput roomId={currentRoom.id} />
                    </Card.Body>
                  </>
                ) : (
                  // Empty state when no room is selected
                  <div className="h-100 d-flex align-items-center justify-content-center">
                    <div className="text-center text-muted">
                      <i className="bi bi-chat-square-text display-1 mb-3"></i>
                      <h4>چتی انتخاب نشده است</h4>
                      <p>برای شروع گفتگو، یک چت از لیست انتخاب کنید</p>
                    </div>
                  </div>
                )}
              </Card>
            </Col>
          </Row>
        )}
      </Container>

      {/* Forward Modal */}
      {isForwardModalVisible && <ForwardModal isVisible={isForwardModalVisible} onClose={hideForwardModal} rooms={rooms} />}
    </div>
  );
};

export default Chat;
