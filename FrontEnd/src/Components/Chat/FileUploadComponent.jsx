import React, { useState, useRef } from 'react';
import { Paperclip, X, Upload, FileText, Image, Music, Film, Download } from 'lucide-react';
import fileUploadService from '../services/fileUploadService';
import { formatFileSize, getFileIcon } from '../utils/messageHelpers';

const FileUploadComponent = ({ onFileUploaded, disabled }) => {
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [selectedFile, setSelectedFile] = useState(null);
  const [previewUrl, setPreviewUrl] = useState(null);
  const fileInputRef = useRef(null);

  const handleFileSelect = async (e) => {
    const file = e.target.files[0];
    if (!file) return;

    setSelectedFile(file);
    
    // Create preview for images
    if (file.type.startsWith('image/')) {
      const reader = new FileReader();
      reader.onloadend = () => {
        setPreviewUrl(reader.result);
      };
      reader.readAsDataURL(file);
    }
  };

  const uploadFile = async () => {
    if (!selectedFile) return;

    setIsUploading(true);
    setUploadProgress(0);

    try {
      // Determine file type
      let fileType = 'file';
      if (selectedFile.type.startsWith('image/')) fileType = 'image';
      else if (selectedFile.type.startsWith('audio/')) fileType = 'audio';
      else if (selectedFile.type.startsWith('video/')) fileType = 'video';

      // Validate file
      fileUploadService.validateFile(selectedFile, fileType);

      // Create FormData with progress tracking
      const formData = new FormData();
      formData.append('file', selectedFile);
      formData.append('type', fileUploadService.getMessageType(fileType));

      const xhr = new XMLHttpRequest();
      
      // Track upload progress
      xhr.upload.addEventListener('progress', (e) => {
        if (e.lengthComputable) {
          const percentComplete = Math.round((e.loaded / e.total) * 100);
          setUploadProgress(percentComplete);
        }
      });

      // Handle completion
      xhr.addEventListener('load', () => {
        if (xhr.status === 200) {
          const result = JSON.parse(xhr.responseText);
          onFileUploaded({
            url: result.fileUrl,
            name: selectedFile.name,
            size: selectedFile.size,
            type: fileType,
            mimeType: selectedFile.type
          });
          resetUpload();
        } else {
          throw new Error('Upload failed');
        }
      });

      xhr.addEventListener('error', () => {
        throw new Error('Upload failed');
      });

      // Send request
      xhr.open('POST', '/api/chat/upload');
      xhr.setRequestHeader('Authorization', `Bearer ${localStorage.getItem('token')}`);
      xhr.send(formData);

    } catch (error) {
      console.error('Upload error:', error);
      alert(error.message || 'Failed to upload file');
      setIsUploading(false);
    }
  };

  const resetUpload = () => {
    setSelectedFile(null);
    setPreviewUrl(null);
    setIsUploading(false);
    setUploadProgress(0);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const getFileTypeIcon = () => {
    if (!selectedFile) return <FileText className="w-8 h-8" />;
    
    if (selectedFile.type.startsWith('image/')) return <Image className="w-8 h-8" />;
    if (selectedFile.type.startsWith('audio/')) return <Music className="w-8 h-8" />;
    if (selectedFile.type.startsWith('video/')) return <Film className="w-8 h-8" />;
    return <FileText className="w-8 h-8" />;
  };

  return (
    <div className="relative">
      <input
        ref={fileInputRef}
        type="file"
        onChange={handleFileSelect}
        className="hidden"
        accept="image/*,audio/*,video/*,.pdf,.doc,.docx,.xls,.xlsx,.txt,.zip,.rar"
        disabled={disabled || isUploading}
      />
      
      {!selectedFile ? (
        <button
          onClick={() => fileInputRef.current?.click()}
          disabled={disabled}
          className="p-2 hover:bg-gray-100 rounded-lg transition disabled:opacity-50"
          title="Attach file"
        >
          <Paperclip className="w-5 h-5 text-gray-500" />
        </button>
      ) : (
        <div className="absolute bottom-12 left-0 bg-white shadow-lg rounded-lg p-4 w-80 border">
          <div className="flex items-start gap-3">
            {previewUrl ? (
              <img src={previewUrl} alt="Preview" className="w-16 h-16 rounded object-cover" />
            ) : (
              <div className="w-16 h-16 bg-gray-100 rounded flex items-center justify-center text-gray-400">
                {getFileTypeIcon()}
              </div>
            )}
            
            <div className="flex-1">
              <p className="font-medium text-sm truncate">{selectedFile.name}</p>
              <p className="text-xs text-gray-500">{formatFileSize(selectedFile.size)}</p>
              
              {isUploading && (
                <div className="mt-2">
                  <div className="w-full bg-gray-200 rounded-full h-1.5">
                    <div 
                      className="bg-blue-500 h-1.5 rounded-full transition-all duration-300"
                      style={{ width: `${uploadProgress}%` }}
                    />
                  </div>
                  <p className="text-xs text-gray-500 mt-1">{uploadProgress}%</p>
                </div>
              )}
            </div>
            
            {!isUploading && (
              <button
                onClick={resetUpload}
                className="p-1 hover:bg-gray-100 rounded"
              >
                <X className="w-4 h-4 text-gray-500" />
              </button>
            )}
          </div>
          
          {!isUploading && (
            <div className="flex gap-2 mt-3">
              <button
                onClick={uploadFile}
                className="flex-1 bg-blue-500 text-white py-2 rounded-lg hover:bg-blue-600 transition flex items-center justify-center gap-2"
              >
                <Upload className="w-4 h-4" />
                Upload
              </button>
              <button
                onClick={resetUpload}
                className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition"
              >
                Cancel
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

// کامپوننت نمایش فایل در پیام
export const FileMessageDisplay = ({ message }) => {
  const isImage = message.type === 1;
  const isAudio = message.type === 3;
  const isVideo = message.type === 4;
  const isFile = message.type === 2;

  const handleDownload = () => {
    const link = document.createElement('a');
    link.href = message.attachmentUrl;
    link.download = message.content || 'download';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  if (isImage) {
    return (
      <div className="max-w-xs">
        <img 
          src={message.attachmentUrl} 
          alt={message.content}
          className="rounded-lg max-w-full cursor-pointer hover:opacity-90 transition"
          onClick={() => window.open(message.attachmentUrl, '_blank')}
        />
        {message.content && (
          <p className="text-sm mt-1">{message.content}</p>
        )}
      </div>
    );
  }

  if (isAudio) {
    return (
      <div className="bg-gray-100 rounded-lg p-3 min-w-[250px]">
        <audio 
          controls 
          className="w-full"
          preload="metadata"
        >
          <source src={message.attachmentUrl} type="audio/webm" />
          <source src={message.attachmentUrl} type="audio/mpeg" />
          Your browser does not support the audio element.
        </audio>
        {message.content && message.content !== 'Voice message' && (
          <p className="text-sm mt-1">{message.content}</p>
        )}
      </div>
    );
  }

  if (isVideo) {
    return (
      <div className="max-w-sm">
        <video 
          controls 
          className="rounded-lg max-w-full"
          preload="metadata"
        >
          <source src={message.attachmentUrl} type="video/mp4" />
          <source src={message.attachmentUrl} type="video/webm" />
          Your browser does not support the video element.
        </video>
        {message.content && (
          <p className="text-sm mt-1">{message.content}</p>
        )}
      </div>
    );
  }

  if (isFile) {
    return (
      <div 
        className="bg-gray-100 rounded-lg p-4 flex items-center gap-3 cursor-pointer hover:bg-gray-200 transition"
        onClick={handleDownload}
      >
        <div className="text-3xl">
          {getFileIcon(message.attachmentUrl)}
        </div>
        <div className="flex-1">
          <p className="font-medium text-sm">{message.content}</p>
          <p className="text-xs text-gray-500">Click to download</p>
        </div>
        <Download className="w-5 h-5 text-gray-500" />
      </div>
    );
  }

  return null;
};

export default FileUploadComponent;