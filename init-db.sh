#!/bin/bash
# Espera o SQL Server iniciar
echo "Aguardando SQL Server iniciar..."
sleep 15

# Executa o dump.sql
echo "Executando dump.sql..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -d master -i /docker-entrypoint-initdb.d/dump.sql

echo "Banco inicializado."