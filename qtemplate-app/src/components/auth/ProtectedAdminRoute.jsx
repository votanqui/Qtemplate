import { Navigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

export default function ProtectedAdminRoute({ children }) {
  const { isAuth, user, loading } = useAuth();
  if (loading) return null;
  if (!isAuth || user?.role !== 'Admin') return <Navigate to="/login" replace />;
  return children;
}