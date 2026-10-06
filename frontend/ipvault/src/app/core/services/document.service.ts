import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response.model';
import { PatentDocument } from '../../shared/models/document.model';

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private readonly baseUrl = `${environment.apiUrl}`;

  constructor(private http: HttpClient) {}

  getDocuments(patentId: number): Observable<ApiResponse<PatentDocument[]>> {
    return this.http.get<ApiResponse<PatentDocument[]>>(`${this.baseUrl}/patents/${patentId}/documents`);
  }

  uploadDocument(patentId: number, file: File, documentType: string): Observable<ApiResponse<PatentDocument>> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('documentType', documentType);

    return this.http.post<ApiResponse<PatentDocument>>(`${this.baseUrl}/patents/${patentId}/documents`, formData);
  }

  deleteDocument(documentId: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/documents/${documentId}`);
  }

  downloadDocument(documentId: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/documents/${documentId}/download`, {
      responseType: 'blob'
    });
  }
}
