import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ContestInstance, CreateContestInstance, InterruptContest } from '../models/contest-instance.model';

@Injectable({
  providedIn: 'root'
})
export class ContestInstanceService {
  private apiUrl = '/api/ContestInstance';

  constructor(private http: HttpClient) { }

  getContestInstances(): Observable<ContestInstance[]> {
    return this.http.get<ContestInstance[]>(this.apiUrl);
  }

  getContestInstance(id: string): Observable<ContestInstance> {
    return this.http.get<ContestInstance>(`${this.apiUrl}/${id}`);
  }

  getContestInstancesByProcedure(procedureInstanceId: string): Observable<ContestInstance[]> {
    return this.http.get<ContestInstance[]>(`${this.apiUrl}/byProcedure/${procedureInstanceId}`);
  }

  createContestInstance(instance: CreateContestInstance): Observable<ContestInstance> {
    return this.http.post<ContestInstance>(this.apiUrl, instance);
  }

  interruptContest(interrupt: InterruptContest): Observable<ContestInstance> {
    return this.http.post<ContestInstance>(`${this.apiUrl}/${interrupt.contestInstanceId}/interrupt`, interrupt);
  }

  updateTemplateVersion(id: string, newVersion: number): Observable<ContestInstance> {
    return this.http.post<ContestInstance>(`${this.apiUrl}/${id}/updateTemplate`, newVersion);
  }
}
