import { useState, useEffect, useCallback } from 'react';
import { adminNotificationApi } from '../../api/adminApi';
import {
  PageHeader, Card, Field, Input, Select, Textarea,
  BtnPrimary, BtnSecondary, Toast, Tabs,
  FiltersBar, Table, Pager, Chip, SearchInput,
  Empty, trBase, tdBase, fmtFull,
} from '../../components/ui/AdminUI';

const TYPE_OPTIONS = [
  { value: 'Info',    label: 'ℹ️ Info',    desc: 'Thông tin chung' },
  { value: 'Success', label: '✅ Success', desc: 'Thành công, khen thưởng' },
  { value: 'Warning', label: '⚠️ Warning', desc: 'Cảnh báo, lưu ý' },
];

const TEMPLATES = [
  { label: 'Tuỳ chỉnh', title: '', message: '' },
  { label: 'Bảo trì hệ thống', title: 'Thông báo bảo trì', message: 'Hệ thống sẽ bảo trì từ 23:00 - 01:00 ngày hôm nay. Xin lỗi vì sự bất tiện.' },
  { label: 'Template mới', title: 'Template mới đã ra mắt! 🎉', message: 'Chúng tôi vừa ra mắt bộ template mới. Khám phá ngay tại trang Templates!' },
  { label: 'Khuyến mãi', title: 'Ưu đãi đặc biệt 🔥', message: 'Giảm giá đến 50% tất cả template trong tuần này. Mua ngay kẻo lỡ!' },
];

const empty = { userId: '', title: '', message: '', type: 'Info', redirectUrl: '' };

const TYPE_COLOR = { Info: 'blue', Success: 'green', Warning: 'orange' };

// ── Send Tab ──────────────────────────────────────────────────
function SendTab() {
  const [form,    setForm]    = useState(empty);
  const [busy,    setBusy]    = useState(false);
  const [toast,   setToast]   = useState('');
  const [history, setHistory] = useState([]);

  const ok  = msg => { setToast(msg); setTimeout(() => setToast(''), 2800); };
  const set = (k, v) => setForm(f => ({ ...f, [k]: v }));

  const applyTemplate = (tpl) => {
    if (!tpl.title) return;
    setForm(f => ({ ...f, title: tpl.title, message: tpl.message }));
  };

  const doSend = async () => {
    if (!form.title.trim())   return alert('Nhập tiêu đề thông báo.');
    if (!form.message.trim()) return alert('Nhập nội dung thông báo.');
    setBusy(true);
    try {
      await adminNotificationApi.send({
        userId:      form.userId.trim() || null,
        title:       form.title.trim(),
        message:     form.message.trim(),
        type:        form.type,
        redirectUrl: form.redirectUrl.trim() || null,
      });
      const isBroadcast = !form.userId.trim();
      const msg = isBroadcast ? '📢 Đã broadcast đến tất cả user' : '📨 Đã gửi thông báo';
      ok(msg);
      setHistory(h => [
        {
          id: Date.now(),
          ...form,
          isBroadcast,
          sentAt: new Date().toLocaleString('vi-VN'),
        },
        ...h.slice(0, 9),
      ]);
      setForm(empty);
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi gửi thông báo.'); }
    finally { setBusy(false); }
  };

  const doReset = () => setForm(empty);

  const isBroadcast = !form.userId.trim();
  const typeInfo    = TYPE_OPTIONS.find(t => t.value === form.type);

  return (
    <>
      <Toast msg={toast} />
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">

        {/* ── Form chính ── */}
        <div className="lg:col-span-2 flex flex-col gap-4">

          {/* Quick templates */}
          <Card>
            <div className="px-4 py-3">
              <div className="text-[11px] font-bold text-slate-400 uppercase tracking-widest mb-3">Quick templates</div>
              <div className="flex flex-wrap gap-2">
                {TEMPLATES.slice(1).map(tpl => (
                  <button
                    key={tpl.label}
                    onClick={() => applyTemplate(tpl)}
                    className="px-3 py-1.5 rounded-xl bg-slate-100 text-slate-700 text-[12px] font-semibold hover:bg-slate-900 hover:text-white transition-colors"
                  >
                    {tpl.label}
                  </button>
                ))}
              </div>
            </div>
          </Card>

          {/* Form */}
          <Card>
            <div className="px-4 py-4 flex flex-col gap-3">
              <Field label="User ID (để trống = broadcast tất cả user)">
                <Input
                  value={form.userId}
                  onChange={e => set('userId', e.target.value)}
                  placeholder="UUID user — để trống để gửi tất cả"
                  className="font-mono text-[12px]"
                />
                {isBroadcast && (
                  <p className="text-[11px] text-amber-500 mt-1 font-semibold">
                    ⚠️ Sẽ broadcast đến TẤT CẢ user đang online
                  </p>
                )}
              </Field>

              <div className="grid grid-cols-2 gap-3">
                <Field label="Tiêu đề" required>
                  <Input
                    value={form.title}
                    onChange={e => set('title', e.target.value)}
                    placeholder="Tiêu đề thông báo"
                  />
                </Field>
                <Field label="Loại thông báo">
                  <Select value={form.type} onChange={e => set('type', e.target.value)} className="w-full">
                    {TYPE_OPTIONS.map(t => (
                      <option key={t.value} value={t.value}>{t.label} — {t.desc}</option>
                    ))}
                  </Select>
                </Field>
              </div>

              <Field label="Nội dung" required>
                <Textarea
                  value={form.message}
                  onChange={e => set('message', e.target.value)}
                  rows={4}
                  placeholder="Nội dung thông báo sẽ hiển thị cho user…"
                />
              </Field>

              <Field label="Redirect URL (tuỳ chọn)">
                <Input
                  value={form.redirectUrl}
                  onChange={e => set('redirectUrl', e.target.value)}
                  placeholder="/dashboard/orders, /templates/ten-template…"
                />
              </Field>

              <div className="flex justify-end gap-2 pt-2 border-t border-slate-100">
                <BtnSecondary onClick={doReset}>Xoá form</BtnSecondary>
                <BtnPrimary onClick={doSend} disabled={busy}>
                  {busy ? '…' : isBroadcast ? '📢 Broadcast' : '📨 Gửi thông báo'}
                </BtnPrimary>
              </div>
            </div>
          </Card>
        </div>

        {/* ── Sidebar: preview + history ── */}
        <div className="flex flex-col gap-4">

          {/* Preview */}
          <Card>
            <div className="px-4 py-3">
              <div className="text-[11px] font-bold text-slate-400 uppercase tracking-widest mb-3">Preview</div>
              <div className={`rounded-2xl p-4 border-l-4 ${
                form.type === 'Success' ? 'bg-green-50 border-green-400'
                : form.type === 'Warning' ? 'bg-amber-50 border-amber-400'
                : 'bg-blue-50 border-blue-400'
              }`}>
                <div className="flex items-start gap-2">
                  <span className="text-lg">{typeInfo?.label?.split(' ')[0]}</span>
                  <div className="flex-1 min-w-0">
                    <div className="font-bold text-[13px] text-slate-900 leading-snug">
                      {form.title || <span className="text-slate-300 italic">Tiêu đề…</span>}
                    </div>
                    <div className="text-[12px] text-slate-600 mt-1 leading-relaxed">
                      {form.message || <span className="text-slate-300 italic">Nội dung…</span>}
                    </div>
                    {form.redirectUrl && (
                      <div className="text-[11px] text-blue-500 mt-1.5 truncate">
                        🔗 {form.redirectUrl}
                      </div>
                    )}
                  </div>
                </div>
              </div>
              <div className="mt-3 text-[11px] text-slate-400">
                Gửi đến: <span className="font-semibold text-slate-700">
                  {isBroadcast ? 'Tất cả user' : `User ${form.userId.slice(0, 8)}…`}
                </span>
              </div>
            </div>
          </Card>

          {/* Lịch sử gửi (session) */}
          {history.length > 0 && (
            <Card>
              <div className="px-4 py-3">
                <div className="text-[11px] font-bold text-slate-400 uppercase tracking-widest mb-3">
                  Đã gửi (phiên này)
                </div>
                <div className="flex flex-col gap-2">
                  {history.map(h => (
                    <div key={h.id} className="flex items-start gap-2 py-2 border-b border-slate-50 last:border-0">
                      <span className="text-[13px] flex-shrink-0">
                        {h.type === 'Success' ? '✅' : h.type === 'Warning' ? '⚠️' : 'ℹ️'}
                      </span>
                      <div className="flex-1 min-w-0">
                        <div className="text-[12px] font-semibold text-slate-800 truncate">{h.title}</div>
                        <div className="text-[10px] text-slate-400">
                          {h.isBroadcast ? 'Broadcast' : `→ ${h.userId.slice(0, 12)}…`} · {h.sentAt}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </Card>
          )}
        </div>
      </div>
    </>
  );
}

// ── History Tab ───────────────────────────────────────────────
function HistoryTab() {
  const [items,   setItems]   = useState([]);
  const [total,   setTotal]   = useState(0);
  const [loading, setLoading] = useState(true);
  const [page,    setPage]    = useState(1);
  const [search,  setSearch]  = useState('');
  const [type,    setType]    = useState('');
  const pageSize = 20;

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const params = { page, pageSize };
      if (search) params.search = search;
      if (type)   params.type   = type;
      const r = await adminNotificationApi.getHistory(params);
      const d = r.data.data;
      setItems(d?.items || d || []);
      setTotal(d?.totalCount ?? 0);
    } catch { setItems([]); }
    finally { setLoading(false); }
  }, [page, search, type]);

  useEffect(() => { setPage(1); }, [search, type]);
  useEffect(() => { load(); }, [load]);

  return (
    <>
      <FiltersBar>
        <SearchInput
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="Tìm tiêu đề, nội dung…"
          className="w-56"
        />
        <Select value={type} onChange={e => setType(e.target.value)}>
          <option value="">Tất cả loại</option>
          {TYPE_OPTIONS.map(t => (
            <option key={t.value} value={t.value}>{t.label}</option>
          ))}
        </Select>
        <span className="ml-auto text-[12px] text-slate-400">{total} thông báo</span>
      </FiltersBar>

      <Card>
        <Table
          heads={['Loại', 'Tiêu đề & Nội dung', 'Gửi đến', 'Đã đọc', 'Redirect', 'Thời gian']}
          loading={loading}
          colCount={6}
        >
          {items.length === 0 && !loading
            ? <Empty msg="Chưa có thông báo nào." />
            : items.map(n => (
              <tr key={n.id} className={trBase}>
                <td className={tdBase}>
                  <Chip label={n.type} color={TYPE_COLOR[n.type] || 'slate'} />
                </td>
                <td className={tdBase} style={{ maxWidth: 280 }}>
                  <div className="text-[12px] font-semibold text-slate-800 truncate">{n.title}</div>
                  <div className="text-[11px] text-slate-400 truncate mt-0.5">{n.message}</div>
                </td>
                <td className={tdBase}>
                  <div className="text-[12px] font-semibold text-slate-800">{n.userName || '—'}</div>
                  <div className="text-[11px] text-slate-400 truncate max-w-[140px]">{n.userEmail || '—'}</div>
                </td>
                <td className={tdBase}>
                  {n.isRead
                    ? <span className="text-[11px] font-semibold text-green-600">✓ Đã đọc</span>
                    : <span className="text-[11px] text-slate-400">Chưa đọc</span>
                  }
                </td>
                <td className={`${tdBase} max-w-[140px]`}>
                  {n.redirectUrl
                    ? <span className="font-mono text-[11px] text-blue-500 truncate block" title={n.redirectUrl}>{n.redirectUrl}</span>
                    : <span className="text-slate-300 text-[11px]">—</span>
                  }
                </td>
                <td className={`${tdBase} text-[11px] text-slate-400`}>{fmtFull(n.createdAt)}</td>
              </tr>
            ))
          }
        </Table>
        <Pager page={page} total={total} pageSize={pageSize} onChange={setPage} />
      </Card>
    </>
  );
}

// ── Main page ─────────────────────────────────────────────────
export default function AdminNotificationsPage() {
  const [tab, setTab] = useState('send');

  return (
    <div>
      <PageHeader title="Thông báo" sub="Gửi realtime đến user hoặc broadcast toàn hệ thống" />

      <Tabs
        tabs={[
          ['send',    '📨 Gửi thông báo'],
          ['history', '📋 Lịch sử'],
        ]}
        active={tab}
        onChange={setTab}
      />

      {tab === 'send'    && <SendTab />}
      {tab === 'history' && <HistoryTab />}
    </div>
  );
}