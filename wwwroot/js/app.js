/* PharMarket App JS */
(function () {
    'use strict';

    // Auto-dismiss alerts after 5s
    document.querySelectorAll('.alert').forEach(function (a) {
        setTimeout(function () { a.style.opacity = '0'; setTimeout(function () { a.remove(); }, 300); }, 5000);
    });

    // Close sidebar on overlay click (mobile)
    document.addEventListener('click', function (e) {
        var sidebar = document.getElementById('sidebar');
        if (sidebar && sidebar.classList.contains('open') && !sidebar.contains(e.target) && !e.target.closest('.sidebar-toggle')) {
            sidebar.classList.remove('open');
        }
    });

    // Confirm delete actions
    document.querySelectorAll('form[data-confirm]').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            if (!confirm(form.dataset.confirm || 'Are you sure?')) {
                e.preventDefault();
            }
        });
    });

    // Loading on ALL form submissions
    document.querySelectorAll('form').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            var btn = form.querySelector('button[type="submit"]');
            var loadingText = btn && btn.dataset.loading ? btn.dataset.loading : 'Loading...';
            if (btn) btn.disabled = true;
            if (typeof showLoading === 'function') showLoading(loadingText);
        });
    });

    // Loading on ALL buttons and clickable elements with data-loading
    document.addEventListener('click', function (e) {
        var el = e.target.closest('[data-loading]');
        if (el) {
            var text = el.dataset.loading || 'Loading...';
            if (typeof showLoading === 'function') showLoading(text);
        }
    });

    // Keyboard shortcut: ESC closes modals/loading
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            if (typeof hideLoading === 'function') hideLoading();
        }
    });
})();
