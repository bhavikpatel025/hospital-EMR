import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, MatIconModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  loading = signal(false);
  errorMsg = signal('');
  showPassword = signal(false);

  // Forgot Password Modal State
  showForgotModal = signal(false);
  forgotEmail = '';
  forgotLoading = signal(false);
  forgotSuccess = signal<string | null>(null);
  forgotError = signal<string | null>(null);

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  get email() { return this.loginForm.get('email'); }
  get password() { return this.loginForm.get('password'); }

  togglePassword(): void {
    this.showPassword.update(v => !v);
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMsg.set('');

    this.authService.login(this.loginForm.getRawValue() as any).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/app/dashboard']);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMsg.set(err?.error?.message || 'Invalid email or password');
      }
    });
  }

  openForgotModal(): void {
    this.forgotEmail = this.email?.value || '';
    this.forgotSuccess.set(null);
    this.forgotError.set(null);
    this.showForgotModal.set(true);
  }

  closeForgotModal(): void {
    if (this.forgotLoading()) return;
    this.showForgotModal.set(false);
  }

  submitForgotPassword(): void {
    this.forgotError.set(null);
    this.forgotSuccess.set(null);

    const email = this.forgotEmail.trim();
    if (!email) {
      this.forgotError.set('Please enter your registered email address.');
      return;
    }

    this.forgotLoading.set(true);
    this.authService.forgotPassword(email).subscribe({
      next: (res) => {
        this.forgotLoading.set(false);
        this.forgotSuccess.set(res.message || 'If registered, a password reset link has been sent to your inbox.');
      },
      error: (err) => {
        this.forgotLoading.set(false);
        this.forgotError.set(err.error?.message || 'Failed to send reset email. Please try again.');
      }
    });
  }
}