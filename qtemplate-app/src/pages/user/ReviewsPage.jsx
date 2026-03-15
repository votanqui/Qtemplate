import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { userApi } from '../../api/services';
import { extractError } from '../../api/client';
import { LoadingPage, StarRating, EmptyState, Spinner, useToast } from '../../components/ui';
import { ReviewEditModal } from '../../modals/user/ReviewEditModal';
import { useLang } from '../../context/Langcontext';

export default function ReviewsPage() {
  const { t } = useLang();
  const toast = useToast();
  const [reviews, setReviews] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editModal, setEditModal] = useState(null);
  const [deleteLoading, setDeleteLoading] = useState({});

  const statusBadge = {
    Pending:  <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full bg-amber-50 border border-amber-200 text-amber-700 text-xs font-semibold">{t('reviews.status_pending')}</span>,
    Approved: <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full bg-emerald-50 border border-emerald-200 text-emerald-700 text-xs font-semibold">{t('reviews.status_approved')}</span>,
    Rejected: <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full bg-red-50 border border-red-200 text-red-600 text-xs font-semibold">{t('reviews.status_rejected')}</span>,
  };

  const fetchReviews = () => {
    setLoading(true);
    userApi.getMyReviews()
      .then(res => setReviews(res.data.data || []))
      .catch(err => toast.error(extractError(err), t('reviews.load_err')))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchReviews(); }, []);

  const handleDelete = async (id) => {
    if (!confirm(t('reviews.confirm_delete'))) return;
    setDeleteLoading(d => ({ ...d, [id]: true }));
    try {
      await userApi.deleteReview(id);
      setReviews(r => r.filter(x => x.id !== id));
      toast.success(t('reviews.delete_ok'));
    } catch (err) {
      toast.error(extractError(err), t('reviews.delete_err'));
    } finally {
      setDeleteLoading(d => ({ ...d, [id]: false }));
    }
  };

  const handleEditSaved = () => {
    setEditModal(null);
    fetchReviews();
    toast.success(t('reviews.update_ok'));
  };

  if (loading) return <LoadingPage />;

  return (
    <div className="animate-fade-in">

      {/* Header */}
      <div className="mb-7">
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full text-xs font-semibold uppercase tracking-wider mb-3"
          style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-secondary)' }}>
          ⭐ {t('reviews.title')}
        </div>
        <h1 className="text-2xl font-black tracking-tight" style={{ color: 'var(--text-primary)' }}>
          {t('reviews.title')}
        </h1>
        <p className="text-sm mt-1" style={{ color: 'var(--text-muted)' }}>
          {reviews.length} {t('reviews.count')}
        </p>
      </div>

      {reviews.length === 0 ? (
        <EmptyState
          icon="⭐"
          title={t('review.empty')}
          description={t('reviews.empty_desc')}
        />
      ) : (
        <div className="space-y-3">
          {reviews.map(review => (
            <div key={review.id}
              className="rounded-2xl p-5 shadow-sm transition-colors"
              style={{ backgroundColor: 'var(--bg-card)', border: '1px solid var(--border)' }}
            >
              {/* Top row */}
              <div className="flex items-start justify-between gap-3 mb-3">
                <div className="flex-1 min-w-0">
                  <Link
                    to={`/templates/${review.templateSlug}`}
                    className="inline-flex items-center gap-1.5 font-bold text-sm transition-colors group/link hover:text-violet-500"
                    style={{ color: 'var(--text-primary)' }}
                  >
                    <span className="truncate">{review.templateName}</span>
                    <span className="text-xs transition-colors" style={{ color: 'var(--text-muted)' }}>→</span>
                  </Link>
                  <div className="flex items-center gap-2.5 mt-1.5">
                    <StarRating value={review.rating} />
                    <span className="text-xs" style={{ color: 'var(--text-muted)' }}>
                      {new Date(review.createdAt).toLocaleDateString('vi-VN')}
                    </span>
                  </div>
                </div>
                <div className="flex items-center gap-2 flex-wrap justify-end shrink-0">
                  {statusBadge[review.aiStatus]}
                  {review.isApproved && (
                    <span className="inline-flex items-center px-2.5 py-0.5 rounded-full bg-emerald-50 border border-emerald-200 text-emerald-700 text-xs font-semibold">
                      {t('reviews.show')}
                    </span>
                  )}
                </div>
              </div>

              {/* Content */}
              {review.title && (
                <p className="font-semibold text-sm mb-1" style={{ color: 'var(--text-primary)' }}>
                  {review.title}
                </p>
              )}
              {review.comment && (
                <p className="text-sm leading-relaxed" style={{ color: 'var(--text-secondary)' }}>
                  {review.comment}
                </p>
              )}

              {/* Admin reply */}
              {review.adminReply && (
                <div className="mt-3 pl-3 border-l-2 border-violet-300 rounded-r-xl py-2.5 pr-3"
                  style={{ backgroundColor: 'var(--bg-elevated)' }}>
                  <p className="text-xs font-bold text-violet-500 mb-1">{t('reviews.admin_reply')}</p>
                  <p className="text-sm leading-relaxed" style={{ color: 'var(--text-secondary)' }}>
                    {review.adminReply}
                  </p>
                </div>
              )}

              {/* Actions */}
              <div className="flex items-center gap-2 mt-4 pt-3"
                style={{ borderTop: '1px solid var(--border)' }}>
                <Link
                  to={`/templates/${review.templateSlug}`}
                  className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-bold transition-all"
                  style={{ backgroundColor: 'var(--bg-elevated)', border: '1px solid var(--border)', color: 'var(--text-secondary)' }}
                >
                  {t('reviews.view_tpl')}
                </Link>
                <button
                  onClick={() => setEditModal(review)}
                  className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-bold transition-all"
                  style={{ border: '1px solid var(--border)', color: 'var(--text-secondary)' }}
                  onMouseEnter={e => e.currentTarget.style.color = 'var(--text-primary)'}
                  onMouseLeave={e => e.currentTarget.style.color = 'var(--text-secondary)'}
                >
                  {t('reviews.edit_btn')}
                </button>
                <button
                  onClick={() => handleDelete(review.id)}
                  disabled={deleteLoading[review.id]}
                  className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-bold text-red-500 hover:bg-red-500/10 transition-all disabled:opacity-50"
                  style={{ border: '1px solid rgba(239,68,68,0.3)' }}
                >
                  {deleteLoading[review.id] ? <Spinner /> : t('reviews.delete_btn')}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      <ReviewEditModal
        review={editModal}
        onClose={() => setEditModal(null)}
        onSaved={handleEditSaved}
      />
    </div>
  );
}