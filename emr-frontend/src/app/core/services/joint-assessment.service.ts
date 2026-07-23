import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface JointAssessmentDto {
  id?: number;
  patientId: number;
  appointmentId?: number;
  assessmentDate: string;
  jointsDataJson: string;
  notes?: string;
  totalTender: number;
  totalSwollen: number;
  totalBoth: number;
  totalLimited: number;
  totalNormal: number;
  totalJointsAssessed: number;
}

@Injectable({
  providedIn: 'root'
})
export class JointAssessmentService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/JointAssessments`;

  createAssessment(data: JointAssessmentDto): Observable<JointAssessmentDto> {
    return this.http.post<JointAssessmentDto>(this.apiUrl, data);
  }

  getAssessmentsByPatient(patientId: number): Observable<JointAssessmentDto[]> {
    return this.http.get<JointAssessmentDto[]>(`${this.apiUrl}/patient/${patientId}`);
  }

  getLatestAssessment(patientId: number): Observable<JointAssessmentDto> {
    return this.http.get<JointAssessmentDto>(`${this.apiUrl}/patient/${patientId}/latest`);
  }
}
