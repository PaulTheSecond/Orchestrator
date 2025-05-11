import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApplicationInstanceService } from '../../services/application-instance.service';
import { ContestInstanceService } from '../../services/contest-instance.service';
import { ApplicationInstance } from '../../models/application-instance.model';
import { ContestInstance } from '../../models/contest-instance.model';

@Component({
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  selector: 'app-application-instance-list',
  templateUrl: './application-instance-list.component.html'
})
export class ApplicationInstanceListComponent implements OnInit {
  applications: ApplicationInstance[] = [];
  contestInstance: ContestInstance | null = null;
  loading = false;
  error = '';
  successMessage = '';
  contestId: string = '';
  
  // For result viewing
  selectedApplication: ApplicationInstance | null = null;
  selectedResult: any = null;

  constructor(
    private applicationInstanceService: ApplicationInstanceService,
    private contestInstanceService: ContestInstanceService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('contestId');
      if (id) {
        this.contestId = id;
        this.loadApplications(id);
        this.loadContestInstance(id);
      } else {
        this.error = 'No contest ID provided';
      }
    });
  }

  loadApplications(contestId: string): void {
    this.loading = true;
    this.applicationInstanceService.getApplicationsByContest(contestId)
      .subscribe({
        next: (applications) => {
          this.applications = applications;
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to load applications';
          this.loading = false;
        }
      });
  }

  loadContestInstance(contestId: string): void {
    this.contestInstanceService.getContestInstance(contestId)
      .subscribe({
        next: (contest) => {
          this.contestInstance = contest;
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to load contest details';
        }
      });
  }

  createApplication(): void {
    this.router.navigate(['/applications/new'], { 
      queryParams: { contestId: this.contestId } 
    });
  }

  viewContestInstance(): void {
    this.router.navigate([`/contest-instances/${this.contestId}`]);
  }

  dismissAlert(): void {
    this.error = '';
    this.successMessage = '';
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'submitted':
        return 'status-submitted';
      case 'validated':
      case 'approved':
        return 'status-approved';
      case 'rejected':
      case 'failed':
        return 'status-rejected';
      case 'interrupted':
        return 'status-interrupted';
      default:
        return '';
    }
  }

  getResultStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'success':
        return 'bg-success';
      case 'failed':
        return 'bg-warning';
      case 'rejected':
        return 'bg-danger';
      default:
        return 'bg-secondary';
    }
  }

  viewApplication(application: ApplicationInstance): void {
    this.selectedApplication = application;
  }

  closeApplicationView(): void {
    this.selectedApplication = null;
    this.selectedResult = null;
  }

  viewResultDetails(result: any): void {
    this.selectedResult = result;
  }

  closeResultView(): void {
    this.selectedResult = null;
  }

  formatResultData(data: string | null): string {
    if (!data) return 'No data';
    
    try {
      // Try to parse and pretty print
      const json = JSON.parse(data);
      return JSON.stringify(json, null, 2);
    } catch {
      // If not JSON, return as is
      return data;
    }
  }
}
