import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PublicBookingService, PublicDoctorDto } from '../../../core/services/public-booking.service';

@Component({
  selector: 'app-landing-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './landing-page.component.html',
  styleUrl: './landing-page.component.scss'
})
export class LandingPageComponent implements OnInit {
  private bookingService = inject(PublicBookingService);

  doctors = signal<PublicDoctorDto[]>([]);
  isLoadingDoctors = signal(true);

  departments = [
    { name: 'Cardiology', icon: 'pi-heart-fill', desc: 'Expert care for your heart with modern diagnostics.' },
    { name: 'Neurology', icon: 'pi-bolt', desc: 'Advanced neurological treatments and therapies.' },
    { name: 'Pediatrics', icon: 'pi-star-fill', desc: 'Compassionate healthcare for infants and children.' },
    { name: 'Orthopedics', icon: 'pi-compass', desc: 'Comprehensive care for bones, joints, and muscles.' },
  ];

  ngOnInit(): void {
    this.bookingService.getActiveDoctors().subscribe({
      next: (docs) => {
        // Show up to 4 doctors on the landing page
        this.doctors.set(docs.slice(0, 4));
        this.isLoadingDoctors.set(false);
      },
      error: (err) => {
        console.error('Failed to load doctors for landing page', err);
        this.isLoadingDoctors.set(false);
      }
    });
  }
}
