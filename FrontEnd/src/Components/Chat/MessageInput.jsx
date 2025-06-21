import React, {useState, useRef, useEffect} from 'react';
import {Form, Button, Spinner, CloseButton, OverlayTrigger, Popover} from 'react-bootstrap';
import {EmojiSmile, Paperclip, Mic, Send} from 'react-bootstrap-icons';
import {useChat} from '../../hooks/useChat';
import {MessageType} from '../../types/chat';
import './Chat.css';
import {IoSend, IoCheckmark, IoClose} from 'react-icons/io5';
import {BsEmojiSmile, BsPaperclip, BsMic, BsMicFill, BsReply as Reply, BsPencil as Pencil, BsForward as Forward} from 'react-icons/bs';
import EmojiPicker, {EmojiStyle} from 'emoji-picker-react';
import './Chat.css';

const MessageInput = ({roomId}) => {
  const [message, setMessage] = useState('');
  const {sendMessage, startTyping, stopTyping, replyingToMessage, clearReplyingToMessage, editingMessage, clearEditingMessage, editMessage, forwardingMessage, clearForwardingMessage, forwardMessage} = useChat();
  const [isSending, setIsSending] = useState(false);
  const textareaRef = useRef(null);
  const typingTimeoutRef = useRef(null);
  const [showEmojiPicker, setShowEmojiPicker] = useState(false);
  const [isRecording, setIsRecording] = useState(false);
  const [recordingTime, setRecordingTime] = useState(0);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [isUploading, setIsUploading] = useState(false);
  const fileInputRef = useRef(null);
  const mediaRecorderRef = useRef(null);
  const recordingIntervalRef = useRef(null);

  useEffect(() => {
    if (textareaRef.current) {
      textareaRef.current.style.height = 'auto';
      textareaRef.current.style.height = `${textareaRef.current.scrollHeight}px`;
    }
  }, [message]);

  useEffect(() => {
    if (replyingToMessage) {
      textareaRef.current?.focus();
    }
  }, [replyingToMessage]);

  const isEditing = !!editingMessage;
  const isReplying = !!replyingToMessage;
  const isForwarding = !!forwardingMessage;

  useEffect(() => {
    if (isEditing) {
      setMessage(editingMessage.content);
      textareaRef.current?.focus();
    } else if (isReplying || isForwarding) {
      textareaRef.current?.focus();
    }
  }, [isEditing, editingMessage, isReplying, isForwarding]);

  const handleCancelAction = () => {
    if (isEditing) clearEditingMessage();
    if (isReplying) clearReplyingToMessage();
    if (isForwarding) clearForwardingMessage();
    setMessage('');
  };

  const handleSubmit = async () => {
    const content = message.trim();
    if (!content && !isForwarding) return;
    if (isSending) return;

    setIsSending(true);

    try {
      if (isEditing) {
        await editMessage(editingMessage.id, content);
      } else if (isReplying) {
        await sendMessage(roomId, {content, type: MessageType.Text, replyToMessageId: replyingToMessage.id});
      } else if (isForwarding) {
        // برای هدایت، محتوای جدیدی ارسال نمی‌شود
        await forwardMessage(forwardingMessage.id, roomId);
      } else {
        await sendMessage(roomId, {content, type: MessageType.Text});
      }

      handleCancelAction(); // پاک کردن همه حالت‌ها
      setMessage('');
    } catch (error) {
      console.error('Action failed:', error);
      // پیام را در صورت خطا بازگردان
      if (!isForwarding) setMessage(content);
    } finally {
      setIsSending(false);
      textareaRef.current?.focus();
    }
  };

  const handleTyping = () => {
    if (!typingTimeoutRef.current) {
      startTyping(roomId);
    } else {
      clearTimeout(typingTimeoutRef.current);
    }
    typingTimeoutRef.current = setTimeout(() => {
      stopTyping(roomId);
      typingTimeoutRef.current = null;
    }, 1500);
  };

  const handleSendMessage = async () => {
    const content = message.trim();
    if (!content || isSending) return;

    setIsSending(true);
    setMessage('');

    try {
      await sendMessage(roomId, {
        content,
        type: MessageType.Text,
        replyToMessageId: replyingToMessage?.id,
      });
      clearReplyingToMessage();
    } catch (error) {
      console.error('Failed to send message:', error);
      setMessage(content); // Restore message on failure
    } finally {
      setIsSending(false);
      textareaRef.current?.focus();
    }
  };

  const handleKeyPress = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
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
    <Popover id="emoji-picker-popover" style={{maxWidth: 'none', direction: 'ltr'}}>
      <Popover.Body className="p-0">
        <EmojiPicker onEmojiClick={onEmojiClick} emojiStyle={EmojiStyle.NATIVE} />
      </Popover.Body>
    </Popover>
  );

  const renderActionPreview = () => {
    if (isEditing) {
      return (
        <div className="action-preview">
          <Pencil className="me-2" />
          <div>
            <div className="fw-bold text-primary">ویرایش پیام</div>
            <div className="preview-content text-muted">{editingMessage.content}</div>
          </div>
          <CloseButton onClick={handleCancelAction} />
        </div>
      );
    }
    if (isReplying) {
      return (
        <div className="action-preview">
          <Reply className="me-2" />
          <div>
            <div className="fw-bold text-primary">پاسخ به: {replyingToMessage.senderFullName}</div>
            <div className="preview-content text-muted">{replyingToMessage.content}</div>
          </div>
          <CloseButton onClick={handleCancelAction} />
        </div>
      );
    }
    if (isForwarding) {
      return (
        <div className="action-preview">
          <Forward className="me-2" />
          <div>
            <div className="fw-bold text-primary">هدایت پیام</div>
            <div className="preview-content text-muted">از: {forwardingMessage.senderFullName}</div>
          </div>
          <CloseButton onClick={handleCancelAction} />
        </div>
      );
    }
    return null;
  };

  return (
    <div className="message-input-container p-2">
      {renderActionPreview()}
      {replyingToMessage && (
        <div className="reply-preview d-flex justify-content-between align-items-center">
          <div>
            <div className="fw-bold text-primary" style={{fontSize: '0.8rem'}}>
              پاسخ به: {replyingToMessage.senderFullName}
            </div>
            <div className="reply-preview-content text-muted">{replyingToMessage.content}</div>
          </div>
          <CloseButton onClick={clearReplyingToMessage} />
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

      <div className="message-input-wrapper gap-1">
        <div className="message-input-actions d-flex flex-grow-1 gap-1">
          <div>
            {/* Emoji picker button */}
            <OverlayTrigger trigger="click" placement="top" overlay={emojiPickerPopover} rootClose>
              <Button variant="link" className="attachment-button p-1" disabled={isUploading || isRecording} title="افزودن ایموجی">
                <BsEmojiSmile size={20} />
              </Button>
            </OverlayTrigger>
          </div>
          <div className="flex-grow-1">
            <Form.Control
              ref={textareaRef}
              as="textarea"
              rows={1}
              placeholder={isForwarding ? 'برای ارسال پیام هدایت شده، دکمه ارسال را بزنید' : 'پیام...'}
              value={message}
              onChange={(e) => {
                setMessage(e.target.value);
                handleTyping();
              }}
              onKeyDown={handleKeyPress}
              disabled={isSending || isForwarding}
              className="message-input-field"
            />
          </div>
          <div className="d-flex gap-0">
            {message.trim() ? (
              <Button variant="link" onClick={handleSendMessage} disabled={!message.trim() || isSending} className="send-button p-1">
                {isSending ? <Spinner size="sm" /> : <IoSend size={20} className="icon_flip" />}
              </Button>
            ) : (
              <>
                <input ref={fileInputRef} type="file" className="d-none p-1" onChange={handleFileSelect} accept="image/*,audio/*,video/*,.pdf,.doc,.docx,.xls,.xlsx,.txt,.zip,.rar" disabled={isUploading || isRecording} />
                <Button variant="link" className="attachment-button p-1" onClick={() => fileInputRef.current?.click()} disabled={isUploading || isRecording} title="ضمیمه فایل">
                  <Paperclip size={22} />
                </Button>
                <Button
                  variant="link"
                  className={`attachment-button p-1 ${isRecording ? 'recording-indicator p-1' : 'p-1'}`}
                  onClick={isRecording ? stopRecording : startRecording}
                  disabled={isUploading}
                  title={isRecording ? 'توقف ضبط' : 'ضبط صدا'}
                >
                  {isRecording ? <BsMicFill size={20} color="#dc3545" /> : <Mic size={22} />}
                </Button>
              </>
            )}
          </div>
          <div className="d-flex gap-0">
            <Button variant="link" onClick={handleSubmit} disabled={isSending || (!message.trim() && !isForwarding)} className="send-button p-1">
              {isSending ? <Spinner size="sm" /> : isEditing ? <IoCheckmark size={24} /> : <IoSend size={20} className="icon_flip" />}
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default MessageInput;
