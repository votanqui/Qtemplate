/* ═══════════════════════════════════════════════
   LUXE STORE — script.js
═══════════════════════════════════════════════ */

document.addEventListener('DOMContentLoaded', () => {

  // ─── Custom Cursor ─────────────────────────────
  const cursor    = document.getElementById('cursor');
  const cursorDot = document.getElementById('cursorDot');

  if (window.innerWidth > 768) {
    let mx = 0, my = 0;
    let cx = 0, cy = 0;

    document.addEventListener('mousemove', e => {
      mx = e.clientX;
      my = e.clientY;
      cursorDot.style.left = mx + 'px';
      cursorDot.style.top  = my + 'px';
    });

    // Smooth cursor follow
    const animateCursor = () => {
      cx += (mx - cx) * 0.12;
      cy += (my - cy) * 0.12;
      cursor.style.left = cx + 'px';
      cursor.style.top  = cy + 'px';
      requestAnimationFrame(animateCursor);
    };
    animateCursor();

    // Hover effect on interactive elements
    const hoverEls = document.querySelectorAll('a, button, .product-card, .filter-tab');
    hoverEls.forEach(el => {
      el.addEventListener('mouseenter', () => cursor.classList.add('hover'));
      el.addEventListener('mouseleave', () => cursor.classList.remove('hover'));
    });
  }

  // ─── Navbar scroll ────────────────────────────
  const nav = document.getElementById('nav');
  window.addEventListener('scroll', () => {
    nav.classList.toggle('scrolled', window.scrollY > 60);
  }, { passive: true });

  // ─── Mobile hamburger ─────────────────────────
  const hamburger  = document.getElementById('hamburger');
  const mobileMenu = document.getElementById('mobileMenu');

  hamburger.addEventListener('click', () => {
    hamburger.classList.toggle('open');
    mobileMenu.classList.toggle('open');
  });

  mobileMenu.querySelectorAll('a').forEach(a => {
    a.addEventListener('click', () => {
      hamburger.classList.remove('open');
      mobileMenu.classList.remove('open');
    });
  });

  // ─── Reveal on scroll ─────────────────────────
  const reveals = document.querySelectorAll('.reveal');
  const revealObs = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        entry.target.classList.add('visible');
        revealObs.unobserve(entry.target);
      }
    });
  }, { threshold: 0.12, rootMargin: '0px 0px -40px 0px' });

  reveals.forEach(el => revealObs.observe(el));

  // Trigger hero reveals immediately
  document.querySelectorAll('.hero .reveal').forEach((el, i) => {
    setTimeout(() => el.classList.add('visible'), i * 120);
  });

  // ─── Counter animation ─────────────────────────
  const counters = document.querySelectorAll('.stat-num');
  const counterObs = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (!entry.isIntersecting) return;
      const el     = entry.target;
      const target = parseInt(el.dataset.target);
      const duration = 1800;
      const step   = target / (duration / 16);
      let current  = 0;
      const update = () => {
        current = Math.min(current + step, target);
        el.textContent = Math.floor(current).toLocaleString('vi-VN');
        if (current < target) requestAnimationFrame(update);
      };
      update();
      counterObs.unobserve(el);
    });
  }, { threshold: 0.5 });

  counters.forEach(el => counterObs.observe(el));

  // ─── Product filter tabs ───────────────────────
  const tabs     = document.querySelectorAll('.filter-tab');
  const products = document.querySelectorAll('.product-card');

  tabs.forEach(tab => {
    tab.addEventListener('click', () => {
      tabs.forEach(t => t.classList.remove('active'));
      tab.classList.add('active');

      const filter = tab.dataset.filter;
      products.forEach(card => {
        const tags = card.dataset.tag || '';
        const show = filter === 'all' || tags.includes(filter);

        if (show) {
          card.style.display = '';
          card.style.opacity = '0';
          card.style.transform = 'scale(0.95) translateY(16px)';
          requestAnimationFrame(() => {
            card.style.transition = 'opacity 0.4s, transform 0.4s';
            card.style.opacity = '1';
            card.style.transform = '';
          });
        } else {
          card.style.transition = 'opacity 0.25s';
          card.style.opacity = '0';
          setTimeout(() => { card.style.display = 'none'; }, 250);
        }
      });
    });
  });

  // ─── Add to cart ──────────────────────────────
  const cartToast = document.getElementById('cartToast');

  document.querySelectorAll('.btn-add-cart').forEach(btn => {
    btn.addEventListener('click', (e) => {
      e.stopPropagation();

      // Button feedback
      const orig = btn.textContent;
      btn.textContent = '✓ Đã thêm!';
      btn.style.background = '#2ECC71';
      setTimeout(() => {
        btn.textContent = orig;
        btn.style.background = '';
      }, 1500);

      // Toast
      cartToast.classList.add('show');
      setTimeout(() => cartToast.classList.remove('show'), 2800);
    });
  });

  // ─── Countdown Timer ──────────────────────────
  // Set target = now + 48 hours (stored in sessionStorage for persistence)
  let deadline = sessionStorage.getItem('luxe_deadline');
  if (!deadline) {
    deadline = Date.now() + 48 * 3600 * 1000;
    sessionStorage.setItem('luxe_deadline', deadline);
  } else {
    deadline = parseInt(deadline);
  }

  const cdH = document.getElementById('cd-h');
  const cdM = document.getElementById('cd-m');
  const cdS = document.getElementById('cd-s');

  const pad = n => String(Math.max(0, n)).padStart(2, '0');

  const updateCountdown = () => {
    const diff = Math.max(0, deadline - Date.now());
    const h = Math.floor(diff / 3600000);
    const m = Math.floor((diff % 3600000) / 60000);
    const s = Math.floor((diff % 60000)  / 1000);

    const setWithFlip = (el, val) => {
      const newVal = pad(val);
      if (el.textContent !== newVal) {
        el.style.transform = 'translateY(-4px)';
        el.style.opacity   = '0.4';
        setTimeout(() => {
          el.textContent   = newVal;
          el.style.transform = '';
          el.style.opacity   = '1';
        }, 120);
      }
    };

    setWithFlip(cdH, h);
    setWithFlip(cdM, m);
    setWithFlip(cdS, s);
  };

  cdH.style.transition = cdM.style.transition = cdS.style.transition = 'transform 0.12s, opacity 0.12s';
  updateCountdown();
  setInterval(updateCountdown, 1000);

  // ─── Smooth anchor scroll ─────────────────────
  document.querySelectorAll('a[href^="#"]').forEach(a => {
    a.addEventListener('click', e => {
      const target = document.querySelector(a.getAttribute('href'));
      if (target) {
        e.preventDefault();
        target.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    });
  });

  // ─── Parallax hero title ──────────────────────
  const heroTitle = document.querySelector('.hero-title');
  if (heroTitle && window.innerWidth > 768) {
    window.addEventListener('scroll', () => {
      const y = window.scrollY;
      heroTitle.style.transform = `translateY(${y * 0.25}px)`;
      heroTitle.style.opacity   = `${1 - y / 500}`;
    }, { passive: true });
  }

  // ─── Product card tilt effect ─────────────────
  if (window.innerWidth > 768) {
    document.querySelectorAll('.product-card').forEach(card => {
      card.addEventListener('mousemove', e => {
        const rect = card.getBoundingClientRect();
        const x = (e.clientX - rect.left) / rect.width  - 0.5;
        const y = (e.clientY - rect.top)  / rect.height - 0.5;
        card.style.transform = `translateY(-6px) rotateX(${-y * 5}deg) rotateY(${x * 5}deg)`;
        card.style.transition = 'box-shadow 0.3s, border-color 0.3s';
      });
      card.addEventListener('mouseleave', () => {
        card.style.transform  = '';
        card.style.transition = 'transform 0.4s, box-shadow 0.3s, border-color 0.3s';
      });
    });
  }

  // ─── CTA email validation ─────────────────────
  const ctaForm  = document.querySelector('.cta-form');
  const ctaInput = document.querySelector('.cta-input');
  const ctaBtn   = ctaForm?.querySelector('button');

  if (ctaBtn) {
    ctaBtn.addEventListener('click', () => {
      const email = ctaInput?.value?.trim();
      if (!email || !email.includes('@')) {
        ctaInput.style.borderColor = '#E84040';
        ctaInput.style.animation = 'shake 0.3s';
        setTimeout(() => {
          ctaInput.style.borderColor = '';
          ctaInput.style.animation = '';
        }, 800);
        return;
      }
      ctaBtn.textContent = '✓ Đăng ký thành công!';
      ctaBtn.style.background = 'linear-gradient(135deg, #2ECC71, #27AE60)';
      ctaInput.value = '';
      setTimeout(() => {
        ctaBtn.textContent = 'Nhận Ưu Đãi';
        ctaBtn.style.background = '';
      }, 3000);
    });
  }

  // Shake keyframe
  const shakeStyle = document.createElement('style');
  shakeStyle.textContent = `
    @keyframes shake {
      0%, 100% { transform: translateX(0); }
      25% { transform: translateX(-6px); }
      75% { transform: translateX(6px); }
    }
  `;
  document.head.appendChild(shakeStyle);

});
