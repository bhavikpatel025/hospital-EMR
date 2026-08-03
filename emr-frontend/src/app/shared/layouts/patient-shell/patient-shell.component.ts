import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { AvatarModule } from 'primeng/avatar';

@Component({
  selector: 'app-patient-shell',
  standalone: true,
  imports: [CommonModule, RouterModule, MenuModule, AvatarModule],
  templateUrl: './patient-shell.component.html',
  styleUrl: './patient-shell.component.scss'
})
export class PatientShellComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);

  patientName = this.authService.getUser()?.fullName || 'Patient';
  isMenuOpen = false;
  
  userMenuItems: MenuItem[] = [];

  ngOnInit() {
    this.userMenuItems = [
      {
        label: 'Logout',
        icon: 'pi pi-sign-out',
        styleClass: 'text-danger fw-bold',
        command: () => this.logout()
      }
    ];
  }

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/patient-login']);
  }
}
