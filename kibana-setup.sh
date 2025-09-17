#!/bin/bash

KIBANA_URL="http://kibana:5601"
MAX_RETRIES=30
RETRY_INTERVAL=10

echo "🔄 Waiting for Kibana to be ready..."

# Wait for Kibana to be available
for i in $(seq 1 $MAX_RETRIES); do
    if curl -s "$KIBANA_URL/api/status" | grep -q '"level":"available"'; then
        echo "✅ Kibana is ready!"
        break
    fi

    if [ $i -eq $MAX_RETRIES ]; then
        echo "❌ Timeout waiting for Kibana to be ready"
        exit 1
    fi

    echo "⏳ Kibana not ready yet, waiting... (attempt $i/$MAX_RETRIES)"
    sleep $RETRY_INTERVAL
done

# Wait a bit more for Kibana to fully initialize
sleep 5

echo "🔍 Checking for existing data view..."

# Check if data view already exists
DATA_VIEW_CHECK=$(curl -s "$KIBANA_URL/api/data_views" -H "kbn-xsrf: true" | grep -o '"title":"logs-dotnet-\*"' || echo "")

if [ -n "$DATA_VIEW_CHECK" ]; then
    echo "✅ Data view 'logs-dotnet-*' already exists, skipping creation"
else
    echo "📊 Creating data view 'logs-dotnet-*'..."

    # Create data view
    RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$KIBANA_URL/api/data_views/data_view" \
        -H "Content-Type: application/json" \
        -H "kbn-xsrf: true" \
        -d '{
            "data_view": {
                "title": "logs-dotnet-*",
                "timeFieldName": "@timestamp",
                "name": "Serilog Logs"
            }
        }')

    HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
    RESPONSE_BODY=$(echo "$RESPONSE" | head -n -1)

    if [ "$HTTP_CODE" = "200" ]; then
        echo "✅ Data view created successfully!"

        # Extract data view ID
        DATA_VIEW_ID=$(echo "$RESPONSE_BODY" | grep -o '"id":"[^"]*"' | head -1 | sed 's/"id":"\([^"]*\)"/\1/')

        if [ -n "$DATA_VIEW_ID" ]; then
            echo "🎯 Setting as default data view (ID: $DATA_VIEW_ID)..."

            # Set as default data view
            DEFAULT_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$KIBANA_URL/api/data_views/default" \
                -H "Content-Type: application/json" \
                -H "kbn-xsrf: true" \
                -d "{\"data_view_id\": \"$DATA_VIEW_ID\", \"force\": true}")

            DEFAULT_HTTP_CODE=$(echo "$DEFAULT_RESPONSE" | tail -n1)

            if [ "$DEFAULT_HTTP_CODE" = "200" ]; then
                echo "✅ Default data view set successfully!"
            else
                echo "⚠️  Warning: Failed to set as default data view (HTTP $DEFAULT_HTTP_CODE)"
            fi
        else
            echo "⚠️  Warning: Could not extract data view ID"
        fi
    else
        echo "❌ Failed to create data view (HTTP $HTTP_CODE)"
        echo "Response: $RESPONSE_BODY"
    fi
fi

# Set dark mode as default theme
echo "🌙 Setting dark mode as default theme..."

DARK_MODE_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$KIBANA_URL/api/saved_objects/config/8.13.4" \
    -H "Content-Type: application/json" \
    -H "kbn-xsrf: true" \
    -d '{"attributes":{"theme:darkMode":true}}')

DARK_MODE_HTTP_CODE=$(echo "$DARK_MODE_RESPONSE" | tail -n1)

if [ "$DARK_MODE_HTTP_CODE" = "200" ] || [ "$DARK_MODE_HTTP_CODE" = "409" ]; then
    # Try with PUT if POST fails due to existing config
    if [ "$DARK_MODE_HTTP_CODE" = "409" ]; then
        DARK_MODE_RESPONSE=$(curl -s -w "\n%{http_code}" -X PUT "$KIBANA_URL/api/saved_objects/config/8.13.4" \
            -H "Content-Type: application/json" \
            -H "kbn-xsrf: true" \
            -d '{"attributes":{"theme:darkMode":true}}')
        DARK_MODE_HTTP_CODE=$(echo "$DARK_MODE_RESPONSE" | tail -n1)
    fi

    if [ "$DARK_MODE_HTTP_CODE" = "200" ]; then
        echo "✅ Dark mode enabled successfully!"
    else
        echo "⚠️  Warning: Failed to enable dark mode (HTTP $DARK_MODE_HTTP_CODE)"
    fi
else
    echo "⚠️  Warning: Failed to enable dark mode (HTTP $DARK_MODE_HTTP_CODE)"
fi

echo "🎉 Kibana setup completed!"
echo "📍 Access Kibana at: http://localhost:5601"
echo "📊 Data view 'logs-dotnet-*' is ready for your Serilog logs"
echo "🌙 Dark mode has been enabled by default"