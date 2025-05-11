import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApplicationInstance, CreateApplication } from '../models/application-instance.model';

@Injectable({
  providedIn: 'root'
})
export class ApplicationInstanceService {
  private apiUrl = '/api/ApplicationInstance';

  constructor(private http: HttpClient) { }

  getApplicationsByContest(contestInstanceId: string): Observable<ApplicationInstance[]> {
    return this.http.get<ApplicationInstance[]>(`${this.apiUrl}/byContest/${contestInstanceId}`);
  }

  getApplicationInstance(id: string): Observable<ApplicationInstance> {
    return this.http.get<ApplicationInstance>(`${this.apiUrl}/${id}`);
  }

  createApplicationInstance(application: CreateApplication): Observable<ApplicationInstance> {
    return this.http.post<ApplicationInstance>(this.apiUrl, application);
  }
}
