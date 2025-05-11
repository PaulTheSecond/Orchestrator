#!/bin/bash

# Найдем все файлы компонентов TypeScript
FILES=$(find ClientApp/src/app -name "*.component.ts")

# Для каждого файла
for file in $FILES; do
  echo "Converting $file to standalone component..."
  
  # Проверим, есть ли уже standalone: true
  if grep -q "standalone: true" "$file"; then
    echo "Already standalone, skipping"
    continue
  fi
  
  # Добавим import CommonModule
  sed -i '1s/^/import { CommonModule } from "@angular\/common";\n/' "$file"
  
  # Найдем строку с @Component и добавим параметры standalone: true и imports: [CommonModule]
  sed -i 's/@Component({/@Component({\n  standalone: true,\n  imports: [CommonModule],/' "$file"
  
  echo "Done with $file"
done

echo "Conversion complete!"