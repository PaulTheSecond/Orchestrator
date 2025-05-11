import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ContestTemplateService } from '../../services/contest-template.service';
import { ProcedureTemplateService } from '../../services/procedure-template.service';
import { ContestTemplate, StageTemplate } from '../../models/contest-template.model';
import { ProcedureTemplate } from '../../models/procedure-template.model';

@Component({
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule],
  selector: 'app-contest-template-form',
  templateUrl: './contest-template-form.component.html'
})
export class ContestTemplateFormComponent implements OnInit {
  templateForm!: FormGroup;
  jsonMode = false;
  jsonValue = '';
  editMode = false;
  templateId = '';
  loading = false;
  error = '';
  successMessage = '';
  stageTypes = ['ApplicationSubmission', 'Validation', 'Expertise', 'FundAllocation', 'WinnerAnnouncement'];
  procedureTemplates: ProcedureTemplate[] = [];

  constructor(
    private fb: FormBuilder,
    private contestTemplateService: ContestTemplateService,
    private procedureTemplateService: ProcedureTemplateService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.initForm();
    this.loadProcedureTemplates();

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
      procedureTemplateId: ['', Validators.required],
      name: ['', [Validators.required, Validators.maxLength(255)]],
      version: [1, [Validators.required, Validators.min(1)]],
      isPublished: [false],
      statusModel: [['NotStarted', 'InProgress', 'Completed', 'Interrupted'], Validators.required],
      stages: this.fb.array([])
    });
  }

  loadProcedureTemplates(): void {
    this.procedureTemplateService.getProcedureTemplates()
      .subscribe({
        next: (templates) => {
          this.procedureTemplates = templates.filter(t => t.isPublished);
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to load procedure templates';
        }
      });
  }

  loadTemplate(id: string): void {
    this.loading = true;
    this.contestTemplateService.getContestTemplate(id)
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

  patchFormValues(template: ContestTemplate): void {
    this.templateForm.patchValue({
      id: template.id,
      procedureTemplateId: template.procedureTemplateId,
      name: template.name,
      version: template.version,
      isPublished: template.isPublished,
      statusModel: template.statusModel
    });

    // Clear existing stages and add from template
    this.stages.clear();
    template.stages.forEach(stage => {
      this.addStage(stage);
    });

    // Update JSON value
    this.updateJsonValue();
  }

  get stages(): FormArray {
    return this.templateForm.get('stages') as FormArray;
  }

  addStage(stage?: StageTemplate): void {
    const stageForm = this.fb.group({
      id: [stage?.id || ''],
      stageType: [stage?.stageType || '', Validators.required],
      order: [stage?.order || this.stages.length + 1, Validators.required],
      previousStageId: [stage?.previousStageId || null],
      nextStageId: [stage?.nextStageId || null],
      defaultServiceName: [stage?.defaultServiceName || '', Validators.required]
    });

    this.stages.push(stageForm);
    this.updateJsonValue();
  }

  removeStage(index: number): void {
    this.stages.removeAt(index);
    
    // Update order for remaining stages
    this.stages.controls.forEach((control, i) => {
      control.get('order')?.setValue(i + 1);
    });
    
    this.updateJsonValue();
  }

  moveStageUp(index: number): void {
    if (index <= 0) return;
    
    // Swap stages
    const stagesArray = this.stages.controls;
    const temp = stagesArray[index];
    stagesArray[index] = stagesArray[index - 1];
    stagesArray[index - 1] = temp;
    
    // Update order values
    stagesArray.forEach((control, i) => {
      control.get('order')?.setValue(i + 1);
    });
    
    this.updateJsonValue();
  }

  moveStageDown(index: number): void {
    if (index >= this.stages.length - 1) return;
    
    // Swap stages
    const stagesArray = this.stages.controls;
    const temp = stagesArray[index];
    stagesArray[index] = stagesArray[index + 1];
    stagesArray[index + 1] = temp;
    
    // Update order values
    stagesArray.forEach((control, i) => {
      control.get('order')?.setValue(i + 1);
    });
    
    this.updateJsonValue();
  }

  addStatus(): void {
    const statusModel = this.templateForm.get('statusModel')?.value || [];
    statusModel.push('');
    this.templateForm.get('statusModel')?.setValue(statusModel);
    this.updateJsonValue();
  }

  removeStatus(index: number): void {
    const statusModel = this.templateForm.get('statusModel')?.value || [];
    statusModel.splice(index, 1);
    this.templateForm.get('statusModel')?.setValue(statusModel);
    this.updateJsonValue();
  }

  updateStatus(index: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    const statusModel = this.templateForm.get('statusModel')?.value || [];
    statusModel[index] = input.value;
    this.templateForm.get('statusModel')?.setValue(statusModel);
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
    const template: ContestTemplate = this.templateForm.value;

    // Submit the form
    const operation = this.editMode
      ? this.contestTemplateService.updateContestTemplate(template)
      : this.contestTemplateService.createContestTemplate(template);

    operation.subscribe({
      next: (result) => {
        this.loading = false;
        this.successMessage = `Contest template ${this.editMode ? 'updated' : 'created'} successfully!`;
        
        if (!this.editMode) {
          // Clear form and prepare for new entry
          setTimeout(() => {
            this.router.navigate(['/contest-templates']);
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
    this.router.navigate(['/contest-templates']);
  }
}
