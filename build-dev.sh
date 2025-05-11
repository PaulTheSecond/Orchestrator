#!/bin/bash
echo "Building Angular application (development mode)..."

# Перейти в директорию ClientApp
cd ClientApp

# Сборка приложения с путём к wwwroot в режиме development (быстрее)
echo "Building Angular application in development mode..."
ng build --configuration development --output-path=../wwwroot

echo "Angular dev build completed."