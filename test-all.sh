#!/bin/bash

# ConfigR Test Helper Script for Linux/macOS

if [ -z "$1" ] || [ "$1" = "help" ]; then
    cat << EOF

ConfigR Testing Helper

Usage: ./test-all.sh [command]

Commands:
  up               - Start all Docker containers
  down             - Stop and remove all Docker containers
  logs             - Show logs from all containers
  test             - Run tests (requires containers running)
  test-sql         - Run SQL Server tests only
  test-mysql       - Run MySQL tests only
  test-postgres    - Run PostgreSQL tests only
  test-mongo       - Run MongoDB tests only
  test-redis       - Run Redis tests only
  test-raven       - Run RavenDB tests only
  clean            - Stop containers and remove all data
  help             - Show this help message

EOF
    exit 0
fi

case "$1" in
    up)
        echo "Starting all services..."
        docker-compose up -d
        echo ""
        echo "Waiting for services to be healthy..."
        sleep 30
        echo "Services are ready!"
        ;;
    
    down)
        echo "Stopping all services..."
        docker-compose down
        echo "Services stopped!"
        ;;
    
    logs)
        docker-compose logs -f
        ;;
    
    test)
        if [ ! -f ".env" ]; then
            echo "Creating .env file from .env.example..."
            cp .env.example .env
        fi
        echo "Running all tests..."
        dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj --configuration Debug --verbosity normal
        ;;
    
    test-sql)
        echo "Running SQL Server tests..."
        dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj --filter "ClassName~SqlServer" --configuration Debug --verbosity normal
        ;;
    
    test-mysql)
        echo "Running MySQL tests..."
        dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj --filter "ClassName~MySql" --configuration Debug --verbosity normal
        ;;
    
    test-postgres)
        echo "Running PostgreSQL tests..."
        dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj --filter "ClassName~Npgsql" --configuration Debug --verbosity normal
        ;;
    
    test-mongo)
        echo "Running MongoDB tests..."
        dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj --filter "ClassName~Mongo" --configuration Debug --verbosity normal
        ;;
    
    test-redis)
        echo "Running Redis tests..."
        dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj --filter "ClassName~Redis" --configuration Debug --verbosity normal
        ;;
    
    test-raven)
        echo "Running RavenDB tests..."
        dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj --filter "ClassName~Raven" --configuration Debug --verbosity normal
        ;;
    
    clean)
        echo "Stopping and removing all containers..."
        docker-compose down -v
        echo "Cleaned up!"
        ;;
    
    *)
        echo "Unknown command: $1"
        echo ""
        ./test-all.sh help
        exit 1
        ;;
esac
