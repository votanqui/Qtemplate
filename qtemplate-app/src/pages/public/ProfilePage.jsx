import { useState, useEffect, useRef } from 'react';
import { userApi } from '../../api/services';
import { extractError, toAbsoluteUrl } from '../../api/client';
import { Spinner, LoadingPage, useToast } from '../../components/ui';
import { useAuth } from '../../context/AuthContext';
import { useLang } from '../../context/Langcontext';

export default function ProfilePage() {
  const { t } = useLang();
  const { updateUserData } = useAuth();
  const toast = useToast();
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState({ fullName: '', phoneNumber: '' });
  const [saving, setSaving] = useState(false);
  const [avatarLoading, setAvatarLoading] = useState(false);
  const fileRef = useRef();

  useEffect(() => {
    userApi.getProfile()
      .then(res => {
        setProfile(res.data.data);
        setForm({
          fullName: res.data.data.fullName || '',
          phoneNumber: res.data.data.phoneNumber || ''
        });
      })
      .catch(err => toast.error(extractError(err), t('profile.load_err')))
      .finally(() => setLoading(false));
  }, []);

  const handleSave = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const res = await userApi.updateProfile(form);
      setProfile(res.data.data);
      updateUserData({ fullName: res.data.data.fullName });
      toast.success(t('profile.update_ok'));
    } catch (err) {
      toast.error(extractError(err), t('profile.update_err'));
    } finally { setSaving(false); }
  };

  const handleAvatarChange = async (e) => {
    const file = e.target.files[0];
    if (!file) return;
    if (file.size > 2 * 1024 * 1024) {
      toast.warning(t('profile.size_warning'));
      return;
    }
    setAvatarLoading(true);
    try {
      const res = await userApi.updateAvatar(file);
      setProfile(p => ({ ...p, avatarUrl: res.data.data.avatarUrl }));
      updateUserData({ avatarUrl: res.data.data.avatarUrl });
      toast.success(t('profile.avatar_ok'));
    } catch (err) {
      toast.error(extractError(err), t('profile.avatar_err'));
    } finally { setAvatarLoading(false); }
  };

  if (loading) return <LoadingPage />;

  const metaItems = [
    {
      label: t('profile.role_lbl'),
      value: <span className="capitalize">{profile?.role || '—'}</span>
    },
    {
      label: t('profile.email_status'),
      value: profile?.isEmailVerified
        ? <span className="text-emerald-500 font-semibold">{t('profile.verified')}</span>
        : <span className="text-amber-500 font-semibold">{t('profile.not_verified')}</span>
    },
    {
      label: t('profile.last_login'),
      value: profile?.lastLoginAt
        ? new Date(profile.lastLoginAt).toLocaleString('vi-VN') : '—'
    },
    {
      label: t('profile.created_at'),
      value: profile?.createdAt
        ? new Date(profile.createdAt).toLocaleDateString('vi-VN') : '—'
    },
  ];

  return (
    <div className="animate-fade-in">

      {/* Header */}
      <div className="mb-7">
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full text-xs font-semibold uppercase tracking-wider mb-3"
          style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-secondary)' }}>
          👤 {t('profile.title')}
        </div>
        <h1 className="text-2xl font-black tracking-tight" style={{ color: 'var(--text-primary)' }}>
          {t('profile.title')}
        </h1>
        <p className="text-sm mt-1" style={{ color: 'var(--text-muted)' }}>{t('profile.subtitle')}</p>
      </div>

      {/* Avatar card */}
      <div className="rounded-2xl p-5 shadow-sm mb-4"
        style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>
        <h2 className="text-xs font-bold uppercase tracking-widest mb-4" style={{ color: 'var(--text-muted)' }}>
          {t('profile.avatar_lbl')}
        </h2>
        <div className="flex items-center gap-5">
          <div className="relative shrink-0">
            <div className="w-20 h-20 rounded-2xl border-2 overflow-hidden flex items-center justify-center"
              style={{ background: 'linear-gradient(135deg,#ede9fe,#fce7f3)', borderColor: 'var(--border)' }}>
              {profile?.avatarUrl ? (
                <img src={toAbsoluteUrl(profile.avatarUrl)} alt="Avatar" className="w-full h-full object-cover" />
              ) : (
                <span className="text-2xl font-black text-violet-500">
                  {profile?.fullName?.[0]?.toUpperCase() || 'U'}
                </span>
              )}
            </div>
            {avatarLoading && (
              <div className="absolute inset-0 rounded-2xl flex items-center justify-center"
                style={{ backgroundColor: 'var(--bg-card)', opacity: 0.85 }}>
                <Spinner />
              </div>
            )}
          </div>

          <div className="flex-1 min-w-0">
            <p className="font-black text-base" style={{ color: 'var(--text-primary)' }}>{profile?.fullName}</p>
            <p className="text-sm mb-3 truncate" style={{ color: 'var(--text-muted)' }}>{profile?.email}</p>
            <div className="flex items-center gap-2 flex-wrap">
              <button
                onClick={() => fileRef.current?.click()}
                disabled={avatarLoading}
                className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl border text-xs font-bold transition-all disabled:opacity-50"
                style={{ borderColor: 'var(--border)', color: 'var(--text-secondary)' }}
                onMouseEnter={e => e.currentTarget.style.color = 'var(--text-primary)'}
                onMouseLeave={e => e.currentTarget.style.color = 'var(--text-secondary)'}
              >
                {t('profile.change_avatar')}
              </button>
              <input
                ref={fileRef}
                type="file"
                accept="image/jpeg,image/png,image/webp"
                className="hidden"
                onChange={handleAvatarChange}
              />
              <span className="text-xs" style={{ color: 'var(--text-muted)' }}>{t('profile.avatar_hint')}</span>
            </div>
          </div>
        </div>
      </div>

      {/* Form card */}
      <div className="rounded-2xl p-6 shadow-sm"
        style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>
        <h2 className="text-xs font-bold uppercase tracking-widest mb-5" style={{ color: 'var(--text-muted)' }}>
          {t('profile.personal_info')}
        </h2>

        <form onSubmit={handleSave} className="space-y-5">

          {/* Full name */}
          <div>
            <label className="block text-sm font-semibold mb-1.5" style={{ color: 'var(--text-secondary)' }}>
              {t('profile.fullname_lbl')} <span className="text-red-500">*</span>
            </label>
            <input
              className="w-full px-4 py-3 rounded-xl text-sm transition-all focus:outline-none"
              style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-primary)' }}
              value={form.fullName}
              onChange={e => setForm(f => ({ ...f, fullName: e.target.value }))}
              placeholder={t('profile.name_ph')}
              required
              onFocus={e => e.target.style.borderColor = '#0ea5e9'}
              onBlur={e => e.target.style.borderColor = 'var(--border)'}
            />
          </div>

          {/* Email readonly */}
          <div>
            <label className="block text-sm font-semibold mb-1.5" style={{ color: 'var(--text-secondary)' }}>
              {t('auth.email')}
            </label>
            <div className="relative">
              <input
                className="w-full px-4 py-3 rounded-xl text-sm cursor-not-allowed"
                style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-muted)' }}
                value={profile?.email || ''}
                readOnly
              />
              <span className="absolute right-3.5 top-1/2 -translate-y-1/2 text-xs font-medium"
                style={{ color: 'var(--text-muted)' }}>
                {t('profile.email_fixed')}
              </span>
            </div>
            <p className="text-xs mt-1" style={{ color: 'var(--text-muted)' }}>{t('profile.email_note')}</p>
          </div>

          {/* Phone */}
          <div>
            <label className="block text-sm font-semibold mb-1.5" style={{ color: 'var(--text-secondary)' }}>
              {t('profile.phone_lbl')}
            </label>
            <input
              className="w-full px-4 py-3 rounded-xl text-sm transition-all focus:outline-none"
              style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-primary)' }}
              value={form.phoneNumber}
              onChange={e => setForm(f => ({ ...f, phoneNumber: e.target.value }))}
              placeholder="0901234567"
              onFocus={e => e.target.style.borderColor = '#0ea5e9'}
              onBlur={e => e.target.style.borderColor = 'var(--border)'}
            />
          </div>

          {/* Meta grid */}
          <div className="grid grid-cols-2 gap-4 pt-1">
            {metaItems.map(({ label, value }) => (
              <div key={label} className="rounded-xl px-4 py-3"
                style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)' }}>
                <p className="text-[10px] font-bold uppercase tracking-widest mb-1"
                  style={{ color: 'var(--text-muted)' }}>{label}</p>
                <p className="text-sm font-semibold" style={{ color: 'var(--text-secondary)' }}>{value}</p>
              </div>
            ))}
          </div>

          <div className="pt-1">
            <button
              type="submit"
              disabled={saving}
              className="flex items-center gap-2 px-5 py-2.5 rounded-xl font-bold text-sm transition-all shadow-md disabled:opacity-50"
              style={{ backgroundColor: 'var(--sidebar-active-bg)', color: 'var(--sidebar-active-text)' }}
            >
              {saving
                ? <><Spinner /> {t('profile.saving')}</>
                : t('profile.save_btn')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}