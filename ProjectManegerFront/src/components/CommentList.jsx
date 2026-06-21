import React, { useState, useEffect } from 'react';
import { commentApi } from '../api/commentApi';
import { useAuth } from './AuthContext';
import { isAdmin } from '../utils/roleUtils';
import '../styles/Comments.css';

const getAuthorName = (comment, currentUser) => {
  // Если это комментарий текущего пользователя
  if (currentUser && Number(comment.authorId) === Number(currentUser.id)) {
    return currentUser.fullName || currentUser.name || currentUser.email || `User ${comment.authorId}`;
  }
  // Если у комментария есть имя автора
  if (comment.authorName && comment.authorName !== 'unauthorized user' && comment.authorName.trim()) {
    return comment.authorName;
  }
  // Fallback
  return `User ${comment.authorId}`;
};

const CommentList = ({ taskId }) => {
  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const { user } = useAuth();

  const fetchComments = async () => {
    try {
      setLoading(true);
      const response = await commentApi.getTaskComments(taskId);
      setComments(response.data);
      setError('');
    } catch (err) {
      console.error('Error loading comments:', err);
      setError('Failed to load comments.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (taskId) {
      fetchComments();
    }
  }, [taskId]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!newComment.trim() || !user) return;
    setSubmitting(true);
    try {
      const response = await commentApi.createComment({
        taskId: parseInt(taskId),
        content: newComment
      });
      // Обогатим комментарий данными текущего пользователя
      const enrichedComment = {
        ...response.data,
        authorName: user.fullName || user.name || user.email,
        authorId: user.id
      };
      setComments(prev => [enrichedComment, ...prev]);
      setNewComment('');
      setError('');
    } catch (err) {
      console.error('Error creating comment:', err);
      setError('Failed to send comment.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (commentId, authorId) => {
    const isAuthor = user?.id === authorId;
    const isAdminUser = isAdmin(user);
    if (!isAuthor && !isAdminUser) {
      alert('You do not have permission to delete this comment');
      return;
    }
    if (!window.confirm('Delete comment?')) return;
    try {
      await commentApi.deleteComment(commentId);
      setComments(prev => prev.filter(c => c.id !== commentId));
    } catch (err) {
      console.error('Error deleting comment:', err);
      setError('Failed to delete comment.');
    }
  };

  if (loading) return <div style={{ padding: '1rem', textAlign: 'center', color: '#999' }}>⏳ Loading comments...</div>;

  return (
    <div className="comment-section">
      <h3>💬 Comments</h3>
      {error && <div className="error-message">{error}</div>}

      <form onSubmit={handleSubmit} className="comment-form-group">
        <textarea
          value={newComment}
          onChange={(e) => setNewComment(e.target.value)}
          placeholder="Write a comment..."
          disabled={submitting}
        />
        <button type="submit" disabled={submitting || !newComment.trim()}>
          {submitting ? '⏳ Sending...' : '📤 Send'}
        </button>
      </form>

      <div className="comments-list">
        {comments.length === 0 ? (
          <p className="no-comments">No comments yet. Be the first!</p>
        ) : (
          comments.map(comment => (
            <div key={comment.id} className="comment-item">
              <div className="comment-header">
                <span className="comment-author">👤 {getAuthorName(comment, user)}</span>
                <span className="comment-date">{new Date(comment.createdAt).toLocaleString('en-US')}</span>
              </div>
              <p className="comment-content">{comment.content}</p>
              {(user?.id === comment.authorId || isAdmin(user)) && (
                <div className="comment-actions">
                  <button 
                    className="comment-btn-delete"
                    onClick={() => handleDelete(comment.id, comment.authorId)}
                  >
                    
                    🗑️ Delete
                  </button>
                </div>
              )}
            </div>
          ))
        )}
      </div>
    </div>
  );
};

export default CommentList;