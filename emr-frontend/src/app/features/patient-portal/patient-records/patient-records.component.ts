import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TimelineModule } from 'primeng/timeline';
import { CardModule } from 'primeng/card';
import { PatientTimelineService, TimelineEventDto } from '../../../core/services/patient-timeline.service';
import { AuthService } from '../../../core/services/auth.service';
import { PrescriptionService } from '../../../core/services/prescription.service';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-patient-records',
  standalone: true,
  imports: [CommonModule, TimelineModule, CardModule],
  templateUrl: './patient-records.component.html',
  styleUrl: './patient-records.component.scss'
})
export class PatientRecordsComponent implements OnInit {
  private timelineService = inject(PatientTimelineService);
  private authService = inject(AuthService);
  private prescriptionService = inject(PrescriptionService);

  events: TimelineEventDto[] = [];
  loading = true;
  downloadingEventId: string | null = null; // Track which event is downloading

  ngOnInit() {
    this.fetchRealTimeline();
  }

  fetchRealTimeline() {
    const user = this.authService.getUser();
    if (user && user.userId) {
      const patientId = parseInt(user.userId, 10);
      this.timelineService.getTimeline(patientId).subscribe({
        next: (events) => {
          this.events = events;
          this.loading = false;
        },
        error: (err) => {
          console.error('Failed to fetch patient timeline records', err);
          this.loading = false;
        }
      });
    } else {
      this.loading = false;
    }
  }

  downloadPrescription(event: TimelineEventDto) {
    if (event.eventType === 'Prescription' && event.eventId.startsWith('RX_')) {
      const rxId = parseInt(event.eventId.replace('RX_', ''), 10);
      this.downloadingEventId = event.eventId;
      
      this.prescriptionService.getById(rxId).subscribe({
        next: (fullPrescription) => {
          this.generatePrescriptionPDF(fullPrescription, event);
          this.downloadingEventId = null;
        },
        error: (err) => {
          console.error('Failed to fetch prescription details', err);
          alert('Could not download prescription at this time.');
          this.downloadingEventId = null;
        }
      });
    } else {
      // For Lab Reports or other documents without a specific file endpoint currently
      alert('This document type is not available for download yet.');
    }
  }

  private generatePrescriptionPDF(rxDetails: any, event: TimelineEventDto) {
    const doc = new jsPDF();
    const user = this.authService.getUser();
    
    // 1. Header Section
    doc.setFontSize(22);
    doc.setTextColor(17, 157, 164); // EMR Primary color
    doc.text('EMR NextGen Medical Center', 14, 20);
    
    doc.setFontSize(11);
    doc.setTextColor(100, 100, 100);
    doc.text('123 Health Avenue, Medical District, NY 10001', 14, 28);
    doc.text('Phone: (555) 123-4567 | Email: info@emrnextgen.com', 14, 34);
    
    // Divider line
    doc.setLineWidth(0.5);
    doc.setDrawColor(200, 200, 200);
    doc.line(14, 40, 196, 40);

    // 2. Patient & Consultation Info
    doc.setFontSize(14);
    doc.setTextColor(50, 50, 50);
    doc.text('Patient & Consultation Info', 14, 52);
    
    doc.setFontSize(11);
    doc.setTextColor(80, 80, 80);
    doc.text(`Patient Name: ${user?.fullName || 'Unknown'}`, 14, 60);
    doc.text(`Prescription Date: ${new Date(event.eventDate).toLocaleDateString()}`, 14, 66);
    
    if (rxDetails.chiefComplaints) doc.text(`Chief Complaints: ${rxDetails.chiefComplaints}`, 14, 72);
    if (rxDetails.diagnosis) doc.text(`Diagnosis: ${rxDetails.diagnosis}`, 14, 78);
    if (rxDetails.vitals) doc.text(`Vitals: ${rxDetails.vitals}`, 14, 84);

    let startY = 95;

    // 3. Medications Table
    doc.setFontSize(14);
    doc.setTextColor(30, 30, 30);
    doc.text('Rx - Medications Prescribed', 14, startY);
    
    const tableData = (rxDetails.medications || []).map((m: any, index: number) => [
      index + 1,
      m.medicineName,
      m.strength || '-',
      m.dosage || '-',
      m.frequency || '-',
      m.duration || '-',
      m.instructions || '-'
    ]);

    autoTable(doc, {
      startY: startY + 5,
      head: [['#', 'Medicine', 'Strength', 'Dosage', 'Freq', 'Duration', 'Instructions']],
      body: tableData,
      theme: 'grid',
      headStyles: { fillColor: [17, 157, 164] },
      styles: { fontSize: 9 }
    });

    // 4. Investigations & Follow-up
    let finalY = (doc as any).lastAutoTable.finalY + 15;
    
    if (rxDetails.investigationsOrdered) {
      doc.setFontSize(12);
      doc.setTextColor(50, 50, 50);
      doc.text('Investigations Ordered:', 14, finalY);
      doc.setFontSize(10);
      doc.setTextColor(80, 80, 80);
      const splitInv = doc.splitTextToSize(rxDetails.investigationsOrdered, 180);
      doc.text(splitInv, 14, finalY + 6);
      finalY += (splitInv.length * 5) + 10;
    }

    if (rxDetails.guidelines) {
      doc.setFontSize(12);
      doc.setTextColor(50, 50, 50);
      doc.text('Doctor Guidelines:', 14, finalY);
      doc.setFontSize(10);
      doc.setTextColor(80, 80, 80);
      const splitGuide = doc.splitTextToSize(rxDetails.guidelines, 180);
      doc.text(splitGuide, 14, finalY + 6);
      finalY += (splitGuide.length * 5) + 10;
    }

    if (rxDetails.nextFollowUpDate) {
      doc.setFontSize(11);
      doc.setTextColor(17, 157, 164);
      doc.text(`Next Follow-up Date: ${new Date(rxDetails.nextFollowUpDate).toLocaleDateString()}`, 14, finalY + 5);
    }

    // 5. Footer
    doc.setFontSize(9);
    doc.setTextColor(150, 150, 150);
    doc.text('This is a computer-generated E-Prescription and does not require a physical signature.', 14, 280);

    // Trigger download
    const filename = `Prescription_${new Date(event.eventDate).toISOString().split('T')[0]}.pdf`;
    doc.save(filename);
  }
}
