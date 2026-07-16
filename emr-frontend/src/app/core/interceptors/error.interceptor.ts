import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const messageService = inject(MessageService);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unexpected error occurred. Please try again later.';

      if (error.error instanceof ErrorEvent) {
        // Client-side or network connectivity error
        errorMessage = `Network Error: ${error.error.message}`;
        messageService.add({
          severity: 'error',
          summary: 'Connection Error',
          detail: errorMessage,
          life: 4500
        });
      } else {
        // Server-side error response
        if (error.status === 401) {
          errorMessage = 'Your session has expired or you are unauthorized. Please log in again.';
          messageService.add({
            severity: 'warn',
            summary: 'Session Expired',
            detail: errorMessage,
            life: 4000
          });
          authService.logout();
          router.navigate(['/login']);
        } else if (error.status === 403) {
          errorMessage = 'You do not have permission to access this resource or perform this action.';
          messageService.add({
            severity: 'error',
            summary: 'Access Denied',
            detail: errorMessage,
            life: 4500
          });
        } else if (error.status === 404) {
          errorMessage = error.error?.message || 'The requested resource was not found on the server.';
          messageService.add({
            severity: 'warn',
            summary: 'Not Found',
            detail: errorMessage,
            life: 3500
          });
        } else if (error.status === 400) {
          if (typeof error.error === 'string') {
            errorMessage = error.error;
          } else if (error.error?.message) {
            errorMessage = error.error.message;
          } else if (error.error?.detail) {
            errorMessage = error.error.detail;
          } else {
            errorMessage = 'Invalid request parameters submitted.';
          }

          messageService.add({
            severity: 'error',
            summary: 'Validation Error',
            detail: errorMessage,
            life: 4500
          });
        } else if (error.status >= 500) {
          errorMessage = error.error?.message || 'Server error occurred. Our team has been notified.';
          messageService.add({
            severity: 'error',
            summary: 'Server Error (500)',
            detail: errorMessage,
            life: 5000
          });
        } else if (error.status === 0) {
          errorMessage = 'Cannot connect to the EMR Backend API. Please check if the server is running.';
          messageService.add({
            severity: 'error',
            summary: 'Server Offline',
            detail: errorMessage,
            life: 5000
          });
        }
      }

      console.error('HTTP Interceptor Caught Error:', error);
      return throwError(() => error);
    })
  );
};
