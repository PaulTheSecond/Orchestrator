import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { FormsModule } from '@angular/forms';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ContestInstanceService } from '../../services/contest-instance.service';
import { ContestInstance, InterruptContest } from '../../models/contest-instance.model';

@Component({
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  selector: 'app-contest-instance-list',
  templateUrl: './contest-instance-list.component.html'
})
export class ContestInstanceListComponent implements OnInit {
  contestInstances: ContestInstance[] = [];
  selectedInstance: ContestInstance | null = null;
  loading = false;
  error = '';
  successMessage = '';
  contestId: string | null = null;
  interruptReason: string = '';
  showInterruptModal = false;

  constructor(
    private contestInstanceService: ContestInstanceService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.contestId = params.get('id');
      if (this.contestId) {
        this.loadContestInstance(this.contestId);
      } else {
        this.loadContestInstances();
      }
    });
  }

  loadContestInstances(): void {
    this.loading = true;
    this.contestInstanceService.getContestInstances()
      .subscribe({
        next: (instances) => {
          this.contestInstances = instances;
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to load contest instances';
          this.loading = false;
        }
      });
  }

  loadContestInstance(id: string): void {
    this.loading = true;
    this.contestInstanceService.getContestInstance(id)
      .subscribe({
        next: (instance) => {
          this.selectedInstance = instance;
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to load contest instance';
          this.loading = false;
        }
      });
  }

  createContestInstance(): void {
    this.router.navigate(['/contest-instances/new']);
  }

  viewInstance(id: string): void {
    this.router.navigate([`/contest-instances/${id}`]);
  }

  viewProcedureInstance(id: string): void {
    this.router.navigate([`/procedure-instances/${id}`]);
  }

  viewApplications(contestId: string): void {
    this.router.navigate([`/applications/${contestId}`]);
  }

  createApplication(contestId: string): void {
    this.router.navigate(['/applications/new'], { 
      queryParams: { contestId: contestId } 
    });
  }

  openInterruptModal(id: string): void {
    this.contestId = id;
    this.interruptReason = '';
    this.showInterruptModal = true;
  }

  closeInterruptModal(): void {
    this.showInterruptModal = false;
  }

  interruptContest(): void {
    if (!this.contestId || !this.interruptReason) {
      this.error = 'Contest ID and reason are required to interrupt a contest';
      return;
    }

    this.loading = true;
    const interruptData: InterruptContest = {
      contestInstanceId: this.contestId,
      reason: this.interruptReason
    };

    this.contestInstanceService.interruptContest(interruptData)
      .subscribe({
        next: (result) => {
          this.successMessage = 'Contest instance interrupted successfully!';
          this.showInterruptModal = false;
          
          if (this.selectedInstance) {
            this.selectedInstance = result;
          } else {
            this.loadContestInstances();
          }
          
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to interrupt contest instance';
          this.loading = false;
        }
      });
  }

  updateTemplateVersion(id: string, newVersion: number): void {
    this.loading = true;
    this.contestInstanceService.updateTemplateVersion(id, newVersion)
      .subscribe({
        next: (result) => {
          this.successMessage = 'Contest template version updated successfully!';
          
          if (this.selectedInstance) {
            this.selectedInstance = result;
          } else {
            this.loadContestInstances();
          }
          
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to update contest template version';
          this.loading = false;
        }
      });
  }

  goBack(): void {
    this.router.navigate(['/contest-instances']);
  }

  dismissAlert(): void {
    this.error = '';
    this.successMessage = '';
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'notstarted':
        return 'status-notstarted';
      case 'inprogress':
        return 'status-inprogress';
      case 'completed':
        return 'status-completed';
      case 'interrupted':
        return 'status-interrupted';
      default:
        return '';
    }
  }
}
