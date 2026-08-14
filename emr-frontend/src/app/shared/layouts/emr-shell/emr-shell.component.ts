import { CommonModule } from '@angular/common';
import { Component, DestroyRef, ElementRef, HostListener, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
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
  imports: [CommonModule, FormsModule, RouterOutlet, RouterLink, RouterLinkActive, MatIconModule],
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
  protected showUserMenu = signal(false);

  // Change Password Modal State
  protected showPasswordModal = signal(false);
  protected changePasswordLoading = signal(false);
  protected passwordError = signal<string | null>(null);
  protected passwordSuccess = signal<string | null>(null);

  protected currentPassword = '';
  protected newPassword = '';
  protected confirmNewPassword = '';

  protected showCurrentPassword = false;
  protected showNewPassword = false;
  protected showConfirmPassword = false;

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
        this.showUserMenu.set(false);
      });

    this.notificationService.startConnection();

    this.destroyRef.onDestroy(() => {
      this.notificationService.stopConnection();
    });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const targetElement = event.target as HTMLElement;

    if (this.showNotifications()) {
      const notificationContainer = this.elementRef.nativeElement.querySelector('.notification-container');
      if (notificationContainer && !notificationContainer.contains(targetElement)) {
        this.showNotifications.set(false);
      }
    }

    if (this.showUserMenu()) {
      const userContainer = this.elementRef.nativeElement.querySelector('.user-menu-container');
      if (userContainer && !userContainer.contains(targetElement)) {
        this.showUserMenu.set(false);
      }
    }
  }

  protected toggleSidebar(): void {
    this.showNotifications.set(false);
    this.showUserMenu.set(false);
    if (typeof window !== 'undefined' && window.innerWidth <= 960) {
      this.sidebarOpenMobile.update(current => !current);
    } else {
      this.sidebarCollapsed.update(current => !current);
    }
  }

  protected closeSidebar(): void {
    this.showNotifications.set(false);
    this.showUserMenu.set(false);
    if (typeof window !== 'undefined' && window.innerWidth <= 960) {
      this.sidebarOpenMobile.set(false);
    }
  }

  protected toggleUserMenu(event: MouseEvent): void {
    event.stopPropagation();
    this.showUserMenu.update(v => !v);
  }

  protected openChangePasswordModal(): void {
    this.currentPassword = '';
    this.newPassword = '';
    this.confirmNewPassword = '';
    this.passwordError.set(null);
    this.passwordSuccess.set(null);
    this.showCurrentPassword = false;
    this.showNewPassword = false;
    this.showConfirmPassword = false;
    this.showUserMenu.set(false);
    this.showPasswordModal.set(true);
  }

  protected closeChangePasswordModal(): void {
    if (this.changePasswordLoading()) return;
    this.showPasswordModal.set(false);
  }

  protected submitChangePassword(): void {
    this.passwordError.set(null);
    this.passwordSuccess.set(null);

    const curr = this.currentPassword.trim();
    const next = this.newPassword.trim();
    const conf = this.confirmNewPassword.trim();

    if (!curr) {
      this.passwordError.set('Please enter your current password.');
      return;
    }
    if (!next) {
      this.passwordError.set('Please enter a new password.');
      return;
    }
    if (next.length < 6) {
      this.passwordError.set('New password must be at least 6 characters long.');
      return;
    }
    if (next === curr) {
      this.passwordError.set('New password cannot be the same as your current password.');
      return;
    }
    if (next !== conf) {
      this.passwordError.set('New password and confirmation password do not match.');
      return;
    }

    this.changePasswordLoading.set(true);
    this.authService.changePassword({
      currentPassword: curr,
      newPassword: next,
      confirmNewPassword: conf
    }).subscribe({
      next: (res) => {
        this.changePasswordLoading.set(false);
        this.passwordSuccess.set(res.message || 'Password changed successfully!');
        setTimeout(() => {
          this.closeChangePasswordModal();
        }, 1500);
      },
      error: (err) => {
        this.changePasswordLoading.set(false);
        this.passwordError.set(err.error?.message || 'Failed to change password. Please verify current password.');
      }
    });
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
