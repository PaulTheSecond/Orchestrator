
#!/bin/bash
echo "Building Angular application..."

# Перейти в директорию ClientApp
cd ClientApp

# Установить зависимости если нужно
npm install

# Очистить директорию wwwroot перед сборкой
echo "Cleaning wwwroot directory..."
rm -rf ../wwwroot/*

# Собрать приложение с путём к wwwroot
echo "Building Angular application..."
ng build --configuration production --output-path=../wwwroot

# Копирование app.js в корень wwwroot для обратной совместимости
echo "Creating compatibility JavaScript file..."
cat > ../wwwroot/app.js << 'EOF'
/**
 * Compatibility script - redirecting to Angular app
 * This file exists only for backward compatibility and will be removed in future versions
 */
console.warn('app.js is deprecated - Angular application is now handling all functionality');

// Автоматическое перенаправление старых запросов в Angular приложение
document.addEventListener('DOMContentLoaded', function() {
  console.log('Legacy app.js loaded, but all functionality is now in Angular');
});
EOF

echo "Angular build completed."
