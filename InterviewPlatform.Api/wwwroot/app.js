// Состояние 
let token = sessionStorage.getItem('token');
let user = null;
let currentPage = null;

const main = document.getElementById('main');
const nav = document.getElementById('nav');
const header = document.getElementById('header');

const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const NAME_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';

const statusNames = {
  Pending: 'Ожидает решения',
  Accepted: 'Принять',
  Rejected: 'Отклонить',
  NextStage: 'На следующий этап'
};

const roleNames = {
  Admin: 'Администратор',
  HR: 'HR',
  Approver: 'Согласующий'
};

// Вспомогательные функции 
function h(tag, props, ...children) {
  const node = document.createElement(tag);

  for (const key in props || {}) {
    const value = props[key];

    if (value === false || value === null) {
      continue;
    }

    if (key === 'class') {
      node.className = value;
    } else if (key.startsWith('on')) {
      node.addEventListener(key.slice(2), value);
    } else {
      node.setAttribute(key, value === true ? '' : value);
    }
  }

  for (const child of children.flat()) {
    if (child === null || child === undefined || child === false) {
      continue;
    }
    node.append(child);
  }

  return node;
}

function showMessage(text, isError) {
  const box = document.getElementById('message');
  box.textContent = text;
  box.className = 'message ' + (isError ? 'error' : 'success');

  clearTimeout(showMessage.timer);
  showMessage.timer = setTimeout(() => {
    box.className = 'message hidden';
  }, 4000);
}

function errorText(res) {
  if (res.status === 403) {
    return 'Нет доступа';
  }

  const data = res.data || {};

  if (data.error) {
    return data.error;
  }

  if (data.errors) {
    return Object.values(data.errors).flat().join('; ');
  }

  return 'Ошибка ' + res.status;
}

function formatDate(value) {
  return new Date(value).toLocaleString('ru-RU', { dateStyle: 'short', timeStyle: 'short' });
}

function splitList(text) {
  return text.split(',').map(x => x.trim()).filter(x => x !== '');
}

function tableOf(headers, rows) {
  if (rows.length === 0) {
    return h('p', { class: 'muted' }, 'Нет данных');
  }

  return h('table', {},
    h('thead', {}, h('tr', {}, headers.map(x => h('th', {}, x)))),
    h('tbody', {}, rows.map(row => h('tr', {}, row.map(cell => h('td', {}, cell))))));
}

function field(label, name, type) {
  return h('div', {},
    h('label', {}, label),
    h('input', { name: name, type: type || 'text', required: true }));
}

// Запросы к API 
async function api(method, url, body) {
  const headers = {};

  if (token) {
    headers['Authorization'] = 'Bearer ' + token;
  }

  if (body) {
    headers['Content-Type'] = 'application/json';
  }

  const response = await fetch(url, {
    method: method,
    headers: headers,
    body: body ? JSON.stringify(body) : undefined
  });

  // токен просрочен или недействителен - возвращаем на вход
  if (response.status === 401 && token) {
    logout();
    return { ok: false, status: 401, data: null };
  }

  const text = await response.text();
  const data = text ? JSON.parse(text) : null;

  return { ok: response.ok, status: response.status, data: data };
}

// PDF-документы 
async function downloadDocument(url, label) {
  showMessage('Формируем документ: ' + label + '...', false);

  const start = await api('POST', url);
  if (!start.ok) {
    showMessage(errorText(start), true);
    return;
  }

  const id = start.data.requestId;

  // Worker делает PDF асинхронно через Kafka, поэтому опрашиваем статус
  for (let i = 0; i < 20; i++) {
    await new Promise(resolve => setTimeout(resolve, 1000));

    const info = await api('GET', '/api/documents/' + id);
    if (!info.ok) {
      showMessage(errorText(info), true);
      return;
    }

    if (info.data.status === 'Ready') {
      await saveFile(id, info.data.fileName);
      showMessage('Документ готов: ' + label, false);
      return;
    }

    if (info.data.status === 'Failed') {
      showMessage('Не удалось создать документ', true);
      return;
    }
  }

  showMessage('Документ ещё не готов, попробуйте позже', true);
}

async function saveFile(id, fileName) {
  const response = await fetch('/api/documents/' + id + '/file', {
    headers: { 'Authorization': 'Bearer ' + token }
  });

  const blob = await response.blob();
  const link = h('a', { href: URL.createObjectURL(blob), download: fileName });
  link.click();
  URL.revokeObjectURL(link.href);
}

// Архив 
async function archiveItem(kind, id) {
  if (!confirm('Перенести в архив?')) {
    return;
  }

  const res = await api('POST', '/api/archive/' + kind + '/' + id);

  if (!res.ok) {
    showMessage(errorText(res), true);
    return;
  }

  showMessage('Перенесено в архив', false);
  openPage(currentPage);
}

async function restoreItem(kind, id) {
  const res = await api('POST', '/api/archive/' + kind + '/' + id + '/restore');

  if (!res.ok) {
    showMessage(errorText(res), true);
    return;
  }

  showMessage('Восстановлено', false);
  openPage(currentPage);
}

// Вход и выход 
function readUser(jwt) {
  const base64 = jwt.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
  const payload = JSON.parse(atob(base64));

  if (payload.exp * 1000 < Date.now()) {
    return null;
  }

  let roles = payload[ROLE_CLAIM] || payload.role || [];
  if (!Array.isArray(roles)) {
    roles = [roles];
  }

  return { name: payload[NAME_CLAIM] || payload.unique_name, roles: roles };
}

function loginPage() {
  main.replaceChildren();

  const form = h('form', { onsubmit: login },
    h('h2', {}, 'Вход'),
    field('Логин', 'login'),
    field('Пароль', 'password', 'password'),
    h('button', { type: 'submit' }, 'Войти'));

  main.append(h('div', { class: 'login-box' }, form));
}

async function login(e) {
  e.preventDefault();
  const form = e.target;

  const res = await api('POST', '/api/auth/login', {
    login: form.login.value,
    password: form.password.value
  });

  if (!res.ok) {
    showMessage(errorText(res), true);
    return;
  }

  token = res.data.token;
  sessionStorage.setItem('token', token);
  start();
}

function logout() {
  token = null;
  user = null;
  sessionStorage.removeItem('token');
  header.classList.add('hidden');
  loginPage();
}

document.getElementById('logoutBtn').addEventListener('click', logout);

// Страница: пользователи (Администратор)
async function usersPage() {
  const res = await api('GET', '/api/users');
  const users = res.data || [];

  const rows = users.map(u => [String(u.id), u.fullName, u.login, u.roles.map(r => roleNames[r]).join(', ')]);

  const form = h('form', { onsubmit: createUser },
    h('h3', {}, 'Новый пользователь'),
    field('ФИО', 'fullName'),
    field('Логин', 'login'),
    field('Пароль (минимум 6 символов)', 'password', 'password'),
    h('div', { class: 'checks' },
      h('label', {}, 'Роли:'),
      Object.keys(roleNames).map(role =>
        h('label', {}, h('input', { type: 'checkbox', name: 'roles', value: role }), ' ' + roleNames[role]))),
    h('button', { type: 'submit' }, 'Создать'));

  main.append(h('h2', {}, 'Пользователи'), tableOf(['ID', 'ФИО', 'Логин', 'Роли'], rows), form);
}

async function createUser(e) {
  e.preventDefault();
  const form = e.target;
  const roles = [...form.querySelectorAll('input[name=roles]:checked')].map(x => x.value);

  const res = await api('POST', '/api/users', {
    fullName: form.fullName.value,
    login: form.login.value,
    password: form.password.value,
    roles: roles
  });

  if (!res.ok) {
    showMessage(errorText(res), true);
    return;
  }

  showMessage('Пользователь создан', false);
  openPage('users');
}

// Страница: кандидаты (HR, Согласующий)
async function candidatesPage() {
  const isHr = user.roles.includes('HR');
  const list = h('div', {});
  const searchInput = h('input', { type: 'search', placeholder: 'Поиск по ФИО' });

  const searchForm = h('form', {
    class: 'inline',
    onsubmit: e => {
      e.preventDefault();
      loadCandidates(list, searchInput.value, isHr);
    }
  }, searchInput, h('button', { type: 'submit' }, 'Найти'));

  main.append(h('h2', {}, 'Кандидаты'), searchForm);

  if (isHr) {
    main.append(candidateForm());
  }

  main.append(list);
  await loadCandidates(list, '', isHr);
}

async function loadCandidates(container, search, isHr) {
  const res = await api('GET', '/api/candidates?search=' + encodeURIComponent(search));
  const candidates = res.data || [];

  const rows = candidates.map(c => {
    const row = [
      c.fullName,
      c.phone,
      c.city,
      c.skills.map(s => s.name + ' (' + s.rating + ')').join(', '),
      c.interviews.map(i => h('div', {}, i.vacancyTitle + ': ' + statusNames[i.status]))
    ];

    if (isHr) {
      row.push(h('div', { class: 'actions' },
        h('button', { onclick: () => downloadDocument('/api/documents/candidate-card/' + c.id, 'карточка кандидата') }, 'PDF'),
        h('button', { class: 'danger', onclick: () => archiveItem('candidates', c.id) }, 'В архив')));
    }

    return row;
  });

  const headers = ['ФИО', 'Телефон', 'Город', 'Навыки', 'Собеседования'];
  if (isHr) {
    headers.push('Действия');
  }

  container.replaceChildren(tableOf(headers, rows));
}

function candidateForm() {
  const skillsBox = h('div', {});

  const addSkill = () => {
    skillsBox.append(h('div', { class: 'row' },
      h('input', { class: 'skill-name', placeholder: 'Навык' }),
      h('input', { class: 'skill-rating', type: 'number', min: 1, max: 5, value: 3 })));
  };
  addSkill();

  const form = h('form', { onsubmit: createCandidate },
    field('ФИО', 'fullName'),
    field('Телефон', 'phone'),
    field('Город', 'city'),
    h('label', {}, 'Навыки (оценка от 1 до 5):'),
    skillsBox,
    h('button', { type: 'button', onclick: addSkill }, '+ навык'),
    h('button', { type: 'submit' }, 'Сохранить'));

  return h('details', {}, h('summary', {}, 'Новый кандидат'), form);
}

async function createCandidate(e) {
  e.preventDefault();
  const form = e.target;

  const skills = [...form.querySelectorAll('.row')]
    .map(row => ({
      name: row.querySelector('.skill-name').value,
      rating: Number(row.querySelector('.skill-rating').value)
    }))
    .filter(skill => skill.name.trim() !== '');

  const res = await api('POST', '/api/candidates', {
    fullName: form.fullName.value,
    phone: form.phone.value,
    city: form.city.value,
    skills: skills
  });

  if (!res.ok) {
    showMessage(errorText(res), true);
    return;
  }

  showMessage('Кандидат создан', false);
  openPage('candidates');
}

// Страница: вакансии (HR) 
async function vacanciesPage() {
  const res = await api('GET', '/api/vacancies');
  const vacancies = res.data || [];

  const rows = vacancies.map(v => [v.title, v.competencies.map(c => c.name).join(', ')]);

  const form = h('form', { onsubmit: createVacancy },
    h('h3', {}, 'Новая вакансия'),
    field('Название', 'title'),
    field('Компетенции (через запятую)', 'competencies'),
    h('button', { type: 'submit' }, 'Создать'));

  main.append(h('h2', {}, 'Вакансии'), tableOf(['Название', 'Компетенции'], rows), form);
}

async function createVacancy(e) {
  e.preventDefault();
  const form = e.target;

  const res = await api('POST', '/api/vacancies', {
    title: form.title.value,
    competencies: splitList(form.competencies.value)
  });

  if (!res.ok) {
    showMessage(errorText(res), true);
    return;
  }

  showMessage('Вакансия создана', false);
  openPage('vacancies');
}

// Страница: собеседования (HR, Согласующий) 
async function interviewsPage() {
  const isHr = user.roles.includes('HR');
  const isApprover = user.roles.includes('Approver');

  const res = await api('GET', '/api/interviews');
  const interviews = res.data || [];

  main.append(h('h2', {}, 'Собеседования'));

  let vacancies = [];

  if (isHr) {
    vacancies = (await api('GET', '/api/vacancies')).data || [];
    const candidates = (await api('GET', '/api/candidates')).data || [];
    main.append(interviewForm(candidates, vacancies));
  }

  if (interviews.length === 0) {
    main.append(h('p', { class: 'muted' }, 'Собеседований пока нет'));
  }

  for (const interview of interviews) {
    main.append(interviewCard(interview, vacancies, isHr, isApprover));
  }
}

function interviewForm(candidates, vacancies) {
  const form = h('form', { onsubmit: createInterview },
    h('label', {}, 'Кандидат'),
    h('select', { name: 'candidateId', required: true },
      candidates.map(c => h('option', { value: c.id }, c.fullName))),
    h('label', {}, 'Вакансия'),
    h('select', { name: 'vacancyId', required: true },
      vacancies.map(v => h('option', { value: v.id }, v.title))),
    h('label', {}, 'Дата и время'),
    h('input', { name: 'scheduledAt', type: 'datetime-local', required: true }),
    h('label', {}, 'Этапы (через запятую)'),
    h('input', { name: 'stages', required: true, placeholder: 'Скрининг, Техническое интервью' }),
    h('label', {}, 'Комментарий'),
    h('input', { name: 'note' }),
    h('button', { type: 'submit' }, 'Создать'));

  return h('details', {}, h('summary', {}, 'Новое собеседование'), form);
}

async function createInterview(e) {
  e.preventDefault();
  const form = e.target;

  const res = await api('POST', '/api/interviews', {
    candidateId: Number(form.candidateId.value),
    vacancyId: Number(form.vacancyId.value),
    scheduledAt: new Date(form.scheduledAt.value).toISOString(),
    stages: splitList(form.stages.value),
    comment: form.note.value,
    ratings: []
  });

  if (!res.ok) {
    showMessage(errorText(res), true);
    return;
  }

  showMessage('Собеседование создано, теперь можно заполнить оценки', false);
  openPage('interviews');
}

function interviewCard(interview, vacancies, isHr, isApprover) {
  const ratingRows = interview.ratings.map(r => [r.competencyName, String(r.score), r.comment]);

  const card = h('div', { class: 'card' },
    h('h3', {}, interview.candidateName + ' — ' + interview.vacancyTitle),
    h('p', {}, formatDate(interview.scheduledAt) + ' · ',
      h('span', { class: 'badge ' + interview.status }, statusNames[interview.status])),
    h('p', {}, 'Этапы: ' + interview.stages.join(', ')),
    tableOf(['Компетенция', 'Оценка', 'Комментарий'], ratingRows),
    interview.comment ? h('p', {}, 'Комментарий: ' + interview.comment) : null);

  if (isHr) {
    card.append(
      ratingsEditor(interview, vacancies),
      h('div', { class: 'actions' },
        h('button', { onclick: () => downloadDocument('/api/documents/interview-protocol/' + interview.id, 'протокол') }, 'PDF протокол'),
        h('button', { onclick: () => downloadDocument('/api/documents/invitation/' + interview.id, 'приглашение') }, 'PDF приглашение'),
        h('button', { onclick: () => downloadDocument('/api/documents/rejection/' + interview.id, 'отказ') }, 'PDF отказ'),
        h('button', { class: 'danger', onclick: () => archiveItem('interviews', interview.id) }, 'В архив')));
  }

  if (isApprover) {
    card.append(decisionBlock(interview));
  }

  return card;
}

function ratingsEditor(interview, vacancies) {
  const vacancy = vacancies.find(v => v.id === interview.vacancyId);
  const body = h('div', {});

  if (!vacancy) {
    body.append(h('p', { class: 'muted' }, 'Вакансия не найдена'));
    return h('details', {}, h('summary', {}, 'Заполнить оценки'), body);
  }

  const rows = vacancy.competencies.map(c => {
    const old = interview.ratings.find(r => r.competencyId === c.id);

    return h('tr', { 'data-id': c.id },
      h('td', {}, c.name),
      h('td', {}, h('input', { class: 'score', type: 'number', min: 1, max: 5, value: old ? old.score : '' })),
      h('td', {}, h('input', { class: 'comment', value: old ? old.comment : '' })));
  });

  const table = h('table', {},
    h('thead', {}, h('tr', {}, h('th', {}, 'Компетенция'), h('th', {}, 'Оценка 1–5'), h('th', {}, 'Комментарий'))),
    h('tbody', {}, rows));

  const saveButton = h('button', { onclick: () => saveRatings(interview.id, table) }, 'Сохранить оценки');

  body.append(table, saveButton);
  return h('details', {}, h('summary', {}, 'Заполнить оценки'), body);
}

async function saveRatings(interviewId, table) {
  const ratings = [...table.querySelectorAll('tbody tr')]
    .filter(row => row.querySelector('.score').value !== '')
    .map(row => ({
      competencyId: Number(row.dataset.id),
      score: Number(row.querySelector('.score').value),
      comment: row.querySelector('.comment').value
    }));

  const res = await api('PUT', '/api/interviews/' + interviewId + '/ratings', { ratings: ratings });

  if (!res.ok) {
    showMessage(errorText(res), true);
    return;
  }

  showMessage('Оценки сохранены', false);
  openPage('interviews');
}

function decisionBlock(interview) {
  const select = h('select', {},
    ['Accepted', 'Rejected', 'NextStage'].map(status => h('option', { value: status }, statusNames[status])));

  const button = h('button', { onclick: async () => {
    const res = await api('PUT', '/api/interviews/' + interview.id + '/decision', { status: select.value });

    if (!res.ok) {
      showMessage(errorText(res), true);
      return;
    }

    showMessage('Решение сохранено', false);
    openPage('interviews');
  } }, 'Сохранить решение');

  return h('div', { class: 'actions' }, select, button);
}

// Страница: архив (HR) 
async function archivePage() {
  const candidates = (await api('GET', '/api/archive/candidates')).data || [];
  const interviews = (await api('GET', '/api/archive/interviews')).data || [];

  const candidateRows = candidates.map(c => [
    c.fullName, c.phone, c.city,
    h('button', { onclick: () => restoreItem('candidates', c.id) }, 'Восстановить')
  ]);

  const interviewRows = interviews.map(i => [
    i.candidateName, i.vacancyTitle, formatDate(i.scheduledAt), statusNames[i.status],
    h('button', { onclick: () => restoreItem('interviews', i.id) }, 'Восстановить')
  ]);

  main.append(
    h('h2', {}, 'Архив'),
    h('h3', {}, 'Кандидаты'),
    tableOf(['ФИО', 'Телефон', 'Город', ''], candidateRows),
    h('h3', {}, 'Собеседования'),
    tableOf(['Кандидат', 'Вакансия', 'Дата', 'Статус', ''], interviewRows));
}

// Страницы и меню 
const pages = {
  users: { title: 'Пользователи', roles: ['Admin'], render: usersPage },
  candidates: { title: 'Кандидаты', roles: ['HR', 'Approver'], render: candidatesPage },
  vacancies: { title: 'Вакансии', roles: ['HR'], render: vacanciesPage },
  interviews: { title: 'Собеседования', roles: ['HR', 'Approver'], render: interviewsPage },
  archive: { title: 'Архив', roles: ['HR'], render: archivePage }
};

function allowedPages() {
  return Object.keys(pages).filter(key => pages[key].roles.some(role => user.roles.includes(role)));
}

async function openPage(name) {
  currentPage = name;
  main.replaceChildren();

  for (const button of nav.children) {
    button.classList.toggle('active', button.dataset.page === name);
  }

  await pages[name].render();
}

function start() {
  user = token ? readUser(token) : null;

  if (!user) {
    logout();
    return;
  }

  header.classList.remove('hidden');
  document.getElementById('userInfo').textContent = user.name + ' (' + user.roles.map(r => roleNames[r]).join(', ') + ')';

  nav.replaceChildren(...allowedPages().map(key =>
    h('button', { 'data-page': key, onclick: () => openPage(key) }, pages[key].title)));

  openPage(allowedPages()[0]);
}

start();
