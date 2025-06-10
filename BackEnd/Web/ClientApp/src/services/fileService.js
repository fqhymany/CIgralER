import api from '../api/axios';

/**
 * سرویس مدیریت فایل برای تعامل با API های فایل
 */
const fileService = {
  /**
   * آپلود فایل
   * @param {File} file - فایل برای آپلود
   * @param {string} caseId - شناسه پرونده
   * @param {string} area - ناحیه (بخش) مربوط به فایل
   * @returns {Promise<FileDto>} - اطلاعات فایل آپلود شده
   */
  async uploadFile(file, caseId, area) {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('caseId', caseId);
    formData.append('area', area);
    
    const response = await api.post('/api/files/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    
    return response.data;
  },

  /**
   * دریافت URL دانلود فایل
   * @param {string} fileId - شناسه فایل
   * @returns {string} - URL دانلود فایل
   */
  getFileDownloadUrl(fileId) {
    return `${api.defaults.baseURL}/api/files/download/${fileId}`;
  },

  /**
   * دریافت URL تصویر
   * @param {string} fileId - شناسه فایل
   * @returns {string} - URL تصویر
   */
  getImageUrl(fileId) {
    return `${api.defaults.baseURL}/api/files/image/${fileId}`;
  },

  /**
   * ایجاد لینک دسترسی امن با زمان انقضا برای فایل
   * @param {string} fileId - شناسه فایل
   * @param {number} expirationMinutes - مدت اعتبار لینک به دقیقه
   * @returns {Promise<FileAccessTokenResult>} - نتیجه ایجاد توکن دسترسی
   */
  async generateSecureAccess(fileId, expirationMinutes = 30) {
    const response = await api.get(`/api/files/secure-access/${fileId}`, {
      params: { expirationMinutes }
    });
    
    return response.data;
  },

  /**
   * دریافت URL دسترسی به فایل با توکن
   * @param {string} token - توکن دسترسی
   * @returns {string} - URL دسترسی به فایل
   */
  getSecureAccessUrl(token) {
    return `${api.defaults.baseURL}/api/files/access/${token}`;
  },

  /**
   * تشخیص نوع فایل بر اساس پسوند یا نوع MIME
   * @param {string} fileName - نام فایل
   * @param {string} mimeType - نوع MIME فایل (اختیاری)
   * @returns {string} - نوع فایل
   */
  getFileType(fileName, mimeType = '') {
    const extension = fileName.split('.').pop().toLowerCase();
    
    // تشخیص بر اساس پسوند
    const imageExtensions = ['jpg', 'jpeg', 'png', 'gif', 'bmp', 'webp', 'svg'];
    const documentExtensions = ['pdf', 'doc', 'docx', 'xls', 'xlsx', 'ppt', 'pptx', 'txt', 'rtf'];
    const archiveExtensions = ['zip', 'rar', '7z', 'tar', 'gz'];
    
    if (imageExtensions.includes(extension) || mimeType.startsWith('image/')) {
      return 'image';
    } else if (documentExtensions.includes(extension) || 
              mimeType.includes('pdf') || 
              mimeType.includes('document') || 
              mimeType.includes('sheet') || 
              mimeType.includes('presentation')) {
      return 'document';
    } else if (archiveExtensions.includes(extension) || mimeType.includes('archive')) {
      return 'archive';
    }
    
    return 'other';
  },

  /**
   * تبدیل حجم فایل به فرمت خوانا
   * @param {number} bytes - حجم فایل به بایت
   * @returns {string} - حجم فایل با فرمت خوانا
   */
  formatFileSize(bytes) {
    if (bytes === 0) return '0 بایت';
    const k = 1024;
    const sizes = ['بایت', 'کیلوبایت', 'مگابایت', 'گیگابایت', 'ترابایت'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
};

export default fileService;