export type AppointmentStatus = 'Pending' | 'Confirmed' | 'Completed' | 'Cancelled';

export interface AppointmentListDto {
  appointmentId: number;
  patientId: number;
  patientName: string;
  patientMobile: string;
  doctorId: number;
  doctorName: string;
  specialization: string;
  appointmentDate: string;
  startTime: string;   // "10:00:00"
  endTime: string;
  status: AppointmentStatus;
  reason?: string;
}

export interface AppointmentDetailDto extends AppointmentListDto {
  notes?: string;
  createdAt: string;
}

export interface AppointmentCreateDto {
  patientId: number;
  doctorId: number;
  appointmentDate: string;
  startTime: string;
  endTime: string;
  reason?: string;
  notes?: string;
}

export interface AppointmentUpdateDto extends AppointmentCreateDto {
  appointmentId: number;
}

export interface AppointmentStatusUpdateDto {
  appointmentId: number;
  status: number;   // enum index: 0=Pending, 1=Confirmed, 2=Completed, 3=Cancelled
}

export interface AppointmentQueryParams {
  searchTerm?: string;
  doctorId?: number;
  status?: number;
  fromDate?: string;
  toDate?: string;
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface CalendarEventDto {
  appointmentId: number;
  title: string;
  start: string;
  end: string;
  status: AppointmentStatus;
  color: string;
}

export interface AppointmentRescheduleDto {
  appointmentId: number;
  newDate: string;
  newStartTime: string;
  newEndTime: string;
}