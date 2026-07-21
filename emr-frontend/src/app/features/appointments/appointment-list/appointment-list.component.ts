import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PaginatorModule } from 'primeng/paginator';
import { MatIconModule } from '@angular/material/icon';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatMenuModule } from '@angular/material/menu';
import { debounceTime, forkJoin, of, Subject } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AppointmentService } from '../../../core/services/appointment.service';
import { DoctorService } from '../../../core/services/doctor.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AppointmentListDto, AppointmentQueryParams } from '../../../core/models/appointment.model';
import { DoctorListDto } from '../../../core/models/doctor.model';

@Component({
  selector: 'app-appointment-list',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginatorModule, MatIconModule, ProgressSpinnerModule, MatDialogModule, MatMenuModule],
  templateUrl: './appointment-list.component.html',
  styleUrl: './appointment-list.component.scss'
})
export class AppointmentListComponent implements OnInit {
  private readonly appointmentService = inject(AppointmentService);
  private readonly doctorService = inject(DoctorService);
  private readonly notify = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);

  appointments = signal<AppointmentListDto[]>([]);
  doctors = signal<DoctorListDto[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  todayCount = signal(0);
  upcomingCount = signal(0);
  completedCount = signal(0);
  cancelledCount = signal(0);

  searchTerm = '';
  doctorFilter: number | null = null;
  statusFilter: number | null = null;
  selectedDate = new Date().toISOString().slice(0, 10);
  pageNumber = 1;
  pageSize = 10;

  statusOptions = [
    { label: 'Pending', value: 0 },
    { label: 'Confirmed', value: 1 },
    { label: 'Completed', value: 2 },
    { label: 'Cancelled', value: 3 }
  ];

  private readonly searchSubject = new Subject<string>();

  ngOnInit(): void {
    this.searchSubject.pipe(debounceTime(400)).subscribe(() => {
      this.pageNumber = 1;
      this.loadAppointments();
    });

    this.doctorService.getActiveDoctors().subscribe(result => this.doctors.set(result));
    this.loadSummary();
    this.loadAppointments();
  }

  onSearchChange(value: string): void {
    this.searchTerm = value;
    this.searchSubject.next(value);
  }

  loadAppointments(): void {
    this.loading.set(true);
    const params: AppointmentQueryParams = {
      searchTerm: this.searchTerm || undefined,
      doctorId: this.doctorFilter ?? undefined,
      status: this.statusFilter ?? undefined,
      fromDate: this.selectedDate || undefined,
      toDate: this.selectedDate || undefined,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };

    this.appointmentService.getAll(params).subscribe({
      next: response => {
        this.appointments.set(response.items);
        this.totalCount.set(response.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load appointments');
        this.loading.set(false);
      }
    });
  }

  loadSummary(): void {
    forkJoin({
      today: this.appointmentService.getToday().pipe(catchError(() => of([]))),
      upcoming: this.appointmentService.getUpcoming().pipe(catchError(() => of([]))),
      completed: this.appointmentService.getAll({ pageNumber: 1, pageSize: 1, status: 2 }).pipe(catchError(() => of({ items: [], totalCount: 0 } as any))),
      cancelled: this.appointmentService.getAll({ pageNumber: 1, pageSize: 1, status: 3 }).pipe(catchError(() => of({ items: [], totalCount: 0 } as any)))
    }).subscribe(result => {
      this.todayCount.set(result.today.length);
      this.upcomingCount.set(result.upcoming.length);
      this.completedCount.set(result.completed.totalCount ?? 0);
      this.cancelledCount.set(result.cancelled.totalCount ?? 0);
    });
  }

  onPageChange(event: any): void {
    this.pageNumber = (event.page || 0) + 1;
    this.pageSize = event.rows || 10;
    this.loadAppointments();
  }

  onAdd(): void {
    this.router.navigate(['/appointments/add']);
  }

  onCalendar(): void {
    this.router.navigate(['/appointments/calendar']);
  }

  onEdit(id: number): void {
    this.router.navigate(['/appointments/edit', id]);
  }

  onChangeStatus(id: number, statusValue: number): void {
    this.appointmentService.updateStatus(id, { appointmentId: id, status: statusValue }).subscribe({
      next: () => {
        this.notify.success('Status updated successfully');
        this.loadSummary();
        this.loadAppointments();
      },
      error: () => this.notify.error('Failed to update status')
    });
  }

  onDelete(id: number, patientName: string): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete Appointment', message: `Delete appointment for ${patientName}?` }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.appointmentService.delete(id).subscribe({
          next: () => {
            this.notify.success('Appointment deleted successfully');
            this.loadSummary();
            this.loadAppointments();
          },
          error: () => this.notify.error('Failed to delete appointment')
        });
      }
    });
  }

  protected formatTime(time: string): string {
    const [hours, minutes] = time.split(':');
    const hourValue = Number(hours);
    const period = hourValue >= 12 ? 'PM' : 'AM';
    const displayHour = hourValue % 12 || 12;
    return `${displayHour}:${minutes} ${period}`;
  }

  protected initials(name: string): string {
    return name
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map(part => part.charAt(0).toUpperCase())
      .join('');
  }

  protected get showingRangeText(): string {
    if (this.totalCount() === 0 || this.appointments().length === 0) {
      return 'Showing 0 to 0 of 0 entries';
    }
    const start = (this.pageNumber - 1) * this.pageSize + 1;
    const end = Math.min(this.pageNumber * this.pageSize, this.totalCount());
    return `Showing ${start} to ${end} of ${this.totalCount()} entries`;
  }
}
