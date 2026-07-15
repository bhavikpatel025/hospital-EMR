import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatIconModule } from '@angular/material/icon';
import { filter } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';

interface NavItem {
  label: string;
  icon: string;
  primeIcon?: string;
  qaColor?: string;
  route?: string;
  exact?: boolean;
}

@Component({
  selector: 'app-emr-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, MatIconModule],
  templateUrl: './emr-shell.component.html',
  styleUrl: './emr-shell.component.scss'
})
export class EmrShellComponent {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly pageTitle = signal('Hospital Dashboard');
  protected readonly sidebarOpen = signal(false);
  protected readonly user = this.authService.getUser();
  protected readonly todayLabel = new Intl.DateTimeFormat('en-IN', {
    day: '2-digit',
    month: 'long',
    year: 'numeric',
    weekday: 'long'
  }).format(new Date());

  protected readonly primaryNav: NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard', primeIcon: 'pi pi-th-large', qaColor: 'qa-blue', route: '/dashboard', exact: true },
    { label: 'Patients', icon: 'groups', primeIcon: 'pi pi-users', qaColor: 'qa-green', route: '/patients' },
    { label: 'Appointments', icon: 'calendar_month', primeIcon: 'pi pi-calendar-plus', qaColor: 'qa-purple', route: '/appointments', exact: true },
    { label: 'Calendar', icon: 'calendar_view_month', primeIcon: 'pi pi-calendar', qaColor: 'qa-orange', route: '/appointments/calendar' },
    { label: 'Doctors', icon: 'medical_services', primeIcon: 'pi pi-id-card', qaColor: 'qa-teal', route: '/doctors' }
  ];

  protected readonly secondaryNav: NavItem[] = [
    { label: 'Consultation', icon: 'health_and_safety' },
    { label: 'Prescription', icon: 'description' },
    { label: 'Investigations', icon: 'science' },
    { label: 'Reports', icon: 'bar_chart' }
  ];

  constructor() {
    this.updateTitle();
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.updateTitle());
  }

  protected toggleSidebar(): void {
    this.sidebarOpen.update(current => !current);
  }

  protected closeSidebar(): void {
    if (typeof window !== 'undefined' && window.innerWidth <= 960) {
      this.sidebarOpen.set(false);
    }
  }

  protected logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  private updateTitle(): void {
    let currentSnapshot = this.router.routerState.snapshot.root;

    while (currentSnapshot.firstChild) {
      currentSnapshot = currentSnapshot.firstChild;
    }

    this.pageTitle.set(currentSnapshot.data?.['title'] ?? 'Dashboard');
  }
}
