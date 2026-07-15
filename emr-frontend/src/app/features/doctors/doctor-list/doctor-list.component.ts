import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';

import { DoctorService } from '../../../core/services/doctor.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { DoctorListDto, DoctorQueryParams } from '../../../core/models/doctor.model';
import { debounceTime, Subject } from 'rxjs';

@Component({
  selector: 'app-doctor-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatTableModule, MatPaginatorModule, MatSortModule,
    MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule,
    MatProgressSpinnerModule, MatDialogModule, MatChipsModule
  ],
  templateUrl: './doctor-list.component.html',
  styleUrl: './doctor-list.component.scss'
})
export class DoctorListComponent implements OnInit {
  private doctorService = inject(DoctorService);
  private notify = inject(NotificationService);
  private router = inject(Router);
  private dialog = inject(MatDialog);

  displayedColumns = ['fullName', 'specialization', 'email', 'consultationFee', 'actions'];
  doctors = signal<DoctorListDto[]>([]);
  totalCount = signal(0);
  loading = signal(false);

  searchTerm = '';
  pageNumber = 1;
  pageSize = 10;
  sortBy = 'FullName';
  sortDescending = false;

  private searchSubject = new Subject<string>();

  ngOnInit(): void {
    this.searchSubject.pipe(debounceTime(400)).subscribe(() => {
      this.pageNumber = 1;
      this.loadDoctors();
    });
    this.loadDoctors();
  }

  onSearchChange(value: string): void {
    this.searchTerm = value;
    this.searchSubject.next(value);
  }

  loadDoctors(): void {
    this.loading.set(true);
    const params: DoctorQueryParams = {
      searchTerm: this.searchTerm || undefined,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      sortBy: this.sortBy,
      sortDescending: this.sortDescending
    };

    this.doctorService.getAll(params).subscribe({
      next: (res) => {
        this.doctors.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load doctors');
        this.loading.set(false);
      }
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadDoctors();
  }

  onSortChange(sort: Sort): void {
    if (!sort.direction) return;
    this.sortBy = sort.active;
    this.sortDescending = sort.direction === 'desc';
    this.loadDoctors();
  }

  onAdd(): void {
    this.router.navigate(['/doctors/add']);
  }

  onEdit(id: number): void {
    this.router.navigate(['/doctors/edit', id]);
  }

  onDelete(id: number, name: string): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Delete Doctor', message: `Are you sure you want to delete Dr. ${name}?` }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.doctorService.delete(id).subscribe({
          next: () => {
            this.notify.success('Doctor deleted successfully');
            this.loadDoctors();
          },
          error: () => this.notify.error('Failed to delete doctor')
        });
      }
    });
  }

  protected get showingRangeText(): string {
    const total = this.totalCount();
    if (total === 0) return 'Showing 0 to 0 of 0 entries';
    const start = (this.pageNumber - 1) * this.pageSize + 1;
    const end = Math.min(this.pageNumber * this.pageSize, total);
    return `Showing ${start} to ${end} of ${total} entries`;
  }

  protected formatId(id: number): string {
    return `#DOC-${String(id).padStart(4, '0')}`;
  }

  protected initials(name: string): string {
    if (!name) return 'D';
    return name
      .replace(/^Dr\.?\s*/i, '')
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map(part => part[0].toUpperCase())
      .join('');
  }
}