// src/hooks/useCommunityHub.js
// Module-level singleton — 1 WebSocket duy nhất cho toàn app.
// Pattern giống NotificationContext: negotiate -> ws -> handshake -> listen.
// Không có JSX, không có Context, không có stale closure.

import { useEffect, useRef } from 'react';

const BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7217';
const HUB_URL  = `${BASE_URL}/hubs/community`;

// ── Singleton state (module-level) ───────────────────────────────────────────
let ws             = null;
let reconnectTimer = null;
let reconnectCount = 0;
let connected      = false;
let joinedGroups   = new Set(); // 'JoinFeed' | 'JoinPost:42'

// Map<eventName, Set<stableHandler>>
const listeners = new Map();

// ── Helpers ──────────────────────────────────────────────────────────────────
function sendFrame(method, args = []) {
  if (!ws || ws.readyState !== WebSocket.OPEN) return;
  ws.send(JSON.stringify({ type: 1, target: method, arguments: args }) + '\x1e');
}

function dispatch(eventName, ...args) {
  listeners.get(eventName)?.forEach(fn => { try { fn(...args); } catch {} });
}

function rejoinAll() {
  joinedGroups.forEach(key => {
    const i = key.indexOf(':');
    if (i === -1) sendFrame(key, []);
    else sendFrame(key.slice(0, i), [Number(key.slice(i + 1))]);
  });
}

function connect() {
  if (ws && ws.readyState <= WebSocket.OPEN) return;

  fetch(`${HUB_URL}/negotiate?negotiateVersion=1`, {
    method: 'POST', credentials: 'include',
  })
    .then(r => r.json())
    .then(data => {
      if (ws && ws.readyState <= WebSocket.OPEN) return;

      const token = encodeURIComponent(data.connectionToken || '');
      const wsUrl = HUB_URL
        .replace('https://', 'wss://')
        .replace('http://', 'ws://')
        + `?id=${token}`;

      ws = new WebSocket(wsUrl);

      ws.onopen = () => {
        reconnectCount = 0;
        ws.send(JSON.stringify({ protocol: 'json', version: 1 }) + '\x1e');
      };

      ws.onmessage = (event) => {
        const parts = event.data.split('\x1e').filter(Boolean);
        parts.forEach(part => {
          let msg;
          try { msg = JSON.parse(part); } catch { return; }

          // Handshake response — không có type
          if (!msg.type) {
            connected = true;
            rejoinAll(); // re-join tất cả groups sau reconnect
            return;
          }
          if (msg.type === 6) return; // Ping

          if (msg.type === 1) {
            const a = msg.arguments || [];
            switch (msg.target) {
              case 'NewPost':        dispatch('NewPost',        a[0]);               break;
              case 'PostUpdated':    dispatch('PostUpdated',    a[0]);               break;
              case 'PostDeleted':    dispatch('PostDeleted',    a[0]);               break;
              case 'LikeUpdated':    dispatch('LikeUpdated',    a[0], a[1], a[2]);   break;
              case 'NewComment':     dispatch('NewComment',     a[0]);               break;
              case 'CommentUpdated': dispatch('CommentUpdated', a[0]);               break;
              case 'CommentDeleted': dispatch('CommentDeleted', a[0], a[1]);         break;
              default: break;
            }
          }
        });
      };

      ws.onclose = () => {
        connected = false;
        ws = null;
        if (reconnectCount < 5) {
          const delay = Math.min(1000 * 2 ** reconnectCount, 30000);
          reconnectCount++;
          reconnectTimer = setTimeout(connect, delay);
        }
      };

      ws.onerror = () => ws?.close();
    })
    .catch(() => { reconnectTimer = setTimeout(connect, 5000); });
}

function joinGroup(method, postId) {
  const key = postId != null ? `${method}:${postId}` : method;
  if (joinedGroups.has(key)) return;
  joinedGroups.add(key);
  if (connected) sendFrame(method, postId != null ? [postId] : []);
}

function leaveGroup(method, postId) {
  const key = postId != null ? `${method}:${postId}` : method;
  joinedGroups.delete(key);
  sendFrame(method, postId != null ? [postId] : []);
}

function subscribe(eventName, handler) {
  if (!listeners.has(eventName)) listeners.set(eventName, new Set());
  listeners.get(eventName).add(handler);
  return () => listeners.get(eventName)?.delete(handler);
}

// ── Public API ────────────────────────────────────────────────────────────────

/**
 * startCommunityHub() — gọi trong useEffect([]) ở CommunityPage.
 * Trả về cleanup (đóng WS khi page unmount).
 */
export function startCommunityHub() {
  connect();
  return () => {
    clearTimeout(reconnectTimer);
    ws?.close();
    ws = null;
    connected = false;
    joinedGroups.clear();
    listeners.clear();
  };
}

/**
 * useCommunityFeed(handlers) — dùng trong CommunityPageInner.
 * Join community_feed, nhận NewPost/PostUpdated/PostDeleted/LikeUpdated.
 */
export function useCommunityFeed(handlers) {
  const ref = useRef(handlers);
  useEffect(() => { ref.current = handlers; });

  useEffect(() => {
    joinGroup('JoinFeed');
    const unsubs = [
      subscribe('NewPost',     (...a) => ref.current.onNewPost?.(...a)),
      subscribe('PostUpdated', (...a) => ref.current.onPostUpdated?.(...a)),
      subscribe('PostDeleted', (...a) => ref.current.onPostDeleted?.(...a)),
      subscribe('LikeUpdated', (...a) => ref.current.onLikeUpdated?.(...a)),
    ];
    return () => unsubs.forEach(fn => fn());
  }, []); // eslint-disable-line react-hooks/exhaustive-deps
}

/**
 * useCommunityComments(postId, active, handlers)
 * Join post_{postId} khi active=true, leave khi false/unmount.
 * Nhận NewComment/CommentUpdated/CommentDeleted.
 */
export function useCommunityComments(postId, active, handlers) {
  const ref = useRef(handlers);
  useEffect(() => { ref.current = handlers; });

  useEffect(() => {
    if (!postId || !active) return;
    joinGroup('JoinPost', postId);
    return () => leaveGroup('LeavePost', postId);
  }, [postId, active]);

  useEffect(() => {
    if (!active) return;
    const unsubs = [
      subscribe('NewComment',     (...a) => ref.current.onNewComment?.(...a)),
      subscribe('CommentUpdated', (...a) => ref.current.onCommentUpdated?.(...a)),
      subscribe('CommentDeleted', (...a) => ref.current.onCommentDeleted?.(...a)),
    ];
    return () => unsubs.forEach(fn => fn());
  }, [active]);
}

/**
 * useCommunityHub — backward compat alias.
 * Giữ để không crash nếu còn import cũ.
 */
export function useCommunityHub(handlers = {}, options = {}) {
  const { joinFeed = true, joinPostId = null } = options;
  useCommunityFeed(joinFeed ? handlers : {});
  useCommunityComments(joinPostId, !!joinPostId, handlers);
}