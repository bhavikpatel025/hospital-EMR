import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AppointmentListDto } from '../../core/models/appointment.model';
import { DoctorListDto } from '../../core/models/doctor.model';
import { PatientListDto, PagedResult } from '../../core/models/patient.model';
import { AppointmentService } from '../../core/services/appointment.service';
import { DoctorService } from '../../core/services/doctor.service';
import { PatientService } from '../../core/services/patient.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly patientService = inject(PatientService);
  private readonly doctorService = inject(DoctorService);
  private readonly appointmentService = inject(AppointmentService);

  protected readonly loading = signal(true);
  protected readonly todayAppointments = signal<AppointmentListDto[]>([]);
  protected readonly upcomingAppointments = signal<AppointmentListDto[]>([]);
  protected readonly patients = signal<PatientListDto[]>([]);
  protected readonly showAllPatients = signal(false);
  protected readonly displayedPatients = computed(() => {
    const all = this.patients();
    return this.showAllPatients() ? all : all.slice(0, 4);
  });

  protected readonly doctors = signal<DoctorListDto[]>([]);
  protected readonly totalPatients = signal(0);
  protected readonly completedAppointments = signal(0);
  protected readonly cancelledAppointments = signal(0);

  protected readonly completionRate = computed(() => {
    const total = this.todayAppointments().length;
    if (!total) {
      return '0%';
    }

    return `${Math.round((this.completedTodayCount() / total) * 100)}%`;
  });

  ngOnInit(): void {
    forkJoin({
      today: this.appointmentService.getToday().pipe(catchError(() => of([]))),
      upcoming: this.appointmentService.getUpcoming().pipe(catchError(() => of([]))),
      patients: this.patientService.getAll({ pageNumber: 1, pageSize: 20, sortBy: 'FullName', sortDescending: false }).pipe(
        catchError(() => of({ items: [], totalCount: 0, pageNumber: 1, pageSize: 20, totalPages: 0 } as PagedResult<PatientListDto>))
      ),
      doctors: this.doctorService.getActiveDoctors().pipe(catchError(() => of([]))),
      completed: this.appointmentService.getAll({ pageNumber: 1, pageSize: 1, status: 2 }).pipe(
        catchError(() => of({ items: [], totalCount: 0, pageNumber: 1, pageSize: 1, totalPages: 0 } as PagedResult<AppointmentListDto>))
      ),
      cancelled: this.appointmentService.getAll({ pageNumber: 1, pageSize: 1, status: 3 }).pipe(
        catchError(() => of({ items: [], totalCount: 0, pageNumber: 1, pageSize: 1, totalPages: 0 } as PagedResult<AppointmentListDto>))
      )
    }).subscribe(result => {
      this.todayAppointments.set(result.today);
      this.upcomingAppointments.set(result.upcoming.slice(0, 5));
      this.patients.set(result.patients.items);
      this.totalPatients.set(result.patients.totalCount);
      this.doctors.set(result.doctors);
      this.completedAppointments.set(result.completed.totalCount);
      this.cancelledAppointments.set(result.cancelled.totalCount);
      this.loading.set(false);
    });
  }

  protected completedTodayCount(): number {
    return this.todayAppointments().filter(appointment => appointment.status === 'Completed').length;
  }

  protected pendingTodayCount(): number {
    return this.todayAppointments().filter(appointment => appointment.status === 'Pending' || appointment.status === 'Confirmed').length;
  }

  protected initials(name: string): string {
    return name
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map(part => part.charAt(0).toUpperCase())
      .join('');
  }

  protected formatTime(time: string): string {
    const [hours, minutes] = time.split(':');
    const hourValue = Number(hours);
    const displayHour = hourValue % 12 || 12;
    const period = hourValue >= 12 ? 'PM' : 'AM';
    return `${displayHour}:${minutes} ${period}`;
  }

  protected toggleShowMorePatients(): void {
    this.showAllPatients.update(val => !val);
  }
}
