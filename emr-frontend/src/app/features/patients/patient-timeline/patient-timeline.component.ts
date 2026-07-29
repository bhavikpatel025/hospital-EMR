import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccordionModule } from 'primeng/accordion';
import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TooltipModule } from 'primeng/tooltip';
import { PatientTimelineService, TimelineEventDto } from '../../../core/services/patient-timeline.service';

@Component({
  selector: 'app-patient-timeline',
  standalone: true,
  imports: [CommonModule, AccordionModule, BadgeModule, ButtonModule, DialogModule, TooltipModule],
  templateUrl: './patient-timeline.component.html',
  styleUrls: ['./patient-timeline.component.scss']
})
export class PatientTimelineComponent implements OnInit {
  @Input({ required: true }) patientId!: number;

  events: TimelineEventDto[] = [];
  groupedEvents: { date: string, events: TimelineEventDto[] }[] = [];
  isLoading = true;
  showInfoModal = false;
  selectedEventInfo = '';
  selectedEventTitle = '';

  private timelineService = inject(PatientTimelineService);

  ngOnInit() {
    this.loadTimeline();
  }

  loadTimeline() {
    this.isLoading = true;
    this.timelineService.getTimeline(this.patientId).subscribe({
      next: (data) => {
        this.events = data;
        this.groupEventsByDate();
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load timeline', err);
        this.isLoading = false;
      }
    });
  }

  private groupEventsByDate() {
    const groups = this.events.reduce((acc, event) => {
      // Group by exact date (without time)
      const dateStr = new Date(event.eventDate).toDateString(); 
      if (!acc[dateStr]) acc[dateStr] = [];
      acc[dateStr].push(event);
      return acc;
    }, {} as Record<string, TimelineEventDto[]>);
    
    this.groupedEvents = Object.keys(groups).map(date => ({
      date: date,
      events: groups[date]
    }));
  }

  openInfo(event: TimelineEventDto) {
    if (event.additionalInfo) {
      this.selectedEventTitle = event.title;
      this.selectedEventInfo = event.additionalInfo;
      this.showInfoModal = true;
    }
  }
}
