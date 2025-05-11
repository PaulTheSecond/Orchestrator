import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: true,
  imports: [RouterModule, CommonModule]
})
export class AppComponent {
  title = 'Subsidy Procedure Orchestrator';
  navItems = [
    { path: '/procedure-templates', label: 'Шаблоны процедур' },
    { path: '/contest-templates', label: 'Шаблоны конкурсов' },
    { path: '/procedure-instances', label: 'Активные процедуры' },
    { path: '/contest-instances', label: 'Активные конкурсы' }
  ];
}
