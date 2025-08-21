import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import {
  RegisterRequest,
  LoginRequest,
} from '../../models/requests/auth.request';
import * as bootstrap from 'bootstrap';

@Component({
  selector: 'app-sign-in',
  templateUrl: './sign-in.component.html',
  standalone: false,
})
export class SignInComponent {
  isLogin = false;
  isVisible = true;

  registerData: RegisterRequest = {
    userName: '',
    email: '',
    password: '',
    confirmPassword: '',
  };

  loginData: LoginRequest = {
    userName: '',
    password: '',
  };

  message: string = '';

  constructor(private authService: AuthService) {}

  showLogin(event: Event) {
    event.preventDefault();
    this.isLogin = true;
    this.message = '';
  }

  showRegister(event: Event) {
    event.preventDefault();
    this.isLogin = false;
    this.message = '';
  }

  onRegister(event: Event) {
    event.preventDefault();
    this.authService.register(this.registerData).subscribe({
      next: (res) => {
        localStorage.setItem('accessToken', res.accessToken);
        localStorage.setItem('refreshToken', res.refreshToken);
        localStorage.setItem('userName', this.registerData.userName);
        this.message = 'Registration successful!';
        this.isLogin = true;
        this.closeModal();
        this.authService.notifyAuthChanged(true);
      },
      error: (err) => {
        this.message = err.error || 'Registration failed';
      },
    });
  }

  onLogin(event: Event) {
    event.preventDefault();
    this.authService.login(this.loginData).subscribe({
      next: (res) => {
        localStorage.setItem('accessToken', res.accessToken);
        localStorage.setItem('refreshToken', res.refreshToken);
        localStorage.setItem('userName', this.loginData.userName);
        this.message = 'Login successful!';
        this.isLogin = true;
        this.closeModal();
        this.authService.notifyAuthChanged(true);
      },
      error: (err) => {
        this.message = err.error || 'Login failed';
      },
    });
  }

  closeModal() {
    const modalElement = document.getElementById('registerModal');
    if (modalElement) {
      const modal = window.bootstrap.Modal.getOrCreateInstance(modalElement);
      modal.hide();
    }
    const backdrops = document.getElementsByClassName('modal-backdrop');
    while (backdrops.length > 0) {
      backdrops[0].parentNode?.removeChild(backdrops[0]);
    }
  }
}
