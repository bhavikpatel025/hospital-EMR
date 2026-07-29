import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { DropdownModule } from 'primeng/dropdown';
import { TableModule } from 'primeng/table';
import { MessageService } from 'primeng/api';
import { PrescriptionService, PrescriptionDto } from '../../../core/services/prescription.service';

@Component({
  selector: 'app-e-prescription',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    ButtonModule, InputTextModule, TextareaModule, DropdownModule, TableModule
  ],
  templateUrl: './e-prescription.component.html',
  styleUrls: ['./e-prescription.component.scss']
})
export class EPrescriptionComponent implements OnInit {
  @Input() patientId!: number;
  @Input() appointmentId?: number;

  todayDate = new Date();

  private fb = inject(FormBuilder);
  private messageService = inject(MessageService);
  private prescriptionService = inject(PrescriptionService);

  prescriptionForm!: FormGroup;
  isLoadingHistory = false;
  hasHistory = false;

  dosageOptions = [
    { label: '1-0-1', value: '1-0-1' },
    { label: '1-0-0', value: '1-0-0' },
    { label: '0-0-1', value: '0-0-1' },
    { label: '1-1-1', value: '1-1-1' },
    { label: '0-1-0', value: '0-1-0' },
    { label: '1/2-0-1/2', value: '1/2-0-1/2' }
  ];

  frequencyOptions = [
    { label: 'Daily', value: 'Daily' },
    { label: 'Weekly', value: 'Weekly' },
    { label: 'SOS', value: 'SOS' },
    { label: 'Stat (At once)', value: 'Stat' }
  ];

  instructionOptions = [
    { label: 'After food', value: 'After food' },
    { label: 'Before food', value: 'Before food' },
    { label: 'With food', value: 'With food' },
    { label: 'At bedtime', value: 'At bedtime' }
  ];

  durationOptions = [
    { label: '3 Days', value: '3 Days' },
    { label: '5 Days', value: '5 Days' },
    { label: '1 Week', value: '1 Week' },
    { label: '2 Weeks', value: '2 Weeks' },
    { label: '1 Month', value: '1 Month' },
    { label: 'Till next visit', value: 'Till next visit' }
  ];

  ngOnInit() {
    this.initForm();
    // Start with one empty medication row
    this.addMedication();
    this.checkHistory();
  }

  checkHistory() {
    this.prescriptionService.getByPatientId(this.patientId).subscribe({
      next: (history) => {
        if (history && history.length > 0) {
          this.hasHistory = true;
        }
      }
    });
  }

  loadPreviousPrescription() {
    this.isLoadingHistory = true;
    this.prescriptionService.getByPatientId(this.patientId).subscribe({
      next: (history) => {
        this.isLoadingHistory = false;
        if (history && history.length > 0) {
          const lastRx = history[0]; // Gets the most recent one
          
          this.prescriptionForm.patchValue({
            chiefComplaints: lastRx.chiefComplaints,
            diagnosis: lastRx.diagnosis,
            vitals: lastRx.vitals,
            investigationsOrdered: lastRx.investigationsOrdered,
            guidelines: lastRx.guidelines
          });

          // Clear and fill medications
          this.medications.clear();
          if (lastRx.medications && lastRx.medications.length > 0) {
            lastRx.medications.forEach(med => {
              this.medications.push(this.fb.group({
                medicineName: [med.medicineName, Validators.required],
                strength: [med.strength || ''],
                dosage: [med.dosage || '1-0-1'],
                frequency: [med.frequency || 'Daily'],
                instructions: [med.instructions || 'After food'],
                duration: [med.duration || '5 Days']
              }));
            });
          } else {
            this.addMedication();
          }

          this.messageService.add({ severity: 'success', summary: 'Loaded', detail: 'Previous prescription loaded successfully.' });
        } else {
          this.messageService.add({ severity: 'info', summary: 'No History', detail: 'No previous prescriptions found.' });
        }
      },
      error: (err) => {
        this.isLoadingHistory = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load history.' });
      }
    });
  }

  initForm() {
    this.prescriptionForm = this.fb.group({
      chiefComplaints: [''],
      diagnosis: [''],
      vitals: [''],
      investigationsOrdered: [''],
      guidelines: [''],
      nextFollowUpDate: [''],
      medications: this.fb.array([])
    });
  }

  get medications() {
    return this.prescriptionForm.get('medications') as FormArray;
  }

  addMedication() {
    const medGroup = this.fb.group({
      medicineName: ['', Validators.required],
      strength: [''],
      dosage: ['1-0-1'],
      frequency: ['Daily'],
      instructions: ['After food'],
      duration: ['5 Days']
    });
    this.medications.push(medGroup);
  }

  removeMedication(index: number) {
    this.medications.removeAt(index);
    if (this.medications.length === 0) {
      this.addMedication(); // Always keep at least one row
    }
  }

  savePrescription() {
    if (this.prescriptionForm.invalid) {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fill all required fields.' });
      return;
    }

    const formData = this.prescriptionForm.value;
    const payload: PrescriptionDto = {
      patientId: this.patientId,
      appointmentId: this.appointmentId,
      ...formData
    };

    console.log('Saving Prescription payload:', payload);
    
    this.prescriptionService.create(payload).subscribe({
      next: (res) => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Prescription Saved Successfully!' });
      },
      error: (err) => {
        console.error(err);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to save prescription.' });
      }
    });
  }

  printPrescription() {
    const originalTitle = document.title;
    const dateStr = new Date().toISOString().split('T')[0];
    
    // Set a professional filename for the PDF download
    document.title = `Rx_Patient_${this.patientId}_${dateStr}`;
    
    window.print();
    
    // Restore the original title so the browser tab name goes back to normal
    document.title = originalTitle;
  }
}
