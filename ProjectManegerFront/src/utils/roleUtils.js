/**
 * Normalize role (оставлена для совместимости, но проверки теперь используют прямое приведение)
 */
export const normalizeRole = (role) => {
  if (!role) return 'Member';
  const normalizedRole = String(role).trim().toLowerCase();
  if (normalizedRole === 'admin') return 'admin';
  if (normalizedRole === 'projectmanager' || normalizedRole === 'project_manager') return 'ProjectManager';
  if (normalizedRole === 'member') return 'member';
  return 'member';
};

const getRole = (user) => {
  if (!user) return null;
  if (Array.isArray(user.roles) && user.roles.length > 0) {
    return user.roles[0].toLowerCase();
  }
  return (user.role ?? user.Role ?? '').toLowerCase();
};
export const isAdmin = (user) => {
  return getRole(user) === 'admin';
};

export const isProjectManager = (user) => {
  const role = getRole(user);
  return role === 'projectmanager' || role === 'project_manager';
};

export const isMember = (user) => {
  if (!user) return true;
  return getRole(user) === 'member';
};

export const canEdit = (user) => {
  if (!user) return false;
  return !isMember(user);
};

export const isAdminOrManager = (user) => {
  return isAdmin(user) || isProjectManager(user);
};

export const canCreateProject = (user) => {
  const role = getRole(user);
  return role === 'admin';
};
