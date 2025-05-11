import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import { ProcedureTemplateService } from '../../services/procedure-template.service';
import { TemplateExportService } from '../../services/template-export.service';
import { ProcedureTemplate } from '../../models/procedure-template.model';

@Component({
  selector: 'app-procedure-template-list',
  templateUrl: './procedure-template-list.component.html',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule]
})
export class ProcedureTemplateListComponent implements OnInit {
  templates: ProcedureTemplate[] = [];
  loading = false;
  error = '';
  successMessage = '';

  constructor(
    private procedureTemplateService: ProcedureTemplateService,
    private templateExportService: TemplateExportService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadTemplates();
  }

  loadTemplates(): void {
    this.loading = true;
    this.procedureTemplateService.getProcedureTemplates()
      .subscribe({
        next: (templates) => {
          this.templates = templates;
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to load templates';
          this.loading = false;
        }
      });
  }

  createTemplate(): void {
    this.router.navigate(['/procedure-templates/new']);
  }

  editTemplate(id: string): void {
    this.router.navigate([`/procedure-templates/${id}/edit`]);
  }

  publishTemplate(id: string): void {
    this.loading = true;
    this.procedureTemplateService.publishProcedureTemplate(id)
      .subscribe({
        next: () => {
          this.successMessage = 'Template published successfully!';
          this.loadTemplates();
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to publish template';
          this.loading = false;
        }
      });
  }

  deleteTemplate(id: string): void {
    if (confirm('Are you sure you want to delete this template?')) {
      this.loading = true;
      this.procedureTemplateService.deleteProcedureTemplate(id)
        .subscribe({
          next: () => {
            this.successMessage = 'Template deleted successfully!';
            this.loadTemplates();
          },
          error: (err) => {
            this.error = err.error?.error || 'Failed to delete template';
            this.loading = false;
          }
        });
    }
  }

  dismissAlert(): void {
    this.error = '';
    this.successMessage = '';
  }

  onTemplateImported(template: ProcedureTemplate): void {
    this.successMessage = `Template "${template.name}" imported successfully!`;
    this.loadTemplates();
  }

  @ViewChild('fileInput') fileInput!: ElementRef;

  exportTemplate(id: string): void {
    this.templateExportService.exportTemplate(id);
  }

  triggerFileInput(): void {
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
    this.templateExportService.importTemplate(file).subscribe({
      next: (template) => {
        console.log('Template imported successfully:', template);
        this.onTemplateImported(template);
      },
      error: (error) => {
        console.error('Error importing template', error);
        this.error = 'Error importing template. Please check the file format.';
      }
    });
    
    // Очищаем выбранный файл
    event.target.value = '';
  }
}
