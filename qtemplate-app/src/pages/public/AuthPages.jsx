// ─────────────────────────────────────────────
// auth-pages.jsx — ForgotPassword, ResetPassword, VerifyEmail
// ─────────────────────────────────────────────
import { useState, useEffect } from 'react';
import { Link, useSearchParams, useNavigate } from 'react-router-dom';
import { authApi } from '../../api/services';
import { extractError } from '../../api/client';
import { Spinner } from '../../components/ui';
import { useLang } from '../../context/Langcontext';

// ── Shared wrapper ──────────────────────────
const AuthShell = ({ children, backTo = '/login' }) => {
  const { t } = useLang();
  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      <div className="px-6 py-4">
        <Link
          to={backTo}
          className="inline-flex items-center gap-2 text-sm text-gray-500 hover:text-gray-900 transition-colors font-medium group"
        >
          <span className="w-7 h-7 rounded-lg bg-white border border-gray-200 flex items-center justify-center text-xs group-hover:border-gray-400 transition-colors shadow-sm">←</span>
          {t('auth.back_to_login')}
        </Link>
      </div>
      <div className="flex-1 flex items-center justify-center p-4">
        <div className="w-full max-w-[400px]">
          {children}
        </div>
      </div>
    </div>
  );
};

// ── AuthCard ─────────────────────────────────
const AuthCard = ({ icon, title, subtitle, children }) => (
  <>
    <div className="text-center mb-8">
      <div className="inline-flex w-14 h-14 rounded-2xl bg-black items-center justify-center text-2xl mb-4 shadow-xl">
        {icon}
      </div>
      <h1 className="text-2xl font-black text-gray-900 tracking-tight">{title}</h1>
      {subtitle && <p className="text-gray-500 text-sm mt-0.5">{subtitle}</p>}
    </div>
    <div className="bg-white rounded-3xl border border-gray-200 shadow-xl shadow-gray-100/80 p-8">
      {children}
    </div>
  </>
);

// ── Input ─────────────────────────────────────
const Input = ({ label, required, error, type = 'text', ...props }) => (
  <div>
    <label className="block text-sm font-semibold text-gray-700 mb-1.5">
      {label} {required && <span className="text-red-400">*</span>}
    </label>
    <input
      type={type}
      className={`w-full px-4 py-3 rounded-xl border text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-black/10 focus:bg-white transition-all
        ${error ? 'border-red-300 bg-red-50' : 'border-gray-200 bg-gray-50 focus:border-gray-500'}`}
      {...props}
    />
    {error && <p className="text-xs text-red-500 mt-1 font-medium">{error}</p>}
  </div>
);

// ── SubmitBtn ────────────────────────────────
const SubmitBtn = ({ loading, loadingText, children, disabled }) => (
  <button
    type="submit"
    disabled={loading || disabled}
    className="w-full py-3.5 rounded-xl bg-black text-white font-bold text-sm flex items-center justify-center gap-2 hover:bg-gray-800 active:scale-[0.99] transition-all shadow-lg shadow-black/20 disabled:opacity-50 disabled:cursor-not-allowed"
  >
    {loading ? <><Spinner /> {loadingText}</> : children}
  </button>
);

// ── ForgotPasswordPage ───────────────────────
export function ForgotPasswordPage() {
  const { t } = useLang();
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [msg, setMsg] = useState('');
  const [isSuccess, setIsSuccess] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = await authApi.forgotPassword(email);
      setMsg(res.data.message);
      setIsSuccess(true);
    } catch (err) {
      setMsg(extractError(err));
      setIsSuccess(false);
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthShell>
      <AuthCard
        icon="🔑"
        title={t('auth.forgot_title')}
        subtitle={t('auth.forgot_subtitle')}
      >
        <form onSubmit={handleSubmit} className="space-y-5">
          {msg && (
            <div className={`flex items-start gap-2.5 p-3.5 rounded-xl text-sm border
              ${isSuccess
                ? 'bg-emerald-50 border-emerald-200 text-emerald-700'
                : 'bg-red-50 border-red-200 text-red-600'
              }`}>
              <span>{isSuccess ? '✅' : '⚠️'}</span>
              <p>{msg}</p>
            </div>
          )}

          <Input
            label={t('auth.email')}
            type="email"
            placeholder="you@example.com"
            value={email}
            onChange={e => setEmail(e.target.value)}
            required
            autoComplete="email"
          />

          <SubmitBtn loading={loading} loadingText={t('auth.sending')}>
            {t('auth.send_reset_btn')} <span>→</span>
          </SubmitBtn>
        </form>

        <p className="text-center text-sm text-gray-500 mt-5">
          {t('auth.remember_pw')}{' '}
          <Link to="/login" className="text-black font-bold hover:underline">
            {t('nav.login')}
          </Link>
        </p>
      </AuthCard>
    </AuthShell>
  );
}

// ── ResetPasswordPage ────────────────────────
export function ResetPasswordPage() {
  const { t } = useLang();
  const [params] = useSearchParams();
  const token = params.get('token');
  const navigate = useNavigate();
  const [form, setForm] = useState({ newPassword: '', confirmPassword: '' });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showPw, setShowPw] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (form.newPassword !== form.confirmPassword) {
      setError(t('auth.pw_no_match')); return;
    }
    setLoading(true); setError('');
    try {
      const res = await authApi.resetPassword({ token, ...form });
      setSuccess(res.data.message);
      setTimeout(() => navigate('/login'), 2000);
    } catch (err) {
      setError(extractError(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthShell>
      <AuthCard
        icon="🔒"
        title={t('auth.reset_title')}
        subtitle={t('auth.reset_subtitle')}
      >
        <form onSubmit={handleSubmit} className="space-y-5">
          {!token && (
            <div className="flex items-start gap-2.5 p-3.5 bg-red-50 border border-red-200 rounded-xl text-sm text-red-600">
              <span>⚠️</span>
              <p>{t('auth.invalid_token')}</p>
            </div>
          )}
          {error && (
            <div className="flex items-start gap-2.5 p-3.5 bg-red-50 border border-red-200 rounded-xl text-sm text-red-600">
              <span>⚠️</span><p>{error}</p>
            </div>
          )}
          {success && (
            <div className="flex items-start gap-2.5 p-3.5 bg-emerald-50 border border-emerald-200 rounded-xl text-sm text-emerald-700">
              <span>✅</span><p>{success}</p>
            </div>
          )}

          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-1.5">
              {t('auth.new_pw_label')} <span className="text-red-400">*</span>
            </label>
            <div className="relative">
              <input
                type={showPw ? 'text' : 'password'}
                className="w-full px-4 py-3 pr-12 rounded-xl border border-gray-200 bg-gray-50 text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-black/10 focus:border-gray-500 focus:bg-white transition-all"
                placeholder="NewPass@123"
                value={form.newPassword}
                onChange={e => setForm(f => ({ ...f, newPassword: e.target.value }))}
                required disabled={!token}
              />
              <button type="button" onClick={() => setShowPw(s => !s)}
                className="absolute right-3.5 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-700 transition-colors text-base">
                {showPw ? '🙈' : '👁️'}
              </button>
            </div>
          </div>

          <Input
            label={t('auth.confirm_pw_label')}
            type="password"
            placeholder={t('auth.confirm_pw_ph')}
            value={form.confirmPassword}
            onChange={e => setForm(f => ({ ...f, confirmPassword: e.target.value }))}
            required disabled={!token}
          />

          <SubmitBtn loading={loading} loadingText={t('auth.resetting')} disabled={!token}>
            {t('auth.reset_btn')} <span>→</span>
          </SubmitBtn>
        </form>
      </AuthCard>
    </AuthShell>
  );
}

// ── VerifyEmailPage ──────────────────────────
export function VerifyEmailPage() {
  const { t } = useLang();
  const [params] = useSearchParams();
  const token = params.get('token');
  const [status, setStatus] = useState('loading');
  const [msg, setMsg] = useState('');

  useEffect(() => {
    if (!token) { setStatus('error'); setMsg(t('auth.token_invalid')); return; }
    authApi.verifyEmail(token)
      .then(res => { setStatus('success'); setMsg(res.data.message); })
      .catch(err => { setStatus('error'); setMsg(extractError(err)); });
  }, [token]);

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="w-full max-w-sm text-center">
        {status === 'loading' && (
          <div className="bg-white rounded-3xl border border-gray-200 shadow-xl p-12 flex flex-col items-center gap-4">
            <Spinner size="lg" className="text-black" />
            <p className="text-gray-600 font-medium">{t('auth.verifying')}</p>
          </div>
        )}

        {status === 'success' && (
          <div className="bg-white rounded-3xl border border-gray-200 shadow-xl p-12">
            <div className="w-16 h-16 rounded-2xl bg-emerald-50 border border-emerald-200 flex items-center justify-center text-3xl mx-auto mb-5">✅</div>
            <h1 className="text-xl font-black text-gray-900 mb-2">{t('auth.verify_ok_title')}</h1>
            <p className="text-gray-500 text-sm mb-6">{msg}</p>
            <Link to="/login"
              className="inline-flex items-center gap-2 px-6 py-3 rounded-xl bg-black text-white font-bold text-sm hover:bg-gray-800 transition-all shadow-lg shadow-black/20">
              {t('auth.verify_ok_btn')}
            </Link>
          </div>
        )}

        {status === 'error' && (
          <div className="bg-white rounded-3xl border border-gray-200 shadow-xl p-12">
            <div className="w-16 h-16 rounded-2xl bg-red-50 border border-red-200 flex items-center justify-center text-3xl mx-auto mb-5">❌</div>
            <h1 className="text-xl font-black text-gray-900 mb-2">{t('auth.verify_fail_title')}</h1>
            <p className="text-gray-500 text-sm mb-6">{msg}</p>
            <Link to="/login"
              className="inline-flex items-center gap-2 px-6 py-3 rounded-xl border-2 border-gray-200 text-gray-700 font-bold text-sm hover:border-gray-400 hover:text-gray-900 transition-all">
              {t('auth.verify_fail_btn')}
            </Link>
          </div>
        )}
      </div>
    </div>
  );
}