import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MatIconModule } from '@angular/material/icon';

import { DoctorService } from '../../../core/services/doctor.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-doctor-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, ProgressSpinnerModule, MatIconModule
  ],
  templateUrl: './doctor-form.component.html',
  styleUrl: './doctor-form.component.scss'
})
export class DoctorFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private doctorService = inject(DoctorService);
  private notify = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  isEditMode = signal(false);
  loading = signal(false);
  doctorId: number | null = null;

  doctorForm = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(150)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.minLength(6)]],   // Edit mode mein required nahi
    specialization: ['', Validators.required],
    qualification: [''],
    consultationFee: [null as number | null, [Validators.required, Validators.min(0)]],
    experienceYears: [null as number | null, [Validators.required, Validators.min(0), Validators.max(60)]]
  });

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode.set(true);
      this.doctorId = +idParam;

      // Edit mode mein email/password fields hide/disable karenge (HTML mein *ngIf se)
      this.doctorForm.get('email')?.clearValidators();
      this.doctorForm.get('password')?.clearValidators();
      this.doctorForm.get('email')?.updateValueAndValidity();
      this.doctorForm.get('password')?.updateValueAndValidity();

      this.loadDoctor(this.doctorId);
    } else {
      this.doctorForm.get('password')?.setValidators([Validators.required, Validators.minLength(6)]);
      this.doctorForm.get('password')?.updateValueAndValidity();
    }
  }

  loadDoctor(id: number): void {
    this.loading.set(true);
    this.doctorService.getById(id).subscribe({
      next: (doctor) => {
        this.doctorForm.patchValue(doctor);
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load doctor details');
        this.loading.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.doctorForm.invalid) {
      this.doctorForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    const formValue = this.doctorForm.getRawValue();

    if (this.isEditMode() && this.doctorId) {
      const updateDto = {
        doctorId: this.doctorId,
        fullName: formValue.fullName,
        specialization: formValue.specialization,
        qualification: formValue.qualification,
        consultationFee: formValue.consultationFee,
        experienceYears: formValue.experienceYears
      };

      this.doctorService.update(this.doctorId, updateDto as any).subscribe({
        next: () => {
          this.notify.success('Doctor updated successfully');
          this.router.navigate(['/doctors']);
        },
        error: (err) => this.handleError(err)
      });
    } else {
      this.doctorService.create(formValue as any).subscribe({
        next: () => {
          this.notify.success('Doctor added successfully');
          this.router.navigate(['/doctors']);
        },
        error: (err) => this.handleError(err)
      });
    }
  }

  private handleError(err: any): void {
    this.loading.set(false);
    this.notify.error(err?.error?.message || 'Something went wrong');
  }

  onCancel(): void {
    this.router.navigate(['/doctors']);
  }

  get fullName() { return this.doctorForm.get('fullName'); }
  get email() { return this.doctorForm.get('email'); }
  get password() { return this.doctorForm.get('password'); }
  get specialization() { return this.doctorForm.get('specialization'); }
  get consultationFee() { return this.doctorForm.get('consultationFee'); }
  get experienceYears() { return this.doctorForm.get('experienceYears'); }
}