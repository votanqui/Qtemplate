import { useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { Alert, Spinner, FormField } from '../../components/ui';
import { authApi } from '../../api/services';
import { extractError } from '../../api/client';
import { useLang } from '../../context/Langcontext';

export default function LoginPage() {
  const { t } = useLang();
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = location.state?.from || '/templates';

  const [form, setForm] = useState({ email: '', password: '' });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showResend, setShowResend] = useState(false);
  const [resendLoading, setResendLoading] = useState(false);
  const [resendMsg, setResendMsg] = useState('');
  const [showPw, setShowPw] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(''); setShowResend(false);
    setLoading(true);
    const result = await login(form.email, form.password);
    setLoading(false);
    if (result.success) {
      navigate(from, { replace: true });
    } else {
      setError(result.error);
      if (result.error?.includes('xác minh')) setShowResend(true);
    }
  };

  const handleResend = async () => {
    setResendLoading(true);
    try {
      const res = await authApi.resendVerifyEmail(form.email);
      setResendMsg(res.data.message || t('login.resend_ok'));
    } catch (err) {
      setResendMsg(extractError(err));
    } finally {
      setResendLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">

      {/* Back to home bar */}
      <div className="px-6 py-4">
        <Link
          to="/"
          className="inline-flex items-center gap-2 text-sm text-gray-500 hover:text-gray-900 transition-colors font-medium group"
        >
          <span className="w-7 h-7 rounded-lg bg-white border border-gray-200 flex items-center justify-center text-xs group-hover:border-gray-400 transition-colors shadow-sm">←</span>
          {t('common.back')}
        </Link>
      </div>

      {/* Center content */}
      <div className="flex-1 flex items-center justify-center p-4">
        <div className="w-full max-w-[400px]">

          {/* Logo + heading */}
          <div className="text-center mb-8">
            <Link to="/" className="inline-flex flex-col items-center gap-3 group">
              <div className="w-14 h-14 rounded-2xl bg-black flex items-center justify-center text-white font-black text-2xl font-mono shadow-xl group-hover:scale-105 transition-transform">
                Q
              </div>
              <div>
                <h1 className="text-2xl font-black text-gray-900 tracking-tight">{t('auth.login_title')}</h1>
                <p className="text-gray-500 text-sm mt-0.5">{t('login.tagline')}</p>
              </div>
            </Link>
          </div>

          {/* Card */}
          <div className="bg-white rounded-3xl border border-gray-200 shadow-xl shadow-gray-100/80 p-8">
            <form onSubmit={handleSubmit} className="space-y-5">

              {error && (
                <div className="flex items-start gap-2.5 p-3.5 bg-red-50 border border-red-200 rounded-xl text-sm text-red-600">
                  <span>⚠️</span>
                  <div className="flex-1">
                    <p>{error}</p>
                    {showResend && (
                      <div className="mt-2">
                        {resendMsg ? (
                          <p className={`text-xs font-medium ${resendMsg.includes('đã') ? 'text-emerald-600' : 'text-red-500'}`}>
                            {resendMsg}
                          </p>
                        ) : (
                          <button
                            type="button"
                            onClick={handleResend}
                            disabled={resendLoading}
                            className="text-xs font-semibold text-violet-600 hover:underline flex items-center gap-1"
                          >
                            {resendLoading && <Spinner />}
                            {t('auth.resend_verify')} →
                          </button>
                        )}
                      </div>
                    )}
                  </div>
                </div>
              )}

              {/* Email */}
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-1.5">
                  {t('auth.email')} <span className="text-red-400">*</span>
                </label>
                <input
                  type="email"
                  className="w-full px-4 py-3 rounded-xl border border-gray-200 bg-gray-50 text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-black/10 focus:border-gray-500 focus:bg-white transition-all"
                  placeholder="you@example.com"
                  value={form.email}
                  onChange={e => setForm(f => ({ ...f, email: e.target.value }))}
                  required
                  autoComplete="email"
                />
              </div>

              {/* Password */}
              <div>
                <div className="flex items-center justify-between mb-1.5">
                  <label className="text-sm font-semibold text-gray-700">
                    {t('auth.password')} <span className="text-red-400">*</span>
                  </label>
                  <Link to="/forgot-password" className="text-xs text-violet-600 font-semibold hover:underline">
                    {t('auth.forgot_pw')}
                  </Link>
                </div>
                <div className="relative">
                  <input
                    type={showPw ? 'text' : 'password'}
                    className="w-full px-4 py-3 pr-12 rounded-xl border border-gray-200 bg-gray-50 text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-black/10 focus:border-gray-500 focus:bg-white transition-all"
                    placeholder="••••••••"
                    value={form.password}
                    onChange={e => setForm(f => ({ ...f, password: e.target.value }))}
                    required
                    autoComplete="current-password"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPw(s => !s)}
                    className="absolute right-3.5 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-700 transition-colors text-base"
                  >
                    {showPw ? '🙈' : '👁️'}
                  </button>
                </div>
              </div>

              {/* Submit */}
              <button
                type="submit"
                disabled={loading}
                className="w-full py-3.5 rounded-xl bg-black text-white font-bold text-sm flex items-center justify-center gap-2 hover:bg-gray-800 active:scale-[0.99] transition-all shadow-lg shadow-black/20 disabled:opacity-50 disabled:cursor-not-allowed mt-1"
              >
                {loading
                  ? <><Spinner /> {t('login.logging_in')}</>
                  : <>{t('auth.login_btn')} <span>→</span></>}
              </button>
            </form>

            {/* Divider */}
            <div className="flex items-center gap-3 my-5">
              <div className="flex-1 h-px bg-gray-100" />
              <span className="text-xs text-gray-400 font-medium">{t('login.or')}</span>
              <div className="flex-1 h-px bg-gray-100" />
            </div>

            {/* Register link */}
            <p className="text-center text-sm text-gray-500">
              {t('login.no_account')}{' '}
              <Link to="/register" className="text-black font-bold hover:underline">
                {t('login.register_free')}
              </Link>
            </p>
          </div>

          {/* Footer */}
          <p className="text-center text-xs text-gray-400 mt-6">
            {t('login.terms')}{' '}
            <a href="#" className="underline hover:text-gray-600">{t('login.terms_link')}</a>
          </p>
        </div>
      </div>
    </div>
  );
}