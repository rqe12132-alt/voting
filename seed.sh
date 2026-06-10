#!/bin/bash

# Seed script: creates random votes on a specific poll
# Usage: ./seed.sh <POLL_ID> [NUM_USERS]

API="http://localhost:5000/api"
POLL_ID="${1:-}"
NUM_USERS="${2:-15}"

if [ -z "$POLL_ID" ]; then
    echo "Usage: $0 <POLL_ID> [NUM_USERS]"
    echo "Example: $0 7df0e9e7-0a6f-492a-403f-08dec52ce2bb 20"
    exit 1
fi

echo "================================"
echo "  Seed: Random votes generator"
echo "================================"
echo "Poll ID: $POLL_ID"
echo "Users to create: $NUM_USERS"
echo ""

# 1. Register a master user to get a token
MASTER_EMAIL="seedmaster@demo.local"
MASTER_RESP=$(curl -s -X POST "$API/auth/register" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"$MASTER_EMAIL\",\"password\":\"SeedPass123!\",\"fullName\":\"Seed Master\"}")

MASTER_TOKEN=$(echo "$MASTER_RESP" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)

# If registration failed, try login
if [ -z "$MASTER_TOKEN" ]; then
    MASTER_RESP=$(curl -s -X POST "$API/auth/login" \
        -H "Content-Type: application/json" \
        -d "{\"email\":\"$MASTER_EMAIL\",\"password\":\"SeedPass123!\"}")
    MASTER_TOKEN=$(echo "$MASTER_RESP" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
fi

if [ -z "$MASTER_TOKEN" ]; then
    echo "ERROR: Could not register or login master user. Backend running?"
    exit 1
fi

echo "Master user OK"

# 2. Get poll options with token
POLL_DATA=$(curl -s "$API/polls/$POLL_ID" \
    -H "Authorization: Bearer $MASTER_TOKEN")

OPTIONS=$(echo "$POLL_DATA" | python3 -c "
import sys, json
try:
    data = json.load(sys.stdin)
    for opt in data.get('options', []):
        print(f\"{opt['id']}|{opt['text']}\")
except Exception as e:
    pass
")

if [ -z "$OPTIONS" ]; then
    echo "ERROR: Could not fetch poll options. Is the poll ID correct?"
    echo "Response: $POLL_DATA"
    exit 1
fi

# Parse options into arrays
OPTION_IDS=()
OPTION_NAMES=()
while IFS='|' read -r id name; do
    OPTION_IDS+=("$id")
    OPTION_NAMES+=("$name")
done <<< "$OPTIONS"

NUM_OPTIONS=${#OPTION_IDS[@]}
echo "Options found: $NUM_OPTIONS"
for i in "${!OPTION_IDS[@]}"; do
    echo "  [$i] ${OPTION_NAMES[$i]}"
done
echo ""

# 3. Create users and vote
VOTES=()
for ((i=0; i<NUM_OPTIONS; i++)); do
    VOTES+=(0)
done

echo "Generating votes..."
for i in $(seq 1 $NUM_USERS); do
    EMAIL="seeduser$i@demo.local"
    
    # Register user
    USER_RESP=$(curl -s -X POST "$API/auth/register" \
        -H "Content-Type: application/json" \
        -d "{\"email\":\"$EMAIL\",\"password\":\"SeedPass123!\",\"fullName\":\"Seed User $i\"}")
    
    TOKEN=$(echo "$USER_RESP" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
    
    if [ -z "$TOKEN" ]; then
        echo "  [$i/$NUM_USERS] Failed to register $EMAIL"
        continue
    fi
    
    # Pick random option
    RAND_IDX=$((RANDOM % NUM_OPTIONS))
    OPTION_ID="${OPTION_IDS[$RAND_IDX]}"
    OPTION_NAME="${OPTION_NAMES[$RAND_IDX]}"
    
    # Vote
    VOTE_RESP=$(curl -s -X POST "$API/polls/$POLL_ID/vote" \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $TOKEN" \
        -d "{\"optionIds\":[\"$OPTION_ID\"]}")
    
    VOTES[$RAND_IDX]=$((VOTES[$RAND_IDX] + 1))
    echo "  [$i/$NUM_USERS] $EMAIL -> voted for [$OPTION_NAME]"
done

echo ""
echo "================================"
echo "  Results summary"
echo "================================"
TOTAL=0
for i in "${!OPTION_IDS[@]}"; do
    COUNT=${VOTES[$i]}
    TOTAL=$((TOTAL + COUNT))
    echo "  ${OPTION_NAMES[$i]}: $COUNT votes"
done
echo ""
echo "Total votes: $TOTAL"
echo ""
echo "Open http://localhost:8080/poll.html?id=$POLL_ID to see results"
