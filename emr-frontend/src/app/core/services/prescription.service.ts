import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PrescribedMedicationDto {
  prescribedMedicationId?: number;
  prescriptionId?: number;
  medicineName: string;
  strength?: string;
  dosage?: string;
  frequency?: string;
  instructions?: string;
  duration?: string;
}

export interface PrescriptionDto {
  prescriptionId?: number;
  patientId: number;
  appointmentId?: number;
  chiefComplaints?: string;
  diagnosis?: string;
  vitals?: string;
  investigationsOrdered?: string;
  guidelines?: string;
  nextFollowUpDate?: string;
  medications: PrescribedMedicationDto[];
  createdAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PrescriptionService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Prescription`;

  getById(id: number): Observable<PrescriptionDto> {
    return this.http.get<PrescriptionDto>(`${this.apiUrl}/${id}`);
  }

  getByPatientId(patientId: number): Observable<PrescriptionDto[]> {
    return this.http.get<PrescriptionDto[]>(`${this.apiUrl}/patient/${patientId}`);
  }

  getByAppointmentId(appointmentId: number): Observable<PrescriptionDto> {
    return this.http.get<PrescriptionDto>(`${this.apiUrl}/appointment/${appointmentId}`);
  }

  create(prescription: PrescriptionDto): Observable<PrescriptionDto> {
    return this.http.post<PrescriptionDto>(this.apiUrl, prescription);
  }
}
