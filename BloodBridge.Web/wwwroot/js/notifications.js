(function () {
    const list = document.getElementById('notificationList');
    const count = document.getElementById('notificationCount');
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    async function loadNotifications() {
        try {
            const response = await fetch('/notifications/unread', { headers: { 'Accept': 'application/json' } });
            if (!response.ok) throw new Error('Unable to load notifications.');
            const notifications = await response.json();
            count.textContent = notifications.length;
            count.classList.toggle('d-none', notifications.length === 0);
            if (notifications.length === 0) {
                list.textContent = 'No unread notifications.';
                return;
            }
            list.innerHTML = notifications.map(notification => `
                <div class="notification-item border-bottom py-2" data-id="${notification.id}">
                    <div class="fw-semibold">${escapeHtml(notification.title)}</div>
                    <div>${escapeHtml(notification.message)}</div>
                    <button type="button" class="btn btn-link btn-sm p-0 mark-read">Dismiss</button>
                </div>`).join('');
            list.querySelectorAll('.mark-read').forEach(button => button.addEventListener('click', markRead));
        } catch (error) {
            list.textContent = error.message;
        }
    }

    async function markRead(event) {
        const item = event.target.closest('.notification-item');
        const response = await fetch(`/notifications/${item.dataset.id}/read`, {
            method: 'POST',
            headers: { 'RequestVerificationToken': token }
        });
        if (response.ok) {
            item.remove();
            const remaining = document.querySelectorAll('.notification-item').length;
            count.textContent = remaining;
            count.classList.toggle('d-none', remaining === 0);
            if (remaining === 0) list.textContent = 'No unread notifications.';
        }
    }

    function escapeHtml(value) {
        return String(value).replace(/[&<>'"]/g, character => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[character]));
    }

    loadNotifications();
    setInterval(loadNotifications, 30000);
})();
