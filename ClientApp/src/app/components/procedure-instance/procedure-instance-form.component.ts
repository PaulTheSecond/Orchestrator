import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ProcedureInstanceService } from '../../services/procedure-instance.service';
import { ProcedureTemplateService } from '../../services/procedure-template.service';
import { ProcedureTemplate } from '../../models/procedure-template.model';
import { CreateProcedureInstance } from '../../models/procedure-instance.model';

@Component({
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule],
  selector: 'app-procedure-instance-form',
  templateUrl: './procedure-instance-form.component.html'
})
export class ProcedureInstanceFormComponent implements OnInit {
  instanceForm!: FormGroup;
  procedureTemplates: ProcedureTemplate[] = [];
  loading = false;
  error = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private procedureInstanceService: ProcedureInstanceService,
    private procedureTemplateService: ProcedureTemplateService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.initForm();
    this.loadProcedureTemplates();
  }

  initForm(): void {
    this.instanceForm = this.fb.group({
      procedureTemplateId: ['', Validators.required],
      name: ['', [Validators.required, Validators.maxLength(255)]]
    });
  }

  loadProcedureTemplates(): void {
    this.loading = true;
    this.procedureTemplateService.getProcedureTemplates()
      .subscribe({
        next: (templates) => {
          this.procedureTemplates = templates.filter(t => t.isPublished);
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to load procedure templates';
          this.loading = false;
        }
      });
  }

  submitForm(): void {
    if (this.instanceForm.invalid) {
      this.error = 'Please fill in all required fields correctly';
      return;
    }

    this.loading = true;
    const instance: CreateProcedureInstance = this.instanceForm.value;

    this.procedureInstanceService.createProcedureInstance(instance)
      .subscribe({
        next: (result) => {
          this.loading = false;
          this.successMessage = 'Procedure instance created successfully!';
          setTimeout(() => {
            this.router.navigate(['/procedure-instances']);
          }, 1500);
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to create procedure instance';
          this.loading = false;
        }
      });
  }

  cancel(): void {
    this.router.navigate(['/procedure-instances']);
  }
}
