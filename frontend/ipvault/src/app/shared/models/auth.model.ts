export interface User {
  userId: number;
  fullName: string;
  email: string;
  role: 'Admin' | 'Attorney' | 'Staff';
  createdAt: string;
}

export interface AuthResponse {
  token: string;
  user: User;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  role: string;
}
