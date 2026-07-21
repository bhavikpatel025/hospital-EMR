import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatIconModule } from '@angular/material/icon';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DatePickerModule } from 'primeng/datepicker';

import { AppointmentService } from '../../../core/services/appointment.service';
import { PatientService } from '../../../core/services/patient.service';
import { DoctorService } from '../../../core/services/doctor.service';
import { NotificationService } from '../../../core/services/notification.service';
import { debounceTime, distinctUntilChanged, switchMap, of } from 'rxjs';
import { PatientListDto } from '../../../core/models/patient.model';
import { DoctorListDto } from '../../../core/models/doctor.model';

@Component({
  selector: 'app-appointment-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatButtonModule, MatDatepickerModule, MatNativeDateModule,
    ProgressSpinnerModule, MatAutocompleteModule, MatIconModule, ButtonModule, InputTextModule, DatePickerModule
  ],
  templateUrl: './appointment-form.component.html',
  styleUrl: './appointment-form.component.scss'
})
export class AppointmentFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private appointmentService = inject(AppointmentService);
  private patientService = inject(PatientService);
  private doctorService = inject(DoctorService);
  private notify = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  isEditMode = signal(false);
  loading = signal(false);
  appointmentId: number | null = null;

  doctors = signal<DoctorListDto[]>([]);
  patientResults = signal<PatientListDto[]>([]);
  selectedPatientName = signal('');

  // Time Widget Signals
  startHour = signal('09');
  startMinute = signal('00');
  startAmPm = signal<'AM' | 'PM'>('AM');

  endHour = signal('09');
  endMinute = signal('30');
  endAmPm = signal<'AM' | 'PM'>('AM');

  appointmentForm = this.fb.group({
    patientId: [null as number | null, Validators.required],
    patientSearch: [''],   // sirf display/search ke liye, backend ko nahi jaata
    doctorId: [null as number | null, Validators.required],
    appointmentDate: [null as Date | null, Validators.required],
    startTime: ['09:00', Validators.required],
    endTime: ['09:30', Validators.required],
    reason: [''],
    notes: ['']
  });

  ngOnInit(): void {
    this.doctorService.getActiveDoctors().subscribe(res => this.doctors.set(res));

    // Patient search-as-you-type (autocomplete)
    this.appointmentForm.get('patientSearch')?.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(term => {
        if (!term || term.length < 2) return of({ items: [], totalCount: 0, pageNumber: 1, pageSize: 10, totalPages: 0 });
        return this.patientService.getAll({ searchTerm: term, pageNumber: 1, pageSize: 10 });
      })
    ).subscribe(res => this.patientResults.set(res.items));

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode.set(true);
      this.appointmentId = +idParam;
      this.loadAppointment(this.appointmentId);
    } else {
      // Default new appointment date to today
      this.appointmentForm.patchValue({ appointmentDate: new Date() });
      this.syncTimesToForm();
    }
  }

  loadAppointment(id: number): void {
    this.loading.set(true);
    this.appointmentService.getById(id).subscribe({
      next: (appt) => {
        const sTime = appt.startTime.substring(0, 5);
        const eTime = appt.endTime.substring(0, 5);
        this.appointmentForm.patchValue({
          patientId: appt.patientId,
          patientSearch: appt.patientName,
          doctorId: appt.doctorId,
          appointmentDate: new Date(appt.appointmentDate),
          startTime: sTime,
          endTime: eTime,
          reason: appt.reason,
          notes: (appt as any).notes
        });
        this.selectedPatientName.set(appt.patientName);
        this.parse24HourToStart(sTime);
        this.parse24HourToEnd(eTime);
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load appointment');
        this.loading.set(false);
      }
    });
  }

  // Custom Time Widget Helper Methods
  incrementHour(type: 'start' | 'end'): void {
    if (type === 'start') {
      let h = parseInt(this.startHour(), 10);
      h = h === 12 ? 1 : h + 1;
      this.startHour.set(String(h).padStart(2, '0'));
    } else {
      let h = parseInt(this.endHour(), 10);
      h = h === 12 ? 1 : h + 1;
      this.endHour.set(String(h).padStart(2, '0'));
    }
    this.syncTimesToForm();
  }

  decrementHour(type: 'start' | 'end'): void {
    if (type === 'start') {
      let h = parseInt(this.startHour(), 10);
      h = h === 1 ? 12 : h - 1;
      this.startHour.set(String(h).padStart(2, '0'));
    } else {
      let h = parseInt(this.endHour(), 10);
      h = h === 1 ? 12 : h - 1;
      this.endHour.set(String(h).padStart(2, '0'));
    }
    this.syncTimesToForm();
  }

  incrementMinute(type: 'start' | 'end'): void {
    if (type === 'start') {
      let m = parseInt(this.startMinute(), 10);
      m = (m + 5) % 60;
      this.startMinute.set(String(m).padStart(2, '0'));
    } else {
      let m = parseInt(this.endMinute(), 10);
      m = (m + 5) % 60;
      this.endMinute.set(String(m).padStart(2, '0'));
    }
    this.syncTimesToForm();
  }

  decrementMinute(type: 'start' | 'end'): void {
    if (type === 'start') {
      let m = parseInt(this.startMinute(), 10);
      m = (m - 5 + 60) % 60;
      this.startMinute.set(String(m).padStart(2, '0'));
    } else {
      let m = parseInt(this.endMinute(), 10);
      m = (m - 5 + 60) % 60;
      this.endMinute.set(String(m).padStart(2, '0'));
    }
    this.syncTimesToForm();
  }

  toggleAmPm(type: 'start' | 'end'): void {
    if (type === 'start') {
      this.startAmPm.set(this.startAmPm() === 'AM' ? 'PM' : 'AM');
    } else {
      this.endAmPm.set(this.endAmPm() === 'AM' ? 'PM' : 'AM');
    }
    this.syncTimesToForm();
  }

  onTimeInputChange(type: 'start' | 'end'): void {
    this.syncTimesToForm();
  }

  syncTimesToForm(): void {
    const s24 = this.to24Hour(this.startHour(), this.startMinute(), this.startAmPm());
    const e24 = this.to24Hour(this.endHour(), this.endMinute(), this.endAmPm());
    this.appointmentForm.patchValue({
      startTime: s24,
      endTime: e24
    });
  }

  to24Hour(hourStr: string, minStr: string, amPm: 'AM' | 'PM'): string {
    let h = parseInt(hourStr || '12', 10);
    if (isNaN(h) || h < 1) h = 12;
    if (h > 12) h = 12;
    let m = parseInt(minStr || '00', 10);
    if (isNaN(m) || m < 0) m = 0;
    if (m > 59) m = 59;

    if (amPm === 'AM' && h === 12) h = 0;
    else if (amPm === 'PM' && h < 12) h += 12;

    return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`;
  }

  parse24HourToStart(timeStr: string): void {
    if (!timeStr) return;
    const parts = timeStr.split(':');
    if (parts.length < 2) return;
    let h = parseInt(parts[0], 10);
    const m = parseInt(parts[1], 10);
    let amPm: 'AM' | 'PM' = 'AM';
    if (h >= 12) {
      amPm = 'PM';
      if (h > 12) h -= 12;
    } else if (h === 0) {
      h = 12;
    }
    this.startHour.set(String(h).padStart(2, '0'));
    this.startMinute.set(String(m).padStart(2, '0'));
    this.startAmPm.set(amPm);
  }

  parse24HourToEnd(timeStr: string): void {
    if (!timeStr) return;
    const parts = timeStr.split(':');
    if (parts.length < 2) return;
    let h = parseInt(parts[0], 10);
    const m = parseInt(parts[1], 10);
    let amPm: 'AM' | 'PM' = 'AM';
    if (h >= 12) {
      amPm = 'PM';
      if (h > 12) h -= 12;
    } else if (h === 0) {
      h = 12;
    }
    this.endHour.set(String(h).padStart(2, '0'));
    this.endMinute.set(String(m).padStart(2, '0'));
    this.endAmPm.set(amPm);
  }

  onSelectPatient(patient: PatientListDto): void {
    this.appointmentForm.patchValue({
      patientId: patient.patientId,
      patientSearch: patient.fullName
    });
    this.selectedPatientName.set(patient.fullName);
    this.patientResults.set([]);
  }

  onSubmit(): void {
    if (this.appointmentForm.invalid) {
      this.appointmentForm.markAllAsTouched();
      return;
    }

    const formValue = this.appointmentForm.getRawValue();
    const dateStr = this.formatDate(formValue.appointmentDate!);
    const startStr = formValue.startTime!.length === 5 ? formValue.startTime + ':00' : formValue.startTime!;
    const endStr = formValue.endTime!.length === 5 ? formValue.endTime + ':00' : formValue.endTime!;

    const payload = {
      patientId: formValue.patientId!,
      doctorId: formValue.doctorId!,
      appointmentDate: dateStr,
      startTime: startStr,
      endTime: endStr,
      reason: formValue.reason || undefined,
      notes: formValue.notes || undefined
    };

    this.loading.set(true);

    if (this.isEditMode() && this.appointmentId) {
      this.appointmentService.update(this.appointmentId, { ...payload, appointmentId: this.appointmentId }).subscribe({
        next: () => {
          this.notify.success('Appointment updated successfully');
          this.router.navigate(['/appointments']);
        },
        error: (err) => this.handleError(err)
      });
    } else {
      this.appointmentService.create(payload).subscribe({
        next: () => {
          this.notify.success('Appointment created successfully');
          this.router.navigate(['/appointments']);
        },
        error: (err) => this.handleError(err)
      });
    }
  }

  private formatDate(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  private handleError(err: any): void {
    this.loading.set(false);
    this.notify.error(err?.error?.message || 'Something went wrong');
  }

  onCancel(): void {
    this.router.navigate(['/appointments']);
  }

  displayPatientName = (patient: PatientListDto): string => patient?.fullName || '';

  get patientId() { return this.appointmentForm.get('patientId'); }
  get doctorId() { return this.appointmentForm.get('doctorId'); }
  get appointmentDate() { return this.appointmentForm.get('appointmentDate'); }
  get startTime() { return this.appointmentForm.get('startTime'); }
  get endTime() { return this.appointmentForm.get('endTime'); }
}