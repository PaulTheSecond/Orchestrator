import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProcedureTemplate } from '../models/procedure-template.model';

@Injectable({
  providedIn: 'root'
})
export class TemplateExportService {
  private readonly baseUrl = '/api/TemplateExport';

  constructor(private http: HttpClient) { }

  /**
   * Экспортирует шаблон процедуры (скачивает как файл)
   * @param id Идентификатор шаблона процедуры
   */
  exportTemplate(id: string): void {
    // Создаем скрытую ссылку для скачивания файла
    const link = document.createElement('a');
    link.style.display = 'none';
    link.href = `${this.baseUrl}/${id}`;
    link.download = 'template-export.json';
    
    // Добавляем ссылку в DOM, кликаем по ней и удаляем
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  /**
   * Импортирует шаблон процедуры из файла
   * @param file Файл с данными шаблона
   * @returns Observable с импортированным шаблоном
   */
  importTemplate(file: File): Observable<ProcedureTemplate> {
    const formData = new FormData();
    formData.append('file', file);
    
    return this.http.post<ProcedureTemplate>(this.baseUrl, formData);
  }
}