function isLoggedIn() {
    return !!localStorage.getItem('accessToken');
}

function getUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
}

function isAdmin() {
    const user = getUser();
    return user && user.isAdmin === true;
}

function logout() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    window.location.href = '/login.html';
}

function setAuth(data) {
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('user', JSON.stringify(data.user));
}

function updateNav() {
    const nav = document.getElementById('mainNav');
    if (!nav) return;

    const user = getUser();
    const isAdminUser = isAdmin();

    let adminLinks = '';
    if (isAdminUser) {
        adminLinks = `
            <li class="nav-item"><a class="nav-link" href="/admin.html">Админка</a></li>
        `;
    }

    nav.innerHTML = `
        <div class="container-fluid">
            <a class="navbar-brand" href="/index.html">Голосования</a>
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navCollapse">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="navCollapse">
                <ul class="navbar-nav me-auto">
                    ${user ? `<li class="nav-item"><a class="nav-link" href="/history.html">История</a></li>` : ''}
                    ${adminLinks}
                </ul>
                <ul class="navbar-nav ms-auto">
                    ${user ? `
                        <li class="nav-item"><a class="nav-link" href="/profile.html">${user.fullName}</a></li>
                        <li class="nav-item"><a class="nav-link" href="#" onclick="logout(); return false;">Выйти</a></li>
                    ` : `
                        <li class="nav-item"><a class="nav-link" href="/login.html">Вход</a></li>
                    `}
                </ul>
            </div>
        </div>
    `;
}

async function initAuth() {
    updateNav();

    const path = window.location.pathname;
    const isLoginPage = path.includes('/login.html');
    const isVerifyPage = path.includes('/verify-email.html');

    if (!isLoggedIn() && !isLoginPage && !isVerifyPage) {
        window.location.href = '/login.html';
        return false;
    }

    if (isLoggedIn()) {
        const user = getUser();
        if (user && !user.emailVerified && !isVerifyPage) {
            window.location.href = '/verify-email.html';
            return false;
        }
    }

    return true;
}
