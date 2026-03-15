import { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { authApi, userApi } from '../api/services';
import { extractError } from '../api/client';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(false);
  const [initializing, setInitializing] = useState(true);

  useEffect(() => {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) { setInitializing(false); return; }
    userApi.getProfile()
      .then(res => setUser(res.data.data))
      .catch(() => localStorage.removeItem('refreshToken'))
      .finally(() => setInitializing(false));
  }, []);

  const login = useCallback(async (email, password) => {
    setLoading(true);
    try {
      const res = await authApi.login({ email, password });
      const { refreshToken } = res.data.data;
      localStorage.setItem('refreshToken', refreshToken);
      const profileRes = await userApi.getProfile();
      setUser(profileRes.data.data);
      return { success: true };
    } catch (err) {
      return { success: false, error: extractError(err) };
    } finally { setLoading(false); }
  }, []);

  const register = useCallback(async (data) => {
    setLoading(true);
    try {
      const res = await authApi.register(data);
      const { refreshToken } = res.data.data;
      localStorage.setItem('refreshToken', refreshToken);
      const profileRes = await userApi.getProfile();
      setUser(profileRes.data.data);
      return { success: true, message: res.data.message };
    } catch (err) {
      return { success: false, error: extractError(err) };
    } finally { setLoading(false); }
  }, []);

  const logout = useCallback(async () => {
    const refreshToken = localStorage.getItem('refreshToken');
    try { await authApi.logout(refreshToken); } catch {}
    localStorage.removeItem('refreshToken');
    setUser(null);
  }, []);

  const refreshProfile = useCallback(async () => {
    try {
      const res = await userApi.getProfile();
      setUser(res.data.data);
    } catch (err) {
      return { success: false, error: extractError(err) };
    }
  }, []);

  const updateUserData = useCallback((newData) => {
    setUser(prev => ({ ...prev, ...newData }));
  }, []);

  if (initializing) return null;

  return (
    <AuthContext.Provider value={{
      user, loading, login, register, logout,
      refreshProfile, updateUserData,
      isAuth: !!user
    }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
};