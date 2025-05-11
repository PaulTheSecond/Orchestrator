import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TemplateExportService } from '../../services/template-export.service';

@Component({
  selector: 'app-template-export',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button class="btn btn-sm btn-outline-info" 
            (click)="exportTemplate()"
            title="Export Template">
      <i class="bi bi-download"></i> Export
    </button>
  `,
  styles: []
})
export class TemplateExportComponent {
  @Input() templateId!: string;
  
  constructor(private exportService: TemplateExportService) { }
  
  exportTemplate(): void {
    if (!this.templateId) {
      console.error('No template ID provided');
      return;
    }
    
    this.exportService.exportTemplate(this.templateId);
  }
}