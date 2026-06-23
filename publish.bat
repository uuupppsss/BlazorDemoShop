@echo off
echo ========================================
echo    ПУБЛИКАЦИЯ ПРОЕКТОВ
echo ========================================
echo.

:: Удаляем старую папку publish
if exist "%~dp0publish" (
    echo Удаление старой папки publish...
    rmdir /s /q "%~dp0publish"
)

:: Публикуем API
echo [1/2] Публикация ApiDemoShop...
cd /d "%~dp0ApiDemoShop"
dotnet publish -c Release -o "%~dp0publish\api"
if errorlevel 1 (
    echo Ошибка при публикации API!
    pause
    exit /b
)

:: Публикуем Blazor
echo [2/2] Публикация BlazorDemoShop...
cd /d "%~dp0BlazorDemoShop"
dotnet publish -c Release -o "%~dp0publish\blazor"
if errorlevel 1 (
    echo Ошибка при публикации Blazor!
    pause
    exit /b
)

:: Копируем картинки (если нужно)
echo Копирование картинок...
if exist "%~dp0ApiDemoShop\wwwroot\uploads" (
    xcopy "%~dp0ApiDemoShop\wwwroot\uploads" "%~dp0publish\api\wwwroot\uploads\" /E /I /Y
    echo Картинки скопированы
)

echo.
echo ========================================
echo    ✓ ПУБЛИКАЦИЯ ЗАВЕРШЕНА!
echo    API:    publish\api
echo    Blazor: publish\blazor
echo ========================================
echo.

pause