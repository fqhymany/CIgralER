// Utility functions for role translations

// Returns a map of role names to their Persian translations
export const getRoleTranslations = () => ({
  Admin: 'مدیر سیستم',
  Lawyer: 'وکیل',
  LawyerAssistant: 'دستیار وکیل',
  Paralegal: 'کارآموز وکیل',
  Secretary: 'منشی',
  Client: 'موکل',
  Judge: 'قاضی',
  Express: 'مسئول پرونده',
  Litigant: 'طرف دعوی',
});

// Translate a single role name to Persian
export const translateRole = (roleName) => {
  const translations = getRoleTranslations();
  return translations[roleName] || roleName;
};

// Translate an array of role names to a Persian summary string
export const translateRoles = (rolesArr) => {
  const translations = getRoleTranslations();
  return (rolesArr || []).map((r) => translations[r] || r).join('، ');
};
