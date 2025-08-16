@echo off
echo Starting SQL Server container...
docker-compose up -d sqlserver
echo.
echo SQL Server is starting with the following connection string:
echo Server=localhost;Database=FlowerSellingDB;User Id=sa;Password=123abc@;Trusted_Connection=False;MultipleActiveResultSets=True;Encrypt=False
echo.
echo Wait a moment for SQL Server to fully initialize before connecting.
echo To stop the container, run: docker-compose down
pause
