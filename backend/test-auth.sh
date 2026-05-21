#!/bin/bash
# Script per testare l'autenticazione API

# Colori per output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# URL base API (modifica se necessario)
API_URL="http://localhost:5299"

echo "========================================="
echo "Test Autenticazione BotDashboard API"
echo "========================================="
echo ""

# Test 1: Endpoint di test senza autenticazione
echo -e "${YELLOW}Test 1: Endpoint di test (senza auth)${NC}"
echo "GET $API_URL/api/auth/test"
echo ""
curl -X GET "$API_URL/api/auth/test" \
  -H "Content-Type: application/json" \
  -w "\nStatus: %{http_code}\n\n" \
  -s | jq '.' 2>/dev/null || echo "Response received"
echo ""
echo "========================================="
echo ""

# Test 2: Login con credenziali admin
echo -e "${YELLOW}Test 2: Login con admin${NC}"
echo "POST $API_URL/api/auth/login"
echo "Body: {\"username\": \"admin\", \"password\": \"Admin@123456\"}"
echo ""

LOGIN_RESPONSE=$(curl -X POST "$API_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "Admin@123456"}' \
  -w "\n%{http_code}" \
  -s)

HTTP_STATUS=$(echo "$LOGIN_RESPONSE" | tail -n 1)
RESPONSE_BODY=$(echo "$LOGIN_RESPONSE" | sed '$d')

echo "$RESPONSE_BODY" | jq '.' 2>/dev/null || echo "$RESPONSE_BODY"
echo ""
echo "HTTP Status: $HTTP_STATUS"
echo ""

if [ "$HTTP_STATUS" = "200" ]; then
    echo -e "${GREEN}✓ Login riuscito!${NC}"
    
    # Estrai il token
    TOKEN=$(echo "$RESPONSE_BODY" | jq -r '.data.token // .token' 2>/dev/null)
    
    if [ -n "$TOKEN" ] && [ "$TOKEN" != "null" ]; then
        echo "Token JWT ricevuto (primi 50 caratteri): ${TOKEN:0:50}..."
        echo ""
        echo "========================================="
        echo ""
        
        # Test 3: Chiamata autenticata
        echo -e "${YELLOW}Test 3: GET /api/user (con autenticazione)${NC}"
        echo "Authorization: Bearer ${TOKEN:0:50}..."
        echo ""
        
        curl -X GET "$API_URL/api/user" \
          -H "Authorization: Bearer $TOKEN" \
          -H "Content-Type: application/json" \
          -w "\nStatus: %{http_code}\n\n" \
          -s | jq '.' 2>/dev/null || echo "Response received"
        
        echo ""
        echo "========================================="
        echo ""
        
        # Test 4: Verifica permessi admin
        echo -e "${YELLOW}Test 4: GET /api/user/available-permissions (admin only)${NC}"
        echo ""
        
        curl -X GET "$API_URL/api/user/available-permissions" \
          -H "Authorization: Bearer $TOKEN" \
          -H "Content-Type: application/json" \
          -w "\nStatus: %{http_code}\n\n" \
          -s | jq '.' 2>/dev/null || echo "Response received"
        
        echo ""
        echo "========================================="
        echo ""
        
        # Salva il token in un file per usi futuri
        echo "$TOKEN" > .token
        echo -e "${GREEN}Token salvato in .token per usi futuri${NC}"
        echo ""
        echo "Usa il token nelle prossime chiamate con:"
        echo "export TOKEN=\$(cat .token)"
        echo "curl -H \"Authorization: Bearer \$TOKEN\" $API_URL/api/user"
        
    else
        echo -e "${RED}✗ Token non trovato nella risposta${NC}"
    fi
else
    echo -e "${RED}✗ Login fallito con status $HTTP_STATUS${NC}"
    echo ""
    echo "Possibili cause:"
    echo "1. Server non in esecuzione su $API_URL"
    echo "2. Credenziali errate (username: admin, password: Admin@123456)"
    echo "3. Database non inizializzato"
    echo "4. Problema con middleware di autenticazione"
fi

echo ""
echo "========================================="
echo "Test completati"
echo "========================================="
