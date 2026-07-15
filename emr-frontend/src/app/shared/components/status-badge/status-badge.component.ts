import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `<span class="badge" [ngClass]="badgeClass">{{ status }}</span>`,
  styles: [`
    .badge { padding: 4px 12px; border-radius: 6px; font-size: 12px; font-weight: 500; color: white; }
    .pending { background-color: #FFA726; }
    .confirmed { background-color: #42A5F5; }
    .completed { background-color: #66BB6A; }
    .cancelled { background-color: #EF5350; }
  `]
})
export class StatusBadgeComponent {
  @Input() status: string = '';

  get badgeClass(): string {
    return this.status?.toLowerCase() || '';
  }
}