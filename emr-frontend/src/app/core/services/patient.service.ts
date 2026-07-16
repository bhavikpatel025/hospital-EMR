import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  PatientListDto, PatientDetailDto, PatientCreateDto,
  PatientUpdateDto, PatientQueryParams, PagedResult, ExtractedMedicalDataDto
} from '../models/patient.model';

@Injectable({ providedIn: 'root' })
export class PatientService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/patients`;

  getAll(params: PatientQueryParams): Observable<PagedResult<PatientListDto>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber)
      .set('pageSize', params.pageSize)
      .set('sortBy', params.sortBy || 'FullName')
      .set('sortDescending', params.sortDescending ?? false);

    if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
    if (params.gender) httpParams = httpParams.set('gender', params.gender);

    return this.http.get<PagedResult<PatientListDto>>(this.baseUrl, { params: httpParams });
  }

  getById(id: number): Observable<PatientDetailDto> {
    return this.http.get<PatientDetailDto>(`${this.baseUrl}/${id}`);
  }

  create(dto: PatientCreateDto): Observable<PatientDetailDto> {
    return this.http.post<PatientDetailDto>(this.baseUrl, dto);
  }

  update(id: number, dto: PatientUpdateDto): Observable<any> {
    return this.http.put(`${this.baseUrl}/${id}`, dto);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }

  getPatientRecords(patientId: number): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/documents/${patientId}/records`);
  }

  getPatientClinicalSummary(patientId: number): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/documents/${patientId}/clinical-summary`);
  }

  uploadAndExtractDocument(patientId: number, category: string, file: File): Observable<ExtractedMedicalDataDto> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('category', category);
    return this.http.post<ExtractedMedicalDataDto>(`${environment.apiUrl}/documents/${patientId}/extract`, formData);
  }

  uploadAndExtractWithAI(patientId: number, category: string, file: File): Observable<ExtractedMedicalDataDto> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('category', category);
    return this.http.post<ExtractedMedicalDataDto>(`${environment.apiUrl}/ai-extraction/${patientId}/extract`, formData);
  }

  saveExtractedRecords(patientId: number, payload: any): Observable<any> {
    return this.http.post<any>(`${environment.apiUrl}/documents/${patientId}/batch-save`, payload);
  }

  deletePatientDocument(patientId: number, docId: number): Observable<any> {
    return this.http.delete<any>(`${environment.apiUrl}/documents/${patientId}/document/${docId}`);
  }

  deletePatientMedication(patientId: number, medId: number): Observable<any> {
    return this.http.delete<any>(`${environment.apiUrl}/documents/${patientId}/medication/${medId}`);
  }

  deletePatientRadiology(patientId: number, radId: number): Observable<any> {
    return this.http.delete<any>(`${environment.apiUrl}/documents/${patientId}/radiology/${radId}`);
  }
}
