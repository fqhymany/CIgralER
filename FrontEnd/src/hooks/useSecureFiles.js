// src/hooks/useSecureFiles.js
import {useState} from 'react';
import {useQuery, useMutation, useQueryClient} from '@tanstack/react-query';
import secureFileApi from '../api/secureFileApi';
import {useAuth} from '../contexts/AuthContext';

/**
 * هوک سفارشی برای مدیریت فایل‌های امن
 *
 * @param {string} caseId - شناسه پرونده
 * @param {string} area - ناحیه
 * @param {object} options - گزینه‌های اضافی
 * @returns {object} - توابع و داده‌های مربوط به مدیریت فایل
 */
const useSecureFiles = (caseId, area, options = {}) => {
  const {refreshSession} = useAuth();
  const queryClient = useQueryClient();
  const [selectedFile, setSelectedFile] = useState(null);
  const [uploadModalOpen, setUploadModalOpen] = useState(false);
  const [previewModalOpen, setPreviewModalOpen] = useState(false);

  // کوئری دریافت فایل‌ها
  const filesQuery = useQuery({
    queryKey: ['case-files', caseId, area],
    queryFn: () => secureFileApi.getCaseFiles(caseId, area),
    enabled: !!caseId && !!area,
    ...options.queryOptions,
  });

  // میوتیشن آپلود فایل
  const uploadMutation = useMutation({
    mutationFn: ({file, customCaseId, customArea}) => secureFileApi.uploadFile(file, customCaseId || caseId, customArea || area),
    onSuccess: () => {
      refreshSession();
      queryClient.invalidateQueries(['case-files', caseId, area]);
      if (options.onUploadSuccess) {
        options.onUploadSuccess();
      }
    },
    ...options.uploadOptions,
  });

  // میوتیشن حذف فایل
  const deleteMutation = useMutation({
    mutationFn: (fileId) => secureFileApi.deleteFile(fileId),
    onSuccess: () => {
      refreshSession();
      queryClient.invalidateQueries(['case-files', caseId, area]);
      if (options.onDeleteSuccess) {
        options.onDeleteSuccess();
      }
    },
    ...options.deleteOptions,
  });

  // گرفتن لینک دانلود امن
  const getSecureLink = async (fileId, expirationMinutes = 30) => {
    refreshSession();
    try {
      const result = await secureFileApi.generateSecureAccessUrl(fileId, expirationMinutes);
      return `${window.location.origin}/api/files/access/${result.accessToken}`;
    } catch (error) {
      console.error('Error generating secure link:', error);
      throw error;
    }
  };

  // دانلود فایل
  const downloadFile = async (fileId, fileName) => {
    refreshSession();
    try {
      const blob = await secureFileApi.downloadFileAsBlob(fileId);

      // ایجاد لینک دانلود
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', fileName || 'file');
      document.body.appendChild(link);
      link.click();

      // پاکسازی
      setTimeout(() => {
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
      }, 100);

      return true;
    } catch (error) {
      console.error('Error downloading file:', error);
      throw error;
    }
  };

  // باز کردن مودال آپلود
  const openUploadModal = () => {
    refreshSession();
    setUploadModalOpen(true);
  };

  // بستن مودال آپلود
  const closeUploadModal = () => {
    setUploadModalOpen(false);
  };

  // باز کردن پیش‌نمایش فایل
  const previewFile = (file) => {
    refreshSession();
    setSelectedFile(file);
    setPreviewModalOpen(true);
  };

  // بستن پیش‌نمایش فایل
  const closePreview = () => {
    setPreviewModalOpen(false);
    setSelectedFile(null);
  };

  // فیلتر کردن فایل‌ها بر اساس نوع
  const filterFilesByType = (type) => {
    if (!filesQuery.data) return [];

    switch (type) {
      case 'images':
        return filesQuery.data.filter((file) => {
          const ext = file.fileName.split('.').pop().toLowerCase();
          return ['jpg', 'jpeg', 'png', 'gif', 'webp', 'svg', 'bmp'].includes(ext);
        });

      case 'documents':
        return filesQuery.data.filter((file) => {
          const ext = file.fileName.split('.').pop().toLowerCase();
          return ['pdf', 'doc', 'docx', 'txt', 'rtf', 'odt'].includes(ext);
        });

      case 'spreadsheets':
        return filesQuery.data.filter((file) => {
          const ext = file.fileName.split('.').pop().toLowerCase();
          return ['xls', 'xlsx', 'csv', 'ods'].includes(ext);
        });

      case 'presentations':
        return filesQuery.data.filter((file) => {
          const ext = file.fileName.split('.').pop().toLowerCase();
          return ['ppt', 'pptx', 'odp'].includes(ext);
        });

      case 'archives':
        return filesQuery.data.filter((file) => {
          const ext = file.fileName.split('.').pop().toLowerCase();
          return ['zip', 'rar', '7z', 'tar', 'gz'].includes(ext);
        });

      default:
        return filesQuery.data;
    }
  };

  return {
    // داده‌ها
    files: filesQuery.data || [],
    isLoading: filesQuery.isLoading,
    isError: filesQuery.isError,
    error: filesQuery.error,
    selectedFile,
    uploadModalOpen,
    previewModalOpen,

    // توابع آپلود
    uploadFile: (file) => uploadMutation.mutate({file}),
    uploadCustomFile: (file, customCaseId, customArea) => uploadMutation.mutate({file, customCaseId, customArea}),
    isUploading: uploadMutation.isLoading,
    uploadError: uploadMutation.error,

    // توابع حذف
    deleteFile: (fileId) => deleteMutation.mutate(fileId),
    isDeleting: deleteMutation.isLoading,
    deleteError: deleteMutation.error,

    // توابع دانلود
    downloadFile,
    getSecureLink,
    getImageUrl: (fileId) => secureFileApi.getImageUrl(fileId),

    // توابع مودال
    openUploadModal,
    closeUploadModal,
    previewFile,
    closePreview,

    // توابع کمکی
    filterFilesByType,
    refreshFiles: () => filesQuery.refetch(),
  };
};

export default useSecureFiles;
