import React, { useState } from 'react';
import { useAuth } from './AuthContext';
import { useNavigate } from 'react-router-dom';
import { isAdmin } from '../utils/roleUtils';
import '../styles/Forms.css';

const AdminLogin = () => {
  const [credentials, setCredentials] = useState({
    email: '',
    password: ''
  });
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();  // ✅ Используем login из контекста
  const navigate = useNavigate();

  const handleChange = (e) => {
    setCredentials({ ...credentials, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      // ✅ login автоматически сохраняет user в контекст
      const userData = await login(credentials);
      
      // Проверяем, что пользователь - админ
      if (!isAdmin(userData)) {
        setError('You do not have administrator rights');
        return;
      }
      
      navigate('/projects');
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="form-container">
      <form onSubmit={handleSubmit}>
        <h2>🔐 Admin Login</h2>
        {error && <div className="error-message">{error}</div>}

        <div className="form-group">
          <label>Admin Email:</label>
          <input
            type="email"
            name="email"
            value={credentials.email}
            onChange={handleChange}
            required
            placeholder="Enter admin email"
          />
        </div>

        <div className="form-group">
          <label>Password:</label>
          <input
            type="password"
            name="password"
            value={credentials.password}
            onChange={handleChange}
            required
            placeholder="Enter password"
          />
        </div>

        <button type="submit" className="btn btn-primary" disabled={loading} style={{ width: '100%' }}>
          {loading ? '⏳ Signing in...' : '→ Sign in as Admin'}
        </button>

        <div className="login-link">
          <button type="button" onClick={() => navigate('/login')} className="btn btn-secondary">
            Back to regular login
          </button>
        </div>
      </form>
    </div>
  );
};

export default AdminLogin;
