export interface RegisterRequest {
  userName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface LoginRequest {
  userName: string;
  password: string;
}

export interface UserIdRequest {
  userName: string;
}
