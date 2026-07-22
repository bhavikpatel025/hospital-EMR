import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ChartModule } from 'primeng/chart';
import { AppointmentListDto } from '../../core/models/appointment.model';
import { DoctorListDto } from '../../core/models/doctor.model';
import { PatientListDto, PagedResult } from '../../core/models/patient.model';
import { AppointmentService } from '../../core/services/appointment.service';
import { DoctorService } from '../../core/services/doctor.service';
import { PatientService } from '../../core/services/patient.service';
import { DashboardService, DashboardAnalyticsDto } from '../../core/services/dashboard.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, ChartModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly patientService = inject(PatientService);
  private readonly doctorService = inject(DoctorService);
  private readonly appointmentService = inject(AppointmentService);
  private readonly dashboardService = inject(DashboardService);

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

  // Chart Data Signals
  protected readonly statusChartData = signal<any>(null);
  protected readonly genderChartData = signal<any>(null);
  protected readonly ageChartData = signal<any>(null);
  protected readonly doctorChartData = signal<any>(null);
  
  protected barChartOptions: any;
  protected ageChartPlugins: any[] = [];
  protected pieChartOptions: any;

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
      ),
      analytics: this.dashboardService.getAnalytics().pipe(catchError(() => of(null)))
    }).subscribe(result => {
      this.todayAppointments.set(result.today);
      this.upcomingAppointments.set(result.upcoming.slice(0, 5));
      this.patients.set(result.patients.items);
      this.totalPatients.set(result.patients.totalCount);
      this.doctors.set(result.doctors);
      this.completedAppointments.set(result.completed.totalCount);
      this.cancelledAppointments.set(result.cancelled.totalCount);
      
      if (result.analytics) {
        this.initCharts(result.analytics);
      }
      
      this.loading.set(false);
    });
  }

  private initCharts(data: DashboardAnalyticsDto): void {
    const documentStyle = getComputedStyle(document.documentElement);
    const textColor = documentStyle.getPropertyValue('--emr-text') || '#334155';
    const textColorSecondary = documentStyle.getPropertyValue('--emr-text-soft') || '#64748b';
    const surfaceBorder = documentStyle.getPropertyValue('--emr-border') || '#e2e8f0';

    this.barChartOptions = {
      maintainAspectRatio: false,
      plugins: {
        legend: {
          labels: { color: textColor, usePointStyle: true, padding: 15 }
        }
      },
      layout: {
        padding: { top: 10, bottom: 10 }
      },
      scales: {
        x: {
          ticks: { color: textColorSecondary },
          grid: { color: surfaceBorder, drawBorder: false }
        },
        y: {
          ticks: { color: textColorSecondary },
          grid: { color: surfaceBorder, drawBorder: false }
        }
      }
    };

    this.pieChartOptions = {
      maintainAspectRatio: false,
      plugins: {
        legend: {
          labels: { color: textColor, usePointStyle: true, padding: 20 }
        }
      },
      layout: {
        padding: { top: 10, bottom: 10, left: 10, right: 10 }
      }
    };

    // Custom inline plugin to draw text above bars for Age chart
    const drawLabelsAboveBarsPlugin = {
      id: 'drawLabelsAboveBars',
      afterDatasetsDraw(chart: any, args: any, options: any) {
        const { ctx } = chart;
        ctx.save();
        chart.data.datasets.forEach((dataset: any, i: number) => {
          const meta = chart.getDatasetMeta(i);
          meta.data.forEach((bar: any, index: number) => {
            const data = dataset.data[index];
            if (data > 0) {
              ctx.fillStyle = textColor;
              ctx.font = 'bold 13px "Inter", sans-serif';
              ctx.textAlign = 'center';
              ctx.textBaseline = 'bottom';
              ctx.fillText(data, bar.x, bar.y - 6);
            }
          });
        });
        ctx.restore();
      }
    };
    this.ageChartPlugins = [drawLabelsAboveBarsPlugin];

    // 1. Status Chart (Doughnut)
    this.statusChartData.set({
      labels: data.appointmentsByStatus.map(x => x.label === 'Pending' ? 'Pending      ' : x.label),
      datasets: [{
        data: data.appointmentsByStatus.map(x => x.value),
        backgroundColor: ['#f59e0b', '#3b82f6', '#10b981', '#ef4444'],
        hoverBackgroundColor: ['#d97706', '#2563eb', '#059669', '#dc2626']
      }]
    });

    // 2. Gender Chart (Pie)
    const totalGender = data.patientsByGender.reduce((sum, item) => sum + item.value, 0);
    this.genderChartData.set({
      labels: data.patientsByGender.map(x => {
        const percentage = totalGender === 0 ? 0 : Math.round((x.value / totalGender) * 100);
        return `${x.label}: ${x.value} (${percentage}%)`;
      }),
      datasets: [{
        data: data.patientsByGender.map(x => x.value),
        backgroundColor: ['#3b82f6', '#ec4899', '#8b5cf6'],
        hoverBackgroundColor: ['#2563eb', '#db2777', '#7c3aed']
      }]
    });

    // 3. Age Demographics (Bar)
    this.ageChartData.set({
      labels: data.patientsByAgeGroup.map(x => x.label),
      datasets: [{
        label: 'Patients',
        data: data.patientsByAgeGroup.map(x => x.value),
        backgroundColor: '#0ea5e9',
        borderRadius: 4,
        barThickness: 32,
        maxBarThickness: 40
      }]
    });

    // 4. Doctor Workload (Bar)
    this.doctorChartData.set({
      labels: data.appointmentsByDoctor.map(x => x.label),
      datasets: [{
        label: 'Appointments',
        data: data.appointmentsByDoctor.map(x => x.value),
        backgroundColor: '#8b5cf6',
        borderRadius: 6
      }]
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
