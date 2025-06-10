import api from './axios';

const secureFileApi = {
  /**
   * آپلود فایل با رمزنگاری
   * @param {File} file - فایل برای آپلود
   * @param {string} caseId - شناسه پرونده
   * @param {string} area - ناحیه
   * @returns {Promise<Object>} - اطلاعات فایل آپلود شده
   */
  uploadFile: async (file, caseId, area, customName) => {
    if (!caseId) {
      console.error('caseId is missing');
      throw new Error('شناسه پرونده اجباری است');
    }

    let guidCaseId;
    if (typeof caseId === 'number' || !caseId.includes('-')) {
      // ساخت یک Guid از عدد (روش ساده)
      guidCaseId = `00000000-0000-0000-0000-${caseId.toString().padStart(12, '0')}`;
    } else {
      guidCaseId = caseId;
    }

    const formData = new FormData();
    formData.append('file', file);
    formData.append('caseId', guidCaseId);
    formData.append('area', area);
    formData.append('customName', customName || file.name);
    
    try {
      // استفاده از مسیر دقیق API بر اساس کد بک‌اند
      const response = await api.post('/api/FileEndpoints/upload', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      return response.data;
    } catch (error) {
      console.error('Error uploading file:', error);
      throw error.response?.data || 'خطا در آپلود فایل';
    }
  },

  /**
   * دریافت URL دانلود فایل
   * @param {string} fileId - شناسه فایل
   * @returns {string} - آدرس URL دانلود
   */
  getFileDownloadUrl: (fileId) => {
    return `${api.defaults.baseURL}/api/FileEndpoints/download/${fileId}`;
  },

  /**
   * دریافت URL تصویر
   * @param {string} fileId - شناسه فایل تصویر
   * @returns {string} - آدرس URL تصویر
   */
  getImageUrl: (fileId) => {
    return `${api.defaults.baseURL}/api/FileEndpoints/image/${fileId}`;
  },

  /**
   * دریافت URL موقت برای دسترسی به فایل
   * @param {string} fileId - شناسه فایل
   * @param {number} expirationMinutes - مدت اعتبار به دقیقه
   * @returns {Promise<Object>} - اطلاعات لینک موقت
   */
  generateSecureAccessUrl: async (fileId, expirationMinutes = 30) => {
    try {
      const response = await api.get(`/api/FileEndpoints/secure-access/${fileId}`, {
        params: {expirationMinutes},
      });
      return response.data;
    } catch (error) {
      console.error('Error generating secure access URL:', error);
      throw error.response?.data || 'خطا در ایجاد لینک موقت';
    }
  },

  /**
   * دانلود فایل با استفاده از blob
   * @param {string} fileId - شناسه فایل
   * @returns {Promise<Blob>} - محتوای فایل
   */
  downloadFileAsBlob: async (fileId) => {
    try {
      const response = await api.get(`/api/FileEndpoints/download/${fileId}`, {
        responseType: 'blob',
      });
      return response.data;
    } catch (error) {
      console.error('Error downloading file:', error);
      throw error.response?.data || 'خطا در دانلود فایل';
    }
  },

  /**
   * دریافت فایل‌های پرونده
   * @param {string} caseId - شناسه پرونده
   * @param {string} area - ناحیه
   * @returns {Promise<Array>} - لیست فایل‌ها
   */
  getCaseFiles: async (caseId, area) => {
    try {
      let guidCaseId;
    if (typeof caseId === 'number' || !caseId.includes('-')) {
      // ساخت یک Guid از عدد (روش ساده)
      guidCaseId = `00000000-0000-0000-0000-${caseId.toString().padStart(12, '0')}`;
    } else {
      guidCaseId = caseId;
    }
      const response = await api.get(`/api/FileEndpoints/case-files/${guidCaseId}`, {
        params: {area},
      });
      return response.data;
    } catch (error) {
      console.error('Error fetching case files:', error);
      throw error.response?.data || 'خطا در بازیابی فایل‌ها';
    }
  },

  /**
   * حذف فایل
   * @param {string} fileId - شناسه فایل
   * @returns {Promise<Object>} - نتیجه حذف
   */
  deleteFile: async (fileId) => {
    try {
      const response = await api.delete(`/api/FileEndpoints/${fileId}`);
      return response.data;
    } catch (error) {
      console.error('Error deleting file:', error);
      throw error.response?.data || 'خطا در حذف فایل';
    }
  },
};

export default secureFileApi;
