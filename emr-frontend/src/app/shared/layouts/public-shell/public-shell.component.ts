import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'app-public-shell',
  standalone: true,
  imports: [CommonModule, RouterModule, MenuModule],
  templateUrl: './public-shell.component.html',
  styleUrl: './public-shell.component.scss'
})
export class PublicShellComponent {
  private router = inject(Router);
  currentYear = new Date().getFullYear();

  loginMenuItems: MenuItem[] = [
    {
      label: 'Patient Portal',
      icon: 'pi pi-shield',
      styleClass: 'fw-bold text-primary',
      command: () => this.router.navigate(['/patient-login'])
    },
    {
      label: 'Staff Login',
      icon: 'pi pi-user',
      styleClass: 'fw-bold',
      command: () => this.router.navigate(['/login'])
    }
  ];
}
