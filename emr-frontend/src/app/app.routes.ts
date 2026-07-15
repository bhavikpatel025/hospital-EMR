import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./shared/layouts/emr-shell/emr-shell.component').then(m => m.EmrShellComponent),
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
        data: { title: 'Dashboard' }
      },
      {
        path: 'patients',
        children: [
          {
            path: '',
            loadComponent: () => import('./features/patients/patient-list/patient-list.component').then(m => m.PatientListComponent),
            data: { title: 'Patients' }
          },
          {
            path: 'add',
            loadComponent: () => import('./features/patients/patient-form/patient-form.component').then(m => m.PatientFormComponent),
            data: { title: 'Add Patient' }
          },
          {
            path: 'edit/:id',
            loadComponent: () => import('./features/patients/patient-form/patient-form.component').then(m => m.PatientFormComponent),
            data: { title: 'Edit Patient' }
          },
          {
            path: 'view/:id',
            loadComponent: () => import('./features/patients/patient-detail/patient-detail.component').then(m => m.PatientDetailComponent),
            data: { title: 'Patient Profile' }
          }
        ]
      },
      {
        path: 'doctors',
        children: [
          { path: '', loadComponent: () => import('./features/doctors/doctor-list/doctor-list.component').then(m => m.DoctorListComponent), data: { title: 'Doctors' } },
          { path: 'add', loadComponent: () => import('./features/doctors/doctor-form/doctor-form.component').then(m => m.DoctorFormComponent), data: { title: 'Add Doctor' } },
          { path: 'edit/:id', loadComponent: () => import('./features/doctors/doctor-form/doctor-form.component').then(m => m.DoctorFormComponent), data: { title: 'Edit Doctor' } }
        ]
      },
      {
        path: 'appointments',
        children: [
          { path: '', loadComponent: () => import('./features/appointments/appointment-list/appointment-list.component').then(m => m.AppointmentListComponent), data: { title: 'Appointments' } },
          { path: 'add', loadComponent: () => import('./features/appointments/appointment-form/appointment-form.component').then(m => m.AppointmentFormComponent), data: { title: 'New Appointment' } },
          { path: 'edit/:id', loadComponent: () => import('./features/appointments/appointment-form/appointment-form.component').then(m => m.AppointmentFormComponent), data: { title: 'Edit Appointment' } },
          { path: 'calendar', loadComponent: () => import('./features/appointments/appointment-calendar/appointment-calendar.component').then(m => m.AppointmentCalendarComponent), data: { title: 'Appointment Calendar' } }
        ]
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: '**', redirectTo: 'login' }
];
