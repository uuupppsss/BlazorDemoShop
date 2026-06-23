@echo off
cd publish\api && start dotnet ApiDemoShop.dll --urls=https://localhost:7299
cd ../blazor && start dotnet BlazorDemoShop.dll --urls=https://localhost:7163
start https://localhost:7163