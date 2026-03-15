import { useState, useEffect, useCallback } from 'react';
import { affiliateApi } from '../../api/services';
import { extractError } from '../../api/client';
import { LoadingPage, Price, Spinner, EmptyState, useToast } from '../../components/ui';
import { useLang } from '../../context/Langcontext';

// ── Status badge ──────────────────────────────────────────────────────────────
const TxStatusBadge = ({ status }) => {
  const map = {
    Paid:     { label: 'Đã thanh toán', bg: 'rgba(16,185,129,0.1)',  border: 'rgba(16,185,129,0.25)',  color: '#10b981' },
    Approved: { label: 'Đã duyệt',      bg: 'rgba(99,102,241,0.1)',  border: 'rgba(99,102,241,0.25)',  color: '#6366f1' },
    Pending:  { label: 'Chờ duyệt',     bg: 'rgba(245,158,11,0.1)', border: 'rgba(245,158,11,0.25)', color: '#f59e0b' },
  };
  const s = map[status] ?? map.Pending;
  return (
    <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-bold"
      style={{ backgroundColor: s.bg, border: `1px solid ${s.border}`, color: s.color }}>
      {s.label}
    </span>
  );
};

// ── Stat card ─────────────────────────────────────────────────────────────────
const StatCard = ({ icon, label, value, accent, accentBg }) => (
  <div className="rounded-2xl p-5 shadow-sm"
    style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>
    <div className="flex items-center gap-3 mb-3">
      <div className="w-9 h-9 rounded-xl flex items-center justify-center text-base shrink-0"
        style={{ backgroundColor: accentBg, border: `1px solid ${accent}22` }}>
        {icon}
      </div>
      <p className="text-xs font-semibold uppercase tracking-wide" style={{ color: 'var(--text-muted)' }}>
        {label}
      </p>
    </div>
    <p className="text-2xl font-black" style={{ color: accent }}>{value}</p>
  </div>
);

// ── Pagination ────────────────────────────────────────────────────────────────
const Pagination = ({ page, totalPages, onPage }) => {
  if (totalPages <= 1) return null;
  return (
    <div className="flex items-center justify-between px-5 py-3"
      style={{ borderTop: '1px solid var(--border)' }}>
      <span className="text-xs" style={{ color: 'var(--text-muted)' }}>
        Trang {page} / {totalPages}
      </span>
      <div className="flex gap-2">
        <button
          onClick={() => onPage(page - 1)}
          disabled={page <= 1}
          className="px-3 py-1.5 rounded-lg text-xs font-semibold transition-all disabled:opacity-30"
          style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-secondary)' }}>
          ← Trước
        </button>
        <button
          onClick={() => onPage(page + 1)}
          disabled={page >= totalPages}
          className="px-3 py-1.5 rounded-lg text-xs font-semibold transition-all disabled:opacity-30"
          style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-secondary)' }}>
          Sau →
        </button>
      </div>
    </div>
  );
};

// ── Main ──────────────────────────────────────────────────────────────────────
export default function AffiliatePage() {
  const { t } = useLang();
  const toast = useToast();

  const [stats, setStats] = useState(null);
  const [statsLoading, setStatsLoading] = useState(true);
  const [notRegistered, setNotRegistered] = useState(false);
  const [registerLoading, setRegisterLoading] = useState(false);
  const [copied, setCopied] = useState(false);

  // Transactions state (independent)
  const [txData, setTxData] = useState(null);       // { items, totalCount, page, totalPages, ... }
  const [txLoading, setTxLoading] = useState(false);
  const [txPage, setTxPage] = useState(1);
  const [txStatus, setTxStatus] = useState('');     // '' | 'Pending' | 'Approved' | 'Paid'

  // ── Load stats ──────────────────────────────────────────────────────────────
  useEffect(() => {
    affiliateApi.getStats()
      .then(res => setStats(res.data.data))
      .catch(err => {
        const msg = extractError(err);
        if (msg.includes('chưa đăng ký')) setNotRegistered(true);
        else toast.error(msg);
      })
      .finally(() => setStatsLoading(false));
  }, []);

  // ── Load transactions ───────────────────────────────────────────────────────
  const loadTx = useCallback((page, status) => {
    setTxLoading(true);
    const params = { page, pageSize: 10 };
    if (status) params.status = status;

    affiliateApi.getTransactions(params)
      .then(res => setTxData(res.data.data))
      .catch(err => toast.error(extractError(err)))
      .finally(() => setTxLoading(false));
  }, []);

  // Load transactions khi đã có stats (đã đăng ký) hoặc khi filter/page thay đổi
  useEffect(() => {
    if (stats) loadTx(txPage, txStatus);
  }, [stats, txPage, txStatus]);

  const handleStatusFilter = (s) => {
    setTxStatus(s);
    setTxPage(1);
  };

  const handlePage = (p) => setTxPage(p);

  // ── Register ────────────────────────────────────────────────────────────────
  const handleRegister = async () => {
    setRegisterLoading(true);
    try {
      const res = await affiliateApi.register();
      setStats(res.data.data);
      setNotRegistered(false);
      toast.success(res.data.message || 'Đăng ký thành công!');
    } catch (err) {
      toast.error(extractError(err));
    } finally { setRegisterLoading(false); }
  };

  const affiliateLink = stats?.affiliateCode
    ? `${window.location.origin}?ref=${stats.affiliateCode}`
    : '';

  const copyLink = () => {
    navigator.clipboard.writeText(affiliateLink);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  // ── Loading ─────────────────────────────────────────────────────────────────
  if (statsLoading) return <LoadingPage />;

  // ── Chưa đăng ký ────────────────────────────────────────────────────────────
  if (notRegistered) return (
    <div className="animate-fade-in">
      <div className="mb-7">
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full text-xs font-semibold uppercase tracking-wider mb-3"
          style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-secondary)' }}>
          🤝 Affiliate
        </div>
        <h1 className="text-2xl font-black tracking-tight" style={{ color: 'var(--text-primary)' }}>
          {t('aff.subtitle')}
        </h1>
      </div>

      <div className="rounded-2xl p-10 text-center shadow-sm"
        style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>
        <div className="text-5xl mb-4">🤝</div>
        <h2 className="text-xl font-black mb-2" style={{ color: 'var(--text-primary)' }}>
          {t('aff.tagline')}
        </h2>
        <p className="text-sm mb-2" style={{ color: 'var(--text-secondary)' }}>
          {t('aff.desc')} <span className="font-bold" style={{ color: '#10b981' }}>{t('aff.rate')}</span>
        </p>
        <p className="text-xs mb-8" style={{ color: 'var(--text-muted)' }}>{t('aff.admin_review')}</p>
        <button
          onClick={handleRegister}
          disabled={registerLoading}
          className="inline-flex items-center gap-2 px-6 py-3 rounded-xl font-bold text-sm transition-all disabled:opacity-50"
          style={{ backgroundColor: 'var(--sidebar-active-bg)', color: 'var(--sidebar-active-text)' }}>
          {registerLoading ? <><Spinner /> {t('aff.registering')}</> : t('aff.register_btn')}
        </button>
      </div>
    </div>
  );

  // ── Đã đăng ký ──────────────────────────────────────────────────────────────
  return (
    <div className="animate-fade-in">

      {/* Header */}
      <div className="mb-7">
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full text-xs font-semibold uppercase tracking-wider mb-3"
          style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-secondary)' }}>
          🤝 Affiliate
        </div>
        <h1 className="text-2xl font-black tracking-tight" style={{ color: 'var(--text-primary)' }}>
          {t('aff.dashboard_title')}
        </h1>
        <p className="text-sm mt-1" style={{ color: 'var(--text-muted)' }}>
          {t('aff.code_detail')}{' '}
          <span className="font-mono font-bold" style={{ color: 'var(--text-secondary)' }}>{stats?.affiliateCode}</span>
          <span className="mx-2">·</span>
          {t('aff.rate_detail')}{' '}
          <span className="font-bold" style={{ color: 'var(--text-secondary)' }}>{stats?.commissionRate}%</span>
        </p>
      </div>

      {/* Chờ duyệt banner */}
      {!stats?.isActive && (
        <div className="flex items-center gap-3 px-4 py-3 rounded-2xl mb-6 text-sm font-medium"
          style={{ backgroundColor: 'rgba(245,158,11,0.08)', border: '1px solid rgba(245,158,11,0.25)', color: '#f59e0b' }}>
          {t('aff.pending_banner')}
        </div>
      )}

      {/* Stats Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
        <StatCard icon="💰" label={t('aff.total_commission')}
          value={<Price amount={stats?.totalEarned} />}
          accent="var(--text-primary)" accentBg="var(--bg-elevated)" />
        <StatCard icon="⏳" label={t('aff.pending_amount')}
          value={<Price amount={stats?.pendingAmount} />}
          accent="#f59e0b" accentBg="rgba(245,158,11,0.08)" />
        <StatCard icon="✅" label={t('aff.paid_amount')}
          value={<Price amount={stats?.paidAmount} />}
          accent="#10b981" accentBg="rgba(16,185,129,0.08)" />
      </div>

      {/* Affiliate Link */}
      {stats?.isActive && (
        <div className="rounded-2xl p-5 mb-6 shadow-sm"
          style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>
          <p className="text-xs font-bold uppercase tracking-widest mb-3" style={{ color: 'var(--text-muted)' }}>
            {t('aff.link_title')}
          </p>
          <div className="flex gap-2">
            <input
              className="flex-1 px-4 py-2.5 rounded-xl text-sm font-mono focus:outline-none"
              style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-primary)' }}
              value={affiliateLink}
              readOnly
            />
            <button
              onClick={copyLink}
              className="flex items-center gap-1.5 px-4 py-2.5 rounded-xl text-sm font-bold transition-all shrink-0"
              style={{
                backgroundColor: copied ? 'rgba(16,185,129,0.1)' : 'var(--bg-elevated)',
                border: `1px solid ${copied ? 'rgba(16,185,129,0.3)' : 'var(--border)'}`,
                color: copied ? '#10b981' : 'var(--text-secondary)',
              }}>
              {copied ? t('aff.copied_btn') : t('aff.copy_btn')}
            </button>
          </div>
          <p className="text-xs mt-2" style={{ color: 'var(--text-muted)' }}>
            {t('aff.link_desc')}{' '}
            <span className="font-semibold" style={{ color: 'var(--text-secondary)' }}>{stats.commissionRate}%</span>{' '}
            {t('aff.link_desc2')}
          </p>
        </div>
      )}

      {/* Transactions */}
      <div className="rounded-2xl shadow-sm overflow-hidden"
        style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}>

        {/* Toolbar */}
        <div className="px-5 py-3 flex items-center justify-between gap-3 flex-wrap"
          style={{ backgroundColor: 'var(--bg-elevated)', borderBottom: '1px solid var(--border)' }}>
          <p className="text-xs font-bold uppercase tracking-widest" style={{ color: 'var(--text-muted)' }}>
            {t('aff.history_title')}
            {txData?.totalCount > 0 && (
              <span className="ml-2 font-semibold px-2 py-0.5 rounded-full normal-case"
                style={{ backgroundColor: 'var(--bg-card)', color: 'var(--text-muted)', border: '1px solid var(--border)' }}>
                {txData.totalCount}
              </span>
            )}
          </p>

          {/* Status filter */}
          <div className="flex gap-1">
            {[['', 'Tất cả'], ['Pending', 'Chờ duyệt'], ['Approved', 'Đã duyệt'], ['Paid', 'Đã thanh toán']].map(([val, label]) => (
              <button
                key={val}
                onClick={() => handleStatusFilter(val)}
                className="px-2.5 py-1 rounded-lg text-xs font-semibold transition-all"
                style={{
                  backgroundColor: txStatus === val ? 'var(--sidebar-active-bg)' : 'var(--bg-card)',
                  color: txStatus === val ? 'var(--sidebar-active-text)' : 'var(--text-muted)',
                  border: `1px solid ${txStatus === val ? 'transparent' : 'var(--border)'}`,
                }}>
                {label}
              </button>
            ))}
          </div>
        </div>

        {/* Table body */}
        {txLoading ? (
          <div className="flex items-center justify-center py-12">
            <Spinner />
          </div>
        ) : !txData?.items?.length ? (
          <EmptyState icon="💸" title={t('aff.no_transactions')} description={t('aff.no_tx_desc')} />
        ) : (
          <>
            {/* Column headers */}
            <div className="grid grid-cols-[1fr_120px_120px_120px] gap-4 px-5 py-2.5"
              style={{ borderBottom: '1px solid var(--border)' }}>
              {[
                [t('aff.col_order'), ''],
                [t('aff.col_value'), 'text-right'],
                [t('aff.col_commission'), 'text-right'],
                [t('aff.col_status'), 'text-center'],
              ].map(([h, cls]) => (
                <span key={h} className={`text-xs font-bold uppercase tracking-widest ${cls}`}
                  style={{ color: 'var(--text-muted)' }}>{h}</span>
              ))}
            </div>

            {/* Rows */}
            <div>
              {txData.items.map((tx, idx) => (
                <div key={tx.id}>
                  {idx > 0 && <div style={{ borderTop: '1px solid var(--border)' }} />}
                  <div
                    className="grid grid-cols-[1fr_120px_120px_120px] gap-4 items-center px-5 py-4 transition-colors"
                    onMouseEnter={e => e.currentTarget.style.backgroundColor = 'var(--bg-elevated)'}
                    onMouseLeave={e => e.currentTarget.style.backgroundColor = ''}
                  >
                    <div>
                      <p className="font-mono text-xs font-bold" style={{ color: '#818cf8' }}>
                        {tx.orderCode}
                      </p>
                      <p className="text-xs mt-0.5" style={{ color: 'var(--text-muted)' }}>
                        {new Date(tx.createdAt).toLocaleDateString('vi-VN')}
                      </p>
                    </div>

                    <div className="text-right text-sm font-medium" style={{ color: 'var(--text-secondary)' }}>
                      <Price amount={tx.orderAmount} />
                    </div>

                    <div className="text-right text-sm font-bold" style={{ color: '#10b981' }}>
                      +<Price amount={tx.commission} />
                    </div>

                    <div className="flex justify-center">
                      <TxStatusBadge status={tx.status} />
                    </div>
                  </div>
                </div>
              ))}
            </div>

            <Pagination page={txData.page} totalPages={txData.totalPages} onPage={handlePage} />
          </>
        )}
      </div>
    </div>
  );
}