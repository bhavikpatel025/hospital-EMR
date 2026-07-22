import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ChartDataDto {
  label: string;
  value: number;
}

export interface DashboardAnalyticsDto {
  appointmentsByStatus: ChartDataDto[];
  patientsByGender: ChartDataDto[];
  patientsByAgeGroup: ChartDataDto[];
  appointmentsByDoctor: ChartDataDto[];
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Dashboard`;

  getAnalytics(): Observable<DashboardAnalyticsDto> {
    return this.http.get<DashboardAnalyticsDto>(`${this.apiUrl}/analytics`);
  }
}
