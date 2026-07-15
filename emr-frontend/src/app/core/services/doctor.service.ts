import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  DoctorListDto, DoctorDetailDto, DoctorCreateDto,
  DoctorUpdateDto, DoctorQueryParams
} from '../models/doctor.model';
import { PagedResult } from '../models/patient.model';

@Injectable({ providedIn: 'root' })
export class DoctorService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/doctors`;

  getAll(params: DoctorQueryParams): Observable<PagedResult<DoctorListDto>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber)
      .set('pageSize', params.pageSize)
      .set('sortBy', params.sortBy || 'FullName')
      .set('sortDescending', params.sortDescending ?? false);

    if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
    if (params.specialization) httpParams = httpParams.set('specialization', params.specialization);

    return this.http.get<PagedResult<DoctorListDto>>(this.baseUrl, { params: httpParams });
  }

  getActiveDoctors(): Observable<DoctorListDto[]> {
    return this.http.get<DoctorListDto[]>(`${this.baseUrl}/active`);
  }

  getById(id: number): Observable<DoctorDetailDto> {
    return this.http.get<DoctorDetailDto>(`${this.baseUrl}/${id}`);
  }

  create(dto: DoctorCreateDto): Observable<DoctorDetailDto> {
    return this.http.post<DoctorDetailDto>(this.baseUrl, dto);
  }

  update(id: number, dto: DoctorUpdateDto): Observable<any> {
    return this.http.put(`${this.baseUrl}/${id}`, dto);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }
}