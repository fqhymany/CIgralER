import React, {useState, useEffect, useRef} from 'react';
import {Check2, Check2All, Clock, Reply, Pencil, Forward, Trash, EmojiSmile} from 'react-bootstrap-icons';
import {MessageDeliveryStatus} from '../../types/chat';
import {useChat} from '../../hooks/useChat';
import {getUserIdFromToken} from '../../utils/jwt';
import './Chat.css';

const EmojiReactionPicker = ({onSelect, onClose}) => {
  const emojis = ['👍', '❤️', '😂', '😮', '😢', '🙏'];
  const pickerRef = useRef(null);

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (pickerRef.current && !pickerRef.current.contains(event.target)) {
        onClose();
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [onClose]);

  return (
    <div ref={pickerRef} className="emoji-reaction-picker">
      {emojis.map((emoji) => (
        <span key={emoji} onClick={() => onSelect(emoji)}>
          {emoji}
        </span>
      ))}
    </div>
  );
};

const MessageItem = ({message, isGroupChat = false}) => {
  const {deleteMessage, setReplyingToMessage, setEditingMessage, setForwardingMessage, sendReaction} = useChat();
  const currentUserId = getUserIdFromToken(localStorage.getItem('token'));
  const isOwnMessage = message.senderId === currentUserId;

  const [contextMenu, setContextMenu] = useState({visible: false, x: 0, y: 0});
  const [showEmojiPicker, setShowEmojiPicker] = useState(false);
  const longPressTimer = useRef();

  const handleContextMenu = (e) => {
    e.preventDefault();
    setContextMenu({visible: true, x: e.clientX, y: e.clientY});
  };

  const handleTouchStart = (e) => {
    longPressTimer.current = setTimeout(() => {
      const touch = e.touches[0];
      setContextMenu({visible: true, x: touch.clientX, y: touch.clientY});
    }, 500); // 500ms for long press
  };

  const handleTouchEnd = () => {
    clearTimeout(longPressTimer.current);
  };

  const closeContextMenu = () => {
    setContextMenu({visible: false, x: 0, y: 0});
  };

  useEffect(() => {
    if (contextMenu.visible) {
      document.addEventListener('click', closeContextMenu);
      return () => document.removeEventListener('click', closeContextMenu);
    }
  }, [contextMenu.visible]);

  const handleAction = (action) => {
    closeContextMenu();
    action();
  };

  const handleReaction = (emoji) => {
    sendReaction(message.id, message.chatRoomId, emoji);
    setShowEmojiPicker(false);
    closeContextMenu();
  };

  const handleReply = () => {
    setReplyingToMessage(message);
  };

  const handleDelete = async () => {
    if (window.confirm('آیا از حذف این پیام اطمینان دارید؟')) {
      await deleteMessage(message.id, message.chatRoomId);
    }
  };

  const renderDeliveryStatus = () => {
    if (!isOwnMessage) return null;

    switch (message.deliveryStatus) {
      case MessageDeliveryStatus.Sent:
        return <Check2 size={18} />;
      case MessageDeliveryStatus.Delivered:
        return <Check2All size={18} />;
      case MessageDeliveryStatus.Read:
        return <Check2All size={18} className="text-primary" />;
      default:
        return <Clock size={14} />;
    }
  };

  const renderRepliedMessage = () => {
    if (!message.replyToMessageId || !message.repliedMessageContent) return null;

    // Simple display for replied message
    return (
      <div className="p-2 rounded mb-1" style={{background: 'rgba(0,0,0,0.05)', borderRight: '2px solid var(--primary-color)'}}>
        <div className="fw-bold text-primary" style={{fontSize: '0.85rem'}}>
          {message.repliedMessageSenderFullName || 'کاربر'}
        </div>
        <div className="text-muted text-truncate" style={{fontSize: '0.8rem'}}>
          {message.repliedMessageContent}
        </div>
      </div>
    );
  };

  const formatTime = (dateString) => {
    return new Date(dateString).toLocaleTimeString('fa-IR', {hour: '2-digit', minute: '2-digit'});
  };

  if (message.isDeleted) {
    return (
      <div className={`message-item-wrapper ${isOwnMessage ? 'sent' : 'received'}`}>
        <div className="message-bubble fst-italic text-muted">پیام حذف شد</div>
      </div>
    );
  }

  return (
    <div
      className={`message-item-wrapper ${isOwnMessage ? 'sent' : 'received'}`}
      onContextMenu={handleContextMenu}
      onTouchStart={handleTouchStart}
      onTouchEnd={handleTouchEnd}
      onTouchMove={handleTouchEnd} // Cancel long press if finger moves
    >
      <div className={`message-bubble ${isOwnMessage ? 'message-sent' : 'message-received'}`}>
        {!isOwnMessage && isGroupChat && (
          <div className="fw-bold text-primary mb-1" style={{fontSize: '0.85rem'}}>
            {message.senderFullName}
          </div>
        )}
        {renderRepliedMessage()}
        <div className="message-content">{message.content}</div>
        <div className="message-footer">
          <span className="message-time">{formatTime(message.createdAt)}</span>
          <span className="message-status">{renderDeliveryStatus()}</span>
        </div>
      </div>
      {contextMenu.visible && (
        <div
          className="custom-context-menu"
          style={{top: contextMenu.y, right: `calc(100vw - ${contextMenu.x}px)`}} // Adjusted for RTL
        >
          <ul>
            <li onClick={() => handleAction(() => setReplyingToMessage(message))}>
              <Reply /> پاسخ
            </li>
            <li onClick={() => handleAction(() => setForwardingMessage(message))}>
              <Forward /> هدایت
            </li>
            {isOwnMessage && (
              <li onClick={() => handleAction(() => setEditingMessage(message))}>
                <Pencil /> ویرایش
              </li>
            )}
            <li onMouseEnter={() => setShowEmojiPicker(true)} onMouseLeave={() => setShowEmojiPicker(false)}>
              <EmojiSmile /> واکنش
              {showEmojiPicker && <EmojiReactionPicker onSelect={handleReaction} onClose={() => setShowEmojiPicker(false)} />}
            </li>
            {isOwnMessage && (
              <li className="danger" onClick={() => handleAction(() => deleteMessage(message.id))}>
                <Trash /> حذف
              </li>
            )}
          </ul>
        </div>
      )}
    </div>
  );
};

export default MessageItem;
