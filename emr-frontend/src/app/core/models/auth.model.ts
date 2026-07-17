export interface LoginRequest {
  email: string;
  password: string;
}

export interface UserDto {
  userId: number;
  fullName: string;
  email: string;
  role: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  refreshToken?: string;
  refreshTokenExpiresAt?: string;
  user: UserDto;
}