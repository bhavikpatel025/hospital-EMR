import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';

// PrimeNG
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { SelectButtonModule } from 'primeng/selectbutton';

import { PublicBookingService, PublicDoctorDto } from '../../../core/services/public-booking.service';

@Component({
  selector: 'app-patient-booking',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    InputTextModule,
    DropdownModule,
    CalendarModule,
    ButtonModule,
    ToastModule,
    ProgressSpinnerModule,
    SelectButtonModule
  ],
  providers: [MessageService],
  templateUrl: './patient-booking.component.html',
  styleUrl: './patient-booking.component.scss'
})
export class PatientBookingComponent implements OnInit {
  private fb = inject(FormBuilder);
  private bookingService = inject(PublicBookingService);
  private messageService = inject(MessageService);
  private router = inject(Router);

  bookingForm!: FormGroup;
  doctors = signal<PublicDoctorDto[]>([]);
  isLoadingDoctors = signal(false);
  isSubmitting = signal(false);

  // Time Widget Signals
  startHour = signal('10');
  startMinute = signal('00');
  startAmPm = signal<'AM' | 'PM'>('AM');

  isSuccess = signal(false);
  today = new Date();

  ngOnInit(): void {
    this.initForm();
    this.loadDoctors();
  }

  private initForm(): void {
    this.bookingForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.maxLength(50)]],
      mobile: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
      doctorId: [null, Validators.required],
      appointmentDate: [null, Validators.required],
      startTime: ['10:00:00', Validators.required],
      reason: ['', Validators.maxLength(500)]
    });
  }

  private loadDoctors(): void {
    this.isLoadingDoctors.set(true);
    this.bookingService.getActiveDoctors().subscribe({
      next: (docs) => {
        this.doctors.set(docs);
        this.isLoadingDoctors.set(false);
      },
      error: (err) => {
        console.error('Failed to load doctors', err);
        this.isLoadingDoctors.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Could not load doctors.' });
      }
    });
  }

  // --- Time Widget Methods ---
  incrementHour(): void {
    let h = parseInt(this.startHour(), 10) || 12;
    h = h === 12 ? 1 : h + 1;
    this.startHour.set(h.toString().padStart(2, '0'));
    this.updateFormTime();
  }

  decrementHour(): void {
    let h = parseInt(this.startHour(), 10) || 12;
    h = h === 1 ? 12 : h - 1;
    this.startHour.set(h.toString().padStart(2, '0'));
    this.updateFormTime();
  }

  incrementMinute(): void {
    let m = parseInt(this.startMinute(), 10) || 0;
    m = (m + 5) % 60;
    this.startMinute.set(m.toString().padStart(2, '0'));
    this.updateFormTime();
  }

  decrementMinute(): void {
    let m = parseInt(this.startMinute(), 10) || 0;
    m = m === 0 ? 55 : m - 5;
    this.startMinute.set(m.toString().padStart(2, '0'));
    this.updateFormTime();
  }

  toggleAmPm(): void {
    this.startAmPm.set(this.startAmPm() === 'AM' ? 'PM' : 'AM');
    this.updateFormTime();
  }

  onTimeInputChange(): void {
    let h = parseInt(this.startHour(), 10) || 12;
    if (h < 1) h = 12;
    if (h > 12) h = 12;

    let m = parseInt(this.startMinute(), 10) || 0;
    if (m < 0) m = 0;
    if (m > 59) m = 59;

    this.startHour.set(h.toString().padStart(2, '0'));
    this.startMinute.set(m.toString().padStart(2, '0'));
    this.updateFormTime();
  }

  private updateFormTime(): void {
    let h = parseInt(this.startHour(), 10);
    const m = this.startMinute();
    const ampm = this.startAmPm();

    if (ampm === 'PM' && h < 12) h += 12;
    if (ampm === 'AM' && h === 12) h = 0;

    const formattedTime = `${h.toString().padStart(2, '0')}:${m}:00`;
    this.bookingForm.patchValue({ startTime: formattedTime });
  }

  onSubmit(): void {
    if (this.bookingForm.invalid) {
      this.bookingForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    const formVal = this.bookingForm.value;
    
    // Calculate End Time (assume 30 mins slot)
    const startTimeStr = formVal.startTime.length === 5 ? formVal.startTime + ':00' : formVal.startTime;
    const [hh, mm, ss] = startTimeStr.split(':').map(Number);
    const seconds = ss || 0;
    const endDate = new Date(1970, 0, 1, hh, mm + 30, seconds);
    const endTimeStr = `${endDate.getHours().toString().padStart(2, '0')}:${endDate.getMinutes().toString().padStart(2, '0')}:00`;

    // Format Date string YYYY-MM-DD
    const apptDate: Date = formVal.appointmentDate;
    const dateStr = apptDate.toLocaleDateString('en-CA'); // e.g. 2026-07-30

    const payload = {
      firstName: formVal.firstName,
      lastName: formVal.lastName,
      mobile: formVal.mobile,
      doctorId: formVal.doctorId,
      appointmentDate: dateStr,
      startTime: startTimeStr,
      endTime: endTimeStr,
      reason: formVal.reason
    };

    this.bookingService.bookAppointment(payload).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        this.isSuccess.set(true);
      },
      error: (err) => {
        console.error(err);
        this.isSubmitting.set(false);
        this.messageService.add({ severity: 'error', summary: 'Booking Failed', detail: err.error?.message || 'Please try another slot.' });
      }
    });
  }

  bookAnother(): void {
    this.isSuccess.set(false);
    this.bookingForm.reset();
  }

  goHome(): void {
    this.router.navigate(['/']);
  }
}
