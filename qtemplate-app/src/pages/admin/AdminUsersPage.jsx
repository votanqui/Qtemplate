import { useState, useEffect, useCallback } from 'react';
import { adminUserApi, adminNotificationApi } from '../../api/adminApi';
import {
  fmt, fmtFull, fmtMoney,
  Chip, ActiveDot, PageHeader, FiltersBar, Card, Table, Pager,
  Modal, Field, Tabs, BtnPrimary, BtnSecondary, BtnDanger, BtnSuccess,
  Input, Select, Textarea, SearchInput, ConfirmModal, Empty, Toast,
  trBase, tdBase,
} from '../../components/ui/AdminUI';

const roleColor = { Admin: 'yellow', Customer: 'blue' };

// ── User detail modal ────────────────────────────────────────
function UserModal({ userId, onClose, onRefresh }) {
  const [user,   setUser]   = useState(null);
  const [orders, setOrders] = useState([]);
  const [tab,    setTab]    = useState('info');
  const [busy,   setBusy]   = useState(false);
  const [loading,setLoading]= useState(true);
  const [toast,  setToast]  = useState('');

  const [banReason, setBanReason] = useState('');
  const [newRole,   setNewRole]   = useState('Customer');
  const [notif, setNotif] = useState({ title: '', message: '', url: '' });

  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };

  useEffect(() => {
    if (!userId) return;
    setLoading(true); setTab('info'); setBanReason('');
    Promise.all([
      adminUserApi.getDetail(userId),
      adminUserApi.getUserOrders(userId, 1, 10),
    ]).then(([u, o]) => {
      const d = u.data.data;
      setUser(d);
      setNewRole(d?.role || 'Customer');
      setOrders(o.data.data?.items || []);
    }).finally(() => setLoading(false));
  }, [userId]);

  const doToggleBan = async () => {
    const next = !user.isActive;
    if (!next && !banReason.trim()) return alert('Nhập lý do khoá.');
    setBusy(true);
    try {
      await adminUserApi.changeStatus(userId, next, next ? null : banReason.trim());
      setUser(u => ({ ...u, isActive: next }));
      setBanReason(''); onRefresh();
      ok(next ? '✅ Đã mở khoá tài khoản' : '🚫 Đã khoá tài khoản');
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  const doChangeRole = async () => {
    if (newRole === user.role) return;
    setBusy(true);
    try {
      await adminUserApi.changeRole(userId, newRole);
      setUser(u => ({ ...u, role: newRole }));
      onRefresh(); ok('✓ Đã đổi role');
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  const doSendNotif = async () => {
    if (!notif.title || !notif.message) return alert('Điền tiêu đề và nội dung.');
    setBusy(true);
    try {
      await adminNotificationApi.send({ userId, ...notif });
      setNotif({ title: '', message: '', url: '' });
      ok('📨 Đã gửi thông báo');
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  return (
    <Modal open={!!userId} onClose={onClose} title="Chi tiết người dùng" width={620}>
      <Toast msg={toast} />
      {loading ? (
        <div className="text-center text-slate-400 py-12">Đang tải...</div>
      ) : !user ? (
        <div className="text-center text-red-500 py-8">Không tìm thấy.</div>
      ) : (
        <>
          {/* User header */}
          <div className="flex items-center gap-4 p-4 bg-slate-50 rounded-2xl mb-5">
            <div className="w-12 h-12 rounded-2xl bg-gradient-to-br from-indigo-500 to-violet-600 flex items-center justify-center text-white text-xl font-extrabold flex-shrink-0">
              {user.fullName?.[0]?.toUpperCase() || '?'}
            </div>
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 flex-wrap mb-1">
                <span className="text-[15px] font-extrabold text-slate-900">{user.fullName}</span>
                <Chip label={user.role} color={roleColor[user.role] || 'slate'} />
                <ActiveDot active={user.isActive} />
              </div>
              <div className="text-[12px] text-slate-500">{user.email}</div>
            </div>
          </div>

          <Tabs
            tabs={[['info', 'Thông tin'], ['actions', 'Thao tác'], ['orders', `Đơn hàng (${orders.length})`], ['notif', 'Thông báo']]}
            active={tab} onChange={setTab}
          />

          {/* INFO */}
          {tab === 'info' && (
            <div className="grid grid-cols-2 gap-3">
              {[
                ['ID', user.id],
                ['Email', user.email],
                ['Họ tên', user.fullName],
                ['Điện thoại', user.phone || '—'],
                ['Role', <Chip label={user.role} color={roleColor[user.role] || 'slate'} />],
                ['Trạng thái', <ActiveDot active={user.isActive} />],
                ['Ngày tạo', fmtFull(user.createdAt)],
                ['Đăng nhập cuối', fmtFull(user.lastLoginAt)],
              ].map(([lbl, val]) => (
                <div key={lbl} className="p-3 bg-slate-50 rounded-xl">
                  <div className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1">{lbl}</div>
                  <div className="text-[13px] font-semibold text-slate-900 break-all">{val}</div>
                </div>
              ))}
            </div>
          )}

          {/* ACTIONS */}
          {tab === 'actions' && (
            <div className="flex flex-col gap-4">
              {/* Role */}
              <div className="p-4 rounded-2xl border border-slate-200">
                <div className="text-[13px] font-bold text-slate-900 mb-3">🔑 Đổi Role</div>
                <div className="flex gap-2">
                  <Select value={newRole} onChange={e => setNewRole(e.target.value)} className="flex-1">
                    <option value="Customer">Customer</option>
                    <option value="Admin">Admin</option>
                  </Select>
                  <BtnPrimary onClick={doChangeRole} disabled={busy || newRole === user.role}>
                    {busy ? '…' : 'Lưu'}
                  </BtnPrimary>
                </div>
              </div>

              {/* Ban / Unban */}
              <div className={`p-4 rounded-2xl border ${user.isActive ? 'border-red-200 bg-red-50/50' : 'border-green-200 bg-green-50/50'}`}>
                <div className="text-[13px] font-bold text-slate-900 mb-3">
                  {user.isActive ? '🚫 Khoá tài khoản' : '✅ Mở khoá tài khoản'}
                </div>
                {user.isActive && (
                  <Textarea
                    value={banReason}
                    onChange={e => setBanReason(e.target.value)}
                    rows={2}
                    placeholder="Lý do khoá (bắt buộc)…"
                    className="mb-3 border-red-200"
                  />
                )}
                {user.isActive
                  ? <BtnDanger onClick={doToggleBan} disabled={busy}>{busy ? '…' : 'Khoá ngay'}</BtnDanger>
                  : <BtnSuccess onClick={doToggleBan} disabled={busy}>{busy ? '…' : 'Mở khoá'}</BtnSuccess>
                }
              </div>
            </div>
          )}

          {/* ORDERS */}
          {tab === 'orders' && (
            orders.length === 0
              ? <div className="text-center text-slate-400 py-10">Chưa có đơn hàng.</div>
              : <div className="flex flex-col gap-2">
                  {orders.map(o => (
                    <div key={o.id} className="flex items-center justify-between p-3 bg-slate-50 rounded-xl">
                      <div>
                        <div className="text-[12px] font-bold text-slate-900">{o.orderCode || o.id?.slice(0, 10)}</div>
                        <div className="text-[11px] text-slate-400 mt-0.5">{fmtFull(o.createdAt)}</div>
                      </div>
                      <div className="text-right">
                        <div className="text-[14px] font-extrabold text-slate-900">{fmtMoney(o.totalAmount)}</div>
                        <Chip label={o.status} color={o.status === 'Paid' ? 'green' : 'yellow'} />
                      </div>
                    </div>
                  ))}
                </div>
          )}

          {/* NOTIF */}
          {tab === 'notif' && (
            <div className="flex flex-col gap-3">
              <Field label="Tiêu đề" required>
                <Input value={notif.title} onChange={e => setNotif(f => ({ ...f, title: e.target.value }))} placeholder="Tiêu đề thông báo" />
              </Field>
              <Field label="Nội dung" required>
                <Textarea value={notif.message} onChange={e => setNotif(f => ({ ...f, message: e.target.value }))} rows={3} placeholder="Nội dung…" />
              </Field>
              <Field label="URL (tuỳ chọn)">
                <Input value={notif.url} onChange={e => setNotif(f => ({ ...f, url: e.target.value }))} placeholder="https://…" />
              </Field>
              <BtnPrimary onClick={doSendNotif} disabled={busy}>
                {busy ? '…' : '📨 Gửi thông báo cho user này'}
              </BtnPrimary>
            </div>
          )}
        </>
      )}
    </Modal>
  );
}

// ── Main page ─────────────────────────────────────────────────
export default function AdminUsersPage() {
  const [users,   setUsers]   = useState([]);
  const [total,   setTotal]   = useState(0);
  const [loading, setLoading] = useState(true);
  const [page,    setPage]    = useState(1);
  const [rawQ,    setRawQ]    = useState('');
  const [search,  setSearch]  = useState('');
  const [role,    setRole]    = useState('');
  const [status,  setStatus]  = useState('');
  const [sel,     setSel]     = useState(null);
  const pageSize = 20;

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const p = { page, pageSize };
      if (search) p.search = search;
      if (role)   p.role   = role;
      if (status) p.isActive = status;
      const r = await adminUserApi.getList(p);
      const d = r.data.data;
      setUsers(d?.items || d || []);
      setTotal(d?.totalCount ?? 0);
    } catch { setUsers([]); }
    finally { setLoading(false); }
  }, [page, search, role, status]);

  useEffect(() => { load(); }, [load]);

  useEffect(() => {
    const t = setTimeout(() => { setSearch(rawQ); setPage(1); }, 380);
    return () => clearTimeout(t);
  }, [rawQ]);

  return (
    <div>
      <PageHeader
        title="Người dùng"
        sub={`${total} tài khoản`}
      />

      <FiltersBar>
        <SearchInput
          value={rawQ}
          onChange={e => setRawQ(e.target.value)}
          placeholder="Tên, email…"
          className="flex-1 min-w-[180px] max-w-xs"
        />
        <Select value={role} onChange={e => { setRole(e.target.value); setPage(1); }}>
          <option value="">Tất cả role</option>
          <option value="Admin">Admin</option>
          <option value="Customer">Customer</option>
        </Select>
        <Select value={status} onChange={e => { setStatus(e.target.value); setPage(1); }}>
          <option value="">Tất cả trạng thái</option>
          <option value="true">Hoạt động</option>
          <option value="false">Đã khoá</option>
        </Select>
        <span className="ml-auto text-[12px] text-slate-400">
          {loading ? 'Đang tải…' : `${total} người dùng`}
        </span>
      </FiltersBar>

      <Card>
        <Table
          heads={['Người dùng', 'Email', 'Role', 'Trạng thái', 'Ngày đăng ký', 'Đăng nhập cuối', '']}
          loading={loading}
          colCount={7}
        >
          {users.length === 0 && !loading
            ? <Empty msg="Không tìm thấy người dùng nào." />
            : users.map(u => (
              <tr key={u.id} className={trBase} onClick={() => setSel(u.id)}>
                <td className={tdBase}>
                  <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded-xl bg-gradient-to-br from-indigo-400 to-violet-500 flex items-center justify-center text-white text-[12px] font-bold flex-shrink-0">
                      {u.fullName?.[0]?.toUpperCase() || '?'}
                    </div>
                    <span className="font-semibold text-slate-900">{u.fullName}</span>
                  </div>
                </td>
                <td className={`${tdBase} text-slate-500`}>{u.email}</td>
                <td className={tdBase}><Chip label={u.role} color={roleColor[u.role] || 'slate'} /></td>
                <td className={tdBase}><ActiveDot active={u.isActive} /></td>
                <td className={`${tdBase} text-slate-400`}>{fmt(u.createdAt)}</td>
                <td className={`${tdBase} text-slate-400`}>{fmtFull(u.lastLoginAt)}</td>
                <td className={tdBase} onClick={e => e.stopPropagation()}>
                  <BtnSecondary className="py-1 px-3 text-[12px]" onClick={() => setSel(u.id)}>
                    Mở
                  </BtnSecondary>
                </td>
              </tr>
            ))
          }
        </Table>
        <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
      </Card>

      <UserModal userId={sel} onClose={() => setSel(null)} onRefresh={load} />
    </div>
  );
}