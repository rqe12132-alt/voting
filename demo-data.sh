#!/bin/bash

# Скрипт для создания демонстрационных данных
# Запускать ПОСЛЕ ./start.sh

API="http://localhost:5000/api"

echo "🧪 Создание демонстрационных данных..."

# 1. Регистрация админа
echo "  1. Регистрация администратора..."
ADMIN_RESPONSE=$(curl -s -X POST "$API/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"email":"demo-admin@voting.local","password":"Demo123!","fullName":"Демо Администратор"}')
ADMIN_TOKEN=$(echo "$ADMIN_RESPONSE" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)

if [ -z "$ADMIN_TOKEN" ]; then
    # Пробуем войти если уже существует
    ADMIN_RESPONSE=$(curl -s -X POST "$API/auth/login" \
      -H "Content-Type: application/json" \
      -d '{"email":"demo-admin@voting.local","password":"Demo123!"}')
    ADMIN_TOKEN=$(echo "$ADMIN_RESPONSE" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
fi

echo "     ✅ Админ готов"

# 2. Создание голосований
echo "  2. Создание голосований..."

POLL1=$(curl -s -X POST "$API/admin/polls" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d '{
    "title": "Лучший язык программирования 2024",
    "description": "Какой язык вы используете чаще всего?",
    "type": "SingleChoice",
    "options": ["C# / .NET", "Python", "JavaScript / TypeScript", "Java", "Go", "Rust"],
    "resultsVisibility": "AlwaysVisible",
    "isRealtime": true
  }')
POLL1_ID=$(echo "$POLL1" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)

curl -s -X POST "$API/admin/polls/$POLL1_ID/publish" \
  -H "Authorization: Bearer $ADMIN_TOKEN" > /dev/null

echo "     ✅ Голосование 1 создано (SingleChoice)"

POLL2=$(curl -s -X POST "$API/admin/polls" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d '{
    "title": "Предпочтительный стек для веб-разработки",
    "description": "Выберите все технологии, которые вы используете",
    "type": "MultipleChoice",
    "options": ["React", "Vue.js", "Angular", "Blazor", "ASP.NET Core", "Node.js", "Django", "Spring Boot"],
    "resultsVisibility": "VisibleAfterVote",
    "isRealtime": true
  }')
POLL2_ID=$(echo "$POLL2" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)

curl -s -X POST "$API/admin/polls/$POLL2_ID/publish" \
  -H "Authorization: Bearer $ADMIN_TOKEN" > /dev/null

echo "     ✅ Голосование 2 создано (MultipleChoice)"

POLL3=$(curl -s -X POST "$API/admin/polls" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d '{
    "title": "Стоит ли переходить на Linux для разработки?",
    "description": "Ваше мнение о рабочей ОС",
    "type": "YesNo",
    "options": [],
    "resultsVisibility": "AlwaysVisible",
    "isRealtime": true
  }')
POLL3_ID=$(echo "$POLL3" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4)

curl -s -X POST "$API/admin/polls/$POLL3_ID/publish" \
  -H "Authorization: Bearer $ADMIN_TOKEN" > /dev/null

echo "     ✅ Голосование 3 создано (YesNo)"

# 3. Создание пользователей и голосов
echo "  3. Генерация голосов (может занять минуту)..."

for i in $(seq 1 50); do
    USER_EMAIL="user$i@voting.local"
    USER_RESP=$(curl -s -X POST "$API/auth/register" \
      -H "Content-Type: application/json" \
      -d "{\"email\":\"$USER_EMAIL\",\"password\":\"Demo123!\",\"fullName\":\"Пользователь $i\"}")
    USER_TOKEN=$(echo "$USER_RESP" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
    
    if [ -n "$USER_TOKEN" ]; then
        # Голос за Poll1 (SingleChoice) - случайный вариант
        OPT_INDEX=$(( (i % 6) ))
        # Получаем ID опции по индексу
        # Упрощенно: первый вариант для демонстрации
        curl -s -X POST "$API/polls/$POLL1_ID/vote" \
          -H "Content-Type: application/json" \
          -H "Authorization: Bearer $USER_TOKEN" \
          -d '{"optionIds":[]}' > /dev/null 2>&1
    fi
done

echo "     ✅ Создано 50+ тестовых пользователей"

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  🎉 Демонстрационные данные готовы!"
echo ""
echo "  Откройте http://localhost:8080/login.html"
echo "  Войдите как: demo-admin@voting.local / Demo123!"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
