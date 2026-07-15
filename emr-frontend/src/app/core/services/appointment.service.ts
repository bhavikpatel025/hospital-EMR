import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AppointmentListDto, AppointmentDetailDto, AppointmentCreateDto,
  AppointmentUpdateDto, AppointmentStatusUpdateDto, AppointmentQueryParams, CalendarEventDto, AppointmentRescheduleDto 
} from '../models/appointment.model';
import { PagedResult } from '../models/patient.model';

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/appointments`;

  getAll(params: AppointmentQueryParams): Observable<PagedResult<AppointmentListDto>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber)
      .set('pageSize', params.pageSize)
      .set('sortBy', params.sortBy || 'AppointmentDate')
      .set('sortDescending', params.sortDescending ?? false);

    if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
    if (params.doctorId) httpParams = httpParams.set('doctorId', params.doctorId);
    if (params.status !== undefined && params.status !== null) httpParams = httpParams.set('status', params.status);
    if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
    if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);

    return this.http.get<PagedResult<AppointmentListDto>>(this.baseUrl, { params: httpParams });
  }

  getById(id: number): Observable<AppointmentDetailDto> {
    return this.http.get<AppointmentDetailDto>(`${this.baseUrl}/${id}`);
  }

  getToday(): Observable<AppointmentListDto[]> {
    return this.http.get<AppointmentListDto[]>(`${this.baseUrl}/today`);
  }

  getUpcoming(): Observable<AppointmentListDto[]> {
    return this.http.get<AppointmentListDto[]>(`${this.baseUrl}/upcoming`);
  }

  create(dto: AppointmentCreateDto): Observable<AppointmentDetailDto> {
    return this.http.post<AppointmentDetailDto>(this.baseUrl, dto);
  }

  update(id: number, dto: AppointmentUpdateDto): Observable<any> {
    return this.http.put(`${this.baseUrl}/${id}`, dto);
  }

  updateStatus(id: number, dto: AppointmentStatusUpdateDto): Observable<any> {
    return this.http.patch(`${this.baseUrl}/${id}/status`, dto);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }

  getCalendarEvents(from: string, to: string, doctorId?: number): Observable<CalendarEventDto[]> {
  let params = new HttpParams().set('from', from).set('to', to);
  if (doctorId) params = params.set('doctorId', doctorId);
  return this.http.get<CalendarEventDto[]>(`${this.baseUrl}/calendar`, { params });
}

reschedule(id: number, dto: AppointmentRescheduleDto): Observable<any> {
  return this.http.patch(`${this.baseUrl}/${id}/reschedule`, dto);
}
}