import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProcedureInstance, CreateProcedureInstance } from '../models/procedure-instance.model';

@Injectable({
  providedIn: 'root'
})
export class ProcedureInstanceService {
  private apiUrl = '/api/ProcedureInstance';

  constructor(private http: HttpClient) { }

  getProcedureInstances(): Observable<ProcedureInstance[]> {
    return this.http.get<ProcedureInstance[]>(this.apiUrl);
  }

  getProcedureInstance(id: string): Observable<ProcedureInstance> {
    return this.http.get<ProcedureInstance>(`${this.apiUrl}/${id}`);
  }

  createProcedureInstance(instance: CreateProcedureInstance): Observable<ProcedureInstance> {
    return this.http.post<ProcedureInstance>(this.apiUrl, instance);
  }

  transitionToNextStage(id: string): Observable<ProcedureInstance> {
    return this.http.post<ProcedureInstance>(`${this.apiUrl}/${id}/transition`, {});
  }
}
