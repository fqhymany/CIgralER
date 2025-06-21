import React, {useState, useEffect, useCallback} from 'react';
import {useParams, useNavigate, useLocation} from 'react-router-dom';
import {Container, Row, Col, Spinner, Alert, Button} from 'react-bootstrap';
import {ArrowRight, ChatSquareText} from 'react-bootstrap-icons';
import {useChat} from '../../hooks/useChat';
import ChatRoomList from './ChatRoomList';
import MessageList from './MessageList';
import MessageInput from './MessageInput';
import TypingIndicatorComponent from './TypingIndicatorComponent';
import ConnectionStatus from './ConnectionStatus';
import ForwardModal from './ForwardModal';
import NewRoomModal from './NewRoomModal';
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
    clearError,
    currentLoggedInUserId,
    isForwardModalVisible,
    hideForwardModal,
    setCurrentRoom,
    loadMessages,
    loadRooms,
    joinRoom,
    leaveRoom,
    markAllMessagesAsReadInRoom,
    forwardingMessage,
    clearForwardingMessage,
    forwardMessage,
  } = useChat();

  const [showNewRoomModal, setShowNewRoomModal] = useState(false);
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 768);
  const [hasTriedLoadingRooms, setHasTriedLoadingRooms] = useState(false);

  const isSupportChat = currentRoom?.chatRoomType === 2;
  const urlParams = new URLSearchParams(location.search);
  const isFromSupport = urlParams.get('support') === 'true';
  const ticketId = urlParams.get('ticketId');

  const handleMobileBack = useCallback(() => {
    setCurrentRoom(null);
    loadRooms(); // لیست چت‌ها را دوباره بارگذاری کن
    navigate('/Chat', {replace: true});
  }, [navigate, setCurrentRoom, loadRooms]);

  useEffect(() => {
    const handleResize = () => setIsMobile(window.innerWidth <= 768);
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  useEffect(() => {
    if (!rooms.length && !isLoading && !hasTriedLoadingRooms) {
      loadRooms();
      setHasTriedLoadingRooms(true);
    }
  }, [loadRooms, rooms.length, isLoading, hasTriedLoadingRooms]);

  useEffect(() => {
    if (roomId && rooms.length > 0) {
      const roomToSelect = rooms.find((r) => r.id === parseInt(roomId, 10));
      if (roomToSelect && (!currentRoom || currentRoom.id !== roomToSelect.id)) {
        handleRoomSelect(roomToSelect);
      }
    }
  }, [roomId, rooms]);

  useEffect(() => {
    if (rooms.length > 0) {
      setHasTriedLoadingRooms(false);
    }
  }, [rooms.length]);

  const handleRoomSelect = async (room) => {
    if (currentRoom?.id === room.id) return;

    if (forwardingMessage) {
      try {
        await forwardMessage(forwardingMessage.id, room.id);
        clearForwardingMessage();
      } catch (err) {
        console.error('Failed to forward message on room select:', err);
        clearForwardingMessage();
        return;
      }
    }

    try {
      if (currentRoom?.id) {
        await leaveRoom(currentRoom.id);
      }
      setCurrentRoom(room);
      await joinRoom(room.id);
      await loadMessages(room.id, 1, 20, false);
      markAllMessagesAsReadInRoom(room.id);
      navigate(`/chat/${room.id}`, {replace: true});
    } catch (err) {
      console.error('Error selecting room:', err);
    }
  };

  const handleLoadMessages = async (roomId, page = 1, pageSize = 20, isLoadingMore = false) => {
    try {
      await loadMessages(roomId, page, pageSize, isLoadingMore);
    } catch (err) {
      console.error('Error loading messages:', err);
    }
  };

  const handleNewRoomCreated = (newRoom) => {
    setShowNewRoomModal(false);
    handleRoomSelect(newRoom);
  };

  if (isLoading && !rooms.length) {
    return (
      <Container fluid className="d-flex h-100 align-items-center justify-content-center">
        <Spinner animation="border" variant="primary" />
      </Container>
    );
  }

  if (error && !rooms.length) {
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

  const renderSidebar = () => <ChatRoomList rooms={rooms} currentRoom={currentRoom} onRoomSelect={handleRoomSelect} onNewRoom={() => setShowNewRoomModal(true)} isLoading={isLoading && !rooms.length} />;

  const renderChatPanelHeader = () => (
    <div className="chat-panel-header">
      {isMobile && (
        <Button variant="link" className="text-secondary p-0 me-3" onClick={handleMobileBack}>
          <ArrowRight size={22} />
        </Button>
      )}
      <div className="d-flex align-items-center me-3">
        {isSupportChat ? (
          <div className="avatar-placeholder bg-warning">
            <i className="bi bi-headset fs-5"></i>
          </div>
        ) : currentRoom.avatar ? (
          <img src={currentRoom.avatar} alt={currentRoom.name} className="avatar" />
        ) : (
          <div className={`avatar-placeholder ${currentRoom.isGroup ? 'bg-success' : 'bg-primary'}`}>{currentRoom.isGroup ? <i className="bi bi-people-fill fs-5"></i> : currentRoom.name.charAt(0).toUpperCase()}</div>
        )}
      </div>
      <div className="flex-grow-1 min-width-0">
        <h6 className="mb-0 text-truncate">{isSupportChat ? `پشتیبانی - ${currentRoom.name}` : currentRoom.name}</h6>
        {typingUsers[currentRoom.id]?.filter((u) => u.userId !== currentLoggedInUserId).length > 0 ? (
          <TypingIndicatorComponent users={typingUsers[currentRoom.id].filter((u) => u.userId !== currentLoggedInUserId)} />
        ) : (
          currentRoom.description && <small className="text-muted text-truncate d-block">{currentRoom.description}</small>
        )}
      </div>
      <div className="ms-auto">
        <ConnectionStatus isConnected={isConnected} />
      </div>
    </div>
  );

  const renderChatPanelContent = () => (
    <div className={`chat-panel-content ${isMobile && currentRoom ? 'd-flex flex-column' : ''}`}>
      <MessageList
        messages={messages[currentRoom.id]?.items || []}
        isLoading={isLoading}
        hasMoreMessages={messages[currentRoom.id]?.hasMore || false}
        onLoadMoreMessages={() => {
          if (messages[currentRoom.id]?.hasMore) {
            const nextPage = (messages[currentRoom.id]?.currentPage || 0) + 1;
            handleLoadMessages(currentRoom.id, nextPage, 20, true);
          }
        }}
        isGroupChat={currentRoom?.isGroup}
      />
      <MessageInput roomId={currentRoom.id} />
    </div>
  );

  return (
    <div className="chat-app-container">
      {forwardingMessage && (
        <div className="forwarding-banner">
          <p>
            پیام در حال هدایت است. یک چت را برای ارسال انتخاب کنید.
            <Button variant="link" className="text-white p-1 ms-2" onClick={clearForwardingMessage}>
              لغو
            </Button>
          </p>
        </div>
      )}

      <div className={`chat-main-layout ${currentRoom ? 'is-room-selected' : ''}`}>
        <aside className={`chat-sidebar ${!currentRoom || !isMobile ? 'is-active' : ''}`}>{renderSidebar()}</aside>

        <main className="chat-panel">
          {currentRoom ? (
            <>
              {renderChatPanelHeader()}
              {renderChatPanelContent()}
            </>
          ) : (
            !isMobile && (
              <div className="chat-empty-state">
                <ChatSquareText size={64} className="mb-3" />
                <h4>چتی انتخاب نشده است</h4>
                <p>برای شروع گفتگو، یک چت از لیست انتخاب کنید.</p>
              </div>
            )
          )}
        </main>
      </div>

      <NewRoomModal show={showNewRoomModal} onHide={() => setShowNewRoomModal(false)} onRoomCreated={handleNewRoomCreated} />
      {isForwardModalVisible && <ForwardModal isVisible={isForwardModalVisible} onClose={hideForwardModal} rooms={rooms} />}

      {error && (
        <div className="position-fixed bottom-0 end-0 p-3" style={{zIndex: 1050}}>
          <Alert variant="danger" onClose={clearError} dismissible>
            {error}
          </Alert>
        </div>
      )}
    </div>
  );
};

export default Chat;
