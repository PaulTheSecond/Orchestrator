import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ContestInstanceService } from '../../services/contest-instance.service';
import { ContestTemplateService } from '../../services/contest-template.service';
import { ProcedureInstanceService } from '../../services/procedure-instance.service';
import { ContestTemplate } from '../../models/contest-template.model';
import { ProcedureInstance } from '../../models/procedure-instance.model';
import { CreateContestInstance } from '../../models/contest-instance.model';
import { StageConfiguration } from '../../models/stage-configuration.model';

@Component({
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule],
  selector: 'app-contest-instance-form',
  templateUrl: './contest-instance-form.component.html'
})
export class ContestInstanceFormComponent implements OnInit {
  instanceForm!: FormGroup;
  procedureInstances: ProcedureInstance[] = [];
  contestTemplates: ContestTemplate[] = [];
  selectedTemplate: ContestTemplate | null = null;
  loading = false;
  error = '';
  successMessage = '';
  procedureId: string | null = null;

  constructor(
    private fb: FormBuilder,
    private contestInstanceService: ContestInstanceService,
    private contestTemplateService: ContestTemplateService,
    private procedureInstanceService: ProcedureInstanceService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.initForm();
    
    // Check if procedureId is provided in query params
    this.route.queryParams.subscribe(params => {
      this.procedureId = params['procedureId'];
      if (this.procedureId) {
        this.instanceForm.patchValue({ procedureInstanceId: this.procedureId });
        this.loadContestTemplates();
      } else {
        this.loadProcedureInstances();
      }
    });
  }

  initForm(): void {
    this.instanceForm = this.fb.group({
      procedureInstanceId: ['', Validators.required],
      contestTemplateId: ['', Validators.required],
      name: ['', [Validators.required, Validators.maxLength(255)]],
      stageConfigurations: this.fb.array([])
    });

    // Watch for changes in the contestTemplateId field
    this.instanceForm.get('contestTemplateId')?.valueChanges.subscribe(templateId => {
      if (templateId) {
        this.loadTemplateDetails(templateId);
      }
    });
  }

  loadProcedureInstances(): void {
    this.loading = true;
    this.procedureInstanceService.getProcedureInstances()
      .subscribe({
        next: (instances) => {
          this.procedureInstances = instances.filter(i => i.status === 'Active');
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to load procedure instances';
          this.loading = false;
        }
      });
  }

  loadContestTemplates(): void {
    if (!this.instanceForm.get('procedureInstanceId')?.value) {
      return;
    }

    this.loading = true;
    this.contestTemplateService.getContestTemplates()
      .subscribe({
        next: (templates) => {
          this.contestTemplates = templates.filter(t => t.isPublished);
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to load contest templates';
          this.loading = false;
        }
      });
  }

  loadTemplateDetails(templateId: string): void {
    this.loading = true;
    this.contestTemplateService.getContestTemplate(templateId)
      .subscribe({
        next: (template) => {
          this.selectedTemplate = template;
          this.setupStageConfigurations(template);
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to load contest template details';
          this.loading = false;
        }
      });
  }

  setupStageConfigurations(template: ContestTemplate): void {
    // Clear existing configurations
    this.stageConfigurations.clear();

    // Add a configuration for each stage in the template
    if (template.stages) {
      template.stages.forEach(stage => {
        this.stageConfigurations.push(this.fb.group({
          stageTemplateId: [stage.id, Validators.required],
          startDate: [null],
          endDate: [null],
          serviceName: [stage.defaultServiceName, Validators.required]
        }));
      });
    }
  }

  get stageConfigurations(): FormArray {
    return this.instanceForm.get('stageConfigurations') as FormArray;
  }

  getStageTypeName(index: number): string {
    if (!this.selectedTemplate || !this.selectedTemplate.stages) {
      return '';
    }

    const stageTemplateId = this.stageConfigurations.at(index).get('stageTemplateId')?.value;
    const stage = this.selectedTemplate.stages.find(s => s.id === stageTemplateId);
    return stage ? stage.stageType : '';
  }

  onProcedureInstanceChange(): void {
    this.loadContestTemplates();
  }

  submitForm(): void {
    if (this.instanceForm.invalid) {
      this.error = 'Please fill in all required fields correctly';
      return;
    }

    this.loading = true;
    const instance: CreateContestInstance = this.instanceForm.value;

    this.contestInstanceService.createContestInstance(instance)
      .subscribe({
        next: (result) => {
          this.loading = false;
          this.successMessage = 'Contest instance created successfully!';
          setTimeout(() => {
            this.router.navigate([`/procedure-instances/${instance.procedureInstanceId}`]);
          }, 1500);
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to create contest instance';
          this.loading = false;
        }
      });
  }

  cancel(): void {
    if (this.procedureId) {
      this.router.navigate([`/procedure-instances/${this.procedureId}`]);
    } else {
      this.router.navigate(['/contest-instances']);
    }
  }
}
