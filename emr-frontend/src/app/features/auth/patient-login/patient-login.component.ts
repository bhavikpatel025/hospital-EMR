import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

import { FloatLabel } from 'primeng/floatlabel';
import { InputText } from 'primeng/inputtext';
import { InputOtp } from 'primeng/inputotp';

@Component({
  selector: 'app-patient-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, FloatLabel, InputText, InputOtp],
  templateUrl: './patient-login.component.html',
  styleUrl: './patient-login.component.scss'
})
export class PatientLoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  step = signal<1 | 2>(1); // 1 = Mobile, 2 = OTP
  loading = signal(false);
  errorMsg = signal('');

  mobileForm = this.fb.group({
    mobile: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]]
  });

  otpForm = this.fb.group({
    otp: ['', [Validators.required, Validators.pattern('^[0-9]{6}$')]]
  });

  // Getter for easy access in HTML
  get mobile() { return this.mobileForm.get('mobile'); }
  get otp() { return this.otpForm.get('otp'); }

  requestOtp(): void {
    if (this.mobileForm.invalid) {
      this.mobileForm.markAllAsTouched();
      return;
    }
    
    this.loading.set(true);
    
    // Simulate API call for OTP
    setTimeout(() => {
      this.loading.set(false);
      this.step.set(2);
    }, 1000);
  }

  verifyOtp(): void {
    if (this.otpForm.invalid) {
      this.otpForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMsg.set('');

    const payload = {
      mobile: this.mobileForm.value.mobile!,
      otp: this.otpForm.value.otp!
    };

    this.authService.patientLogin(payload).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.router.navigate(['/patient/dashboard']);
      },
      error: (err) => {
        this.loading.set(false);
        if (err.status === 401) {
          this.errorMsg.set('Invalid or expired OTP. Please try again.');
        } else if (err.status === 404) {
          this.errorMsg.set('Mobile number not found. Please contact hospital staff.');
        } else {
          this.errorMsg.set('Something went wrong. Please try again later.');
        }
      }
    });
  }
}
