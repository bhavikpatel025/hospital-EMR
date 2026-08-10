import { ApplicationConfig, provideZoneChangeDetection, ErrorHandler, APP_INITIALIZER } from '@angular/core';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';
import { MessageService } from 'primeng/api';
import * as Sentry from '@sentry/angular';
import { environment } from '../environments/environment';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    MessageService,
    provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: Aura,
        options: {
          darkModeSelector: false as any,
          cssLayer: {
            name: 'primeng',
            order: 'primeng'
          }
        }
      },
      ripple: true
    }),
    
    // Configure Sentry ONLY for production if DSN exists
    ...(environment.production && environment.sentryDsn ? [
      {
        provide: ErrorHandler,
        useValue: Sentry.createErrorHandler(),
      },
      {
        provide: Sentry.TraceService,
        deps: [Router],
      },
      {
        provide: APP_INITIALIZER,
        useFactory: () => () => {},
        deps: [Sentry.TraceService],
        multi: true,
      }
    ] : [])
  ]
};