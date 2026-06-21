import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { projectApi } from '../api/projectApi';
import { getToken } from '../services/auth';
import { useAuth } from './AuthContext';
import { isAdmin, isMember, canEdit, isProjectManager, canCreateProject } from '../utils/roleUtils';
import '../styles/Lists.css';

const getProjectManagerId = (project) => {
  if (!project) return null;
  if (project.manager && typeof project.manager === 'object') {
    return project.manager.id ?? project.manager.userId ?? null;
  }
  return project.managerId ?? project.ManagerId ?? project.manager ?? null;
};

const normalizeParticipantIds = (project) => {
  if (!project) return [];
  if (Array.isArray(project.participants)) {
    return project.participants.map(p => Number(p?.id)).filter(Boolean);
  }
  if (Array.isArray(project.ParticipantIds)) {
    return project.ParticipantIds.map(id => Number(id)).filter(Boolean);
  }
  if (Array.isArray(project.participantIds)) {
    return project.participantIds.map(id => Number(id)).filter(Boolean);
  }
  if (typeof project.participantIds === 'string' && project.participantIds.trim()) {
    return project.participantIds.split(',').map(s => Number(s.trim())).filter(n => !Number.isNaN(n));
  }
  if (typeof project.ParticipantIds === 'string' && project.ParticipantIds.trim()) {
    return project.ParticipantIds.split(',').map(s => Number(s.trim())).filter(n => !Number.isNaN(n));
  }
  return [];
};

const isUserConnectedToProject = (project, user) => {
  if (!project || !user) return false;

  const userId = Number(user.id);
  const managerId = Number(getProjectManagerId(project));
  const participantIds = normalizeParticipantIds(project);

  console.log('Checking project:', project.name, {
    userId,
    managerId,
    participantIds,
    isManager: managerId === userId,
    isParticipant: participantIds.includes(userId)
  });

  return (
    managerId === userId ||
    participantIds.includes(userId) ||
    project.participants?.some(p => Number(p?.id) === userId) ||
    project.members?.some(m => Number(m?.id) === userId)
  );
};
const STATUS_OPTIONS = {
  0: 'Active',
  1: 'Pending',
  2: 'Completed',
  3: 'Archived'
};

const STATUS_VALUES = {
  'Active': 0,
  'Pending': 1,
  'Completed': 2,
  'Archived': 3
};

const getProjectName = (project) => {
  return project?.name || project?.Title || project?.title || 'Unnamed Project';
};

const getProjectDescription = (project) => {
  return project?.description || project?.Description || '';
};

const ProjectList = () => {
  const navigate = useNavigate();
  const { user, loading: authLoading } = useAuth();
  const [projects, setProjects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    status: 0,
    deadline: '',
    participantIds: ''
  });


  const safeFormData = {
    name: formData.name || '',
    description: formData.description || '',
    status: formData.status || 0,
    deadline: formData.deadline || '',
    participantIds: formData.participantIds || ''
  };


  useEffect(() => {
    console.log('📊 ProjectList useEffect triggered:');
    console.log('  - authLoading:', authLoading);
    console.log('  - user:', user);
    
    if (authLoading) {
      console.log('  → Waiting for auth to finish loading');
      return; 
    }
    
    console.log('  → Calling fetchProjects');
    fetchProjects();
  }, [authLoading, user]); 

  const fetchProjects = async () => {
  try {
    setLoading(true);
    const response = await projectApi.getAllProjects();
    let filteredProjects = response.data;
console.log('USER:', user);
console.log('isAdmin:', isAdmin(user));
console.log('isProjectManager:', isProjectManager(user));
    if (user && !authLoading) {
      if (isAdmin(user) || isProjectManager(user)) {
        filteredProjects = response.data;
      } else {
        filteredProjects = response.data.filter(project => 
          isUserConnectedToProject(project, user)
        );
      }
    } else {
      filteredProjects = [];
    }

    setProjects(filteredProjects);
    setError('');
  } catch (err) {
    console.error('Error loading projects:', err);
    if (err.response?.status === 401) {
      setError('Authorization required. Please sign in.');
    } else {
      setError('Failed to load projects. Please try again later.');
    }
  } finally {
    setLoading(false);
  }
};

  const handleAddProject = () => {
    setEditingId(null);
    setFormData({ name: '', description: '', status: 0, deadline: '', participantIds: '' });
    setShowForm(true);
  };

  const handleEditProject = (project) => {
    if (isMember(user) || (isProjectManager(user) && !isUserConnectedToProject(project, user))) {
      setError('You do not have permission to edit this project.');
      return;
    }

    setEditingId(project.id);
    setFormData({
      name: getProjectName(project),
      description: getProjectDescription(project),
      status: typeof project.status === 'number' ? project.status : STATUS_VALUES[project.status] || 0,
      deadline: project.deadline ? project.deadline.split('T')[0] : '',
      participantIds: project.participantIds ? (Array.isArray(project.participantIds) ? project.participantIds.join(',') : project.participantIds) : (project.ParticipantIds ? (Array.isArray(project.ParticipantIds) ? project.ParticipantIds.join(',') : project.ParticipantIds) : (project.participants ? project.participants.map(m => m.id).join(',') : ''))
    });
    setShowForm(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (isMember(user)) {
      setError('You do not have permission to create or edit projects.');
      return;
    }
    const token = getToken();
    if (!token) {
      setError('Authorization required. Please sign in.');
      return;
    }
    if (!formData.name || formData.name.trim() === '') {
      setError('Project name is required');
      return;
    }

    const participantIds = (formData.participantIds || '')
      .split(',')
      .map(s => s.trim())
      .filter(s => s !== '')
      .map(s => parseInt(s, 10))
      .filter(n => !isNaN(n));

    if (participantIds.length === 0) {
      setError('Add at least one participant (enter IDs separated by commas)');
      return;
    }

    const payload = {
      Title: formData.name,
      Description: formData.description,
      Status: formData.status,
      Deadline: formData.deadline ? new Date(formData.deadline).toISOString() : null,
      ParticipantIds: participantIds
    };

    console.log('Sending payload to backend:', JSON.stringify(payload, null, 2));

    try {
      if (editingId) {
        await projectApi.updateProject(editingId, payload);
      } else {
        await projectApi.createProject(payload);
      }
      await fetchProjects();
      setShowForm(false);
      setError('');
    } catch (err) {
      console.error('Error saving project:', err);
      const resp = err.response && err.response.data;
      let message = 'Failed to save project.';
      
      console.log('Error response:', resp);
      console.log('Full error:', err);
      
      if (resp) {
        if (resp.message) message = resp.message;
        else if (resp.errors) message = JSON.stringify(resp.errors); 
        else if (typeof resp === 'string') message = resp;
        else message = JSON.stringify(resp);
      }
      setError(message);
    }
  };

  const handleDeleteProject = async (id) => {
    if (isMember(user)) {
      setError('You do not have permission to delete projects.');
      return;
    }
    const token = getToken();
    if (!token) {
      setError('Authorization required. Please sign in.');
      return;
    }
    if (window.confirm('Are you sure you want to delete this project?')) {
      try {
        console.log('Deleting project with id:', id);
        await projectApi.deleteProject(id);
        console.log('Delete successful');
        await fetchProjects();
        console.log('Projects refetched');
      } catch (err) {
        console.error('Error deleting project:', err);
        setError('Failed to delete project.');
      }
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  if (loading || authLoading) return <div className="container"><div className="loading">⏳ Loading projects...</div></div>;

  return (
    <div className="container">
      {error && <div className="error-message">{error}</div>}
      
      <div className="header">
        <h2>📋 My Projects</h2>
        {canCreateProject(user) && (
              <button className="btn btn-primary" onClick={handleAddProject}>
                + New Project
              </button>
      )}
      </div>

      {showForm && (
        <div style={{ background: '#1E1E1E', padding: '1.5rem', borderRadius: '12px', marginBottom: '2rem', border: '1px solid #2A2A2A', boxShadow: '0 4px 20px rgba(0, 0, 0, 0.3)' }}>
          <h3 style={{ color: '#E5E7EB', marginBottom: '1rem' }}>{editingId ? 'Edit Project' : 'Create New Project'}</h3>
          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label>Project Name *</label>
              <input
                type="text"
                name="name"
                value={safeFormData.name}
                onChange={handleChange}
                required
                placeholder="Enter project name"
              />
            </div>
            <div className="form-group">
              <label>Description</label>
              <textarea
                name="description"
                value={safeFormData.description}
                onChange={handleChange}
                placeholder="Enter project description"
              />
            </div>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
              <div className="form-group">
                <label>Status</label>
                <select name="status" value={formData.status} onChange={(e) => setFormData(prev => ({ ...prev, status: parseInt(e.target.value, 10) }))}>
                  <option value="0">Active</option>
                  <option value="1">Pending</option>
                  <option value="2">Completed</option>
                  <option value="3">Archived</option>
                </select>
              </div>
              <div className="form-group">

                <label>Deadline</label>
                <input
                  type="date"
                  name="deadline"
                  value={safeFormData.deadline}
                  onChange={handleChange}
                />
              </div>
            </div>
            <div className="form-group">
              <label>Participants (IDs separated by commas) *</label>
              <input
                type="text"
                name="participantIds"
                value={safeFormData.participantIds}
                onChange={handleChange}
                placeholder="Example: 2,3,4"
              />
            </div>
            <div style={{ display: 'flex', gap: '1rem' }}>
              <button type="submit" className="btn btn-primary">Save</button>
              <button
                type="button"
                className="btn btn-secondary"
                onClick={() => setShowForm(false)}
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}
      {projects.length === 0 ? (
        <div className="empty-state">
          <h3>🚭 No Projects Found</h3>
          <p>Create your first project to start managing tasks.</p>
        </div>
      ) : (
        <div className="items-grid">
          {projects.map(project => (
            <div className="item-card" key={project.id}>
              <div className="item-header">
                <h3 className="item-title">{getProjectName(project)}</h3>
                <span className={`item-status status-${
                  STATUS_OPTIONS[project.status]?.toLowerCase() || 'active'
                }`}>
                  {STATUS_OPTIONS[project.status] || 'Unknown'}
                </span>
              </div>
              
              {getProjectDescription(project) && (
                <p className="item-description">{getProjectDescription(project)}</p>
              )}

              <div className="item-meta">
                <div className="meta-item">
                  <span className="meta-label">Participants</span>
                  <span style={{ fontSize: '0.85rem', wordBreak: 'break-word' }}>
                    {Array.isArray(project.members) && project.members.length > 0
                      ? project.members.map(m => m.email || m.fullName).join(', ')
                      : 'No participants assigned'}
                  </span>
                </div>
                {project.deadline && (
                  <div className="meta-item">
                    <span className="meta-label">Deadline</span>
                    <span>{new Date(project.deadline).toLocaleDateString('en-US')}</span>
                  </div>
                )}
              </div>

              <div className="item-actions">
                <button
                  className="btn btn-primary btn-small"
                  onClick={() => navigate(`/projects/${project.id}/tasks`)}
                >
                  📝 Tasks
                </button>
                {canEdit(user) && (
                  <>
                    <button
                      className="btn btn-secondary btn-small"
                      onClick={() => handleEditProject(project)}
                    > 
                      ✏️ Edit
                    </button>
                    <button
                      className="btn btn-danger btn-small"
                      onClick={() => handleDeleteProject(project.id)}
                    >
                      🗑️ Delete
                    </button>
                  </>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default ProjectList;
