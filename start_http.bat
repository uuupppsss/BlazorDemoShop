@echo off
cd publish\api && start dotnet ApiDemoShop.dll --urls=http://localhost:5000
cd ../blazor && start dotnet BlazorDemoShop.dll --urls=http://localhost:8080
start http://localhost:8080