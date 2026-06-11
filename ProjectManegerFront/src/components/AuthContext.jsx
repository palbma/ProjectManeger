import React, { createContext, useState, useContext, useEffect } from 'react';
import { getToken, getCurrentUser, logout as apiLogout, login as apiLogin } from '../services/auth';

const AuthContext = createContext();

export const useAuth = () => useContext(AuthContext);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  // ✅ Завантажуємо користувача при старті застосунку
  useEffect(() => {
    const loadUser = async () => {
      try {
        if (getToken()) {
          const userData = await getCurrentUser();
          console.log('✅ Користувача завантажено з токена:', userData);
          setUser(userData);
        } else {
          console.log('⚠️ Токен не знайдено');
          setUser(null);
        }
      } catch (error) {
        console.error('❌ Не вдалося завантажити користувача:', error);
        // Якщо токен невалідний — видаляємо його
        apiLogout();
        setUser(null);
      } finally {
        setLoading(false);
      }
    };

    loadUser();
  }, []);

  // ✅ Реалізація функції login, яка зберігає користувача в контексті
  const login = async (credentials) => {
    try {
      console.log('🔐 Спроба входу з email:', credentials.email);
      const userData = await apiLogin(credentials);
      console.log('✅ Успішний вхід, дані користувача:', userData);

      // Зберігаємо користувача в контекст
      setUser(userData);

      return userData;
    } catch (error) {
      console.error('❌ Помилка входу:', error);
      setUser(null);
      throw error;
    }
  };

  const logout = () => {
    console.log('🚪 Вихід із системи');
    apiLogout();
    setUser(null);
  };

  const value = {
    user,
    setUser,
    loading,
    login,
    logout
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};