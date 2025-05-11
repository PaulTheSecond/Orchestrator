import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ContestTemplateService } from '../../services/contest-template.service';
import { ContestTemplate } from '../../models/contest-template.model';

@Component({
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  selector: 'app-contest-template-list',
  templateUrl: './contest-template-list.component.html'
})
export class ContestTemplateListComponent implements OnInit {
  templates: ContestTemplate[] = [];
  loading = false;
  error = '';
  successMessage = '';

  constructor(
    private contestTemplateService: ContestTemplateService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadTemplates();
  }

  loadTemplates(): void {
    this.loading = true;
    this.contestTemplateService.getContestTemplates()
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
    this.router.navigate(['/contest-templates/new']);
  }

  editTemplate(id: string): void {
    this.router.navigate([`/contest-templates/${id}/edit`]);
  }

  publishTemplate(id: string): void {
    this.loading = true;
    this.contestTemplateService.publishContestTemplate(id)
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
      this.contestTemplateService.deleteContestTemplate(id)
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
}
