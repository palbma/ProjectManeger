import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from './AuthContext';
import '../styles/Navigation.css';

const Navigation = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const navItems = [
    { path: '/projects', label: '📋 Projects', icon: '📋' },
  ];

  return (
    <>
      <nav className="navbar">
        <div className="nav-container">
          <div className="nav-brand">
            <h1>📊 Project Manager</h1>
          </div>
          <div className="nav-menu">
            {user && (
              <>
                <span className="user-info">👤 {user.name || user.email}</span>
                <button onClick={handleLogout} className="logout-btn">
                  Logout
                </button>
              </>
            )}
          </div>
        </div>
      </nav>

      {user && (
        <aside className="sidebar">
          <nav className="sidebar-nav">
            {navItems.map(item => (
              <li key={item.path}>
                <a
                  href="#"
                  onClick={(e) => {
                    e.preventDefault();
                    navigate(item.path);
                  }}
                  className={location.pathname === item.path ? 'active' : ''}
                >
                  <span>{item.icon}</span>
                  {item.label}
                </a>
              </li>
            ))}
          </nav>
        </aside>
      )}
    </>
  );
};

export default Navigation;
