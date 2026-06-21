// src/services/auth.js
const API_URL = '/api/auth'; // via Vite proxy

// Save token to localStorage
export const setToken = (token) => localStorage.setItem('token', token);

// Get token
export const getToken = () => localStorage.getItem('token');

// Remove token (logout)
export const removeToken = () => localStorage.removeItem('token');

// Register
export const register = async (userData) => {
  const response = await fetch(`${API_URL}/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(userData)
  });

  if (!response.ok) {
    const errorData = await response.json();
    throw new Error(errorData.message || 'Registration error');
  }

  const data = await response.json();
  // Assume token is in response
  if (data.token) {
    setToken(data.token);
  }
  return data;
};

// Login
export const login = async (credentials) => {
    console.log('📤 Sending login:', credentials); // что реально уходит?
  const response = await fetch(`${API_URL}/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(credentials)
  });

  if (!response.ok) {
    const errorData = await response.json();
    throw new Error(errorData.message || 'Login error');
  }

  const data = await response.json();
  if (data.token) {
    setToken(data.token);
  }
  return data;
};

// Get current user information (if needed)
export const getCurrentUser = async () => {
  const token = getToken();
  if (!token) throw new Error('Not authorized');

  const response = await fetch(`${API_URL}/me`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });

  if (!response.ok) {
    throw new Error('Failed to get user data');
  }

  return await response.json();
};


export const logout = () => {
  removeToken();
};