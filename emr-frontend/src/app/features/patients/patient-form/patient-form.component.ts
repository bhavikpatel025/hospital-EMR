import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MatIconModule } from '@angular/material/icon';

import { PatientService } from '../../../core/services/patient.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-patient-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatButtonModule, ProgressSpinnerModule, MatIconModule
  ],
  templateUrl: './patient-form.component.html',
  styleUrl: './patient-form.component.scss'
})
export class PatientFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private patientService = inject(PatientService);
  private notify = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  isEditMode = signal(false);
  loading = signal(false);
  patientId: number | null = null;

  patientForm = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(150)]],
    age: [null as number | null, [Validators.required, Validators.min(0), Validators.max(150)]],
    gender: ['', Validators.required],
    bloodGroup: [''],
    mobile: ['', [Validators.required, Validators.pattern(/^[0-9]{10}$/)]],
    email: ['', Validators.email],
    address: ['']
  });

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode.set(true);
      this.patientId = +idParam;
      this.loadPatient(this.patientId);
    }
  }

  loadPatient(id: number): void {
    this.loading.set(true);
    this.patientService.getById(id).subscribe({
      next: (patient) => {
        this.patientForm.patchValue(patient);
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load patient details');
        this.loading.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.patientForm.invalid) {
      this.patientForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    const formValue = this.patientForm.getRawValue();

    if (this.isEditMode() && this.patientId) {
      const updateDto = { ...formValue, patientId: this.patientId };
      this.patientService.update(this.patientId, updateDto as any).subscribe({
        next: () => {
          this.notify.success('Patient updated successfully');
          this.router.navigate(['/patients']);
        },
        error: (err) => this.handleError(err)
      });
    } else {
      this.patientService.create(formValue as any).subscribe({
        next: () => {
          this.notify.success('Patient added successfully');
          this.router.navigate(['/patients']);
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
    this.router.navigate(['/patients']);
  }

  // Getters for validation in HTML
  get fullName() { return this.patientForm.get('fullName'); }
  get age() { return this.patientForm.get('age'); }
  get gender() { return this.patientForm.get('gender'); }
  get mobile() { return this.patientForm.get('mobile'); }
  get email() { return this.patientForm.get('email'); }
}