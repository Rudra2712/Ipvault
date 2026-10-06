import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PagedResult } from '../../shared/models/api-response.model';
import {
  CreatePatentRequest,
  PatentDetail,
  PatentFilter,
  PatentInventor,
  PatentListItem,
  PatentStatus,
  UpdatePatentRequest
} from '../../shared/models/patent.model';

@Injectable({
  providedIn: 'root'
})
export class PatentService {
  private readonly baseUrl = `${environment.apiUrl}/patents`;
  private readonly statusUrl = `${environment.apiUrl}/patentstatuses`;

  constructor(private http: HttpClient) {}

  getPatents(filter: PatentFilter): Observable<ApiResponse<PagedResult<PatentListItem>>> {
    let params = new HttpParams();
    if (filter.search && filter.search.trim()) {
      params = params.set('search', filter.search.trim());
    }
    if (filter.status && filter.status.trim()) {
      params = params.set('status', filter.status.trim());
    }
    if (filter.statusId && filter.statusId > 0) {
      params = params.set('statusId', filter.statusId.toString());
    }
    if (filter.clientId && filter.clientId > 0) {
      params = params.set('clientId', filter.clientId.toString());
    }
    if (filter.year && filter.year > 0) {
      params = params.set('year', filter.year.toString());
    }
    if (filter.page) {
      params = params.set('page', filter.page.toString());
    }
    if (filter.pageSize) {
      params = params.set('pageSize', filter.pageSize.toString());
    }

    return this.http.get<ApiResponse<PagedResult<PatentListItem>>>(this.baseUrl, { params });
  }

  getPatentById(id: number): Observable<ApiResponse<PatentDetail>> {
    return this.http.get<ApiResponse<PatentDetail>>(`${this.baseUrl}/${id}`);
  }

  createPatent(request: CreatePatentRequest): Observable<ApiResponse<PatentDetail>> {
    return this.http.post<ApiResponse<PatentDetail>>(this.baseUrl, request);
  }

  updatePatent(id: number, request: UpdatePatentRequest): Observable<ApiResponse<PatentDetail>> {
    return this.http.put<ApiResponse<PatentDetail>>(`${this.baseUrl}/${id}`, request);
  }

  deletePatent(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }

  // Inventors
  getPatentInventors(patentId: number): Observable<ApiResponse<PatentInventor[]>> {
    return this.http.get<ApiResponse<PatentInventor[]>>(`${this.baseUrl}/${patentId}/inventors`);
  }

  assignInventor(patentId: number, inventorId: number, isPrimary: boolean): Observable<ApiResponse<PatentInventor>> {
    return this.http.post<ApiResponse<PatentInventor>>(`${this.baseUrl}/${patentId}/inventors`, {
      inventorId,
      isPrimaryInventor: isPrimary
    });
  }

  removeInventor(patentId: number, inventorId: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${patentId}/inventors/${inventorId}`);
  }

  // Statuses
  getStatuses(): Observable<ApiResponse<PatentStatus[]>> {
    return this.http.get<ApiResponse<PatentStatus[]>>(this.statusUrl);
  }
}
