import { NgModule } from '@angular/core';
import { Routes } from '@angular/router';

// Procedure Template Components
import { ProcedureTemplateFormComponent } from './components/procedure-template/procedure-template-form.component';
import { ProcedureTemplateListComponent } from './components/procedure-template/procedure-template-list.component';

// Contest Template Components
import { ContestTemplateFormComponent } from './components/contest-template/contest-template-form.component';
import { ContestTemplateListComponent } from './components/contest-template/contest-template-list.component';

// Procedure Instance Components
import { ProcedureInstanceFormComponent } from './components/procedure-instance/procedure-instance-form.component';
import { ProcedureInstanceListComponent } from './components/procedure-instance/procedure-instance-list.component';

// Contest Instance Components
import { ContestInstanceFormComponent } from './components/contest-instance/contest-instance-form.component';
import { ContestInstanceListComponent } from './components/contest-instance/contest-instance-list.component';

// Application Instance Components
import { ApplicationInstanceFormComponent } from './components/application-instance/application-instance-form.component';
import { ApplicationInstanceListComponent } from './components/application-instance/application-instance-list.component';

export const routes: Routes = [
  { path: '', redirectTo: '/procedure-templates', pathMatch: 'full' },
  { path: 'procedure-templates', component: ProcedureTemplateListComponent },
  { path: 'procedure-templates/new', component: ProcedureTemplateFormComponent },
  { path: 'procedure-templates/:id/edit', component: ProcedureTemplateFormComponent },
  { path: 'contest-templates', component: ContestTemplateListComponent },
  { path: 'contest-templates/new', component: ContestTemplateFormComponent },
  { path: 'contest-templates/:id/edit', component: ContestTemplateFormComponent },
  { path: 'procedure-instances', component: ProcedureInstanceListComponent },
  { path: 'procedure-instances/new', component: ProcedureInstanceFormComponent },
  { path: 'procedure-instances/:id', component: ProcedureInstanceListComponent },
  { path: 'contest-instances', component: ContestInstanceListComponent },
  { path: 'contest-instances/new', component: ContestInstanceFormComponent },
  { path: 'contest-instances/:id', component: ContestInstanceListComponent },
  { path: 'applications/:contestId', component: ApplicationInstanceListComponent },
  { path: 'applications/new', component: ApplicationInstanceFormComponent }
];

// Для обратной совместимости
@NgModule({})
export class AppModule { }