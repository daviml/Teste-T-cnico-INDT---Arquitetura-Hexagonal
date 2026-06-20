#!/bin/bash
# ---------------------------------------------------------------------------
# Inicialização do PostgreSQL para o INDT Insurance.
#
# Executado AUTOMATICAMENTE pelo entrypoint oficial do Postgres na PRIMEIRA
# inicialização do volume de dados (diretório /docker-entrypoint-initdb.d).
#
# Cria um banco e um usuário DEDICADOS por microserviço, reforçando o
# isolamento de dados: cada serviço só enxerga (e só consegue conectar n)o
# próprio banco. O superusuário do container é usado apenas para administração.
# ---------------------------------------------------------------------------
set -euo pipefail

create_service_db() {
  local db_name="$1"
  local db_user="$2"
  local db_password="$3"

  echo "  -> criando usuário '${db_user}' e banco '${db_name}'"

  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE USER ${db_user} WITH PASSWORD '${db_password}';
    CREATE DATABASE ${db_name} OWNER ${db_user};

    -- Isolamento: ninguém além do dono conecta neste banco.
    REVOKE CONNECT ON DATABASE ${db_name} FROM PUBLIC;
    GRANT  CONNECT ON DATABASE ${db_name} TO ${db_user};
EOSQL
}

echo "Inicializando bancos por microserviço..."

create_service_db "$PROPOSTA_DB_NAME"    "$PROPOSTA_DB_USER"    "$PROPOSTA_DB_PASSWORD"
create_service_db "$CONTRATACAO_DB_NAME" "$CONTRATACAO_DB_USER" "$CONTRATACAO_DB_PASSWORD"

echo "Bancos inicializados com sucesso."
