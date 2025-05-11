import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { AppComponent } from './app/app.component';
import { routes } from './app/app.module';
import { ProcedureTemplateService } from './app/services/procedure-template.service';
import { ContestTemplateService } from './app/services/contest-template.service';
import { ProcedureInstanceService } from './app/services/procedure-instance.service';
import { ContestInstanceService } from './app/services/contest-instance.service';
import { ApplicationInstanceService } from './app/services/application-instance.service';
import { ApiService } from './app/services/api.service';

bootstrapApplication(AppComponent, {
  providers: [
    provideHttpClient(),
    provideRouter(routes),
    provideAnimations(),
    // Services
    ProcedureTemplateService,
    ContestTemplateService,
    ProcedureInstanceService,
    ContestInstanceService,
    ApplicationInstanceService,
    ApiService
  ]
}).catch(err => console.error(err));
