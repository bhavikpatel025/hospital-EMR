import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PaginatorModule } from 'primeng/paginator';
import { MatIconModule } from '@angular/material/icon';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatMenuModule } from '@angular/material/menu';
import { debounceTime, Subject } from 'rxjs';
import { PatientService } from '../../../core/services/patient.service';
import { DoctorService } from '../../../core/services/doctor.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PatientListDto, PatientQueryParams } from '../../../core/models/patient.model';
import { DoctorListDto } from '../../../core/models/doctor.model';

@Component({
  selector: 'app-patient-list',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginatorModule, MatIconModule, ProgressSpinnerModule, MatDialogModule, MatMenuModule],
  templateUrl: './patient-list.component.html',
  styleUrl: './patient-list.component.scss'
})
export class PatientListComponent implements OnInit {
  private readonly patientService = inject(PatientService);
  private readonly notify = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);

  patients = signal<PatientListDto[]>([]);
  totalCount = signal(0);
  loading = signal(false);

  searchTerm = '';
  statusFilter = '';
  genderFilter = '';
  pageNumber = 1;
  pageSize = 10;
  sortBy = 'FullName';
  sortDescending = false;

  private readonly searchSubject = new Subject<string>();

  ngOnInit(): void {
    this.searchSubject.pipe(debounceTime(400)).subscribe(() => {
      this.pageNumber = 1;
      this.loadPatients();
    });

    this.loadPatients();
  }

  onSearchChange(value: string): void {
    this.searchTerm = value;
    this.searchSubject.next(value);
  }

  onFilterChange(): void {
    this.pageNumber = 1;
    this.loadPatients();
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.statusFilter = '';
    this.genderFilter = '';
    this.pageNumber = 1;
    this.loadPatients();
  }

  loadPatients(): void {
    this.loading.set(true);
    const params: PatientQueryParams = {
      searchTerm: this.searchTerm || undefined,
      gender: this.genderFilter || undefined,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      sortBy: this.sortBy,
      sortDescending: this.sortDescending
    };

    this.patientService.getAll(params).subscribe({
      next: response => {
        let items = response.items;
        if (this.statusFilter === 'Active') {
          items = items.filter(p => p.isActive);
        } else if (this.statusFilter === 'Inactive') {
          items = items.filter(p => !p.isActive);
        }
        this.patients.set(items);
        this.totalCount.set(response.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load patients');
        this.loading.set(false);
      }
    });
  }

  onPageChange(event: any): void {
    this.pageNumber = (event.page || 0) + 1;
    this.pageSize = event.rows || 10;
    this.loadPatients();
  }

  onAdd(): void {
    this.router.navigate(['/patients/add']);
  }

  onEdit(id: number): void {
    this.router.navigate(['/patients/edit', id]);
  }

  onView(id: number): void {
    this.router.navigate(['/patients/view', id]);
  }

  onDelete(id: number, name: string): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete Patient', message: `Are you sure you want to delete ${name}?` }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.patientService.delete(id).subscribe({
          next: () => {
            this.notify.success('Patient deleted successfully');
            this.loadPatients();
          },
          error: () => this.notify.error('Failed to delete patient')
        });
      }
    });
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
    if (this.totalCount() === 0 || this.patients().length === 0) {
      return 'Showing 0 to 0 of 0 entries';
    }
    const start = (this.pageNumber - 1) * this.pageSize + 1;
    const end = Math.min(this.pageNumber * this.pageSize, this.totalCount());
    return `Showing ${start} to ${end} of ${this.totalCount()} entries`;
  }

  protected formatId(id: number): string {
    return `#${id}`;
  }

  protected formatUhid(id: number): string {
    return `#${id}`;
  }

  protected formatGender(gender: string): string {
    if (!gender) return 'M';
    return gender.charAt(0).toUpperCase();
  }
}
