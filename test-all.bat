@echo off
REM ConfigR Test Helper Script for Windows

if "%1"=="" (
    echo.
    echo ConfigR Testing Helper
    echo.
    echo Usage: test-all.bat [command]
    echo.
    echo Commands:
    echo   up               - Start all Docker containers
    echo   down             - Stop and remove all Docker containers
    echo   logs             - Show logs from all containers
    echo   test             - Run tests (requires containers running^)
    echo   test-sql         - Run SQL Server tests only
    echo   test-mysql       - Run MySQL tests only
    echo   test-postgres    - Run PostgreSQL tests only
    echo   test-mongo       - Run MongoDB tests only
    echo   test-redis       - Run Redis tests only
    echo   clean            - Stop containers and remove all data
    echo   help             - Show this help message
    echo.
    goto :eof
)

if "%1"=="up" (
    echo Starting all services...
    docker-compose up -d
    echo.
    echo Waiting for services to be healthy...
    timeout /t 30 /nobreak
    echo Services are ready!
    goto :eof
)

if "%1"=="down" (
    echo Stopping all services...
    docker-compose down
    echo Services stopped!
    goto :eof
)

if "%1"=="logs" (
    docker-compose logs -f
    goto :eof
)

if "%1"=="test" (
    if not exist ".env" (
        echo Creating .env file from .env.example...
        copy .env.example .env
    )
    echo Running all tests...
    dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj --configuration Debug --verbosity normal
    goto :eof
)

if "%1"=="test-sql" (
    echo Running SQL Server tests...
    dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj -k "SqlServer" --configuration Debug --verbosity normal
    goto :eof
)

if "%1"=="test-mysql" (
    echo Running MySQL tests...
    dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj -k "MySql" --configuration Debug --verbosity normal
    goto :eof
)

if "%1"=="test-postgres" (
    echo Running PostgreSQL tests...
    dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj -k "Npgsql" --configuration Debug --verbosity normal
    goto :eof
)

if "%1"=="test-mongo" (
    echo Running MongoDB tests...
    dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj -k "Mongo" --configuration Debug --verbosity normal
    goto :eof
)

if "%1"=="test-redis" (
    echo Running Redis tests...
    dotnet test ./tests/ConfigR.Tests/ConfigR.Tests.csproj -k "Redis" --configuration Debug --verbosity normal
    goto :eof
)

if "%1"=="clean" (
    echo Stopping and removing all containers...
    docker-compose down -v
    echo Cleaned up!
    goto :eof
)

if "%1"=="help" (
    call :help
    goto :eof
)

echo Unknown command: %1
call :help

:help
echo.
echo ConfigR Testing Helper
echo.
echo Usage: test-all.bat [command]
echo.
echo Commands:
echo   up               - Start all Docker containers
echo   down             - Stop and remove all Docker containers
echo   logs             - Show logs from all containers
echo   test             - Run tests (requires containers running^)
echo   test-sql         - Run SQL Server tests only
echo   test-mysql       - Run MySQL tests only
echo   test-postgres    - Run PostgreSQL tests only
echo   test-mongo       - Run MongoDB tests only
echo   test-redis       - Run Redis tests only
echo   clean            - Stop containers and remove all data
echo   help             - Show this help message
echo.
