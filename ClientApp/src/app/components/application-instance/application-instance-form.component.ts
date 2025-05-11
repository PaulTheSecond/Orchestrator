import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApplicationInstanceService } from '../../services/application-instance.service';
import { ContestInstanceService } from '../../services/contest-instance.service';
import { ContestInstance } from '../../models/contest-instance.model';
import { CreateApplication } from '../../models/application-instance.model';

@Component({
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule],
  selector: 'app-application-instance-form',
  templateUrl: './application-instance-form.component.html'
})
export class ApplicationInstanceFormComponent implements OnInit {
  applicationForm!: FormGroup;
  contestInstances: ContestInstance[] = [];
  loading = false;
  error = '';
  successMessage = '';
  contestId: string | null = null;

  constructor(
    private fb: FormBuilder,
    private applicationInstanceService: ApplicationInstanceService,
    private contestInstanceService: ContestInstanceService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.initForm();
    
    // Check if contestId is provided in query params
    this.route.queryParams.subscribe(params => {
      this.contestId = params['contestId'];
      if (this.contestId) {
        this.applicationForm.patchValue({ contestInstanceId: this.contestId });
      } else {
        this.loadContestInstances();
      }
    });
  }

  initForm(): void {
    this.applicationForm = this.fb.group({
      contestInstanceId: ['', Validators.required],
      externalApplicationId: [''],
      applicationData: ['', Validators.required]
    });
  }

  loadContestInstances(): void {
    this.loading = true;
    this.contestInstanceService.getContestInstances()
      .subscribe({
        next: (instances) => {
          // Filter out completed or interrupted contests
          this.contestInstances = instances.filter(i => 
            i.status !== 'Completed' && i.status !== 'Interrupted');
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to load contest instances';
          this.loading = false;
        }
      });
  }

  submitForm(): void {
    if (this.applicationForm.invalid) {
      this.error = 'Please fill in all required fields correctly';
      return;
    }

    this.loading = true;
    const application: CreateApplication = this.applicationForm.value;

    this.applicationInstanceService.createApplicationInstance(application)
      .subscribe({
        next: (result) => {
          this.loading = false;
          this.successMessage = 'Application created successfully!';
          setTimeout(() => {
            this.router.navigate([`/applications/${application.contestInstanceId}`]);
          }, 1500);
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to create application';
          this.loading = false;
        }
      });
  }

  isValidJson(): boolean {
    try {
      const data = this.applicationForm.get('applicationData')?.value;
      if (!data) return true;
      JSON.parse(data);
      return true;
    } catch (e) {
      return false;
    }
  }

  cancel(): void {
    if (this.contestId) {
      this.router.navigate([`/applications/${this.contestId}`]);
    } else {
      this.router.navigate(['/contest-instances']);
    }
  }
}
