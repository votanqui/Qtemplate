import { useState, useEffect, useCallback } from 'react';
import { adminSettingApi } from '../../api/adminApi';
import {
  fmtFull,
  PageHeader, FiltersBar, Card, Select,
  BtnPrimary, BtnSecondary, Input, Textarea,
  Field, Toast,
} from '../../components/ui/AdminUI';
import SettingCreateModal from '../../modals/admin/SettingCreateModal';

function SettingRow({ item, onSaved }) {
  const [editing, setEditing] = useState(false);
  const [val,     setVal]     = useState(item.value ?? '');
  const [busy,    setBusy]    = useState(false);
  const isLong = (item.value?.length ?? 0) > 80;

  const doSave = async () => {
    setBusy(true);
    try {
      await adminSettingApi.updateOne(item.key, val);
      setEditing(false);
      onSaved();
    } catch (e) { alert(e?.response?.data?.message || 'Lỗi.'); }
    finally { setBusy(false); }
  };

  const doCancel = () => { setVal(item.value ?? ''); setEditing(false); };

  return (
    <div className="py-3 border-b border-slate-100 last:border-0">
      <div className="flex items-start gap-3">
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 mb-1 flex-wrap">
            <span className="font-mono text-[12px] font-bold text-slate-700 bg-slate-100 px-2 py-0.5 rounded-lg">
              {item.key}
            </span>
            {item.description && (
              <span className="text-[11px] text-slate-400">{item.description}</span>
            )}
          </div>
          {editing ? (
            <div className="flex flex-col gap-2 mt-2">
              {isLong ? (
                <Textarea
                  value={val}
                  onChange={e => setVal(e.target.value)}
                  rows={3}
                  className="font-mono text-[13px]"
                  autoFocus
                />
              ) : (
                <Input
                  value={val}
                  onChange={e => setVal(e.target.value)}
                  className="font-mono text-[13px]"
                  autoFocus
                />
              )}
              <div className="flex gap-2">
                <BtnPrimary onClick={doSave} disabled={busy} className="py-1.5 px-3 text-[12px]">
                  {busy ? '…' : 'Lưu'}
                </BtnPrimary>
                <BtnSecondary onClick={doCancel} className="py-1.5 px-3 text-[12px]">Huỷ</BtnSecondary>
              </div>
            </div>
          ) : (
            <div
              className="text-[13px] text-slate-800 mt-1 font-mono bg-slate-50 px-3 py-2 rounded-xl cursor-pointer hover:bg-slate-100 transition-colors break-all"
              onClick={() => setEditing(true)}
              title="Click để sửa"
            >
              {item.value || <span className="text-slate-300 italic">— trống —</span>}
            </div>
          )}
        </div>
        {!editing && (
          <button
            onClick={() => setEditing(true)}
            className="flex-shrink-0 text-[11px] font-semibold text-slate-400 hover:text-slate-700 transition-colors px-2 py-1.5 rounded-lg hover:bg-slate-100 mt-0.5"
          >
            ✏️ Sửa
          </button>
        )}
      </div>
      <div className="text-[10px] text-slate-300 mt-1.5">
        Cập nhật: {fmtFull(item.updatedAt)}
      </div>
    </div>
  );
}

export default function AdminSettingsPage() {
  // API trả về: [{ group: string, settings: SettingItemDto[] }]
  const [groups,     setGroups]     = useState([]);   // raw array from API
  const [filterGrp,  setFilterGrp]  = useState('');
  const [loading,    setLoading]    = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [toast,      setToast]      = useState('');

  const ok = msg => { setToast(msg); setTimeout(() => setToast(''), 2400); };

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const r = await adminSettingApi.getDetail(filterGrp || null);
      // data = [{ group, settings[] }]
      setGroups(r.data.data || []);
    } catch { setGroups([]); }
    finally { setLoading(false); }
  }, [filterGrp]);

  useEffect(() => { load(); }, [load]);

  const totalCount = groups.reduce((sum, g) => sum + (g.settings?.length ?? 0), 0);

  // Danh sách group names từ API để filter dropdown
  const groupNames = groups.map(g => g.group).sort();

  // Filter theo group nếu có chọn
  const displayed = filterGrp
    ? groups.filter(g => g.group === filterGrp)
    : groups;

  return (
    <div>
      <Toast msg={toast} />

      <PageHeader
        title="Cài đặt hệ thống"
        sub={`${totalCount} settings`}
        action={
          <BtnPrimary onClick={() => setShowCreate(true)}>
            + Tạo setting
          </BtnPrimary>
        }
      />

      <FiltersBar>
        <Select value={filterGrp} onChange={e => setFilterGrp(e.target.value)}>
          <option value="">Tất cả nhóm</option>
          {groupNames.map(g => <option key={g} value={g}>{g}</option>)}
        </Select>
        <span className="ml-auto text-[12px] text-slate-400">
          {loading ? 'Đang tải…' : `${totalCount} settings`}
        </span>
      </FiltersBar>

      {loading ? (
        <div className="text-center text-slate-400 py-16">Đang tải…</div>
      ) : displayed.length === 0 ? (
        <div className="text-center text-slate-400 py-16">
          <div className="text-4xl mb-3">⚙️</div>
          <div className="font-semibold">Chưa có setting nào.</div>
        </div>
      ) : (
        <div className="flex flex-col gap-5">
          {displayed.map(({ group, settings }) => (
            <div key={group}>
              <div className="flex items-center gap-3 mb-3">
                <div className="text-[11px] font-extrabold text-slate-500 uppercase tracking-widest">{group}</div>
                <div className="flex-1 h-px bg-slate-200" />
                <div className="text-[11px] text-slate-400">{settings.length} keys</div>
              </div>
              <Card>
                <div className="px-4">
                  {settings.map(item => (
                    <SettingRow
                      key={item.id}
                      item={item}
                      onSaved={() => { ok(`✅ Đã lưu "${item.key}"`); load(); }}
                    />
                  ))}
                </div>
              </Card>
            </div>
          ))}
        </div>
      )}

      <SettingCreateModal
        open={showCreate}
        onClose={() => setShowCreate(false)}
        onRefresh={() => { load(); ok('✅ Đã tạo setting mới'); }}
      />
    </div>
  );
}