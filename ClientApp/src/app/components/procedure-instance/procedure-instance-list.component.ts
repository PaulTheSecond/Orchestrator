import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProcedureInstanceService } from '../../services/procedure-instance.service';
import { ProcedureInstance } from '../../models/procedure-instance.model';

@Component({
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  selector: 'app-procedure-instance-list',
  templateUrl: './procedure-instance-list.component.html'
})
export class ProcedureInstanceListComponent implements OnInit {
  procedureInstances: ProcedureInstance[] = [];
  selectedInstance: ProcedureInstance | null = null;
  loading = false;
  error = '';
  successMessage = '';
  procedureId: string | null = null;

  constructor(
    private procedureInstanceService: ProcedureInstanceService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.procedureId = params.get('id');
      if (this.procedureId) {
        this.loadProcedureInstance(this.procedureId);
      } else {
        this.loadProcedureInstances();
      }
    });
  }

  loadProcedureInstances(): void {
    this.loading = true;
    this.procedureInstanceService.getProcedureInstances()
      .subscribe({
        next: (instances) => {
          this.procedureInstances = instances;
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to load procedure instances';
          this.loading = false;
        }
      });
  }

  loadProcedureInstance(id: string): void {
    this.loading = true;
    this.procedureInstanceService.getProcedureInstance(id)
      .subscribe({
        next: (instance) => {
          this.selectedInstance = instance;
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to load procedure instance';
          this.loading = false;
        }
      });
  }

  createInstance(): void {
    this.router.navigate(['/procedure-instances/new']);
  }

  viewInstance(id: string): void {
    this.router.navigate([`/procedure-instances/${id}`]);
  }

  transitionToNextStage(id: string): void {
    this.loading = true;
    this.procedureInstanceService.transitionToNextStage(id)
      .subscribe({
        next: (updatedInstance) => {
          this.successMessage = 'Procedure instance transitioned to next stage successfully!';
          if (this.procedureId) {
            this.selectedInstance = updatedInstance;
          } else {
            this.loadProcedureInstances();
          }
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to transition to next stage';
          this.loading = false;
        }
      });
  }

  createContestInstance(procedureId: string): void {
    this.router.navigate(['/contest-instances/new'], { 
      queryParams: { procedureId: procedureId } 
    });
  }

  viewContestInstance(id: string): void {
    this.router.navigate([`/contest-instances/${id}`]);
  }

  viewContestApplications(contestId: string): void {
    this.router.navigate([`/applications/${contestId}`]);
  }

  goBack(): void {
    this.router.navigate(['/procedure-instances']);
  }

  dismissAlert(): void {
    this.error = '';
    this.successMessage = '';
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'active':
        return 'status-active';
      case 'completed':
        return 'status-completed';
      case 'paused':
        return 'status-interrupted';
      default:
        return '';
    }
  }

  getCurrentStageName(instance: ProcedureInstance): string {
    if (!instance.currentStageId) {
      return instance.status === 'Completed' ? 'Completed' : 'Not Started';
    }
    
    // Find the stage in the template - this would be better if we had the stage name in the response
    return `Stage ${instance.currentStageId.substring(0, 8)}...`;
  }
}
