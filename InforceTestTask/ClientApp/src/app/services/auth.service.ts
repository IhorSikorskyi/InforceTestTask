import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import {
  RegisterRequest,
  LoginRequest,
  UserIdRequest,
} from '../models/requests/auth.request';
import {
  AuthResponse,
  UserIdRoleResponse,
} from '../models/responses/auth.response';

export interface JwtPayload {
  userId: number;
  role: string;
  [key: string]: any;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private baseUrl = 'https://localhost:7033/api/auth';

  authChanged = new Subject<boolean>();

  constructor(private http: HttpClient) {}

  register(request: RegisterRequest): Observable<any> {
    return this.http.post(`${this.baseUrl}/register`, request).pipe(
      tap((response: any) => {
        localStorage.setItem('accessToken', response.accessToken);
        localStorage.setItem('refreshToken', response.refreshToken);
      })
    );
  }

  login(request: LoginRequest): Observable<any> {
    return this.http.post(`${this.baseUrl}/login`, request).pipe(
      tap((response: any) => {
        localStorage.setItem('accessToken', response.accessToken);
        localStorage.setItem('refreshToken', response.refreshToken);
      })
    );
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = localStorage.getItem('refreshToken');
    const userId = localStorage.getItem('userId');
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/refresh-token`, {
        id: userId,
        refreshToken,
      })
      .pipe(
        tap((response) => {
          localStorage.setItem('accessToken', response.accessToken);
          localStorage.setItem('refreshToken', response.refreshToken);
        })
      );
  }

  notifyAuthChanged(isAuth: boolean) {
    this.authChanged.next(isAuth);
  }

  isLoggedIn(): boolean {
    const token = localStorage.getItem('accessToken');
    if (!token) {
      return false;
    }
    return true;
  }

  getUserRole(): string | null {
    const token = localStorage.getItem('accessToken');
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return (
        payload.role ||
        payload[
          'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
        ] ||
        null
      );
    } catch {
      return null;
    }
  }

  getUserIdFromToken(): number | null {
    const token = localStorage.getItem('accessToken');
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return (
        payload.userId ||
        payload.sub ||
        payload.id ||
        payload.nameid ||
        payload[
          'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'
        ] ||
        null
      );
    } catch {
      return null;
    }
  }
}
