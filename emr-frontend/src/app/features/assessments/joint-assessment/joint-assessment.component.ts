import { Component, ElementRef, OnInit, ViewChild, computed, effect, inject, signal, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { DropdownModule } from 'primeng/dropdown';
import { ButtonModule } from 'primeng/button';
import { InputTextarea } from 'primeng/inputtextarea';
import { TagModule } from 'primeng/tag';
import { CalendarModule } from 'primeng/calendar';
import { JointAssessmentService, JointAssessmentDto } from '../../../core/services/joint-assessment.service';
import { MessageService } from 'primeng/api';

// Defined Joint states
export type JointState = 'Normal' | 'Tender' | 'Swollen' | 'TenderSwollen' | 'Limited';

export interface JointOption {
  label: string;
  value: JointState;
  color: string;
}

export const JOINT_STATES: JointOption[] = [
  { label: 'Normal', value: 'Normal', color: '#10b981' }, // Green
  { label: 'Tender', value: 'Tender', color: '#ef4444' }, // Red
  { label: 'Swollen', value: 'Swollen', color: '#3b82f6' }, // Blue
  { label: 'Tender & Swollen', value: 'TenderSwollen', color: '#8b5cf6' }, // Purple
  { label: 'Limited Movement', value: 'Limited', color: '#f59e0b' } // Orange
];

@Component({
  selector: 'app-joint-assessment',
  standalone: true,
  imports: [CommonModule, FormsModule, DropdownModule, ButtonModule, InputTextarea, TagModule, CalendarModule],
  templateUrl: './joint-assessment.component.html',
  styleUrls: ['./joint-assessment.component.scss']
})
export class JointAssessmentComponent implements OnInit {
  private http = inject(HttpClient);
  private sanitizer = inject(DomSanitizer);
  private assessmentService = inject(JointAssessmentService);
  private messageService = inject(MessageService);

  @ViewChild('svgWrapper', { static: false }) svgWrapper!: ElementRef;

  @Input() patientId!: number;
  @Input() appointmentId?: number;

  svgContent: SafeHtml = '';
  jointOptions = JOINT_STATES;
  notes: string = '';

  // Key is joint ID, value is state
  jointsState = signal<Record<string, JointState>>({});

  // Hardcoded list of joints for the sidebar (based on standard counts)
  jointList = [
    'RightShoulder', 'LeftShoulder',
    'RightElbow', 'LeftElbow',
    'RightWrist', 'LeftWrist',
    'RightMCP', 'LeftMCP',
    'RightPIP', 'LeftPIP',
    'RightDIP', 'LeftDIP',
    'RightHip', 'LeftHip',
    'RightKnee', 'LeftKnee',
    'RightAnkle', 'LeftAnkle',
    'RightMTP', 'LeftMTP'
  ];

  // Totals computed from the signal
  totalNormal = computed(() => Object.values(this.jointsState()).filter(s => s === 'Normal').length);
  totalTender = computed(() => Object.values(this.jointsState()).filter(s => s === 'Tender').length);
  totalSwollen = computed(() => Object.values(this.jointsState()).filter(s => s === 'Swollen').length);
  totalTenderSwollen = computed(() => Object.values(this.jointsState()).filter(s => s === 'TenderSwollen').length);
  totalLimited = computed(() => Object.values(this.jointsState()).filter(s => s === 'Limited').length);
  totalAssessed = computed(() => Object.keys(this.jointsState()).length);

  // History State
  assessmentHistory = signal<JointAssessmentDto[]>([]);
  selectedDate = signal<Date>(new Date());
  isReadOnly = computed(() => {
    const selected = this.selectedDate();
    if (!selected) return false;
    return selected.toDateString() !== new Date().toDateString();
  });

  // Precise coordinates for the overlay markers based on SVG native rings
  readonly JOINT_COORDS: Record<string, { top: string, left: string }> = {
    'RightShoulder': { top: '19%', left: '41%' },
    'LeftShoulder': { top: '19%', left: '58.5%' },
    'RightElbow': { top: '33.5%', left: '38%' },
    'LeftElbow': { top: '33.5%', left: '62%' },
    'RightWrist': { top: '45.5%', left: '35%' },
    'LeftWrist': { top: '45%', left: '64.5%' },
    'RightMCP': { top: '48.5%', left: '32.5%' },
    'LeftMCP': { top: '48.5%', left: '67.5%' },
    'RightPIP': { top: '51.5%', left: '31.5%' },
    'LeftPIP': { top: '51.5%', left: '68.5%' },
    'RightDIP': { top: '54.5%', left: '30.5%' },
    'LeftDIP': { top: '54.5%', left: '69.5%' },
    'RightHip': { top: '45.3%', left: '45%' },
    'LeftHip': { top: '45.3%', left: '55%' },
    'RightKnee': { top: '67.4%', left: '45%' },
    'LeftKnee': { top: '67.4%', left: '55%' },
    'RightAnkle': { top: '85.9%', left: '45%' },
    'LeftAnkle': { top: '85.9%', left: '54.8%' },
    'RightMTP': { top: '95%', left: '44%' },
    'LeftMTP': { top: '95%', left: '56%' }
  };

  constructor() {
  }

  ngOnInit(): void {
    this.loadAssessment();
  }

  loadAssessment(): void {
    // Load all historical assessments for this patient
    if (this.patientId) {
      this.assessmentService.getAssessmentsByPatient(this.patientId).subscribe({
        next: (assessments) => {
          if (assessments && assessments.length > 0) {
            // Sort by descending date
            assessments.sort((a, b) => new Date(b.assessmentDate).getTime() - new Date(a.assessmentDate).getTime());
            this.assessmentHistory.set(assessments);
          } else {
            this.assessmentHistory.set([]);
          }
          
          // Reset to today on initial load
          this.selectedDate.set(new Date());
          this.jointsState.set({});
          this.notes = '';
        },
        error: (err) => {
          console.error('Failed to load assessment history', err);
        }
      });
    }
  }

  hasAssessment(date: any): boolean {
    if (!date) return false;
    const d = new Date(date.year, date.month, date.day);
    return this.assessmentHistory().some(a => new Date(a.assessmentDate).toDateString() === d.toDateString());
  }

  onDateSelect(date: Date): void {
    this.selectedDate.set(date);
    const selectedStr = date.toDateString();

    const matching = this.assessmentHistory().find(a => new Date(a.assessmentDate).toDateString() === selectedStr);
    if (matching && matching.jointsDataJson) {
      try {
        const parsedState = JSON.parse(matching.jointsDataJson);
        this.jointsState.set(parsedState);
        this.notes = matching.notes || '';
      } catch (e) {
        console.error('Failed to parse joint assessment data', e);
      }
    } else {
      this.jointsState.set({});
      this.notes = '';
    }
  }

  getJointStyle(jointId: string): Record<string, string> {
    const pos = this.JOINT_COORDS[jointId] || { top: '0%', left: '0%' };
    const state = this.jointsState()[jointId];
    const color = this.jointOptions.find(o => o.value === state)?.color; 

    let bgColor = 'rgba(241, 245, 249, 0.6)'; // Very light slate tint
    let borderColor = '#64748b'; // Slate 500 for empty/not assessed (clearly visible)
    let opacity = '1';

    if (state === 'Normal') {
      bgColor = 'rgba(16, 185, 129, 0.2)'; // Light green tint
      borderColor = '#10b981'; // Solid green border
      opacity = '1';
    } else if (state) {
      bgColor = color as string;
      borderColor = color as string;
      opacity = '0.9';
    }

    return {
      'top': pos.top,
      'left': pos.left,
      'background-color': bgColor,
      'opacity': opacity,
      'border': `2px solid ${borderColor}`
    };
  }

  markAllNormal(): void {
    if (this.isReadOnly()) return;
    const newState: Record<string, JointState> = {};
    this.jointList.forEach(j => newState[j] = 'Normal');
    this.jointsState.set(newState);
  }

  clearAll(): void {
    if (this.isReadOnly()) return;
    this.jointsState.set({});
    this.notes = '';
  }

  cancelAssessment(): void {
    // Revert to the last saved state from the database
    this.loadAssessment();
    this.messageService.add({ severity: 'info', summary: 'Cancelled', detail: 'Unsaved changes have been discarded.' });
  }

  cycleJointState(jointId: string): void {
    if (this.isReadOnly()) return;
    const currentState = this.jointsState()[jointId];
    let nextState: JointState = 'Tender'; // Default if none
    
    if (currentState === 'Tender') nextState = 'Swollen';
    else if (currentState === 'Swollen') nextState = 'TenderSwollen';
    else if (currentState === 'TenderSwollen') nextState = 'Limited';
    else if (currentState === 'Limited') nextState = 'Normal';
    else if (currentState === 'Normal') nextState = 'Tender';

    this.updateJoint(jointId, nextState);
  }

  updateJoint(jointId: string, state: JointState): void {
    if (this.isReadOnly()) return;
    this.jointsState.update(current => ({ ...current, [jointId]: state }));
  }

  saveAssessment(): void {
    if (!this.patientId) {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Patient ID missing' });
      return;
    }

    const payload: JointAssessmentDto = {
      patientId: this.patientId,
      appointmentId: this.appointmentId,
      assessmentDate: new Date().toISOString(),
      jointsDataJson: JSON.stringify(this.jointsState()),
      notes: this.notes,
      totalTender: this.totalTender(),
      totalSwollen: this.totalSwollen(),
      totalBoth: this.totalTenderSwollen(),
      totalLimited: this.totalLimited(),
      totalNormal: this.totalNormal(),
      totalJointsAssessed: this.totalAssessed()
    };

    this.assessmentService.createAssessment(payload).subscribe({
      next: (savedAssessment) => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Assessment Saved' });
        // Reload history to include the newly saved assessment
        this.loadAssessment();
      },
      error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to save' })
    });
  }
}
