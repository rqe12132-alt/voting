#!/bin/bash

# Скрипт запуска VotingApp Backend + Frontend

echo "🚀 Запуск VotingApp..."

# 1. Проверяем Podman и MSSQL
echo "📦 Проверка MSSQL Server..."
if ! podman ps | grep -q sqlserver; then
    echo "  Запуск MSSQL..."
    podman start sqlserver || podman run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Pass1" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
    sleep 15
fi
echo "  ✅ MSSQL запущен"

# 2. Запуск Backend
echo "🔧 Запуск Backend (ASP.NET Core)..."
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"
cd "$HOME/code/diplom/voting-backend/VotingApp/bin/Debug/net8.0"
nohup "$HOME/.dotnet/dotnet" VotingApp.dll --urls "http://localhost:5000" > /tmp/votingapp.log 2>&1 &
echo "  ✅ Backend запущен на http://localhost:5000"

# 3. Запуск Frontend
echo "🎨 Запуск Frontend (Python HTTP Server)..."
cd "$HOME/code/diplom/voting-frontend"
nohup python3 -m http.server 8080 > /tmp/frontend.log 2>&1 &
echo "  ✅ Frontend запущен на http://localhost:8080"

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  🎉 Все сервисы запущены!"
echo ""
echo "  📚 Swagger UI:    http://localhost:5000/swagger/index.html"
echo "  🌐 Frontend:      http://localhost:8080/login.html"
echo "  🔌 API:           http://localhost:5000/api"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
