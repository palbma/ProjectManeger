import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { taskApi } from '../api/taskApi';
import CommentList from './CommentList';
import { useAuth } from './AuthContext';
import { isMember, canEdit } from '../utils/roleUtils';
import '../styles/Lists.css';
import '../styles/Comments.css';

// Маппинг статусов
const STATUS_MAP = {
  0: 'ToDo',
  1: 'InProgress',
  2: 'Completed'
};

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

const TaskDetail = ({ taskId, projectId, onClose, startEditing = false }) => {
  const [task, setTask] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState({
    title: '',  
    description: '',
    status: 'Pending',
    assignedToId: ''
  });
  const { user } = useAuth();
  const canEditTask = canEdit(user);

  useEffect(() => {
    fetchTask();
  }, [taskId]);

  useEffect(() => {
    if (isMember(user) && startEditing) {
      setIsEditing(false);
    } else {
      setIsEditing(startEditing);
    }
  }, [startEditing, user?.role]);

  const fetchTask = async () => {
    try {
      setLoading(true);
      const response = await taskApi.getTaskById(taskId);
      const mappedTask = mapTaskStatus(response.data);
      setTask(mappedTask);
      setFormData({
        title: mappedTask.title,
        description: mappedTask.description,
        status: mappedTask.status,
        assignedToId: mappedTask.assignedToId || ''
      });
      setError('');
    } catch (err) {
      console.error('Error loading task:', err);
      setError('Failed to load task.');
    } finally {
      setLoading(false);
    }
  };

  const handlechange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!canEditTask) {
      setError('You do not have permission to edit this task.');
      return;
    }
    try {
      const payload = {
        ...formData,
        Status: mapStatusToNumber(formData.status)
      };
      delete payload.status;
      await taskApi.updateTask(taskId, payload);
      setIsEditing(false);
      await fetchTask();
    } catch (err) {
      console.error('Error saving:', err);
      setError('Failed to save changes.');
    }
  };

  const handleCompleteTask = async () => {
    try {
      await taskApi.completeTask(taskId);
      await fetchTask();
    } catch (err) {
      console.error('Error:', err);
      setError('Failed to mark task as completed.');
    }
  };

  const handleDeleteTask = async () => {
    if (!canEditTask) {
      setError('You do not have permission to delete this task.');
      return;
    }
    if (window.confirm('Are you sure you want to delete this task?')) {
      try {
        await taskApi.deleteTask(taskId);
        onClose && onClose();
      } catch (err) {
        console.error('Error:', err);
        setError('Failed to delete task.');
      }
    }
  };

  if (loading) return <div className="loading">⏳ Loading task...</div>;
  if (!task) return <div className="error-message">Task not found</div>;

  return (
    <div style={{ background: '#1E1E1E', padding: '2rem', borderRadius: '12px', marginBottom: '2rem', border: '1px solid #2A2A2A', boxShadow: '0 4px 20px rgba(0, 0, 0, 0.3)' }}>
      <div className="item-header" style={{ marginBottom: '1.5rem' }}>
        <h2 style={{ margin: 0, color: '#E5E7EB' }}>{task.title}</h2>
        <span className={`item-status status-${task.status?.toLowerCase()}`}>
          {task.status}
        </span>
      </div>

      {error && <div className="error-message">{error}</div>}

      {!isEditing ? (
        <>
          <div className="item-description" style={{ color: '#D1D5DB' }}>{task.description}</div>
          
          <div className="item-meta" style={{ marginBottom: '1.5rem' }}>
            <div className="meta-item">
              <span className="meta-label" style={{ color: '#9CA3AF' }}>Status</span>
              <span style={{ color: '#E5E7EB' }}>{task.status}</span>
            </div>
            {task.assignedTo && (
              <div className="meta-item">
                <span className="meta-label" style={{ color: '#9CA3AF' }}>Assigned to</span>
                <span style={{ color: '#E5E7EB' }}>{task.assignedTo.name || task.assignedTo.email}</span>
              </div>
            )}
          </div>

          <div className="item-actions" style={{ marginBottom: '2rem' }}>
            {task.status !== 'Completed' && (
              <button className="btn btn-success" onClick={handleCompleteTask}>
                ✓ Completed
              </button>
            )}
            {canEditTask && (
              <>
                <button className="btn btn-secondary" onClick={() => setIsEditing(true)}>
                  ✏️ Edit
                </button>
                <button className="btn btn-danger" onClick={handleDeleteTask}>
                  🗑️ Delete
                </button>
              </>
            )}
            {onClose && (
              <button className="btn btn-secondary" onClick={onClose}>
                Close
              </button>
            )}
          </div>
        </>
      ) : (
        <form onSubmit={handleSubmit} style={{ marginBottom: '2rem' }}>
          <div className="form-group">
            <label>Title *</label>
            <input
              type="text"
              name="title"
              value={formData.title}
              onChange={handlechange}
              required
            />
          </div>
          <div className="form-group">
            <label>Description</label>
            <textarea
              name="description"
              value={formData.description}
              onChange={handlechange}
            />
          </div>
          <div className="form-group">
            <label>Status</label>
            <select name="status" value={formData.status} onChange={handlechange}>
              <option>ToDo</option>
              <option>InProgress</option>
              <option>Completed</option>
            </select>
          </div>
          <div style={{ display: 'flex', gap: '1rem' }}>
            <button type="submit" className="btn btn-primary">Save</button>
            <button
              type="button"
              className="btn btn-secondary"
              onClick={() => setIsEditing(false)}
            >
              Cancel
            </button>
          </div>
        </form>
      )}

      <CommentList taskId={taskId} />
    </div>
  );
};

export default TaskDetail;
