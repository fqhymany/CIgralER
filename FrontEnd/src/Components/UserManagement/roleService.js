import {getRoles} from '../../api/axios';
import {translateRole} from './roleUtils';

// دریافت نقش‌ها از بک‌اند و برگرداندن با ترجمه فارسی
export const fetchAndTranslateRoles = async () => {
  const response = await getRoles();
  return response.map((role) => ({
    id: role.name,
    label: translateRole(role.name),
  }));
};
