import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import * as Sentry from '@sentry/angular';
import { environment } from './environments/environment';

if (environment.production && environment.sentryDsn) {
  Sentry.init({
    dsn: environment.sentryDsn,
    integrations: [
      Sentry.browserTracingIntegration(),
      Sentry.replayIntegration()
    ],
    tracesSampleRate: 1.0,
    tracePropagationTargets: ["localhost", /^https:\/\/emr-backend-api-mm1r\.onrender\.com\/api/],
    replaysSessionSampleRate: 0.1,
    replaysOnErrorSampleRate: 1.0,
    enableLogs: true
  });
}

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));
