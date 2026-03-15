import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { userApi, authApi } from '../../api/services';
import { extractError } from '../../api/client';
import { Spinner, useToast, Portal } from '../../components/ui';
import { useAuth } from '../../context/AuthContext';
import { useLang } from '../../context/Langcontext';

export default function SecurityPage() {
  const { t } = useLang();
  const { logout } = useAuth();
  const navigate = useNavigate();
  const toast = useToast();

  const [pwForm, setPwForm] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' });
  const [pwLoading, setPwLoading] = useState(false);
  const [showPw, setShowPw] = useState({ current: false, new: false, confirm: false });

  const [deleteModal, setDeleteModal] = useState(false);
  const [deletePass, setDeletePass] = useState('');
  const [deleteLoading, setDeleteLoading] = useState(false);

  const handleChangePassword = async (e) => {
    e.preventDefault();
    if (pwForm.newPassword !== pwForm.confirmPassword) {
      toast.error(t('security.pw_no_match'));
      return;
    }
    setPwLoading(true);
    try {
      const res = await authApi.changePassword(pwForm);
      toast.success(res.data.message || t('security.change_ok'));
      setPwForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
      setTimeout(async () => { await logout(); navigate('/login'); }, 2000);
    } catch (err) {
      toast.error(extractError(err), t('security.change_err'));
    } finally { setPwLoading(false); }
  };

  const handleDeleteAccount = async () => {
    setDeleteLoading(true);
    try {
      await userApi.deleteAccount(deletePass);
      await logout();
      navigate('/login');
    } catch (err) {
      toast.error(extractError(err), t('security.delete_err'));
      setDeleteLoading(false);
    }
  };

  const inputStyle = {
    backgroundColor: 'var(--bg-elevated)',
    border: '1px solid var(--border)',
    color: 'var(--text-primary)',
  };

  const PwInput = ({ field, placeholder, label, required }) => {
    const key = field === 'current' ? 'currentPassword' : field === 'new' ? 'newPassword' : 'confirmPassword';
    return (
      <div>
        <label className="block text-sm font-semibold mb-1.5" style={{ color: 'var(--text-secondary)' }}>
          {label} {required && <span className="text-red-500">*</span>}
        </label>
        <div className="relative">
          <input
            type={showPw[field] ? 'text' : 'password'}
            className="w-full px-4 py-3 pr-12 rounded-xl text-sm focus:outline-none transition-all"
            style={inputStyle}
            placeholder={placeholder}
            value={pwForm[key]}
            onChange={e => setPwForm(f => ({ ...f, [key]: e.target.value }))}
            required={required}
            onFocus={e => e.target.style.borderColor = '#0ea5e9'}
            onBlur={e => e.target.style.borderColor = 'var(--border)'}
          />
          <button
            type="button"
            onClick={() => setShowPw(s => ({ ...s, [field]: !s[field] }))}
            className="absolute right-3.5 top-1/2 -translate-y-1/2 transition-opacity opacity-50 hover:opacity-100"
          >
            {showPw[field] ? '🙈' : '👁️'}
          </button>
        </div>
      </div>
    );
  };

  return (
    <div className="animate-fade-in">

      {/* Header */}
      <div className="mb-8">
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full text-xs font-semibold uppercase tracking-wider mb-3"
          style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-secondary)' }}>
          🔒 {t('security.title')}
        </div>
        <h1 className="text-2xl font-black tracking-tight" style={{ color: 'var(--text-primary)' }}>
          {t('security.title')}
        </h1>
        <p className="text-sm mt-1" style={{ color: 'var(--text-muted)' }}>{t('security.subtitle')}</p>
      </div>

      {/* Change Password */}
      <div className="rounded-2xl p-6 shadow-sm mb-5"
        style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>
        <h2 className="text-sm font-bold uppercase tracking-widest mb-5" style={{ color: 'var(--text-muted)' }}>
          {t('security.section_title')}
        </h2>
        <form onSubmit={handleChangePassword} className="space-y-4">
          <PwInput field="current" label={t('security.current_pw')} placeholder={t('security.current_ph')} required />
          <PwInput field="new"     label={t('security.new_pw')}     placeholder={t('security.new_ph')}     required />
          <PwInput field="confirm" label={t('security.confirm_pw')} placeholder={t('security.confirm_ph')} required />
          <div className="pt-1">
            <button
              type="submit"
              disabled={pwLoading}
              className="flex items-center gap-2 px-5 py-2.5 rounded-xl font-bold text-sm transition-all shadow-md disabled:opacity-50"
              style={{ backgroundColor: 'var(--sidebar-active-bg)', color: 'var(--sidebar-active-text)' }}
            >
              {pwLoading
                ? <><Spinner /> {t('security.changing')}</>
                : t('security.change_btn')}
            </button>
          </div>
        </form>
      </div>

      {/* Danger zone */}
      <div className="rounded-2xl p-6 shadow-sm"
        style={{ backgroundColor: 'var(--bg-card)', border: '1px solid rgba(239,68,68,0.3)' }}>
        <div className="flex items-start gap-3 mb-4">
          <div className="w-9 h-9 rounded-xl flex items-center justify-center text-lg shrink-0"
            style={{ backgroundColor: 'rgba(239,68,68,0.08)', border: '1px solid rgba(239,68,68,0.2)' }}>
            ⚠️
          </div>
          <div>
            <h2 className="font-black text-base" style={{ color: 'var(--text-primary)' }}>
              {t('security.danger_zone')}
            </h2>
            <p className="text-sm mt-0.5" style={{ color: 'var(--text-muted)' }}>
              {t('security.delete_desc')}
            </p>
          </div>
        </div>
        <button
          onClick={() => setDeleteModal(true)}
          className="flex items-center gap-2 px-4 py-2.5 rounded-xl font-bold text-sm text-red-500 hover:bg-red-500/10 transition-all"
          style={{ border: '2px solid rgba(239,68,68,0.3)' }}
        >
          {t('security.delete_btn')}
        </button>
      </div>

      {/* Delete confirm modal */}
      {deleteModal && (
        <Portal>
          <div
            className="fixed inset-0 bg-black/50 backdrop-blur-sm"
            style={{ zIndex: 200 }}
            onClick={() => { setDeleteModal(false); setDeletePass(''); }}
          />
          <div
            className="fixed inset-0 flex items-start justify-center p-4 pt-20 lg:items-center lg:pt-4 pointer-events-none"
            style={{ zIndex: 201 }}
          >
            <div
              className="relative w-full max-w-md rounded-3xl shadow-2xl animate-fade-in pointer-events-auto"
              style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}
              onClick={e => e.stopPropagation()}
            >
              {/* Header */}
              <div className="flex items-center justify-between px-6 py-5"
                style={{ borderBottom: '1px solid var(--border)' }}>
                <h3 className="font-black text-base tracking-tight" style={{ color: 'var(--text-primary)' }}>
                  {t('security.delete_modal_title')}
                </h3>
                <button
                  onClick={() => { setDeleteModal(false); setDeletePass(''); }}
                  className="w-8 h-8 rounded-lg flex items-center justify-center text-xl leading-none transition-all"
                  style={{ backgroundColor: 'var(--bg-elevated)', color: 'var(--text-secondary)' }}
                >×</button>
              </div>

              {/* Body */}
              <div className="p-6 space-y-4">
                <div className="flex items-start gap-3 p-4 rounded-xl"
                  style={{ backgroundColor: 'rgba(239,68,68,0.06)', border: '1px solid rgba(239,68,68,0.2)' }}>
                  <span className="text-xl shrink-0">⚠️</span>
                  <p className="text-sm font-medium text-red-500">
                    {t('security.delete_warning')}
                  </p>
                </div>

                <div>
                  <label className="block text-sm font-semibold mb-1.5" style={{ color: 'var(--text-secondary)' }}>
                    {t('security.enter_pw_confirm')} <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="password"
                    className="w-full px-4 py-3 rounded-xl text-sm focus:outline-none transition-all"
                    style={inputStyle}
                    placeholder={t('security.current_ph')}
                    value={deletePass}
                    onChange={e => setDeletePass(e.target.value)}
                    onFocus={e => e.target.style.borderColor = '#ef4444'}
                    onBlur={e => e.target.style.borderColor = 'var(--border)'}
                  />
                </div>

                <div className="flex gap-2 justify-end pt-1">
                  <button
                    onClick={() => { setDeleteModal(false); setDeletePass(''); }}
                    className="px-4 py-2 rounded-xl text-sm font-semibold transition-all"
                    style={{ border: '1px solid var(--border)', color: 'var(--text-secondary)' }}
                  >
                    {t('common.cancel')}
                  </button>
                  <button
                    onClick={handleDeleteAccount}
                    disabled={deleteLoading || !deletePass}
                    className="flex items-center gap-2 px-4 py-2 rounded-xl bg-red-600 text-white text-sm font-bold hover:bg-red-700 transition-all disabled:opacity-50"
                  >
                    {deleteLoading
                      ? <><Spinner /> {t('security.deleting')}</>
                      : t('security.confirm_delete_btn')}
                  </button>
                </div>
              </div>
            </div>
          </div>
        </Portal>
      )}
    </div>
  );
}