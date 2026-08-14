import { CommonModule } from '@angular/common';
import { Component, DestroyRef, ElementRef, HostListener, inject, signal } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatIconModule } from '@angular/material/icon';
import { filter } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { SystemNotificationService, SystemNotification } from '../../../core/services/system-notification.service';

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
  private readonly notificationService = inject(SystemNotificationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly elementRef = inject(ElementRef);

  protected readonly pageTitle = signal('Hospital Dashboard');
  protected readonly sidebarCollapsed = signal(false);
  protected readonly sidebarOpenMobile = signal(false);
  protected readonly user = this.authService.getUser();
  protected readonly todayLabel = new Intl.DateTimeFormat('en-IN', {
    day: '2-digit',
    month: 'long',
    year: 'numeric',
    weekday: 'long'
  }).format(new Date());

  protected readonly notifications$ = this.notificationService.notifications$;
  protected readonly unreadCount$ = this.notificationService.unreadCount$;
  protected showNotifications = signal(false);

  protected primaryNav: NavItem[] = [];

  protected readonly secondaryNav: NavItem[] = [
    { label: 'Consultation', icon: 'health_and_safety' },
    { label: 'Prescription', icon: 'description' },
    { label: 'Investigations', icon: 'science' },
    { label: 'Reports', icon: 'bar_chart' }
  ];

  constructor() {
    this.buildNavigation();
    this.updateTitle();
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.updateTitle();
        this.showNotifications.set(false);
      });

    this.notificationService.startConnection();

    this.destroyRef.onDestroy(() => {
      this.notificationService.stopConnection();
    });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.showNotifications()) return;
    const targetElement = event.target as HTMLElement;
    const notificationContainer = this.elementRef.nativeElement.querySelector('.notification-container');
    if (notificationContainer && !notificationContainer.contains(targetElement)) {
      this.showNotifications.set(false);
    }
  }

  protected toggleSidebar(): void {
    this.showNotifications.set(false);
    if (typeof window !== 'undefined' && window.innerWidth <= 960) {
      this.sidebarOpenMobile.update(current => !current);
    } else {
      this.sidebarCollapsed.update(current => !current);
    }
  }

  protected closeSidebar(): void {
    this.showNotifications.set(false);
    if (typeof window !== 'undefined' && window.innerWidth <= 960) {
      this.sidebarOpenMobile.set(false);
    }
  }

  protected logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  private buildNavigation() {
    const allNavs: NavItem[] = [
      { label: 'Dashboard', icon: 'dashboard', primeIcon: 'pi pi-th-large', qaColor: 'qa-blue', route: '/app/dashboard', exact: true },
      { label: 'Patients', icon: 'groups', primeIcon: 'pi pi-users', qaColor: 'qa-green', route: '/app/patients' },
      { label: 'Appointments', icon: 'calendar_month', primeIcon: 'pi pi-calendar-plus', qaColor: 'qa-purple', route: '/app/appointments', exact: true },
      { label: 'Calendar', icon: 'calendar_view_month', primeIcon: 'pi pi-calendar', qaColor: 'qa-orange', route: '/app/appointments/calendar' },
      { label: 'Doctors', icon: 'medical_services', primeIcon: 'pi pi-id-card', qaColor: 'qa-teal', route: '/app/doctors' }
    ];

    if (this.authService.isAdmin()) {
      this.primaryNav = allNavs;
    } else if (this.authService.isDoctor()) {
      this.primaryNav = allNavs.filter(n => n.label !== 'Doctors');
    } else if (this.authService.isReceptionist()) {
      this.primaryNav = allNavs.filter(n => n.label !== 'Doctors');
    } else {
      this.primaryNav = allNavs; // fallback
    }
  }

  private updateTitle(): void {
    let currentSnapshot = this.router.routerState.snapshot.root;

    while (currentSnapshot.firstChild) {
      currentSnapshot = currentSnapshot.firstChild;
    }

    this.pageTitle.set(currentSnapshot.data?.['title'] ?? 'Dashboard');
  }

  protected toggleNotifications(event: MouseEvent): void {
    event.stopPropagation();
    this.showNotifications.update(v => !v);
  }

  protected markAsRead(notification: SystemNotification, event: Event): void {
    event.stopPropagation();
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id);
    }
  }

  protected markAllAsRead(event: Event): void {
    event.stopPropagation();
    this.notificationService.markAllAsRead();
  }

  protected deleteNotification(id: number, event: Event): void {
    event.stopPropagation();
    this.notificationService.deleteNotification(id);
  }

  protected clearAll(event: Event): void {
    event.stopPropagation();
    this.notificationService.clearAll();
  }
}
