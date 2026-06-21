import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { taskApi } from '../api/taskApi';
import { projectApi } from '../api/projectApi';
import TaskDetail from './TaskDetail';
import { useAuth } from './AuthContext';
import { canEdit, isAdmin } from '../utils/roleUtils';

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
  if (isAdmin(user)) return true;

  const userId = Number(user.id);
  const managerId = Number(getProjectManagerId(project));
  const participantIds = normalizeParticipantIds(project);

  return (
    managerId === userId ||
    participantIds.includes(userId) ||
    project.participants?.some(p => Number(p?.id) === userId) ||
    project.members?.some(m => Number(m?.id) === userId)
  );
};

// Маппинг статусов: числа -> строки
const STATUS_MAP = {
  0: 'ToDo',
  1: 'InProgress',
  2: 'Completed'
};

// Обратный маппинг: строки -> числа
const STATUS_REVERSE_MAP = {
  'ToDo': 0,
  'InProgress': 1,
  'Completed': 2
};

const mapTaskStatus = (task) => {
  if (typeof task.status === 'number') {
    return { ...task, status: STATUS_MAP[task.status] || 'Pending' };
  }
  return task;
};

const mapStatusToNumber = (status) => {
  return typeof status === 'number' ? status : (STATUS_REVERSE_MAP[status] ?? 0);
};

const TaskList = () => {
  const { id: projectId } = useParams();
  const navigate = useNavigate();
  const { user, loading: authLoading } = useAuth();
  const [tasks, setTasks] = useState([]);
  const [project, setProject] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [selectedTaskId, setSelectedTaskId] = useState(null);
  const [selectedTaskStartEditing, setSelectedTaskStartEditing] = useState(false);
  const [draggedTaskId, setDraggedTaskId] = useState(null);
  const [dragOverStatus, setDragOverStatus] = useState(null);
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    priority: 0,
    dueDate: '',
    assignedToId: ''
  });

  useEffect(() => {
    if (authLoading) return; // ✅ Защита: ждем загрузки пользователя
    fetchProjectAndTasks();
  }, [projectId, user, authLoading]); // ✅ Добавлена зависимость authLoading

  const fetchProjectAndTasks = async () => {
    if (!user) {
      setLoading(false);
      console.log('⚠️ No user loaded, skipping project and tasks fetch');
      return; // ✅ Защита: не загружаем задачи, пока user не загружен
    }

    try {
      setLoading(true);
      const projectResponse = await projectApi.getProjectById(projectId);
      const projectData = projectResponse.data;
      setProject(projectData);
      console.log('AUTH CHECK:', {
  user,
  role: user?.role,
  isAdminResult: isAdmin(user),
  isConnected: isUserConnectedToProject(projectData, user)
});
      if (!isAdmin(user) && !isUserConnectedToProject(projectData, user)) {
  setTasks([]);
  setError('You do not have access to this project.');
  return;
}

      const tasksResponse = await taskApi.getAllTasks({ projectId });
      const tasksData = Array.isArray(tasksResponse.data) ? tasksResponse.data : [];
      const mappedTasks = tasksData.map(mapTaskStatus);
      setTasks(mappedTasks);
      setError('');
    } catch (err) {
      console.error('Error loading:', err);
      setError('Failed to load data. Check your authorization.');
    } finally {
      setLoading(false);
    }
  };

  const handleAddTask = () => {
    setFormData({ title: '', description: '', priority: 0, dueDate: '', assignedToId: '' });
    setShowForm(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (user?.role === 'Member') {
      setError('You do not have permission to create tasks.');
      return;
    }

    // Validation
    if (!formData.title || formData.title.trim() === '') {
      setError('Task title is required');
      return;
    }

    const payload = {
      ProjectId: parseInt(projectId),
      Title: formData.title,
      Description: formData.description,
      Priority: parseInt(formData.priority) || 0,
      DueDate: formData.dueDate ? new Date(formData.dueDate).toISOString() : null,
      AssignedToId: formData.assignedToId ? parseInt(formData.assignedToId) : null,
      Status: 0
    };

    try {
      await taskApi.createTask(payload);
      await fetchProjectAndTasks();
      setShowForm(false);
      setError('');
    } catch (err) {
      console.error('Error creating task:', err);
      setError('Failed to create task. Check the data.');
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleDeleteTask = async (taskId) => {
    if (!canEdit(user)) {
      setError('You do not have permission to delete tasks.');
      return;
    }
    if (window.confirm('Are you sure?')) {
      try {
        await taskApi.deleteTask(taskId);
        await fetchProjectAndTasks();
      } catch (err) {
        console.error('Error:', err);
        setError('Failed to delete task.');
      }
    }
  };

  const handleDragStart = (taskId) => (e) => {
    e.dataTransfer.setData('text/plain', taskId);
    setDraggedTaskId(taskId);
  };

  const handleDragEnd = () => {
    setDraggedTaskId(null);
  };

  const openTaskDetail = (taskId, startEditing = false) => {
    setSelectedTaskId(taskId);
    setSelectedTaskStartEditing(startEditing);
  };

  const handleDragOver = (status) => (e) => {
    e.preventDefault();
    setDragOverStatus(status);
  };

  const handleDrop = (status) => async (e) => {
    e.preventDefault();
    setDragOverStatus(null);
    const taskId = e.dataTransfer.getData('text/plain');
    if (!taskId) return;

    const task = tasks.find(t => Number(t.id) === Number(taskId));
    if (!task || task.status === status) return;

    const payload = {
      ProjectId: parseInt(projectId),
      Title: task.title,
      Description: task.description,
      Priority: task.priority || 0,
      DueDate: task.dueDate || null,
      AssignedToId: task.assignedToId || task.assignedTo?.id || null,
      Status: mapStatusToNumber(status)
    };

    try {
      await taskApi.changeTaskStatus(task.id, mapStatusToNumber(status));
      await fetchProjectAndTasks();
    } catch (err) {
      console.error('Error moving task:', err);
      setError('Failed to update task status.');
    }
  };

  if (loading) return <div className="container"><div className="loading">⏳ Loading...</div></div>;

  return (
    <div className="container">
      <div className="breadcrumb">
        <a onClick={() => navigate('/projects')}>📋 Projects</a>
        <span>/</span>
        <span>{project?.name}</span>
      </div>

      {error && <div className="error-message">{error}</div>}

      <div className="header">
        <div>
          <h2>📝 Project Tasks: {project?.name}</h2>
          {project?.description && <p style={{ color: '#9CA3AF', marginTop: '0.5rem' }}>{project.description}</p>}
        </div>
        {canEdit(user) && (
          <button className="btn btn-primary" onClick={handleAddTask}>
            + New Task
          </button>
        )}
      </div>

      {showForm && (
        <div style={{ background: '#1E1E1E', padding: '1.5rem', borderRadius: '12px', marginBottom: '2rem', border: '1px solid #2A2A2A', boxShadow: '0 4px 20px rgba(0, 0, 0, 0.3)' }}>
          <h3 style={{ color: '#E5E7EB', marginBottom: '1rem' }}>Create New Task</h3>
          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label>Task Title *</label>
              <input
                type="text"
                name="title"
                value={formData.title}
                onChange={handleChange}
                required
                placeholder="Enter task title"
              />
            </div>
            <div className="form-group">
              <label>Description</label>
              <textarea
                name="description"
                value={formData.description}
                onChange={handleChange}
                placeholder="Enter task description"
              />
            </div>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '1rem' }}>
              <div className="form-group">
                <label>Priority</label>
                <select name="priority" value={formData.priority} onChange={handleChange}>
                  <option value="0">Low (0)</option>
                  <option value="1">Medium (1)</option>
                  <option value="2">High (2)</option>
                </select>
              </div>
              <div className="form-group">
                <label>Due Date</label>
                <input
                  type="datetime-local"
                  name="dueDate"
                  value={formData.dueDate}
                  onChange={handleChange}
                />
              </div>
            </div>
            <div className="form-group">
              <label>Assign to (User ID)</label>
              <input
                type="number"
                name="assignedToId"
                value={formData.assignedToId}
                onChange={handleChange}
                placeholder="Enter user ID"
              />
            </div>
            <div style={{ display: 'flex', gap: '1rem' }}>
              <button type="submit" className="btn btn-primary">Create</button>
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

      {selectedTaskId && (
        <TaskDetail
          taskId={selectedTaskId}
          projectId={projectId}
          startEditing={selectedTaskStartEditing}
          onClose={() => {
            setSelectedTaskId(null);
            setSelectedTaskStartEditing(false);
            fetchProjectAndTasks();
          }}
        />
      )}

      {tasks.length === 0 ? (
        <div className="empty-state">
          <h3>🚭 No Tasks Found</h3>
          <p>Create the first task for this project.</p>
        </div>
      ) : (
        <div className="tasks-board-container">
          <div className="tasks-board">
            {/* Pending Column */}
            <div className="task-column pending-column">
              <div className="column-header pending-header">
                <h3>📋 To Do</h3>
                <span className="task-count">{tasks.filter(t => t.status === 'ToDo').length}</span>
              </div>
              <div
                className={`tasks-list pending-list${dragOverStatus === 'ToDo' ? ' drag-over' : ''}`}
                onDragOver={handleDragOver('ToDo')}
                onDrop={handleDrop('ToDo')}
              >
                {tasks.filter(t => t.status === 'ToDo').map(task => (
                  <div
                    key={task.id}
                    className={`task-card pending-card${draggedTaskId === task.id ? ' dragging' : ''}`}
                    draggable="true"
                    onDragStart={handleDragStart(task.id)}
                    onDragEnd={handleDragEnd}
                  >
                    <div className="task-card-header">
                      <h4 className="task-card-title">{task.title}</h4>
                      <div className="task-card-actions">
                        <button
                          className="btn-icon"
                          onClick={() => openTaskDetail(task.id, false)}
                          title="Details"
                        >
                          👁️
                        </button>
                        {user?.role !== 'Member' && (
                          <>
                            <button
                              className="btn-icon btn-secondary"
                              onClick={() => openTaskDetail(task.id, true)}
                              title="Edit"
                            >
                              ✏️
                            </button>
                            <button
                              className="btn-icon btn-danger"
                              onClick={() => handleDeleteTask(task.id)}
                              title="Delete"
                            >
                              🗑️
                            </button>
                          </>
                        )}
                      </div>
                    </div>
                    {task.description && (
                      <p className="task-card-description">{task.description}</p>
                    )}
                    {task.assignedTo && (
                      <div className="task-card-meta">
                        <span className="task-assigned">👤 {task.assignedTo.name || task.assignedTo.email}</span>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            </div>

            {/* In Progress Column */}
            <div className="task-column inprogress-column">
              <div className="column-header inprogress-header">
                <h3>⚙️ In Progress</h3>
                <span className="task-count">{tasks.filter(t => t.status === 'InProgress').length}</span>
              </div>
              <div
                className={`tasks-list inprogress-list${dragOverStatus === 'InProgress' ? ' drag-over' : ''}`}
                onDragOver={handleDragOver('InProgress')}
                onDrop={handleDrop('InProgress')}
              >
                {tasks.filter(t => t.status === 'InProgress').map(task => (
                  <div
                    key={task.id}
                    className={`task-card inprogress-card${draggedTaskId === task.id ? ' dragging' : ''}`}
                    draggable="true"
                    onDragStart={handleDragStart(task.id)}
                    onDragEnd={handleDragEnd}
                  >
                    <div className="task-card-header">
                      <h4 className="task-card-title">{task.title}</h4>
                      <div className="task-card-actions">
                        <button
                          className="btn-icon"
                          onClick={() => openTaskDetail(task.id, false)}
                          title="Details"
                        >
                          👁️
                        </button>
                        {user?.role !== 'Member' && (
                          <>
                            <button
                              className="btn-icon btn-secondary"
                              onClick={() => openTaskDetail(task.id, true)}
                              title="Edit"
                            >
                              ✏️
                            </button>
                            <button
                              className="btn-icon btn-danger"
                              onClick={() => handleDeleteTask(task.id)}
                              title="Delete"
                            >
                              🗑️
                            </button>
                          </>
                        )}
                      </div>
                    </div>
                    {task.description && (
                      <p className="task-card-description">{task.description}</p>
                    )}
                    {task.assignedTo && (
                      <div className="task-card-meta">
                        <span className="task-assigned">👤 {task.assignedTo.name || task.assignedTo.email}</span>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            </div>

            {/* Completed Column */}
            <div className="task-column completed-column">
              <div className="column-header completed-header">
                <h3>✅ Completed</h3>
                <span className="task-count">{tasks.filter(t => t.status === 'Completed').length}</span>
              </div>
              <div
                className={`tasks-list completed-list${dragOverStatus === 'Completed' ? ' drag-over' : ''}`}
                onDragOver={handleDragOver('Completed')}
                onDrop={handleDrop('Completed')}
              >
                {tasks.filter(t => t.status === 'Completed').map(task => (
                  <div
                    key={task.id}
                    className={`task-card completed-card${draggedTaskId === task.id ? ' dragging' : ''}`}
                    draggable="true"
                    onDragStart={handleDragStart(task.id)}
                    onDragEnd={handleDragEnd}
                  >
                    <div className="task-card-header">
                      <h4 className="task-card-title">{task.title}</h4>
                      <div className="task-card-actions">
                        <button
                          className="btn-icon"
                          onClick={() => openTaskDetail(task.id, false)}
                          title="Details"
                        >
                          👁️
                        </button>
                        {user?.role !== 'Member' && (
                          <>
                            <button
                              className="btn-icon btn-secondary"
                              onClick={() => openTaskDetail(task.id, true)}
                              title="Edit"
                            >
                              ✏️
                            </button>
                            <button
                              className="btn-icon btn-danger"
                              onClick={() => handleDeleteTask(task.id)}
                              title="Delete"
                            >
                              🗑️
                            </button>
                          </>
                        )}
                      </div>
                    </div>
                    {task.description && (
                      <p className="task-card-description">{task.description}</p>
                    )}
                    {task.assignedTo && (
                      <div className="task-card-meta">
                        <span className="task-assigned">👤 {task.assignedTo.name || task.assignedTo.email}</span>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default TaskList; 