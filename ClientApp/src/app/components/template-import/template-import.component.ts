import { Component, EventEmitter, Output, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TemplateExportService } from '../../services/template-export.service';
import { ProcedureTemplate } from '../../models/procedure-template.model';

@Component({
  selector: 'app-template-import',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button class="btn btn-outline-primary" 
            (click)="triggerFileInput()"
            title="Import Template">
      <i class="bi bi-upload"></i> Import
    </button>
    <input 
      type="file" 
      #fileInput
      style="display: none"
      (change)="onFileSelected($event)"
      accept=".json">
  `,
  styles: []
})
export class TemplateImportComponent {
  @Output() imported = new EventEmitter<ProcedureTemplate>();
  
  @ViewChild('fileInput') fileInput!: ElementRef;
  
  constructor(
    private exportService: TemplateExportService
  ) { }
  
  triggerFileInput(): void {
    // Получаем ссылку на скрытый input и симулируем клик через ViewChild
    if (this.fileInput) {
      this.fileInput.nativeElement.click();
    }
  }
  
  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (!file) {
      return;
    }
    
    // Проверяем тип файла
    if (file.type !== 'application/json' && !file.name.endsWith('.json')) {
      alert('Please select a JSON file');
      return;
    }
    
    // Импортируем шаблон
    this.exportService.importTemplate(file).subscribe({
      next: (template) => {
        console.log('Template imported successfully:', template);
        this.imported.emit(template);
      },
      error: (error) => {
        console.error('Error importing template', error);
        alert('Error importing template. Please check the file format.');
      }
    });
    
    // Очищаем выбранный файл
    event.target.value = '';
  }
}