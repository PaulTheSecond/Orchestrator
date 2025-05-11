
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  // API Endpoints
  private API = {
    procedureTemplates: '/api/ProcedureTemplate',
    contestTemplates: '/api/ContestTemplate',
    procedureInstances: '/api/ProcedureInstance',
    contestInstances: '/api/ContestInstance',
    applicationInstances: '/api/ApplicationInstance',
  };

  constructor(private http: HttpClient) { }

  // Получение всех шаблонов процедур
  getProcedureTemplates(): Observable<any[]> {
    return this.http.get<any[]>(this.API.procedureTemplates);
  }

  // Получение шаблона процедуры по ID
  getProcedureTemplate(id: string): Observable<any> {
    return this.http.get<any>(`${this.API.procedureTemplates}/${id}`);
  }

  // Создание шаблона процедуры
  createProcedureTemplate(template: any): Observable<any> {
    return this.http.post<any>(this.API.procedureTemplates, template);
  }

  // Публикация шаблона процедуры
  publishProcedureTemplate(id: string): Observable<any> {
    return this.http.post<any>(`${this.API.procedureTemplates}/${id}/publish`, {});
  }

  // Получение всех шаблонов конкурсов
  getContestTemplates(): Observable<any[]> {
    return this.http.get<any[]>(this.API.contestTemplates);
  }

  // Получение шаблона конкурса по ID
  getContestTemplate(id: string): Observable<any> {
    return this.http.get<any>(`${this.API.contestTemplates}/${id}`);
  }

  // Создание шаблона конкурса
  createContestTemplate(template: any): Observable<any> {
    return this.http.post<any>(this.API.contestTemplates, template);
  }

  // Публикация шаблона конкурса
  publishContestTemplate(id: string): Observable<any> {
    return this.http.post<any>(`${this.API.contestTemplates}/${id}/publish`, {});
  }

  // Получение всех экземпляров процедур
  getProcedureInstances(): Observable<any[]> {
    return this.http.get<any[]>(this.API.procedureInstances);
  }

  // Получение всех экземпляров конкурсов
  getContestInstances(): Observable<any[]> {
    return this.http.get<any[]>(this.API.contestInstances);
  }

  // Получение всех заявок
  getApplicationInstances(): Observable<any[]> {
    return this.http.get<any[]>(this.API.applicationInstances);
  }
}
