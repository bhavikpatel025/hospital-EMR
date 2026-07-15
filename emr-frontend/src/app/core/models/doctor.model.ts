export interface DoctorListDto {
  doctorId: number;
  fullName: string;
  email: string;
  specialization: string;
  consultationFee: number;
  isActive: boolean;
}

export interface DoctorDetailDto {
  doctorId: number;
  fullName: string;
  email: string;
  specialization: string;
  qualification?: string;
  consultationFee: number;
  experienceYears: number;
  isActive: boolean;
  createdAt: string;
}

export interface DoctorCreateDto {
  fullName: string;
  email: string;
  password: string;
  specialization: string;
  qualification?: string;
  consultationFee: number;
  experienceYears: number;
}

export interface DoctorUpdateDto {
  doctorId: number;
  fullName: string;
  specialization: string;
  qualification?: string;
  consultationFee: number;
  experienceYears: number;
}

export interface DoctorQueryParams {
  searchTerm?: string;
  specialization?: string;
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDescending?: boolean;
}