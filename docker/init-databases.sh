#!/bin/bash
set -e

# This script creates multiple databases in PostgreSQL
# Used by docker-compose to initialize both application and Camunda databases

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- Create Camunda database if it doesn't exist
    SELECT 'CREATE DATABASE camunda'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'camunda')\gexec
    
    -- Grant privileges
    GRANT ALL PRIVILEGES ON DATABASE camunda TO postgres;
    GRANT ALL PRIVILEGES ON DATABASE "BpmnWorkflow" TO postgres;
EOSQL

echo "Databases created successfully!"
