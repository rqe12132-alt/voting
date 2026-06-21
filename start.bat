@echo off
setlocal EnableDelayedExpansion

chcp 65001 >nul

set SCRIPT_DIR=%~dp0
set BACKEND_DIR=%SCRIPT_DIR%voting-backend\VotingApp\bin\Debug\net8.0
set FRONTEND_DIR=%SCRIPT_DIR%voting-frontend

echo 🚀 Запуск VotingApp из %SCRIPT_DIR%

REM --- Проверка dotnet ---
where dotnet >nul 2>nul
if errorlevel 1 (
    echo ❌ dotnet не найден. Установите .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

REM --- Проверка Python ---
where python >nul 2>nul
if errorlevel 1 (
    echo ❌ Python не найден. Установите Python 3.
    pause
    exit /b 1
)

REM --- Проверка Docker ---
where docker >nul 2>nul
if errorlevel 1 (
    echo ❌ Docker не найден. Установите Docker Desktop.
    pause
    exit /b 1
)

REM --- Запуск MSSQL Server ---
echo 📦 Проверка MSSQL Server...
docker ps --format "{{.Names}}" | findstr /ix "sqlserver" >nul
if errorlevel 1 (
    echo   Запуск MSSQL...
    docker start sqlserver 2>nul
    if errorlevel 1 (
        docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Pass1" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
    )
    echo   ⏳ Ждём инициализацию БД (15 сек)...
    timeout /t 15 /nobreak >nul
)
echo   ✅ MSSQL запущен

REM --- Запуск MailHog ---
echo 📧 Проверка MailHog (SMTP)...
docker ps --format "{{.Names}}" | findstr /ix "mailhog" >nul
if errorlevel 1 (
    echo   Запуск MailHog...
    docker start mailhog 2>nul
    if errorlevel 1 (
        docker run -d --name mailhog -p 1025:1025 -p 8025:8025 mailhog/mailhog:latest
    )
    timeout /t 2 /nobreak >nul
)
echo   ✅ MailHog запущен на http://localhost:8025

REM --- Сборка backend, если нет DLL ---
echo 🔧 Проверка Backend...
if not exist "%BACKEND_DIR%\VotingApp.dll" (
    echo   ⚠️  Собранный backend не найден. Выполняется dotnet build...
    cd /d "%SCRIPT_DIR%\voting-backend\VotingApp"
    dotnet build
)

REM --- Запуск Backend ---
echo 🔧 Запуск Backend (ASP.NET Core)...
start "VotingApp Backend" dotnet "%BACKEND_DIR%\VotingApp.dll" --urls "http://localhost:5000"
echo   ✅ Backend запущен на http://localhost:5000

REM --- Запуск Frontend ---
echo 🎨 Запуск Frontend (Python HTTP Server)...
start "VotingApp Frontend" python -m http.server 8080 --directory "%FRONTEND_DIR%"
echo   ✅ Frontend запущен на http://localhost:8080

echo.
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo   Все сервисы запущены!
echo.
echo   📚 Swagger UI:    http://localhost:5000/swagger/index.html
echo   🌐 Frontend:      http://localhost:8080/login.html
echo   📧 MailHog:       http://localhost:8025
echo   🔌 API:           http://localhost:5000/api
echo ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
echo.
echo   Чтобы остановить — закрой окна "VotingApp Backend" и "VotingApp Frontend".

pause
