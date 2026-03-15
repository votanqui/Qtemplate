import { useState, useEffect, useCallback } from 'react';
import { adminLogApi } from '../../api/adminApi';
import {
  fmt, fmtFull, Chip, ActiveDot,
  PageHeader, FiltersBar, Card, Table, Pager,
  SearchInput, Select, Empty, Modal,
  trBase, tdBase, Tabs,
} from '../../components/ui/AdminUI';

// ── helpers ───────────────────────────────────────────────────
function StatusBadge({ code }) {
  const color = code >= 500 ? 'red' : code >= 400 ? 'orange' : code >= 300 ? 'yellow' : 'green';
  return <Chip label={String(code)} color={color} />;
}
function MethodBadge({ method }) {
  const color = { GET: 'blue', POST: 'green', PUT: 'yellow', PATCH: 'orange', DELETE: 'red' };
  return <Chip label={method} color={color[method] || 'slate'} />;
}

// ── Request Logs tab ──────────────────────────────────────────
function RequestLogsTab() {
  const [items,   setItems]   = useState([]);
  const [total,   setTotal]   = useState(0);
  const [loading, setLoading] = useState(true);
  const [page,    setPage]    = useState(1);
  const [search,  setSearch]  = useState('');
  const [ip,      setIp]      = useState('');
  const [status,  setStatus]  = useState('');
  const pageSize = 50;

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const params = { page, pageSize };
      if (ip)     params.ip         = ip;
      if (search) params.endpoint   = search;
      if (status) params.statusCode = Number(status);
      const r = await adminLogApi.getRequestLogs(params);
      const d = r.data.data;
      setItems(d?.items || []);
      setTotal(d?.totalCount ?? 0);
    } catch { setItems([]); }
    finally { setLoading(false); }
  }, [page, ip, search, status]);

  useEffect(() => { setPage(1); }, [ip, search, status]);
  useEffect(() => { load(); }, [load]);

  return (
    <>
      <FiltersBar>
        <SearchInput value={search} onChange={e => setSearch(e.target.value)} placeholder="Tìm endpoint…" className="w-56" />
        <SearchInput value={ip} onChange={e => setIp(e.target.value)} placeholder="Lọc IP…" className="w-40 font-mono text-[12px]" />
        <Select value={status} onChange={e => setStatus(e.target.value)}>
          <option value="">Tất cả status</option>
          <option value="200">200 OK</option>
          <option value="400">400 Bad Request</option>
          <option value="401">401 Unauthorized</option>
          <option value="403">403 Forbidden</option>
          <option value="404">404 Not Found</option>
          <option value="500">500 Server Error</option>
        </Select>
        <span className="ml-auto text-[12px] text-slate-400">{total} requests</span>
      </FiltersBar>
      <Card>
        <Table heads={['Method', 'Endpoint', 'Status', 'IP', 'Response (ms)', 'User', 'Thời gian']} loading={loading} colCount={7}>
          {items.length === 0 && !loading ? <Empty msg="Không có request log." /> : items.map(r => (
            <tr key={r.id} className={trBase}>
              <td className={tdBase}><MethodBadge method={r.method} /></td>
              <td className={tdBase}>
                <div className="font-mono text-[11px] text-slate-700 truncate max-w-[280px]" title={r.endpoint}>{r.endpoint}</div>
                {r.referer && <div className="font-mono text-[10px] text-slate-300 truncate max-w-[280px]">{r.referer}</div>}
              </td>
              <td className={tdBase}><StatusBadge code={r.statusCode} /></td>
              <td className={`${tdBase} font-mono text-[12px] text-slate-500`}>{r.ipAddress}</td>
              <td className={tdBase}>
                <span className={`font-mono text-[12px] font-semibold ${r.responseTimeMs > 1000 ? 'text-red-500' : r.responseTimeMs > 300 ? 'text-amber-500' : 'text-green-600'}`}>
                  {r.responseTimeMs}ms
                </span>
              </td>
              <td className={`${tdBase} text-[11px] font-mono text-slate-400 truncate max-w-[100px]`}>{r.userId || '—'}</td>
              <td className={`${tdBase} text-[11px] text-slate-400`}>{fmtFull(r.createdAt)}</td>
            </tr>
          ))}
        </Table>
        <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
      </Card>
    </>
  );
}

// ── Email Logs tab ────────────────────────────────────────────
function EmailLogsTab() {
  const [items,    setItems]    = useState([]);
  const [total,    setTotal]    = useState(0);
  const [loading,  setLoading]  = useState(true);
  const [page,     setPage]     = useState(1);
  const [status,   setStatus]   = useState('');
  const [template, setTemplate] = useState('');
  const pageSize = 20;

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const params = { page, pageSize };
      if (status)   params.status   = status;
      if (template) params.template = template;
      const r = await adminLogApi.getEmailLogs(params);
      const d = r.data.data;
      setItems(d?.items || []);
      setTotal(d?.totalCount ?? 0);
    } catch { setItems([]); }
    finally { setLoading(false); }
  }, [page, status, template]);

  useEffect(() => { setPage(1); }, [status, template]);
  useEffect(() => { load(); }, [load]);

  const statusColor = { Sent: 'green', Failed: 'red', Pending: 'yellow', Retrying: 'orange' };

  return (
    <>
      <FiltersBar>
        <Select value={status} onChange={e => setStatus(e.target.value)}>
          <option value="">Tất cả trạng thái</option>
          <option value="Sent">Sent</option>
          <option value="Failed">Failed</option>
          <option value="Pending">Pending</option>
          <option value="Retrying">Retrying</option>
        </Select>
        <SearchInput value={template} onChange={e => setTemplate(e.target.value)} placeholder="Tìm template…" className="w-48" />
        <span className="ml-auto text-[12px] text-slate-400">{total} emails</span>
      </FiltersBar>
      <Card>
        <Table heads={['To', 'Subject', 'Template', 'Trạng thái', 'Retry', 'Gửi lúc', 'Tạo lúc']} loading={loading} colCount={7}>
          {items.length === 0 && !loading ? <Empty msg="Không có email log." /> : items.map(e => (
            <tr key={e.id} className={trBase}>
              <td className={tdBase}>
                <div className="text-[12px] font-semibold text-slate-800">{e.to}</div>
                {e.cc && <div className="text-[10px] text-slate-400">CC: {e.cc}</div>}
              </td>
              <td className={`${tdBase} max-w-[200px]`}>
                <div className="text-[12px] truncate text-slate-700">{e.subject}</div>
              </td>
              <td className={tdBase}><Chip label={e.template} color="purple" /></td>
              <td className={tdBase}>
                <Chip label={e.status} color={statusColor[e.status] || 'slate'} />
                {e.errorMessage && (
                  <div className="text-[10px] text-red-400 mt-1 truncate max-w-[120px]" title={e.errorMessage}>{e.errorMessage}</div>
                )}
              </td>
              <td className={`${tdBase} text-center text-[12px] font-mono`}>
                {e.retryCount > 0 ? <span className="text-amber-500 font-bold">{e.retryCount}</span> : <span className="text-slate-300">0</span>}
              </td>
              <td className={`${tdBase} text-[11px] text-slate-400`}>{e.sentAt ? fmtFull(e.sentAt) : '—'}</td>
              <td className={`${tdBase} text-[11px] text-slate-400`}>{fmtFull(e.createdAt)}</td>
            </tr>
          ))}
        </Table>
        <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
      </Card>
    </>
  );
}

// ── Refresh Tokens tab ────────────────────────────────────────
function RefreshTokensTab() {
  const [items,    setItems]    = useState([]);
  const [total,    setTotal]    = useState(0);
  const [loading,  setLoading]  = useState(true);
  const [page,     setPage]     = useState(1);
  const [isActive, setIsActive] = useState('');
  const pageSize = 20;

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const params = { page, pageSize };
      if (isActive !== '') params.isActive = isActive === 'true';
      const r = await adminLogApi.getRefreshTokens(params);
      const d = r.data.data;
      setItems(d?.items || []);
      setTotal(d?.totalCount ?? 0);
    } catch { setItems([]); }
    finally { setLoading(false); }
  }, [page, isActive]);

  useEffect(() => { setPage(1); }, [isActive]);
  useEffect(() => { load(); }, [load]);

  return (
    <>
      <FiltersBar>
        <Select value={isActive} onChange={e => setIsActive(e.target.value)}>
          <option value="">Tất cả</option>
          <option value="true">Đang active</option>
          <option value="false">Đã revoke / hết hạn</option>
        </Select>
        <span className="ml-auto text-[12px] text-slate-400">{total} tokens</span>
      </FiltersBar>
      <Card>
        <Table heads={['User', 'Token', 'IP', 'Trạng thái', 'Hết hạn', 'Revoke lúc', 'Tạo lúc']} loading={loading} colCount={7}>
          {items.length === 0 && !loading ? <Empty msg="Không có refresh token." /> : items.map(t => (
            <tr key={t.id} className={trBase}>
              <td className={tdBase}>
                <div className="text-[12px] font-semibold text-slate-800">{t.userEmail || '—'}</div>
                <div className="text-[10px] font-mono text-slate-400 truncate max-w-[120px]">{String(t.userId)}</div>
              </td>
              <td className={tdBase}>
                <div className="font-mono text-[10px] text-slate-400 truncate max-w-[140px]" title={t.token}>{t.token?.slice(0, 20)}…</div>
              </td>
              <td className={`${tdBase} font-mono text-[12px] text-slate-500`}>{t.ipAddress || '—'}</td>
              <td className={tdBase}>
                {t.isActive ? <ActiveDot active={true} onLabel="Active" /> : t.isRevoked ? <Chip label="Revoked" color="red" /> : <Chip label="Expired" color="slate" />}
                {t.revokedReason && <div className="text-[10px] text-slate-400 mt-0.5 truncate max-w-[100px]">{t.revokedReason}</div>}
              </td>
              <td className={`${tdBase} text-[11px] text-slate-400`}>{fmt(t.expiresAt)}</td>
              <td className={`${tdBase} text-[11px] text-slate-400`}>{t.revokedAt ? fmtFull(t.revokedAt) : '—'}</td>
              <td className={`${tdBase} text-[11px] text-slate-400`}>{fmtFull(t.createdAt)}</td>
            </tr>
          ))}
        </Table>
        <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
      </Card>
    </>
  );
}

// ── Audit Logs tab ────────────────────────────────────────────
const ACTION_COLORS = {
  Create: 'green', Update: 'blue', Delete: 'red',
  Login: 'purple', Approve: 'teal', Reject: 'orange',
};

function JsonDiffViewer({ oldVal, newVal }) {
  const parse = (str) => {
    if (!str) return null;
    try { return JSON.parse(str); } catch { return str; }
  };

  const old = parse(oldVal);
  const nw  = parse(newVal);

  const renderJson = (data, compareWith, highlight) => {
    if (data === null || data === undefined) return <span className="text-slate-300 italic">null</span>;
    if (typeof data !== 'object') return (
      <span className={highlight ? 'bg-yellow-100 text-yellow-800 rounded px-1' : 'text-slate-700'}>{String(data)}</span>
    );

    const keys = [...new Set([...Object.keys(data), ...(compareWith ? Object.keys(compareWith) : [])])];
    return (
      <div className="flex flex-col gap-0.5">
        {keys.map(k => {
          const changed = compareWith && JSON.stringify(data[k]) !== JSON.stringify(compareWith[k]);
          return (
            <div key={k} className={`flex gap-2 text-[11px] rounded px-1 py-0.5 ${changed ? 'bg-yellow-50' : ''}`}>
              <span className="text-slate-400 font-mono flex-shrink-0">{k}:</span>
              <span className={`font-mono break-all ${changed ? 'font-bold text-yellow-700' : 'text-slate-600'}`}>
                {data[k] === null ? <span className="italic text-slate-300">null</span> : String(data[k])}
              </span>
            </div>
          );
        })}
      </div>
    );
  };

  if (!old && !nw) return <div className="text-slate-400 text-[12px] italic">Không có dữ liệu chi tiết.</div>;

  return (
    <div className={`grid gap-4 ${old && nw ? 'grid-cols-2' : 'grid-cols-1'}`}>
      {old && (
        <div>
          <div className="text-[10px] font-bold text-red-400 uppercase tracking-widest mb-2">← Trước</div>
          <div className="bg-red-50 border border-red-100 rounded-xl p-3">{renderJson(old, nw, false)}</div>
        </div>
      )}
      {nw && (
        <div>
          <div className="text-[10px] font-bold text-green-500 uppercase tracking-widest mb-2">Sau →</div>
          <div className="bg-green-50 border border-green-100 rounded-xl p-3">{renderJson(nw, old, false)}</div>
        </div>
      )}
    </div>
  );
}

function AuditDetailModal({ log, onClose }) {
  if (!log) return null;
  return (
    <Modal open={!!log} onClose={onClose} title={`Audit Log #${log.id}`} width={640}>
      <div className="flex flex-col gap-4">
        {/* Header info */}
        <div className="grid grid-cols-2 gap-3">
          {[
            ['Action',  <Chip key="a" label={log.action} color={ACTION_COLORS[log.action] || 'slate'} />],
            ['Entity',  <span key="e" className="font-mono text-[12px] font-bold text-slate-800">{log.entityName}{log.entityId ? ` #${log.entityId}` : ''}</span>],
            ['Admin',   <span key="u" className="text-[12px] text-slate-700">{log.userEmail || '—'}</span>],
            ['IP',      <span key="i" className="font-mono text-[12px] text-slate-500">{log.ipAddress || '—'}</span>],
            ['Thời gian', <span key="t" className="text-[12px] text-slate-500">{fmtFull(log.createdAt)}</span>],
          ].map(([label, val]) => (
            <div key={label} className="bg-slate-50 rounded-xl px-3 py-2">
              <div className="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">{label}</div>
              {val}
            </div>
          ))}
        </div>

        {/* Diff */}
        {(log.oldValues || log.newValues) && (
          <div>
            <div className="text-[11px] font-bold text-slate-500 uppercase tracking-widest mb-2">Thay đổi</div>
            <JsonDiffViewer oldVal={log.oldValues} newVal={log.newValues} />
          </div>
        )}

        {/* User agent */}
        {log.userAgent && (
          <div className="bg-slate-50 rounded-xl px-3 py-2">
            <div className="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">User Agent</div>
            <div className="font-mono text-[11px] text-slate-500 break-all">{log.userAgent}</div>
          </div>
        )}
      </div>
    </Modal>
  );
}

function AuditLogsTab() {
  const [items,      setItems]      = useState([]);
  const [total,      setTotal]      = useState(0);
  const [loading,    setLoading]    = useState(true);
  const [page,       setPage]       = useState(1);
  const [userEmail,  setUserEmail]  = useState('');
  const [action,     setAction]     = useState('');
  const [entityName, setEntityName] = useState('');
  const [from,       setFrom]       = useState('');
  const [to,         setTo]         = useState('');
  const [detail,     setDetail]     = useState(null);
  const pageSize = 50;

  // Lấy danh sách entityName unique từ items để populate filter
  const entityNames = [...new Set(items.map(i => i.entityName).filter(Boolean))].sort();

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const params = { page, pageSize };
      if (userEmail)  params.userEmail  = userEmail;
      if (action)     params.action     = action;
      if (entityName) params.entityName = entityName;
      if (from)       params.from       = from;
      if (to)         params.to         = to;
      const r = await adminLogApi.getAuditLogs(params);
      const d = r.data.data;
      setItems(d?.items || []);
      setTotal(d?.totalCount ?? 0);
    } catch { setItems([]); }
    finally { setLoading(false); }
  }, [page, userEmail, action, entityName, from, to]);

  useEffect(() => { setPage(1); }, [userEmail, action, entityName, from, to]);
  useEffect(() => { load(); }, [load]);

  return (
    <>
      <FiltersBar>
        <SearchInput
          value={userEmail}
          onChange={e => setUserEmail(e.target.value)}
          placeholder="Email admin…"
          className="w-52"
        />
        <Select value={action} onChange={e => setAction(e.target.value)}>
          <option value="">Tất cả action</option>
          {['Create','Update','Delete','Login','Approve','Reject','ChangeStatus','Publish','SetSale'].map(a => (
            <option key={a} value={a}>{a}</option>
          ))}
        </Select>
        <Select value={entityName} onChange={e => setEntityName(e.target.value)}>
          <option value="">Tất cả entity</option>
          {entityNames.map(n => <option key={n} value={n}>{n}</option>)}
        </Select>
        <div className="flex items-center gap-1.5">
          <input
            type="date" value={from} onChange={e => setFrom(e.target.value)}
            className="border border-slate-200 rounded-xl px-2.5 py-1.5 text-[12px] text-slate-700 focus:outline-none focus:border-slate-400"
          />
          <span className="text-slate-300 text-[11px]">→</span>
          <input
            type="date" value={to} onChange={e => setTo(e.target.value)}
            className="border border-slate-200 rounded-xl px-2.5 py-1.5 text-[12px] text-slate-700 focus:outline-none focus:border-slate-400"
          />
        </div>
        <span className="ml-auto text-[12px] text-slate-400">{total} records</span>
      </FiltersBar>

      <Card>
        <Table
          heads={['Action', 'Entity', 'Entity ID', 'Admin', 'IP', 'Có diff?', 'Thời gian']}
          loading={loading}
          colCount={7}
        >
          {items.length === 0 && !loading
            ? <Empty msg="Không có audit log." />
            : items.map(log => (
              <tr
                key={log.id}
                className={`${trBase} cursor-pointer`}
                onClick={() => setDetail(log)}
              >
                <td className={tdBase}>
                  <Chip label={log.action} color={ACTION_COLORS[log.action] || 'slate'} />
                </td>
                <td className={tdBase}>
                  <span className="text-[12px] font-semibold text-slate-800">{log.entityName}</span>
                </td>
                <td className={`${tdBase} font-mono text-[11px] text-slate-400`}>
                  {log.entityId
                    ? <span title={log.entityId}>{log.entityId.length > 12 ? log.entityId.slice(0, 12) + '…' : log.entityId}</span>
                    : '—'}
                </td>
                <td className={tdBase}>
                  <div className="text-[12px] text-slate-700">{log.userEmail || '—'}</div>
                </td>
                <td className={`${tdBase} font-mono text-[11px] text-slate-400`}>{log.ipAddress || '—'}</td>
                <td className={`${tdBase} text-center`}>
                  {(log.oldValues || log.newValues)
                    ? <span className="text-[11px] font-bold text-indigo-500">✦ Xem diff</span>
                    : <span className="text-slate-200 text-[11px]">—</span>
                  }
                </td>
                <td className={`${tdBase} text-[11px] text-slate-400`}>{fmtFull(log.createdAt)}</td>
              </tr>
            ))
          }
        </Table>
        <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
      </Card>

      <AuditDetailModal log={detail} onClose={() => setDetail(null)} />
    </>
  );
}

// ── Main page ─────────────────────────────────────────────────
export default function AdminLogsPage() {
  const [tab, setTab] = useState('request');

  return (
    <div>
      <PageHeader title="Logs hệ thống" />

      <Tabs
        tabs={[
          ['request', '🌐 Request Logs'],
          ['email',   '📧 Email Logs'],
          ['token',   '🔑 Refresh Tokens'],
          ['audit',   '📝 Audit Logs'],
        ]}
        active={tab}
        onChange={setTab}
      />

      {tab === 'request' && <RequestLogsTab />}
      {tab === 'email'   && <EmailLogsTab />}
      {tab === 'token'   && <RefreshTokensTab />}
      {tab === 'audit'   && <AuditLogsTab />}
    </div>
  );
}