import React, {useState, useRef, useEffect} from 'react';
import {Form, Button, OverlayTrigger, Popover} from 'react-bootstrap';
import {useChat} from '../../hooks/useChat';
import {MessageType} from '../../types/chat';
import {IoSend} from 'react-icons/io5';
import {BsEmojiSmile, BsPaperclip, BsMic, BsMicFill} from 'react-icons/bs';
import EmojiPicker, {EmojiStyle} from 'emoji-picker-react';
import './Chat.css';

const MessageInput = ({roomId}) => {
  const [message, setMessage] = useState('');
  const [isTyping, setIsTyping] = useState(false);
  const [showEmojiPicker, setShowEmojiPicker] = useState(false);
  const [isRecording, setIsRecording] = useState(false);
  const [recordingTime, setRecordingTime] = useState(0);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [isUploading, setIsUploading] = useState(false);

  const {sendMessage, startTyping, stopTyping, replyingToMessage, clearReplyingToMessage} = useChat();

  const fileInputRef = useRef(null);
  const textareaRef = useRef(null);
  const typingTimeoutRef = useRef(null);
  const mediaRecorderRef = useRef(null);
  const recordingIntervalRef = useRef(null);

  // Auto-resize textarea
  useEffect(() => {
    if (textareaRef.current) {
      textareaRef.current.style.height = 'auto';
      textareaRef.current.style.height = textareaRef.current.scrollHeight + 'px';
    }
  }, [message]);

  // Handle typing indicator
  const handleTyping = () => {
    if (!isTyping) {
      setIsTyping(true);
      startTyping(roomId);
    }

    if (typingTimeoutRef.current) {
      clearTimeout(typingTimeoutRef.current);
    }

    typingTimeoutRef.current = setTimeout(() => {
      setIsTyping(false);
      stopTyping(roomId);
    }, 1000);
  };

  // Cleanup
  useEffect(() => {
    return () => {
      if (typingTimeoutRef.current) {
        clearTimeout(typingTimeoutRef.current);
      }
      if (recordingIntervalRef.current) {
        clearInterval(recordingIntervalRef.current);
      }
      if (isTyping) {
        stopTyping(roomId);
      }
    };
  }, [roomId, isTyping, stopTyping]);

  const handleInputChange = (e) => {
    setMessage(e.target.value);
    handleTyping();
  };

  const handleKeyPress = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
    }
  };

  const handleSendMessage = async () => {
    if (!message.trim()) return;

    const messageContent = message.trim();
    setMessage('');

    try {
      await sendMessage(roomId, {
        content: messageContent,
        type: MessageType.Text,
        replyToMessageId: replyingToMessage?.id,
      });

      if (replyingToMessage) {
        clearReplyingToMessage();
      }
    } catch (error) {
      console.error('Error sending message:', error);
      setMessage(messageContent); // Restore message on error
    }
  };

  const onEmojiClick = (emojiObject) => {
    setMessage((prev) => prev + emojiObject.emoji);
    setShowEmojiPicker(false);
    textareaRef.current?.focus();
  };

  // File upload handler
  const handleFileSelect = async (e) => {
    const file = e.target.files[0];
    if (!file) return;

    setIsUploading(true);
    setUploadProgress(0);

    try {
      const formData = new FormData();
      formData.append('file', file);

      // Determine file type
      let fileType = MessageType.File;
      if (file.type.startsWith('image/')) fileType = MessageType.Image;
      else if (file.type.startsWith('audio/')) fileType = MessageType.Audio;
      else if (file.type.startsWith('video/')) fileType = MessageType.Video;

      formData.append('type', fileType.toString());

      // Upload file with progress
      const xhr = new XMLHttpRequest();

      xhr.upload.onprogress = (e) => {
        if (e.lengthComputable) {
          const progress = (e.loaded / e.total) * 100;
          setUploadProgress(progress);
        }
      };

      xhr.onload = async () => {
        if (xhr.status === 200) {
          const result = JSON.parse(xhr.responseText);

          // Send message with attachment
          await sendMessage(roomId, {
            content: file.name,
            type: fileType,
            attachmentUrl: result.fileUrl,
          });

          setIsUploading(false);
          setUploadProgress(0);

          // Reset file input
          if (fileInputRef.current) {
            fileInputRef.current.value = '';
          }
        } else {
          throw new Error('Upload failed');
        }
      };

      xhr.onerror = () => {
        throw new Error('Upload failed');
      };

      xhr.open('POST', '/api/chat/upload');
      xhr.setRequestHeader('Authorization', `Bearer ${localStorage.getItem('token')}`);
      xhr.send(formData);
    } catch (error) {
      console.error('Upload error:', error);
      setIsUploading(false);
      setUploadProgress(0);
      alert('خطا در آپلود فایل. لطفاً دوباره تلاش کنید.');
    }
  };

  // Voice recording handlers
  const startRecording = async () => {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({audio: true});
      const mediaRecorder = new MediaRecorder(stream);

      mediaRecorderRef.current = mediaRecorder;
      const audioChunks = [];

      mediaRecorder.ondataavailable = (event) => {
        audioChunks.push(event.data);
      };

      mediaRecorder.onstop = async () => {
        const audioBlob = new Blob(audioChunks, {type: 'audio/webm'});
        await uploadVoiceMessage(audioBlob);
        stream.getTracks().forEach((track) => track.stop());
      };

      mediaRecorder.start();
      setIsRecording(true);
      setRecordingTime(0);

      recordingIntervalRef.current = setInterval(() => {
        setRecordingTime((prev) => prev + 1);
      }, 1000);
    } catch (error) {
      console.error('Recording error:', error);
      alert('خطا در دسترسی به میکروفون');
    }
  };

  const stopRecording = () => {
    if (mediaRecorderRef.current && isRecording) {
      mediaRecorderRef.current.stop();
      setIsRecording(false);

      if (recordingIntervalRef.current) {
        clearInterval(recordingIntervalRef.current);
      }
    }
  };

  const uploadVoiceMessage = async (audioBlob) => {
    try {
      const formData = new FormData();
      formData.append('file', audioBlob, 'voice-message.webm');
      formData.append('type', MessageType.Audio.toString());

      const response = await fetch('/api/chat/upload', {
        method: 'POST',
        headers: {
          Authorization: `Bearer ${localStorage.getItem('token')}`,
        },
        body: formData,
      });

      if (!response.ok) throw new Error('Upload failed');

      const result = await response.json();

      await sendMessage(roomId, {
        content: 'پیام صوتی',
        type: MessageType.Audio,
        attachmentUrl: result.fileUrl,
      });
    } catch (error) {
      console.error('Voice upload error:', error);
      alert('خطا در ارسال پیام صوتی');
    }
  };

  const formatRecordingTime = (seconds) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  // Emoji picker popover
  const emojiPickerPopover = (
    <Popover id="emoji-picker-popover" style={{maxWidth: 'none'}}>
      <Popover.Body className="p-0">
        <EmojiPicker onEmojiClick={onEmojiClick} emojiStyle={EmojiStyle.NATIVE} />
      </Popover.Body>
    </Popover>
  );

  return (
    <div className="chat-input-container">
      {/* Reply indicator */}
      {replyingToMessage && (
        <div className="reply-indicator bg-light p-2 border-top d-flex justify-content-between align-items-center">
          <div>
            <small className="text-muted">پاسخ به:</small>
            <div className="fw-bold">{replyingToMessage.senderFullName}</div>
            <div className="text-truncate">{replyingToMessage.content}</div>
          </div>
          <Button variant="link" size="sm" onClick={clearReplyingToMessage} className="p-1">
            <i className="bi bi-x"></i>
          </Button>
        </div>
      )}

      {/* Upload progress */}
      {isUploading && (
        <div className="upload-progress-container p-2 bg-light">
          <div className="d-flex justify-content-between align-items-center mb-1">
            <small>در حال آپلود...</small>
            <small>{Math.round(uploadProgress)}%</small>
          </div>
          <div className="progress" style={{height: '4px'}}>
            <div className="progress-bar" role="progressbar" style={{width: `${uploadProgress}%`}}></div>
          </div>
        </div>
      )}

      {/* Recording indicator */}
      {isRecording && (
        <div className="recording-indicator bg-danger text-white p-2 d-flex justify-content-between align-items-center">
          <div className="d-flex align-items-center">
            <BsMicFill className="me-2 recording-pulse" />
            <span>در حال ضبط...</span>
          </div>
          <span>{formatRecordingTime(recordingTime)}</span>
        </div>
      )}

      {/* Input area */}
      <div className="chat-input-wrapper">
        {/* File upload, voice, and send buttons */}
        <div className="attachment-buttons">
          {/* اگر پیام وجود دارد فقط دکمه ارسال */}
          {message.trim() ? (
            <Button variant="primary" size="sm" className="chat-send-button" onClick={handleSendMessage} disabled={isUploading || isRecording}>
              <IoSend size={20} />
            </Button>
          ) : (
            // اگر پیام خالی است، دکمه‌های فایل و ضبط صدا
            <>
              <input ref={fileInputRef} type="file" className="d-none" onChange={handleFileSelect} accept="image/*,audio/*,video/*,.pdf,.doc,.docx,.xls,.xlsx,.txt,.zip,.rar" disabled={isUploading || isRecording} />
              <button className="attachment-button" onClick={() => fileInputRef.current?.click()} disabled={isUploading || isRecording} title="ضمیمه فایل">
                <BsPaperclip size={20} />
              </button>
              <button className={`attachment-button ${isRecording ? 'recording-indicator' : ''}`} onClick={isRecording ? stopRecording : startRecording} disabled={isUploading} title={isRecording ? 'توقف ضبط' : 'ضبط صدا'}>
                {isRecording ? <BsMicFill size={20} color="#dc3545" /> : <BsMic size={20} />}
              </button>
            </>
          )}
        </div>

        {/* Text input */}
        <div className="flex-grow-1">
          <Form.Control
            ref={textareaRef}
            as="textarea"
            rows="1"
            placeholder={isRecording ? 'در حال ضبط...' : 'پیام'}
            value={message}
            onChange={handleInputChange}
            onKeyDown={handleKeyPress}
            disabled={isUploading || isRecording}
            style={{
              resize: 'none',
              minHeight: '40px',
              maxHeight: '120px',
              border: 'none',
              boxShadow: 'none',
              outline: 'none',
              background: 'transparent',
            }}
            className="chat-input"
          />
        </div>

        {/* Emoji picker button */}
        <OverlayTrigger trigger="click" placement="top" overlay={emojiPickerPopover} rootClose>
          <button className="attachment-button" disabled={isUploading || isRecording} title="افزودن ایموجی">
            <BsEmojiSmile size={20} />
          </button>
        </OverlayTrigger>
      </div>
    </div>
  );
};

export default MessageInput;
