import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface TimelineEventDto {
  eventId: string;
  eventType: string;
  eventDate: string;
  title: string;
  description: string;
  icon: string;
  color: string;
}

@Injectable({
  providedIn: 'root'
})
export class PatientTimelineService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/PatientTimeline`;

  getTimeline(patientId: number): Observable<TimelineEventDto[]> {
    return this.http.get<TimelineEventDto[]>(`${this.apiUrl}/${patientId}`);
  }
}
