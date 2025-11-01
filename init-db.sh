#!/bin/bash
set -e

# Start SQL Server in background
/opt/mssql/bin/sqlservr &

# Wait for SQL Server to be available
echo "Aguardando SQL Server..."
until /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "SuaSenhaForte123!" -Q "SELECT 1" > /dev/null 2>&1; do
  sleep 1
done

# If dump.sql exists, run it
if [ -f /tmp/dump.sql ]; then
  echo "Executando /tmp/dump.sql..."
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "SuaSenhaForte123!" -i /tmp/dump.sql
fi

# Bring sqlservr process to foreground to keep container alive
wait