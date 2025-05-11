import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ContestTemplate } from '../models/contest-template.model';

@Injectable({
  providedIn: 'root'
})
export class ContestTemplateService {
  private apiUrl = '/api/ContestTemplate';

  constructor(private http: HttpClient) { }

  getContestTemplates(): Observable<ContestTemplate[]> {
    return this.http.get<ContestTemplate[]>(this.apiUrl);
  }

  getContestTemplate(id: string): Observable<ContestTemplate> {
    return this.http.get<ContestTemplate>(`${this.apiUrl}/${id}`);
  }

  getContestTemplatesByProcedure(procedureTemplateId: string): Observable<ContestTemplate[]> {
    return this.http.get<ContestTemplate[]>(`${this.apiUrl}/byProcedure/${procedureTemplateId}`);
  }

  createContestTemplate(template: ContestTemplate): Observable<ContestTemplate> {
    return this.http.post<ContestTemplate>(this.apiUrl, template);
  }

  updateContestTemplate(template: ContestTemplate): Observable<ContestTemplate> {
    return this.http.put<ContestTemplate>(`${this.apiUrl}/${template.id}`, template);
  }

  publishContestTemplate(id: string): Observable<ContestTemplate> {
    return this.http.post<ContestTemplate>(`${this.apiUrl}/${id}/publish`, {});
  }

  deleteContestTemplate(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
