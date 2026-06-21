#!/bin/bash

# Универсальный скрипт запуска VotingApp Backend + Frontend
# Работает из любой папки, в которой распакован проект

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$SCRIPT_DIR/voting-backend/VotingApp/bin/Debug/net8.0"
FRONTEND_DIR="$SCRIPT_DIR/voting-frontend"

echo "🚀 Запуск VotingApp из $SCRIPT_DIR..."

# --- Проверка dotnet ---
DOTNET_CMD=""
if command -v dotnet &>/dev/null; then
    DOTNET_CMD="dotnet"
elif [ -x "$HOME/.dotnet/dotnet" ]; then
    DOTNET_CMD="$HOME/.dotnet/dotnet"
    export DOTNET_ROOT="$HOME/.dotnet"
    export PATH="$DOTNET_ROOT:$PATH"
else
    echo "❌ dotnet не найден. Установите .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

# --- Проверка Python ---
PYTHON_CMD=""
if command -v python3 &>/dev/null; then
    PYTHON_CMD="python3"
elif command -v python &>/dev/null; then
    PYTHON_CMD="python"
else
    echo "❌ Python не найден. Установите Python 3."
    exit 1
fi

# --- Выбор контейнерной среды: Podman или Docker ---
CONTAINER=""
if command -v podman &>/dev/null; then
    CONTAINER="podman"
elif command -v docker &>/dev/null; then
    CONTAINER="docker"
else
    echo "❌ Не найден Podman или Docker. Установите одну из сред."
    exit 1
fi

# --- Запуск MSSQL Server ---
echo "📦 Проверка MSSQL Server..."
if ! $CONTAINER ps --format '{{.Names}}' 2>/dev/null | grep -qx "sqlserver"; then
    echo "  Запуск MSSQL..."
    $CONTAINER start sqlserver 2>/dev/null || \
        $CONTAINER run -e "ACCEPT_EULA=Y" \
                       -e "MSSQL_SA_PASSWORD=YourStrong@Pass1" \
                       -p 1433:1433 \
                       --name sqlserver \
                       -d mcr.microsoft.com/mssql/server:2022-latest
    echo "  ⏳ Ждём инициализации БД (15 сек)..."
    sleep 15
fi
echo "  ✅ MSSQL запущен"

# --- Запуск MailHog ---
echo "📧 Проверка MailHog (SMTP)..."
if ! $CONTAINER ps --format '{{.Names}}' 2>/dev/null | grep -qx "mailhog"; then
    echo "  Запуск MailHog..."
    $CONTAINER start mailhog 2>/dev/null || \
        $CONTAINER run -d --name mailhog -p 1025:1025 -p 8025:8025 docker.io/mailhog/mailhog:latest
    sleep 2
fi
echo "  ✅ MailHog запущен на http://localhost:8025"

# --- Запуск Backend ---
echo "🔧 Запуск Backend (ASP.NET Core)..."
if [ ! -f "$BACKEND_DIR/VotingApp.dll" ]; then
    echo "⚠️  Собранный backend не найден. Выполняется dotnet build..."
    cd "$SCRIPT_DIR/voting-backend/VotingApp"
    $DOTNET_CMD build
fi

cd "$BACKEND_DIR"
nohup "$DOTNET_CMD" VotingApp.dll --urls "http://localhost:5000" > /tmp/votingapp.log 2>&1 &
echo "  ✅ Backend запущен на http://localhost:5000 (PID $!)"

# --- Запуск Frontend ---
echo "🎨 Запуск Frontend (Python HTTP Server)..."
cd "$FRONTEND_DIR"
nohup "$PYTHON_CMD" -m http.server 8080 > /tmp/frontend.log 2>&1 &
echo "  ✅ Frontend запущен на http://localhost:8080 (PID $!)"

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  Все сервисы запущены!"
echo ""
echo "  📚 Swagger UI:    http://localhost:5000/swagger/index.html"
echo "  🌐 Frontend:      http://localhost:8080/login.html"
echo "  📧 MailHog:       http://localhost:8025"
echo "  🔌 API:           http://localhost:5000/api"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "  Чтобы остановить: kill \$(lsof -t -i:5000) \$(lsof -t -i:8080)"
