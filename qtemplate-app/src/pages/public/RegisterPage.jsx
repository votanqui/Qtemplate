import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { Spinner } from '../../components/ui';
import { useLang } from '../../context/Langcontext';

function pwStrength(pw) {
  let s = 0;
  if (pw.length >= 8) s++;
  if (/[A-Z]/.test(pw)) s++;
  if (/[0-9]/.test(pw)) s++;
  if (/[^A-Za-z0-9]/.test(pw)) s++;
  return s;
}

export default function RegisterPage() {
  const { t } = useLang();
  const { register } = useAuth();
  const navigate = useNavigate();

  const strengthConfig = [
    null,
    { label: t('register.pw_strength.weak'),   color: 'bg-red-400',    text: 'text-red-500' },
    { label: t('register.pw_strength.fair'),   color: 'bg-amber-400',  text: 'text-amber-500' },
    { label: t('register.pw_strength.good'),   color: 'bg-yellow-400', text: 'text-yellow-600' },
    { label: t('register.pw_strength.strong'), color: 'bg-emerald-500',text: 'text-emerald-600' },
  ];

  const [form, setForm] = useState({ fullName: '', email: '', password: '', confirmPassword: '' });
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [apiError, setApiError] = useState('');
  const [success, setSuccess] = useState('');
  const [showPw, setShowPw] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);

  const strength = pwStrength(form.password);

  const validate = () => {
    const e = {};
    if (!form.fullName || form.fullName.length < 2) e.fullName = t('register.err_name');
    if (!form.email) e.email = t('register.err_email');
    if (!form.password || form.password.length < 8) e.password = t('register.err_pw_short');
    else if (!/[A-Z]/.test(form.password)) e.password = t('register.err_pw_upper');
    if (form.password !== form.confirmPassword) e.confirmPassword = t('register.err_pw_match');
    return e;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const errs = validate();
    if (Object.keys(errs).length) { setErrors(errs); return; }
    setErrors({}); setApiError('');
    setLoading(true);
    const result = await register(form);
    setLoading(false);
    if (result.success) {
      setSuccess(result.message || t('register.success'));
      setTimeout(() => navigate('/dashboard/profile'), 1500);
    } else {
      setApiError(result.error);
    }
  };

  const set = f => e => setForm(p => ({ ...p, [f]: e.target.value }));

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">

      {/* Back to home */}
      <div className="px-6 py-4">
        <Link
          to="/"
          className="inline-flex items-center gap-2 text-sm text-gray-500 hover:text-gray-900 transition-colors font-medium group"
        >
          <span className="w-7 h-7 rounded-lg bg-white border border-gray-200 flex items-center justify-center text-xs group-hover:border-gray-400 transition-colors shadow-sm">←</span>
          {t('common.back')}
        </Link>
      </div>

      <div className="flex-1 flex items-center justify-center p-4 py-6">
        <div className="w-full max-w-[420px]">

          {/* Logo + heading */}
          <div className="text-center mb-8">
            <Link to="/" className="inline-flex flex-col items-center gap-3 group">
              <div className="w-14 h-14 rounded-2xl bg-black flex items-center justify-center text-white font-black text-2xl font-mono shadow-xl group-hover:scale-105 transition-transform">
                Q
              </div>
              <div>
                <h1 className="text-2xl font-black text-gray-900 tracking-tight">{t('auth.register_title')}</h1>
                <p className="text-gray-500 text-sm mt-0.5">{t('register.tagline')}</p>
              </div>
            </Link>
          </div>

          {/* Card */}
          <div className="bg-white rounded-3xl border border-gray-200 shadow-xl shadow-gray-100/80 p-8">
            <form onSubmit={handleSubmit} className="space-y-5">

              {/* API error */}
              {apiError && (
                <div className="flex items-start gap-2.5 p-3.5 bg-red-50 border border-red-200 rounded-xl text-sm text-red-600">
                  <span>⚠️</span>
                  <div>
                    <button
                      type="button"
                      onClick={() => setApiError('')}
                      className="float-right text-red-400 hover:text-red-600 ml-2 text-base leading-none"
                    >×</button>
                    {apiError}
                  </div>
                </div>
              )}

              {/* Success */}
              {success && (
                <div className="flex items-start gap-2.5 p-3.5 bg-emerald-50 border border-emerald-200 rounded-xl text-sm text-emerald-700">
                  <span>✅</span><p>{success}</p>
                </div>
              )}

              {/* Full name */}
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-1.5">
                  {t('register.full_name_lbl')} <span className="text-red-400">*</span>
                </label>
                <input
                  className={`w-full px-4 py-3 rounded-xl border text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-black/10 focus:bg-white transition-all
                    ${errors.fullName ? 'border-red-300 bg-red-50' : 'border-gray-200 bg-gray-50 focus:border-gray-500'}`}
                  placeholder={t('register.name_ph')}
                  value={form.fullName}
                  onChange={set('fullName')}
                  autoComplete="name"
                />
                {errors.fullName && <p className="text-xs text-red-500 mt-1 font-medium">{errors.fullName}</p>}
              </div>

              {/* Email */}
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-1.5">
                  {t('auth.email')} <span className="text-red-400">*</span>
                </label>
                <input
                  type="email"
                  className={`w-full px-4 py-3 rounded-xl border text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-black/10 focus:bg-white transition-all
                    ${errors.email ? 'border-red-300 bg-red-50' : 'border-gray-200 bg-gray-50 focus:border-gray-500'}`}
                  placeholder="you@example.com"
                  value={form.email}
                  onChange={set('email')}
                  autoComplete="email"
                />
                {errors.email && <p className="text-xs text-red-500 mt-1 font-medium">{errors.email}</p>}
              </div>

              {/* Password */}
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-1.5">
                  {t('register.pw_label')} <span className="text-red-400">*</span>
                </label>
                <div className="relative">
                  <input
                    type={showPw ? 'text' : 'password'}
                    className={`w-full px-4 py-3 pr-12 rounded-xl border text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-black/10 focus:bg-white transition-all
                      ${errors.password ? 'border-red-300 bg-red-50' : 'border-gray-200 bg-gray-50 focus:border-gray-500'}`}
                    placeholder="Abc@1234"
                    value={form.password}
                    onChange={set('password')}
                    autoComplete="new-password"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPw(s => !s)}
                    className="absolute right-3.5 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-700 transition-colors text-base"
                  >
                    {showPw ? '🙈' : '👁️'}
                  </button>
                </div>
                {errors.password && <p className="text-xs text-red-500 mt-1 font-medium">{errors.password}</p>}

                {/* Password strength bar */}
                {form.password && (
                  <div className="mt-2.5">
                    <div className="flex gap-1">
                      {[1, 2, 3, 4].map(i => (
                        <div
                          key={i}
                          className={`flex-1 h-1.5 rounded-full transition-all duration-300
                            ${strength >= i
                              ? (strengthConfig[strength]?.color || 'bg-gray-300')
                              : 'bg-gray-200'
                            }`}
                        />
                      ))}
                    </div>
                    <p className={`text-xs mt-1 font-semibold ${strengthConfig[strength]?.text || 'text-gray-400'}`}>
                      {strengthConfig[strength]?.label &&
                        `${t('register.pw_strength_lbl')} ${strengthConfig[strength].label}`}
                    </p>
                  </div>
                )}
              </div>

              {/* Confirm password */}
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-1.5">
                  {t('register.pw_confirm_lbl')} <span className="text-red-400">*</span>
                </label>
                <div className="relative">
                  <input
                    type={showConfirm ? 'text' : 'password'}
                    className={`w-full px-4 py-3 pr-12 rounded-xl border text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-black/10 focus:bg-white transition-all
                      ${errors.confirmPassword ? 'border-red-300 bg-red-50' : 'border-gray-200 bg-gray-50 focus:border-gray-500'}`}
                    placeholder={t('register.pw_confirm_ph')}
                    value={form.confirmPassword}
                    onChange={set('confirmPassword')}
                    autoComplete="new-password"
                  />
                  <button
                    type="button"
                    onClick={() => setShowConfirm(s => !s)}
                    className="absolute right-3.5 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-700 transition-colors text-base"
                  >
                    {showConfirm ? '🙈' : '👁️'}
                  </button>
                </div>
                {errors.confirmPassword && <p className="text-xs text-red-500 mt-1 font-medium">{errors.confirmPassword}</p>}
                {/* Match indicator */}
                {form.confirmPassword && form.password && (
                  <p className={`text-xs mt-1 font-semibold ${form.password === form.confirmPassword ? 'text-emerald-600' : 'text-red-500'}`}>
                    {form.password === form.confirmPassword
                      ? t('register.pw_match')
                      : t('register.pw_no_match')}
                  </p>
                )}
              </div>

              {/* Submit */}
              <button
                type="submit"
                disabled={loading}
                className="w-full py-3.5 rounded-xl bg-black text-white font-bold text-sm flex items-center justify-center gap-2 hover:bg-gray-800 active:scale-[0.99] transition-all shadow-lg shadow-black/20 disabled:opacity-50 disabled:cursor-not-allowed mt-1"
              >
                {loading
                  ? <><Spinner /> {t('register.creating')}</>
                  : <>{t('register.create_btn')} <span>→</span></>}
              </button>
            </form>

            <p className="text-center text-sm text-gray-500 mt-5">
              {t('register.has_account')}{' '}
              <Link to="/login" className="text-black font-bold hover:underline">
                {t('nav.login')}
              </Link>
            </p>
          </div>

          <p className="text-center text-xs text-gray-400 mt-6">
            {t('register.terms')}{' '}
            <a href="#" className="underline hover:text-gray-600">{t('register.terms_link')}</a>
          </p>
        </div>
      </div>
    </div>
  );
}