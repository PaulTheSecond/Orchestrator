import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProcedureTemplate } from '../models/procedure-template.model';

@Injectable({
  providedIn: 'root'
})
export class ProcedureTemplateService {
  private apiUrl = '/api/ProcedureTemplate';

  constructor(private http: HttpClient) { }

  getProcedureTemplates(): Observable<ProcedureTemplate[]> {
    return this.http.get<ProcedureTemplate[]>(this.apiUrl);
  }

  getProcedureTemplate(id: string): Observable<ProcedureTemplate> {
    return this.http.get<ProcedureTemplate>(`${this.apiUrl}/${id}`);
  }

  createProcedureTemplate(template: ProcedureTemplate): Observable<ProcedureTemplate> {
    return this.http.post<ProcedureTemplate>(this.apiUrl, template);
  }

  updateProcedureTemplate(template: ProcedureTemplate): Observable<ProcedureTemplate> {
    return this.http.put<ProcedureTemplate>(`${this.apiUrl}/${template.id}`, template);
  }

  publishProcedureTemplate(id: string): Observable<ProcedureTemplate> {
    return this.http.post<ProcedureTemplate>(`${this.apiUrl}/${id}/publish`, {});
  }

  deleteProcedureTemplate(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
