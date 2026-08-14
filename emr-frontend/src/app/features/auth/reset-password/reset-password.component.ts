import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatIconModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss'
})
export class ResetPasswordComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthService);

  token = signal<string>('');
  email = signal<string>('');
  loading = signal<boolean>(false);
  successMsg = signal<string | null>(null);
  errorMsg = signal<string | null>(null);

  showNewPassword = signal(false);
  showConfirmPassword = signal(false);

  resetForm = this.fb.group({
    newPassword: ['', [Validators.required, Validators.minLength(6)]],
    confirmNewPassword: ['', [Validators.required]]
  });

  get newPassword() { return this.resetForm.get('newPassword'); }
  get confirmNewPassword() { return this.resetForm.get('confirmNewPassword'); }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.token.set(params['token'] || '');
      this.email.set(params['email'] || '');

      if (!this.token() || !this.email()) {
        this.errorMsg.set('Invalid or missing password reset link parameters. Please request a new link.');
      }
    });
  }

  onSubmit(): void {
    if (this.resetForm.invalid) {
      this.resetForm.markAllAsTouched();
      return;
    }

    const newPass = this.newPassword?.value;
    const confirmPass = this.confirmNewPassword?.value;

    if (newPass !== confirmPass) {
      this.errorMsg.set('New password and confirmation password do not match.');
      return;
    }

    this.loading.set(true);
    this.errorMsg.set(null);
    this.successMsg.set(null);

    this.authService.resetPassword({
      email: this.email(),
      token: this.token(),
      newPassword: newPass!,
      confirmNewPassword: confirmPass!
    }).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.successMsg.set(res.message || 'Password reset successfully! You can now sign in.');
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMsg.set(err?.error?.message || 'Failed to reset password. The link may have expired.');
      }
    });
  }
}
