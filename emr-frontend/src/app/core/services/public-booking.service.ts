import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PublicDoctorDto {
  doctorId: number;
  doctorName: string;
  specialization: string;
  qualification: string;
}

export interface PublicBookingRequest {
  firstName: string;
  lastName: string;
  mobile: string;
  doctorId: number;
  appointmentDate: string;
  startTime: string;
  endTime: string;
  reason?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PublicBookingService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/public`;

  getActiveDoctors(): Observable<PublicDoctorDto[]> {
    return this.http.get<PublicDoctorDto[]>(`${this.baseUrl}/doctors`);
  }

  bookAppointment(payload: PublicBookingRequest): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/appointments/book`, payload);
  }
}
