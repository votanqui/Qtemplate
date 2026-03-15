import { useState, useEffect, useCallback } from 'react';
import { adminStatsApi } from '../../api/adminApi';
import {
  fmt, fmtMoney, fmtFull,
  PageHeader, FiltersBar, Card, Tabs,
  Select, StatCard, Chip, Empty,
} from '../../components/ui/AdminUI';

// ── Helpers ───────────────────────────────────────────────────
function GrowthBadge({ value }) {
  if (value == null) return null;
  const up = value >= 0;
  return (
    <span className={`inline-flex items-center gap-0.5 text-[11px] font-bold px-2 py-0.5 rounded-lg ${up ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-600'}`}>
      {up ? '▲' : '▼'} {Math.abs(value).toFixed(1)}%
    </span>
  );
}

function MiniBar({ items, labelKey, valueKey, fmt: fmtFn = v => v, color = 'bg-slate-800' }) {
  const max = Math.max(...items.map(i => i[valueKey] || 0), 1);
  return (
    <div className="flex flex-col gap-1.5">
      {items.map((item, idx) => (
        <div key={idx} className="flex items-center gap-2">
          <div className="text-[11px] text-slate-500 w-24 flex-shrink-0 truncate">{item[labelKey]}</div>
          <div className="flex-1 h-2 bg-slate-100 rounded-full overflow-hidden">
            <div
              className={`h-full rounded-full ${color} transition-all duration-500`}
              style={{ width: `${(item[valueKey] / max) * 100}%` }}
            />
          </div>
          <div className="text-[11px] font-semibold text-slate-700 w-20 text-right flex-shrink-0">
            {fmtFn(item[valueKey])}
          </div>
        </div>
      ))}
    </div>
  );
}

function SectionTitle({ children }) {
  return (
    <div className="flex items-center gap-3 mb-3">
      <div className="text-[11px] font-extrabold text-slate-500 uppercase tracking-widest">{children}</div>
      <div className="flex-1 h-px bg-slate-100" />
    </div>
  );
}

function DateRange({ from, to, onChange }) {
  return (
    <div className="flex items-center gap-2">
      <input type="date" value={from} onChange={e => onChange(e.target.value, to)}
        className="border border-slate-200 rounded-xl px-3 py-1.5 text-[12px] text-slate-700 focus:outline-none focus:border-slate-400" />
      <span className="text-slate-400 text-[12px]">→</span>
      <input type="date" value={to} onChange={e => onChange(from, e.target.value)}
        className="border border-slate-200 rounded-xl px-3 py-1.5 text-[12px] text-slate-700 focus:outline-none focus:border-slate-400" />
    </div>
  );
}

function quickRange(days) {
  const to = new Date();
  const from = new Date();
  from.setDate(from.getDate() - days);
  return { from: from.toISOString().slice(0, 10), to: to.toISOString().slice(0, 10) };
}

const QuickBtns = ({ setFrom, setTo }) => (
  <>
    {[['7 ngày', 7], ['30 ngày', 30], ['90 ngày', 90]].map(([lbl, d]) => (
      <button key={d}
        onClick={() => { const r = quickRange(d); setFrom(r.from); setTo(r.to); }}
        className="px-3 py-1.5 rounded-xl bg-slate-100 hover:bg-slate-900 hover:text-white text-[12px] font-semibold text-slate-600 transition-colors"
      >{lbl}</button>
    ))}
  </>
);

// ── Dashboard Tab ─────────────────────────────────────────────
function DashboardTab() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [from, setFrom] = useState(quickRange(30).from);
  const [to, setTo] = useState(quickRange(30).to);

  const load = useCallback(async () => {
    setLoading(true);
    try { const r = await adminStatsApi.getDashboard(from, to); setData(r.data.data); }
    catch { setData(null); }
    finally { setLoading(false); }
  }, [from, to]);

  useEffect(() => { load(); }, [load]);

  if (loading) return <div className="text-center text-slate-400 py-16">Đang tải…</div>;
  if (!data)   return <div className="text-center text-slate-400 py-16">Không có dữ liệu.</div>;

  const o = data.orders;
  const p = data.payments;
  const c = data.coupons;

  return (
    <div className="flex flex-col gap-5">
      <FiltersBar>
        <DateRange from={from} to={to} onChange={(f, t) => { setFrom(f); setTo(t); }} />
        <QuickBtns setFrom={setFrom} setTo={setTo} />
      </FiltersBar>

      <div>
        <SectionTitle>Đơn hàng</SectionTitle>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          <StatCard label="Tổng đơn"      value={o.totalOrders}     icon="🛍️" />
          <StatCard label="Đã thanh toán" value={o.paidOrders}      icon="✅" />
          <StatCard label="Đang chờ"      value={o.pendingOrders}   icon="⏳" />
          <StatCard label="Đã huỷ"        value={o.cancelledOrders} icon="❌" />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
        <Card>
          <div className="px-4 py-3">
            <SectionTitle>Doanh thu</SectionTitle>
            <div className="text-[28px] font-extrabold text-slate-900 mb-1">{fmtMoney(o.totalRevenue)}</div>
            <div className="text-[12px] text-slate-400 mb-4">Giảm giá: {fmtMoney(o.totalDiscount)}</div>
            {o.revenueByDay?.length > 0 && (
              <MiniBar items={o.revenueByDay.slice(-10)} labelKey="label" valueKey="revenue" fmt={fmtMoney} />
            )}
          </div>
        </Card>
        <Card>
          <div className="px-4 py-3">
            <SectionTitle>Thanh toán</SectionTitle>
            <div className="grid grid-cols-2 gap-3 mb-4">
              <div>
                <div className="text-[24px] font-extrabold text-green-600">{fmtMoney(p.totalPaid)}</div>
                <div className="text-[11px] text-slate-400">Tổng đã thu</div>
              </div>
              <div>
                <div className="text-[24px] font-extrabold text-slate-900">{p.successTransactions}</div>
                <div className="text-[11px] text-slate-400">Giao dịch thành công</div>
              </div>
            </div>
            {p.byBank?.length > 0 && (
              <>
                <div className="text-[11px] font-bold text-slate-400 uppercase tracking-widest mb-2">Theo ngân hàng</div>
                <MiniBar items={p.byBank} labelKey="bankCode" valueKey="totalAmount" fmt={fmtMoney} color="bg-blue-500" />
              </>
            )}
          </div>
        </Card>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
        <Card>
          <div className="px-4 py-3">
            <SectionTitle>Top Template bán chạy</SectionTitle>
            {o.topTemplates?.length > 0 ? (
              <div className="flex flex-col gap-2">
                {o.topTemplates.slice(0, 8).map((t, idx) => (
                  <div key={String(t.templateId)} className="flex items-center gap-2 py-1.5 border-b border-slate-50 last:border-0">
                    <span className={`w-6 h-6 rounded-lg flex items-center justify-center text-[11px] font-extrabold flex-shrink-0 ${
                      idx === 0 ? 'bg-amber-400 text-white' : idx === 1 ? 'bg-slate-300 text-white' : idx === 2 ? 'bg-orange-400 text-white' : 'bg-slate-100 text-slate-500'
                    }`}>{idx + 1}</span>
                    <div className="flex-1 min-w-0">
                      <div className="text-[12px] font-semibold text-slate-800 truncate">{t.templateName}</div>
                      <div className="text-[10px] text-slate-400">{t.salesCount} đơn</div>
                    </div>
                    <div className="text-[12px] font-bold text-slate-900 flex-shrink-0">{fmtMoney(t.revenue)}</div>
                  </div>
                ))}
              </div>
            ) : <Empty msg="Chưa có dữ liệu." />}
          </div>
        </Card>
        <Card>
          <div className="px-4 py-3">
            <SectionTitle>Top Khách hàng</SectionTitle>
            {o.topUsers?.length > 0 ? (
              <div className="flex flex-col gap-2">
                {o.topUsers.slice(0, 8).map((u, idx) => (
                  <div key={String(u.userId)} className="flex items-center gap-2 py-1.5 border-b border-slate-50 last:border-0">
                    <span className={`w-6 h-6 rounded-lg flex items-center justify-center text-[11px] font-extrabold flex-shrink-0 ${
                      idx === 0 ? 'bg-amber-400 text-white' : idx === 1 ? 'bg-slate-300 text-white' : idx === 2 ? 'bg-orange-400 text-white' : 'bg-slate-100 text-slate-500'
                    }`}>{idx + 1}</span>
                    <div className="flex-1 min-w-0">
                      <div className="text-[12px] font-semibold text-slate-800 truncate">{u.fullName || u.email}</div>
                      <div className="text-[10px] text-slate-400">{u.orderCount} đơn</div>
                    </div>
                    <div className="text-[12px] font-bold text-slate-900 flex-shrink-0">{fmtMoney(u.totalSpent)}</div>
                  </div>
                ))}
              </div>
            ) : <Empty msg="Chưa có dữ liệu." />}
          </div>
        </Card>
      </div>

      <div>
        <SectionTitle>Coupons</SectionTitle>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mb-4">
          <StatCard label="Tổng coupon"  value={c.totalCoupons}            />
          <StatCard label="Đang active"  value={c.activeCoupons}           />
          <StatCard label="Đã hết hạn"   value={c.expiredCoupons}          />
          <StatCard label="Tổng đã giảm" value={fmtMoney(c.totalDiscounted)} />
        </div>
        {c.topCoupons?.length > 0 && (
          <Card>
            <div className="px-4 py-3">
              <SectionTitle>Top Coupon dùng nhiều nhất</SectionTitle>
              <MiniBar items={c.topCoupons.slice(0, 6)} labelKey="code" valueKey="usedCount" />
            </div>
          </Card>
        )}
      </div>
    </div>
  );
}

// ── Orders Tab ────────────────────────────────────────────────
function OrdersTab() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [from, setFrom] = useState(quickRange(30).from);
  const [to, setTo] = useState(quickRange(30).to);

  const load = useCallback(async () => {
    setLoading(true);
    try { const r = await adminStatsApi.getOrderStats(from, to); setData(r.data.data); }
    catch { setData(null); }
    finally { setLoading(false); }
  }, [from, to]);

  useEffect(() => { load(); }, [load]);

  if (loading) return <div className="text-center text-slate-400 py-16">Đang tải…</div>;
  if (!data)   return <div className="text-center text-slate-400 py-16">Không có dữ liệu.</div>;

  return (
    <div className="flex flex-col gap-5">
      <FiltersBar>
        <DateRange from={from} to={to} onChange={(f, t) => { setFrom(f); setTo(t); }} />
        <QuickBtns setFrom={setFrom} setTo={setTo} />
      </FiltersBar>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
        <StatCard label="Hôm nay — đơn"       value={data.todayOrders}              icon="📅" />
        <StatCard label="Hôm nay — doanh thu"  value={fmtMoney(data.todayRevenue)}  icon="💰" />
        <StatCard label="Tuần này — đơn"       value={data.weekOrders}               icon="📆" />
        <StatCard label="Tháng này — đơn"      value={data.monthOrders}              icon="🗓️" />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
        <Card>
          <div className="px-4 py-3">
            <SectionTitle>Doanh thu — so sánh kỳ</SectionTitle>
            <div className="grid grid-cols-2 gap-4 mb-3">
              <div>
                <div className="text-[11px] text-slate-400 mb-1">Kỳ này</div>
                <div className="text-[22px] font-extrabold text-slate-900">{fmtMoney(data.currentRevenue)}</div>
              </div>
              <div>
                <div className="text-[11px] text-slate-400 mb-1">Kỳ trước</div>
                <div className="text-[22px] font-extrabold text-slate-400">{fmtMoney(data.previousRevenue)}</div>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-[12px] text-slate-500">Tăng trưởng:</span>
              <GrowthBadge value={data.revenueGrowth} />
            </div>
          </div>
        </Card>
        <Card>
          <div className="px-4 py-3">
            <SectionTitle>Đơn hàng — so sánh kỳ</SectionTitle>
            <div className="grid grid-cols-2 gap-4 mb-3">
              <div>
                <div className="text-[11px] text-slate-400 mb-1">Kỳ này</div>
                <div className="text-[22px] font-extrabold text-slate-900">{data.currentOrders}</div>
              </div>
              <div>
                <div className="text-[11px] text-slate-400 mb-1">Kỳ trước</div>
                <div className="text-[22px] font-extrabold text-slate-400">{data.previousOrders}</div>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-[12px] text-slate-500">Tăng trưởng:</span>
              <GrowthBadge value={data.orderGrowth} />
            </div>
          </div>
        </Card>
      </div>

      {data.byDay?.length > 0 && (
        <Card>
          <div className="px-4 py-3">
            <SectionTitle>Doanh thu theo ngày</SectionTitle>
            <MiniBar items={data.byDay.slice(-14)} labelKey="label" valueKey="revenue" fmt={fmtMoney} />
          </div>
        </Card>
      )}

      {data.hourlyToday?.length > 0 && (
        <Card>
          <div className="px-4 py-3">
            <SectionTitle>Theo giờ hôm nay</SectionTitle>
            <MiniBar items={data.hourlyToday} labelKey="hour" valueKey="orders" fmt={v => `${v} đơn`} color="bg-blue-500" />
          </div>
        </Card>
      )}
    </div>
  );
}

// ── Payments Tab ──────────────────────────────────────────────
function PaymentsTab() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [from, setFrom] = useState(quickRange(30).from);
  const [to, setTo] = useState(quickRange(30).to);

  const load = useCallback(async () => {
    setLoading(true);
    try { const r = await adminStatsApi.getPaymentStats(from, to); setData(r.data.data); }
    catch { setData(null); }
    finally { setLoading(false); }
  }, [from, to]);

  useEffect(() => { load(); }, [load]);

  if (loading) return <div className="text-center text-slate-400 py-16">Đang tải…</div>;
  if (!data)   return <div className="text-center text-slate-400 py-16">Không có dữ liệu.</div>;

  return (
    <div className="flex flex-col gap-5">
      <FiltersBar>
        <DateRange from={from} to={to} onChange={(f, t) => { setFrom(f); setTo(t); }} />
      </FiltersBar>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
        <StatCard label="Tổng giao dịch" value={data.totalTransactions}   icon="💳" />
        <StatCard label="Thành công"      value={data.successTransactions} icon="✅" />
        <StatCard label="Thất bại"        value={data.failedTransactions}  icon="❌" />
        <StatCard label="Đang chờ"        value={data.pendingTransactions} icon="⏳" />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
        <Card>
          <div className="px-4 py-3">
            <SectionTitle>Tỉ lệ</SectionTitle>
            <div className="flex flex-col gap-3">
              {[
                ['Tỉ lệ thành công', data.successRate, 'bg-green-500'],
                ['Tỉ lệ thất bại',   data.failureRate, 'bg-red-400'],
              ].map(([lbl, val, color]) => (
                <div key={lbl}>
                  <div className="flex justify-between mb-1">
                    <span className="text-[12px] text-slate-500">{lbl}</span>
                    <span className="text-[12px] font-bold text-slate-800">{val?.toFixed(1)}%</span>
                  </div>
                  <div className="h-2 bg-slate-100 rounded-full overflow-hidden">
                    <div className={`h-full rounded-full ${color}`} style={{ width: `${val}%` }} />
                  </div>
                </div>
              ))}
              <div className="pt-2 border-t border-slate-50">
                <div className="text-[11px] text-slate-400 mb-1">Trung bình mỗi giao dịch</div>
                <div className="text-[18px] font-extrabold text-slate-900">{fmtMoney(data.averageTransactionValue)}</div>
              </div>
            </div>
          </div>
        </Card>
        <Card>
          <div className="px-4 py-3">
            <SectionTitle>Theo ngân hàng</SectionTitle>
            {data.byBank?.length > 0
              ? <MiniBar items={data.byBank} labelKey="bankCode" valueKey="totalAmount" fmt={fmtMoney} color="bg-blue-500" />
              : <Empty msg="Chưa có dữ liệu." />
            }
          </div>
        </Card>
      </div>

      {data.recentFailed?.length > 0 && (
        <Card>
          <div className="px-4 py-3">
            <SectionTitle>Giao dịch thất bại gần đây</SectionTitle>
            <table className="w-full">
              <thead>
                <tr className="border-b border-slate-100">
                  {['Order', 'Số tiền', 'Ngân hàng', 'Lý do', 'Thời gian'].map(h => (
                    <th key={h} className="px-2 py-2 text-left text-[11px] font-bold text-slate-400 uppercase">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {data.recentFailed.map(f => (
                  <tr key={String(f.paymentId)} className="border-b border-slate-50 hover:bg-slate-50">
                    <td className="px-2 py-2 font-mono text-[11px] text-slate-600">{f.orderCode}</td>
                    <td className="px-2 py-2 text-[12px] font-semibold text-slate-800">{fmtMoney(f.amount)}</td>
                    <td className="px-2 py-2"><Chip label={f.bankCode || '—'} color="slate" /></td>
                    <td className="px-2 py-2 text-[11px] text-red-400 truncate max-w-[150px]">{f.failReason || '—'}</td>
                    <td className="px-2 py-2 text-[11px] text-slate-400">{fmtFull(f.createdAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}
    </div>
  );
}

// ── Analytics Tab ─────────────────────────────────────────────
function AnalyticsTab() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [from, setFrom] = useState(quickRange(30).from);
  const [to, setTo] = useState(quickRange(30).to);

  const load = useCallback(async () => {
    setLoading(true);
    try { const r = await adminStatsApi.getAnalytics(from, to); setData(r.data.data); }
    catch { setData(null); }
    finally { setLoading(false); }
  }, [from, to]);

  useEffect(() => { load(); }, [load]);

  if (loading) return <div className="text-center text-slate-400 py-16">Đang tải…</div>;
  if (!data)   return <div className="text-center text-slate-400 py-16">Không có dữ liệu.</div>;

  return (
    <div className="flex flex-col gap-5">
      <FiltersBar>
        <DateRange from={from} to={to} onChange={(f, t) => { setFrom(f); setTo(t); }} />
        <QuickBtns setFrom={setFrom} setTo={setTo} />
      </FiltersBar>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
        <StatCard label="Tổng pageview"    value={data.totalPageViews}               icon="👁️" />
        <StatCard label="Unique visitors"  value={data.uniqueVisitors}               icon="🌐" />
        <StatCard label="Logged-in users"  value={data.uniqueUsers}                  icon="👤" />
        <StatCard label="Avg time on page" value={`${data.avgTimeOnPage?.toFixed(0)}s`} icon="⏱️" />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
        {data.byDevice?.length > 0 && (
          <Card><div className="px-4 py-3"><SectionTitle>Thiết bị</SectionTitle>
            <MiniBar items={data.byDevice} labelKey="label" valueKey="count" />
          </div></Card>
        )}
        {data.byBrowser?.length > 0 && (
          <Card><div className="px-4 py-3"><SectionTitle>Trình duyệt</SectionTitle>
            <MiniBar items={data.byBrowser} labelKey="label" valueKey="count" />
          </div></Card>
        )}
        {data.byOS?.length > 0 && (
          <Card><div className="px-4 py-3"><SectionTitle>Hệ điều hành</SectionTitle>
            <MiniBar items={data.byOS} labelKey="label" valueKey="count" />
          </div></Card>
        )}
      </div>

      {data.topPages?.length > 0 && (
        <Card>
          <div className="px-4 py-3">
            <SectionTitle>Top trang được xem nhiều nhất</SectionTitle>
            <table className="w-full">
              <thead>
                <tr className="border-b border-slate-100">
                  {['Trang', 'Lượt xem', 'Avg time'].map(h => (
                    <th key={h} className="px-2 py-2 text-left text-[11px] font-bold text-slate-400 uppercase">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {data.topPages.map((p, i) => (
                  <tr key={i} className="border-b border-slate-50 hover:bg-slate-50">
                    <td className="px-2 py-2 font-mono text-[11px] text-slate-700 truncate max-w-[280px]">{p.pageUrl}</td>
                    <td className="px-2 py-2 text-[12px] font-bold text-slate-800">{p.views}</td>
                    <td className="px-2 py-2 text-[11px] text-slate-500">{p.avgTime?.toFixed(0)}s</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}

      {data.topReferers?.length > 0 && (
        <Card>
          <div className="px-4 py-3">
            <SectionTitle>Nguồn traffic (Referer)</SectionTitle>
            <MiniBar items={data.topReferers.slice(0, 8)} labelKey="referer" valueKey="count" color="bg-violet-500" />
          </div>
        </Card>
      )}
    </div>
  );
}

// ── Daily Stats Tab ───────────────────────────────────────────
function DailyStatsTab() {
  const [data,    setData]    = useState([]);
  const [total,   setTotal]   = useState(0);
  const [loading, setLoading] = useState(true);
  const [period,  setPeriod]  = useState('daily');
  const [from,    setFrom]    = useState(quickRange(30).from);
  const [to,      setTo]      = useState(quickRange(30).to);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const r = await adminStatsApi.getDailyStats(period, from, to);
      const d = r.data.data;
      setData(d?.items || d || []);
      setTotal(d?.totalCount ?? (Array.isArray(d) ? d.length : 0));
    } catch { setData([]); }
    finally { setLoading(false); }
  }, [period, from, to]);

  useEffect(() => { load(); }, [load]);

  const maxRevenue = Math.max(...data.map(d => d.revenue || 0), 1);
  const maxOrders  = Math.max(...data.map(d => d.orders  || 0), 1);

  return (
    <div className="flex flex-col gap-5">
      <FiltersBar>
        <div className="flex gap-1 p-1 rounded-xl bg-slate-100">
          {[['daily', 'Ngày'], ['weekly', 'Tuần'], ['monthly', 'Tháng']].map(([val, lbl]) => (
            <button key={val}
              onClick={() => setPeriod(val)}
              className={`px-3 py-1.5 rounded-lg text-[12px] font-semibold transition-all ${
                period === val ? 'bg-white text-slate-900 shadow-sm' : 'text-slate-500 hover:text-slate-700'
              }`}
            >{lbl}</button>
          ))}
        </div>
        <DateRange from={from} to={to} onChange={(f, t) => { setFrom(f); setTo(t); }} />
        <QuickBtns setFrom={setFrom} setTo={setTo} />
        <span className="ml-auto text-[12px] text-slate-400">
          {loading ? 'Đang tải…' : `${total} bản ghi`}
        </span>
      </FiltersBar>

      {loading ? (
        <div className="text-center text-slate-400 py-16">Đang tải…</div>
      ) : data.length === 0 ? (
        <div className="text-center text-slate-400 py-16">Không có dữ liệu.</div>
      ) : (
        <>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
            <StatCard label="Tổng doanh thu"
              value={fmtMoney(data.reduce((s, d) => s + (d.revenue || 0), 0))} icon="💰" />
            <StatCard label="Tổng đơn hàng"
              value={data.reduce((s, d) => s + (d.orders || 0), 0)} icon="🛍️" />
            <StatCard label="Tổng người dùng mới"
              value={data.reduce((s, d) => s + (d.newUsers || 0), 0)} icon="👤" />
            <StatCard label="Tổng lượt xem"
              value={data.reduce((s, d) => s + (d.pageViews || 0), 0)} icon="👁️" />
          </div>

          <Card>
            <div className="px-4 py-3">
              <SectionTitle>Doanh thu theo {period === 'daily' ? 'ngày' : period === 'weekly' ? 'tuần' : 'tháng'}</SectionTitle>
              <div className="flex flex-col gap-1.5 mt-2">
                {data.slice(-30).map((row, idx) => (
                  <div key={idx} className="flex items-center gap-2">
                    <div className="text-[11px] text-slate-500 w-24 flex-shrink-0 truncate">
                      {row.date || row.week || row.month || row.label || `#${idx + 1}`}
                    </div>
                    <div className="flex-1 h-5 bg-slate-100 rounded-lg overflow-hidden relative">
                      <div
                        className="h-full rounded-lg bg-emerald-500 transition-all duration-500"
                        style={{ width: `${(row.revenue / maxRevenue) * 100}%` }}
                      />
                    </div>
                    <div className="text-[11px] font-semibold text-slate-700 w-24 text-right flex-shrink-0">
                      {fmtMoney(row.revenue)}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </Card>

          <Card>
            <div className="px-4 py-3">
              <SectionTitle>Đơn hàng theo {period === 'daily' ? 'ngày' : period === 'weekly' ? 'tuần' : 'tháng'}</SectionTitle>
              <div className="flex flex-col gap-1.5 mt-2">
                {data.slice(-30).map((row, idx) => (
                  <div key={idx} className="flex items-center gap-2">
                    <div className="text-[11px] text-slate-500 w-24 flex-shrink-0 truncate">
                      {row.date || row.week || row.month || row.label || `#${idx + 1}`}
                    </div>
                    <div className="flex-1 h-5 bg-slate-100 rounded-lg overflow-hidden">
                      <div
                        className="h-full rounded-lg bg-blue-500 transition-all duration-500"
                        style={{ width: `${(row.orders / maxOrders) * 100}%` }}
                      />
                    </div>
                    <div className="text-[11px] font-semibold text-slate-700 w-16 text-right flex-shrink-0">
                      {row.orders ?? 0} đơn
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </Card>

          <Card>
            <div className="px-4 py-3">
              <SectionTitle>Bảng chi tiết</SectionTitle>
              <div className="overflow-x-auto">
                <table className="w-full min-w-[600px]">
                  <thead>
                    <tr className="border-b border-slate-100">
                      {['Thời gian', 'Doanh thu', 'Đơn hàng', 'Người dùng mới', 'Lượt xem'].map(h => (
                        <th key={h} className="px-3 py-2 text-left text-[11px] font-bold text-slate-400 uppercase">{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {data.map((row, idx) => (
                      <tr key={idx} className="border-b border-slate-50 hover:bg-slate-50">
                        <td className="px-3 py-2 font-mono text-[12px] text-slate-700">
                          {row.date || row.week || row.month || row.label || `#${idx + 1}`}
                        </td>
                        <td className="px-3 py-2 text-[12px] font-bold text-slate-900">{fmtMoney(row.revenue)}</td>
                        <td className="px-3 py-2 text-[12px] text-slate-700">{row.orders ?? '—'}</td>
                        <td className="px-3 py-2 text-[12px] text-slate-700">{row.newUsers ?? '—'}</td>
                        <td className="px-3 py-2 text-[12px] text-slate-500">{row.pageViews ?? '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </Card>
        </>
      )}
    </div>
  );
}

// ── Coupons Stats Tab ─────────────────────────────────────────
function CouponsStatsTab() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    try { const r = await adminStatsApi.getCouponStats(); setData(r.data.data); }
    catch { setData(null); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);

  if (loading) return <div className="text-center text-slate-400 py-16">Đang tải…</div>;
  if (!data)   return <div className="text-center text-slate-400 py-16">Không có dữ liệu.</div>;

  return (
    <div className="flex flex-col gap-5">
      {/* Tổng quan */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
        <StatCard label="Tổng coupon"        value={data.totalCoupons}                  icon="🎟️" />
        <StatCard label="Đang active"         value={data.activeCoupons}                 icon="✅" />
        <StatCard label="Đã hết hạn"          value={data.expiredCoupons}                icon="⏰" />
        <StatCard label="Tổng đã giảm"        value={fmtMoney(data.totalDiscounted)}     icon="💸" />
      </div>

      <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
        <StatCard label="Đơn dùng coupon"    value={data.ordersWithCoupon}              icon="🧾" />
        <StatCard label="Đơn không dùng"     value={data.ordersWithoutCoupon}           icon="📋" />
        <StatCard label="Tỉ lệ dùng coupon" value={`${data.couponUsageRate?.toFixed(1)}%`} icon="📊" />
      </div>

      <Card>
        <div className="px-4 py-3">
          <SectionTitle>Giảm giá trung bình / đơn</SectionTitle>
          <div className="text-[28px] font-extrabold text-slate-900">{fmtMoney(data.averageDiscount)}</div>
          <div className="text-[12px] text-slate-400 mt-1">Trung bình mỗi đơn hàng có dùng coupon</div>
        </div>
      </Card>

      {/* Tỉ lệ dùng coupon bar */}
      <Card>
        <div className="px-4 py-3">
          <SectionTitle>Tỉ lệ đơn hàng dùng coupon</SectionTitle>
          <div className="flex flex-col gap-3">
            {[
              ['Có dùng coupon', data.ordersWithCoupon, data.ordersWithCoupon + data.ordersWithoutCoupon, 'bg-emerald-500'],
              ['Không dùng coupon', data.ordersWithoutCoupon, data.ordersWithCoupon + data.ordersWithoutCoupon, 'bg-slate-300'],
            ].map(([lbl, val, total, color]) => {
              const pct = total > 0 ? ((val / total) * 100).toFixed(1) : 0;
              return (
                <div key={lbl}>
                  <div className="flex justify-between mb-1">
                    <span className="text-[12px] text-slate-500">{lbl}</span>
                    <span className="text-[12px] font-bold text-slate-800">{val} ({pct}%)</span>
                  </div>
                  <div className="h-2 bg-slate-100 rounded-full overflow-hidden">
                    <div className={`h-full rounded-full ${color}`} style={{ width: `${pct}%` }} />
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </Card>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
        {/* Top coupons */}
        {data.topCoupons?.length > 0 && (
          <Card>
            <div className="px-4 py-3">
              <SectionTitle>Top Coupon dùng nhiều nhất</SectionTitle>
              <MiniBar items={data.topCoupons.slice(0, 8)} labelKey="code" valueKey="usedCount" color="bg-indigo-500" />
            </div>
          </Card>
        )}

        {/* Sắp hết hạn */}
        {data.expiringSoon?.length > 0 && (
          <Card>
            <div className="px-4 py-3">
              <SectionTitle>⚠️ Sắp hết hạn (7 ngày tới)</SectionTitle>
              <div className="flex flex-col gap-2">
                {data.expiringSoon.map(c => (
                  <div key={c.id} className="flex items-center justify-between py-2 border-b border-slate-50 last:border-0">
                    <div>
                      <span className="font-mono text-[12px] font-bold text-slate-800 bg-slate-100 px-2 py-0.5 rounded-lg">{c.code}</span>
                      <span className="ml-2 text-[11px] text-slate-400">{c.usedCount} lần dùng</span>
                    </div>
                    <div className="text-right">
                      <div className="text-[11px] font-bold text-amber-600">{c.daysLeft} ngày</div>
                      <div className="text-[10px] text-slate-400">{fmt(c.expiredAt)}</div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </Card>
        )}
      </div>

      {/* Sắp hết lượt */}
      {data.lowUsage?.length > 0 && (
        <Card>
          <div className="px-4 py-3">
            <SectionTitle>🔴 Sắp hết lượt dùng (≤ 5 lượt còn lại)</SectionTitle>
            <div className="flex flex-col gap-2">
              {data.lowUsage.map(c => (
                <div key={c.id} className="flex items-center justify-between py-2 border-b border-slate-50 last:border-0">
                  <span className="font-mono text-[12px] font-bold text-slate-800 bg-slate-100 px-2 py-0.5 rounded-lg">{c.code}</span>
                  <div className="flex items-center gap-3">
                    <div className="text-[12px] text-slate-500">
                      Đã dùng: <span className="font-bold text-slate-800">{c.usedCount}</span> / {c.usageLimit}
                    </div>
                    <Chip label={`còn ${c.remainingUse}`} color={c.remainingUse <= 2 ? 'red' : 'orange'} />
                  </div>
                </div>
              ))}
            </div>
          </div>
        </Card>
      )}
    </div>
  );
}

// ── Media Stats Tab ───────────────────────────────────────────
function MediaStatsTab() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    try { const r = await adminStatsApi.getMediaStats(); setData(r.data.data); }
    catch { setData(null); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);

  if (loading) return <div className="text-center text-slate-400 py-16">Đang tải…</div>;
  if (!data)   return <div className="text-center text-slate-400 py-16">Không có dữ liệu.</div>;

  return (
    <div className="flex flex-col gap-5">
      <div className="grid grid-cols-2 md:grid-cols-2 gap-3">
        <StatCard label="Tổng file"      value={data.totalFiles}           icon="📁" />
        <StatCard label="Tổng dung lượng" value={data.totalSizeFormatted}  icon="💾" />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
        {/* Theo storage type */}
        {data.byStorage?.length > 0 && (
          <Card>
            <div className="px-4 py-3">
              <SectionTitle>Theo loại storage</SectionTitle>
              <div className="flex flex-col gap-3">
                {data.byStorage.map(s => (
                  <div key={s.storageType} className="flex items-center justify-between py-2 border-b border-slate-50 last:border-0">
                    <div className="flex items-center gap-2">
                      <Chip label={s.storageType} color="blue" />
                      <span className="text-[12px] text-slate-500">{s.count} file</span>
                    </div>
                    <span className="text-[12px] font-semibold text-slate-800">{s.totalSizeFormatted}</span>
                  </div>
                ))}
              </div>
            </div>
          </Card>
        )}

        {/* Theo mime type */}
        {data.byType?.length > 0 && (
          <Card>
            <div className="px-4 py-3">
              <SectionTitle>Theo loại file (MIME)</SectionTitle>
              <div className="flex flex-col gap-2">
                {data.byType.map(t => {
                  const maxCount = Math.max(...data.byType.map(x => x.count), 1);
                  return (
                    <div key={t.mimeType} className="flex items-center gap-2">
                      <div className="text-[11px] text-slate-500 w-36 flex-shrink-0 truncate font-mono">{t.mimeType}</div>
                      <div className="flex-1 h-2 bg-slate-100 rounded-full overflow-hidden">
                        <div
                          className="h-full rounded-full bg-violet-500 transition-all duration-500"
                          style={{ width: `${(t.count / maxCount) * 100}%` }}
                        />
                      </div>
                      <span className="text-[11px] font-semibold text-slate-700 w-12 text-right flex-shrink-0">{t.count}</span>
                    </div>
                  );
                })}
              </div>
            </div>
          </Card>
        )}
      </div>
    </div>
  );
}

// ── Security Stats Tab ────────────────────────────────────────
function SecurityStatsTab() {
  const [data,    setData]    = useState(null);
  const [loading, setLoading] = useState(true);
  const [from,    setFrom]    = useState(quickRange(30).from);
  const [to,      setTo]      = useState(quickRange(30).to);

  const load = useCallback(async () => {
    setLoading(true);
    try { const r = await adminStatsApi.getSecurity(from, to); setData(r.data.data); }
    catch { setData(null); }
    finally { setLoading(false); }
  }, [from, to]);

  useEffect(() => { load(); }, [load]);

  if (loading) return <div className="text-center text-slate-400 py-16">Đang tải…</div>;
  if (!data)   return <div className="text-center text-slate-400 py-16">Không có dữ liệu.</div>;

  const ip  = data.ipBlacklist;
  const req = data.requestLogs;
  const em  = data.emailLogs;

  return (
    <div className="flex flex-col gap-5">
      <FiltersBar>
        <DateRange from={from} to={to} onChange={(f, t) => { setFrom(f); setTo(t); }} />
        <QuickBtns setFrom={setFrom} setTo={setTo} />
      </FiltersBar>

      {/* IP Blacklist */}
      <div>
        <SectionTitle>🛡️ IP Blacklist</SectionTitle>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-3 mb-4">
          <StatCard label="Tổng IP bị chặn"  value={ip.total}     icon="🚫" />
          <StatCard label="Đang active"       value={ip.active}    icon="🔴" />
          <StatCard label="Đã tắt"            value={ip.inactive}  icon="⚫" />
          <StatCard label="Thủ công (manual)" value={ip.manual}    icon="✋" />
          <StatCard label="Tự động (auto)"    value={ip.auto}      icon="🤖" />
          <StatCard label="Vĩnh viễn"         value={ip.permanent} icon="♾️" />
        </div>

        {ip.recentBlocked?.length > 0 && (
          <Card>
            <div className="px-4 py-3">
              <SectionTitle>IP bị chặn gần đây</SectionTitle>
              <div className="flex flex-col gap-1.5">
                {ip.recentBlocked.map((b, idx) => (
                  <div key={idx} className="flex items-center justify-between py-2 border-b border-slate-50 last:border-0">
                    <div className="flex items-center gap-2">
                      <span className="font-mono text-[12px] font-bold text-slate-800">{b.ipAddress}</span>
                      <Chip label={b.type} color={b.type === 'Auto' ? 'orange' : 'red'} />
                    </div>
                    <div className="text-right">
                      <div className="text-[11px] text-slate-500 truncate max-w-[160px]">{b.reason || '—'}</div>
                      <div className="text-[10px] text-slate-400">{fmtFull(b.blockedAt)}</div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </Card>
        )}
      </div>

      {/* Request Logs */}
      <div>
        <SectionTitle>🌐 Request Logs</SectionTitle>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mb-4">
          <StatCard label="Tổng requests"   value={req.totalRequests}   icon="📡" />
          <StatCard label="Thành công (2xx)" value={req.successRequests} icon="✅" />
          <StatCard label="Lỗi client (4xx)" value={req.clientErrors}    icon="⚠️" />
          <StatCard label="Lỗi server (5xx)" value={req.serverErrors}    icon="🔥" />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Card>
            <div className="px-4 py-3">
              <SectionTitle>Tỉ lệ & Hiệu suất</SectionTitle>
              <div className="flex flex-col gap-3">
                {[
                  ['Tỉ lệ thành công', req.successRate, 'bg-green-500'],
                  ['Tỉ lệ lỗi',        req.errorRate,   'bg-red-400'],
                ].map(([lbl, val, color]) => (
                  <div key={lbl}>
                    <div className="flex justify-between mb-1">
                      <span className="text-[11px] text-slate-500">{lbl}</span>
                      <span className="text-[11px] font-bold text-slate-800">{val?.toFixed(1)}%</span>
                    </div>
                    <div className="h-2 bg-slate-100 rounded-full overflow-hidden">
                      <div className={`h-full rounded-full ${color}`} style={{ width: `${val}%` }} />
                    </div>
                  </div>
                ))}
                <div className="pt-2 border-t border-slate-50 grid grid-cols-2 gap-2">
                  <div>
                    <div className="text-[10px] text-slate-400 mb-0.5">Avg response</div>
                    <div className="text-[14px] font-extrabold text-slate-900">{req.avgResponseTime}ms</div>
                  </div>
                  <div>
                    <div className="text-[10px] text-slate-400 mb-0.5">Max response</div>
                    <div className={`text-[14px] font-extrabold ${req.maxResponseTime > 3000 ? 'text-red-500' : 'text-slate-900'}`}>
                      {req.maxResponseTime}ms
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </Card>

          {req.byStatusCode?.length > 0 && (
            <Card>
              <div className="px-4 py-3">
                <SectionTitle>Theo status code</SectionTitle>
                <div className="flex flex-col gap-2">
                  {req.byStatusCode.map(s => {
                    const color = s.statusCode >= 500 ? 'bg-red-500' : s.statusCode >= 400 ? 'bg-orange-400' : 'bg-green-500';
                    return (
                      <div key={s.statusCode} className="flex items-center gap-2">
                        <span className="font-mono text-[11px] font-bold text-slate-700 w-10 flex-shrink-0">{s.statusCode}</span>
                        <div className="flex-1 h-2 bg-slate-100 rounded-full overflow-hidden">
                          <div className={`h-full rounded-full ${color}`} style={{ width: `${s.percentage}%` }} />
                        </div>
                        <span className="text-[11px] text-slate-500 w-14 text-right flex-shrink-0">{s.count} ({s.percentage?.toFixed(0)}%)</span>
                      </div>
                    );
                  })}
                </div>
              </div>
            </Card>
          )}

          {req.topIps?.length > 0 && (
            <Card>
              <div className="px-4 py-3">
                <SectionTitle>Top IP nhiều request nhất</SectionTitle>
                <div className="flex flex-col gap-2">
                  {req.topIps.slice(0, 8).map(ip => (
                    <div key={ip.ipAddress} className="flex items-center justify-between py-1.5 border-b border-slate-50 last:border-0">
                      <span className="font-mono text-[11px] text-slate-700">{ip.ipAddress}</span>
                      <div className="flex items-center gap-2">
                        <span className="text-[11px] font-semibold text-slate-800">{ip.count}</span>
                        {ip.errorCount > 0 && (
                          <span className="text-[10px] text-red-400">{ip.errorCount} lỗi</span>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </Card>
          )}
        </div>

        {req.topEndpoints?.length > 0 && (
          <Card className="mt-4">
            <div className="px-4 py-3">
              <SectionTitle>Top Endpoints</SectionTitle>
              <div className="overflow-x-auto">
                <table className="w-full min-w-[480px]">
                  <thead>
                    <tr className="border-b border-slate-100">
                      {['Method', 'Endpoint', 'Lượt gọi', 'Avg (ms)'].map(h => (
                        <th key={h} className="px-2 py-2 text-left text-[11px] font-bold text-slate-400 uppercase">{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {req.topEndpoints.map((e, i) => {
                      const mc = { GET: 'blue', POST: 'green', PUT: 'yellow', PATCH: 'orange', DELETE: 'red' };
                      return (
                        <tr key={i} className="border-b border-slate-50 hover:bg-slate-50">
                          <td className="px-2 py-2"><Chip label={e.method} color={mc[e.method] || 'slate'} /></td>
                          <td className="px-2 py-2 font-mono text-[11px] text-slate-700 truncate max-w-[220px]">{e.endpoint}</td>
                          <td className="px-2 py-2 text-[12px] font-bold text-slate-800">{e.count}</td>
                          <td className="px-2 py-2">
                            <span className={`font-mono text-[12px] font-semibold ${e.avgResponseTime > 1000 ? 'text-red-500' : e.avgResponseTime > 300 ? 'text-amber-500' : 'text-green-600'}`}>
                              {e.avgResponseTime}ms
                            </span>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>
          </Card>
        )}
      </div>

      {/* Email Logs */}
      <div>
        <SectionTitle>📧 Email Logs</SectionTitle>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mb-4">
          <StatCard label="Tổng email"   value={em.total}   icon="📬" />
          <StatCard label="Đã gửi"       value={em.sent}    icon="✅" />
          <StatCard label="Thất bại"     value={em.failed}  icon="❌" />
          <StatCard label="Đang chờ"     value={em.pending} icon="⏳" />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
          <Card>
            <div className="px-4 py-3">
              <SectionTitle>Tỉ lệ gửi</SectionTitle>
              <div className="flex flex-col gap-3">
                {[
                  ['Tỉ lệ thành công', em.successRate, 'bg-green-500'],
                  ['Tỉ lệ thất bại',   em.failureRate, 'bg-red-400'],
                ].map(([lbl, val, color]) => (
                  <div key={lbl}>
                    <div className="flex justify-between mb-1">
                      <span className="text-[12px] text-slate-500">{lbl}</span>
                      <span className="text-[12px] font-bold text-slate-800">{val?.toFixed(1)}%</span>
                    </div>
                    <div className="h-2 bg-slate-100 rounded-full overflow-hidden">
                      <div className={`h-full rounded-full ${color}`} style={{ width: `${val}%` }} />
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </Card>

          {em.byTemplate?.length > 0 && (
            <Card>
              <div className="px-4 py-3">
                <SectionTitle>Theo template</SectionTitle>
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b border-slate-100">
                        {['Template', 'Tổng', 'Gửi OK', 'Lỗi'].map(h => (
                          <th key={h} className="px-2 py-1.5 text-left text-[11px] font-bold text-slate-400 uppercase">{h}</th>
                        ))}
                      </tr>
                    </thead>
                    <tbody>
                      {em.byTemplate.map(t => (
                        <tr key={t.template} className="border-b border-slate-50 hover:bg-slate-50">
                          <td className="px-2 py-2"><Chip label={t.template} color="purple" /></td>
                          <td className="px-2 py-2 text-[12px] font-semibold text-slate-800">{t.total}</td>
                          <td className="px-2 py-2 text-[12px] text-green-600 font-semibold">{t.sent}</td>
                          <td className="px-2 py-2 text-[12px] text-red-400 font-semibold">{t.failed}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            </Card>
          )}
        </div>

        {em.recentFailed?.length > 0 && (
          <Card className="mt-4">
            <div className="px-4 py-3">
              <SectionTitle>Email thất bại gần đây</SectionTitle>
              <div className="flex flex-col gap-2">
                {em.recentFailed.map(e => (
                  <div key={e.id} className="flex items-start justify-between py-2 border-b border-slate-50 last:border-0 gap-3">
                    <div className="flex-1 min-w-0">
                      <div className="text-[12px] font-semibold text-slate-800 truncate">{e.to}</div>
                      <div className="text-[11px] text-slate-500 truncate">{e.subject}</div>
                      {e.errorMessage && (
                        <div className="text-[10px] text-red-400 mt-0.5 truncate">{e.errorMessage}</div>
                      )}
                    </div>
                    <div className="flex-shrink-0 text-right">
                      <Chip label={e.template || '—'} color="slate" />
                      <div className="text-[10px] text-slate-400 mt-1">{fmtFull(e.createdAt)}</div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </Card>
        )}
      </div>
    </div>
  );
}

// ── Main page ─────────────────────────────────────────────────
export default function AdminStatsPage() {
  const [tab, setTab] = useState('dashboard');

  return (
    <div>
      <PageHeader title="Thống kê" sub="Số liệu tổng hợp toàn hệ thống" />

      <Tabs
        tabs={[
          ['dashboard', '📊 Tổng quan'],
          ['orders',    '🛍️ Đơn hàng'],
          ['payments',  '💳 Thanh toán'],
          ['analytics', '📈 Analytics'],
          ['daily',     '📅 Daily Stats'],
          ['coupons',   '🎟️ Coupons'],
          ['media',     '📁 Media'],
          ['security',  '🛡️ Security'],
        ]}
        active={tab}
        onChange={setTab}
      />

      {tab === 'dashboard' && <DashboardTab />}
      {tab === 'orders'    && <OrdersTab />}
      {tab === 'payments'  && <PaymentsTab />}
      {tab === 'analytics' && <AnalyticsTab />}
      {tab === 'daily'     && <DailyStatsTab />}
      {tab === 'coupons'   && <CouponsStatsTab />}
      {tab === 'media'     && <MediaStatsTab />}
      {tab === 'security'  && <SecurityStatsTab />}
    </div>
  );
}