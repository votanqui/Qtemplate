import { createContext, useContext, useEffect, useRef, useState, useCallback } from 'react';
import { useAuth } from './AuthContext';
import { useToast } from '../components/ui';
import { useNavigate } from 'react-router-dom';

const BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7217';
const NotificationContext = createContext(null);

const typeIcon = { Success: '✅', Warning: '⚠️', Info: '🔔', Error: '❌' };

export const NotificationProvider = ({ children }) => {
  const { isAuth } = useAuth();
  const toast = useToast();
  const navigate = useNavigate();
  const wsRef = useRef(null);
  const reconnectTimer = useRef(null);
  const reconnectCount = useRef(0);
  const [unreadCount, setUnreadCount] = useState(0);

  const toastRef = useRef(toast);
  const navigateRef = useRef(navigate);
  useEffect(() => { toastRef.current = toast; }, [toast]);
  useEffect(() => { navigateRef.current = navigate; }, [navigate]);

  const addUnread = useCallback(() => setUnreadCount(prev => prev + 1), []);

  // Xóa badge — gọi khi user vào trang thông báo
  const clearUnread = useCallback(() => setUnreadCount(0), []);
    const decrementUnread = useCallback((amount = 1) => {
    setUnreadCount(prev => Math.max(0, prev - amount));
    }, []);
  // Fetch số thông báo chưa đọc từ DB khi login
  const fetchUnreadCount = useCallback(() => {
    if (!isAuth) return;
    fetch(`${BASE_URL}/api/user/notifications?page=1&pageSize=1&unreadOnly=true`, {
      credentials: 'include',
    })
      .then(r => r.json())
      .then(data => {
        const total = data?.data?.totalCount ?? 0;
        setUnreadCount(total);
      })
      .catch(() => {});
  }, [isAuth]);

  const connect = useCallback(() => {
    if (!isAuth) return;
    if (wsRef.current && wsRef.current.readyState <= WebSocket.OPEN) return;

    const hubUrl = `${BASE_URL}/hubs/notifications`;

    fetch(`${hubUrl}/negotiate?negotiateVersion=1`, { method: 'POST', credentials: 'include' })
      .then(r => r.json())
      .then(negotiateData => {
        if (wsRef.current && wsRef.current.readyState <= WebSocket.OPEN) return;

        const connectionToken = encodeURIComponent(negotiateData.connectionToken || '');
        const wsUrl = hubUrl
          .replace('https://', 'wss://')
          .replace('http://', 'ws://')
          + `?id=${connectionToken}`;

        const ws = new WebSocket(wsUrl);
        wsRef.current = ws;

        ws.onopen = () => {
          reconnectCount.current = 0;
          ws.send(JSON.stringify({ protocol: 'json', version: 1 }) + '\x1e');
        };

        ws.onmessage = (event) => {
          const parts = event.data.split('\x1e').filter(Boolean);
          parts.forEach(part => {
            try {
              const msg = JSON.parse(part);
              if (msg.type === 1 && msg.target === 'ReceiveNotification') {
                const notif = msg.arguments?.[0];
                if (!notif) return;
                toastRef.current.info({
                  title: notif.title,
                  message: notif.message,
                  duration: 6000,
                  onClick: () => navigateRef.current('/dashboard/notifications'),
                  _icon: typeIcon[notif.type] || '🔔',
                });
                addUnread();
              }
            } catch {}
          });
        };

        ws.onclose = () => {
          if (reconnectCount.current < 5) {
            const delay = Math.min(1000 * 2 ** reconnectCount.current, 30000);
            reconnectCount.current++;
            reconnectTimer.current = setTimeout(connect, delay);
          }
        };

        ws.onerror = () => ws.close();
      })
      .catch(() => {
        reconnectTimer.current = setTimeout(connect, 5000);
      });
  }, [isAuth, addUnread]);

  useEffect(() => {
    if (isAuth) {
      fetchUnreadCount(); // ← load số thực từ DB ngay khi login
      connect();
    } else {
      wsRef.current?.close();
      wsRef.current = null;
      clearTimeout(reconnectTimer.current);
      setUnreadCount(0);
    }
    return () => {
      wsRef.current?.close();
      wsRef.current = null;
      clearTimeout(reconnectTimer.current);
    };
  }, [isAuth]);

  return (
   <NotificationContext.Provider value={{ unreadCount, clearUnread, addUnread, decrementUnread }}>
      {children}
    </NotificationContext.Provider>
  );
};

export const useNotification = () => {
  const ctx = useContext(NotificationContext);
  if (!ctx) throw new Error('useNotification must be used within NotificationProvider');
  return ctx;
};