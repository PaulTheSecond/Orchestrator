import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProcedureTemplateService } from '../../services/procedure-template.service';
import { ProcedureTemplate, ProcedureStageTemplate } from '../../models/procedure-template.model';

@Component({
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule],
  selector: 'app-procedure-template-form',
  templateUrl: './procedure-template-form.component.html'
})
export class ProcedureTemplateFormComponent implements OnInit {
  templateForm!: FormGroup;
  jsonMode = false;
  jsonValue = '';
  editMode = false;
  templateId = '';
  loading = false;
  error = '';
  successMessage = '';
  stageTypes = ['Setup', 'Contests', 'Contracts', 'Reporting'];

  constructor(
    private fb: FormBuilder,
    private procedureTemplateService: ProcedureTemplateService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.initForm();

    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.editMode = true;
        this.templateId = id;
        this.loadTemplate(id);
      }
    });
  }

  initForm(): void {
    this.templateForm = this.fb.group({
      id: [''],
      name: ['', [Validators.required, Validators.maxLength(255)]],
      version: [1, [Validators.required, Validators.min(1)]],
      isPublished: [false],
      procedureStages: this.fb.array([])
    });
  }

  loadTemplate(id: string): void {
    this.loading = true;
    this.procedureTemplateService.getProcedureTemplate(id)
      .subscribe({
        next: (template) => {
          this.patchFormValues(template);
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to load template';
          this.loading = false;
        }
      });
  }

  patchFormValues(template: ProcedureTemplate): void {
    this.templateForm.patchValue({
      id: template.id,
      name: template.name,
      version: template.version,
      isPublished: template.isPublished
    });

    // Clear existing stages and add from template
    this.procedureStages.clear();
    template.procedureStages.forEach(stage => {
      this.addStage(stage);
    });

    // Update JSON value
    this.updateJsonValue();
  }

  get procedureStages(): FormArray {
    return this.templateForm.get('procedureStages') as FormArray;
  }

  addStage(stage?: ProcedureStageTemplate): void {
    const stageForm = this.fb.group({
      id: [stage?.id || ''],
      stageType: [stage?.stageType || '', Validators.required],
      order: [stage?.order || this.procedureStages.length + 1, Validators.required],
      previousStageId: [stage?.previousStageId || null],
      nextStageId: [stage?.nextStageId || null],
      defaultServiceName: [stage?.defaultServiceName || '']
    });

    this.procedureStages.push(stageForm);
    this.updateJsonValue();
  }

  removeStage(index: number): void {
    this.procedureStages.removeAt(index);
    
    // Update order for remaining stages
    this.procedureStages.controls.forEach((control, i) => {
      control.get('order')?.setValue(i + 1);
    });
    
    this.updateJsonValue();
  }

  moveStageUp(index: number): void {
    if (index <= 0) return;
    
    // Swap stages
    const stages = this.procedureStages.controls;
    const temp = stages[index];
    stages[index] = stages[index - 1];
    stages[index - 1] = temp;
    
    // Update order values
    stages.forEach((control, i) => {
      control.get('order')?.setValue(i + 1);
    });
    
    this.updateJsonValue();
  }

  moveStageDown(index: number): void {
    if (index >= this.procedureStages.length - 1) return;
    
    // Swap stages
    const stages = this.procedureStages.controls;
    const temp = stages[index];
    stages[index] = stages[index + 1];
    stages[index + 1] = temp;
    
    // Update order values
    stages.forEach((control, i) => {
      control.get('order')?.setValue(i + 1);
    });
    
    this.updateJsonValue();
  }

  toggleJsonMode(): void {
    this.jsonMode = !this.jsonMode;
    if (this.jsonMode) {
      this.updateJsonValue();
    } else {
      try {
        const template = JSON.parse(this.jsonValue);
        this.patchFormValues(template);
      } catch (error) {
        this.error = 'Invalid JSON format';
      }
    }
  }

  updateJsonValue(): void {
    this.jsonValue = JSON.stringify(this.templateForm.value, null, 2);
  }

  parseJsonValue(): void {
    try {
      const template = JSON.parse(this.jsonValue);
      this.patchFormValues(template);
      this.error = '';
    } catch (error) {
      this.error = 'Invalid JSON format';
    }
  }

  submitForm(): void {
    if (this.jsonMode) {
      this.parseJsonValue();
      if (this.error) {
        return;
      }
    }

    if (this.templateForm.invalid) {
      this.error = 'Please fill in all required fields correctly';
      return;
    }

    this.loading = true;
    const template: ProcedureTemplate = this.templateForm.value;

    // Check for required Contests stage
    const hasContestsStage = template.procedureStages.some(s => s.stageType === 'Contests');
    if (!hasContestsStage) {
      this.error = 'Procedure template must include a Contests stage';
      this.loading = false;
      return;
    }

    // Submit the form
    const operation = this.editMode
      ? this.procedureTemplateService.updateProcedureTemplate(template)
      : this.procedureTemplateService.createProcedureTemplate(template);

    operation.subscribe({
      next: (result) => {
        this.loading = false;
        this.successMessage = `Procedure template ${this.editMode ? 'updated' : 'created'} successfully!`;
        
        if (!this.editMode) {
          // Clear form and prepare for new entry
          setTimeout(() => {
            this.router.navigate(['/procedure-templates']);
          }, 1500);
        }
      },
      error: (err) => {
        this.error = err.error?.error || `Failed to ${this.editMode ? 'update' : 'create'} template`;
        this.loading = false;
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/procedure-templates']);
  }
}
