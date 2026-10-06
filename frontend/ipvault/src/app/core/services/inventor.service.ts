import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response.model';
import { CreateInventorRequest, Inventor, UpdateInventorRequest } from '../../shared/models/inventor.model';

@Injectable({
  providedIn: 'root'
})
export class InventorService {
  private readonly baseUrl = `${environment.apiUrl}/inventors`;

  constructor(private http: HttpClient) {}

  getInventors(search?: string): Observable<ApiResponse<Inventor[]>> {
    let params = new HttpParams();
    if (search && search.trim()) {
      params = params.set('search', search.trim());
    }
    return this.http.get<ApiResponse<Inventor[]>>(this.baseUrl, { params });
  }

  getInventorById(id: number): Observable<ApiResponse<Inventor>> {
    return this.http.get<ApiResponse<Inventor>>(`${this.baseUrl}/${id}`);
  }

  createInventor(request: CreateInventorRequest): Observable<ApiResponse<Inventor>> {
    return this.http.post<ApiResponse<Inventor>>(this.baseUrl, request);
  }

  updateInventor(id: number, request: UpdateInventorRequest): Observable<ApiResponse<Inventor>> {
    return this.http.put<ApiResponse<Inventor>>(`${this.baseUrl}/${id}`, request);
  }

  deleteInventor(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.baseUrl}/${id}`);
  }
}
