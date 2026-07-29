import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TimelineModule } from 'primeng/timeline';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { PatientTimelineService, TimelineEventDto } from '../../../core/services/patient-timeline.service';

@Component({
  selector: 'app-patient-timeline',
  standalone: true,
  imports: [CommonModule, TimelineModule, CardModule, ButtonModule],
  templateUrl: './patient-timeline.component.html',
  styleUrls: ['./patient-timeline.component.scss']
})
export class PatientTimelineComponent implements OnInit {
  @Input({ required: true }) patientId!: number;

  events: TimelineEventDto[] = [];
  isLoading = true;
  private timelineService = inject(PatientTimelineService);

  ngOnInit() {
    this.loadTimeline();
  }

  loadTimeline() {
    this.isLoading = true;
    this.timelineService.getTimeline(this.patientId).subscribe({
      next: (data) => {
        this.events = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load timeline', err);
        this.isLoading = false;
      }
    });
  }
}
