document.addEventListener('DOMContentLoaded', () => {
    const authBtn = document.getElementById('auth-btn');
    const apiKeyInput = document.getElementById('api-key-input');
    const mainInterface = document.getElementById('main-interface');
    const authSection = document.getElementById('auth-section');
    const adminControls = document.getElementById('admin-controls');
    const content = document.getElementById('content');
    const modal = document.getElementById('modal');
    const stationSelect = document.getElementById('service-station-select');
    const requestForm = document.getElementById('request-form');
    const requestIdInput = document.getElementById('request-id-input');

    let savedApiKey = null;

    // --- Авторизация ---
    authBtn.addEventListener('click', () => {
        const key = apiKeyInput.value.trim();
        if (key) {
            savedApiKey = key;
            authSection.classList.add('hidden');
            adminControls.classList.remove('hidden');
            mainInterface.classList.remove('hidden');
            fetchData();
        }
    });

    // --- Получение данных ---
    async function fetchData() {
        try {
            const response = await fetch('http://localhost:5252/api/requests', {
                headers: { 'Admin-Key': savedApiKey }
            });
            if (!response.ok) throw new Error('Ошибка доступа');
            const data = await response.json();
            renderRequests(data);
        } catch (err) {
            alert(err.message);
        }
    }

    async function fetchStations() {
        const response = await fetch('http://localhost:5252/api/stations', {
            headers: { 'Admin-Key': savedApiKey }
        });
        const stations = await response.json();
        stationSelect.innerHTML = '<option value="">-- Выберите СТО --</option>' + 
            stations.map(s => `<option value="${s.id}">${s.name}</option>`).join('');
    }

    // --- Рендер карточек ---
    function renderRequests(requests) {
        window.currentRequests = requests; // Сохраняем для редактирования
        content.innerHTML = requests.map(r => `
            <div class="request-card" data-id="${r.id}">
                <div class="request-actions">
                    <button class="edit-btn" title="Редактировать">✎</button>
                    <button class="delete-btn" title="Удалить">✕</button>
                </div>
                <h3>${r.carModelName}</h3>
                <p>${r.issueDescription}</p>
                <p><small><b>СТО:</b> ${r.stationName}</small></p>
                <small class="text-muted">${new Date(r.createdAt).toLocaleString()}</small>
            </div>
        `).join('');
    }

    // --- Делегирование событий (Edit/Delete) ---
    content.addEventListener('click', async (e) => {
        const card = e.target.closest('.request-card');
        if (!card) return;
        const id = parseInt(card.dataset.id);

        if (e.target.classList.contains('delete-btn')) {
            if (confirm('Удалить эту заявку?')) {
                const res = await fetch(`http://localhost:5252/api/requests/${id}`, {
                    method: 'DELETE',
                    headers: { 'Admin-Key': savedApiKey }
                });
                if (res.ok) fetchData();
            }
        }

        if (e.target.classList.contains('edit-btn')) {
            const req = window.currentRequests.find(x => x.id === id);
            if (req) {
                await fetchStations(); // Обновляем список СТО
                document.getElementById('car-model-name').value = req.carModelName;
                document.getElementById('issue-text').value = req.issueDescription;
                stationSelect.value = req.serviceStationId;
                requestIdInput.value = req.id;
                modal.classList.remove('hidden');
            }
        }
    });

    // --- Сохранение (Создание/Обновление) ---
    requestForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = requestIdInput.value;
        const payload = {
            carName: document.getElementById('car-model-name').value,
            stationId: parseInt(stationSelect.value),
            issueDescription: document.getElementById('issue-text').value
        };

        const url = id ? `http://localhost:5252/api/requests/${id}` : 'http://localhost:5252/api/requests';
        const method = id ? 'PUT' : 'POST';

        const res = await fetch(url, {
            method: method,
            headers: { 
                'Content-Type': 'application/json',
                'Admin-Key': savedApiKey 
            },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            modal.classList.add('hidden');
            requestForm.reset();
            requestIdInput.value = '';
            fetchData();
        }
    });

    document.getElementById('add-btn').addEventListener('click', () => {
        requestIdInput.value = '';
        requestForm.reset();
        fetchStations();
        modal.classList.remove('hidden');
    });

    document.getElementById('close-modal-btn').addEventListener('click', () => modal.classList.add('hidden'));
    document.getElementById('logout-btn').addEventListener('click', () => location.reload());
});