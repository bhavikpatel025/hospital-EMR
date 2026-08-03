import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { PatientTimelineService, TimelineEventDto } from '../../../core/services/patient-timeline.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-patient-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, CardModule],
  templateUrl: './patient-dashboard.component.html',
  styleUrl: './patient-dashboard.component.scss'
})
export class PatientDashboardComponent implements OnInit {
  private timelineService = inject(PatientTimelineService);
  private authService = inject(AuthService);

  upcomingAppointment: any = null;
  recentRecords: TimelineEventDto[] = [];
  loading = true;

  ngOnInit() {
    this.fetchRealData();
  }

  fetchRealData() {
    const user = this.authService.getUser();
    if (user && user.userId) {
      const patientId = parseInt(user.userId, 10);
      this.timelineService.getTimeline(patientId).subscribe({
        next: (events) => {
          this.loading = false;
          
          // 1. Find the most recent upcoming appointment
          const now = new Date();
          const futureAppointments = events
            .filter(e => e.eventType === 'Appointment' && new Date(e.eventDate) > now)
            .sort((a, b) => new Date(a.eventDate).getTime() - new Date(b.eventDate).getTime());

          if (futureAppointments.length > 0) {
            const next = futureAppointments[0];
            const eventDate = new Date(next.eventDate);
            this.upcomingAppointment = {
              doctorName: next.title.replace('Consultation with ', ''),
              department: 'General',
              date: eventDate.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }),
              time: eventDate.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }),
              status: next.description.includes('Confirmed') ? 'Confirmed' : 'Scheduled'
            };
          }

          // 2. Extract recent past records (max 3) to replace the dummy vitals chart
          this.recentRecords = events.filter(e => new Date(e.eventDate) <= now).slice(0, 3);
        },
        error: (err) => {
          this.loading = false;
          console.error('Failed to load dashboard data', err);
        }
      });
    } else {
      this.loading = false;
    }
  }
}
