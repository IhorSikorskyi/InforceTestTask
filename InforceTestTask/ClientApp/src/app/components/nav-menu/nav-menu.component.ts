import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  standalone: false,
})
export class NavMenuComponent implements OnInit, OnDestroy {
  isAuthenticated = false;
  isExpanded = false;
  private authSub?: Subscription;

  constructor(private router: Router, private authService: AuthService) {}

  ngOnInit() {
    this.checkAuth();
    this.authSub = this.authService.authChanged.subscribe((isAuth) => {
      this.isAuthenticated = isAuth;
    });
  }

  ngOnDestroy() {
    this.authSub?.unsubscribe();
  }

  checkAuth() {
    this.isAuthenticated = !!localStorage.getItem('accessToken');
  }

  signOut() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    this.isAuthenticated = false;
    this.authService.notifyAuthChanged(false);
    const loginInput = document.getElementById(
      'loginInput'
    ) as HTMLInputElement;
    const passwordInput = document.getElementById(
      'passwordInput'
    ) as HTMLInputElement;
    if (loginInput) loginInput.value = '';
    if (passwordInput) passwordInput.value = '';
    const protectedRoutes = ['/profile', '/dashboard', '/admin'];
    if (protectedRoutes.includes(this.router.url)) {
      this.router.navigate(['/']);
    }
  }

  openSignInModal() {
    const modalElement = document.getElementById('registerModal');
    if (modalElement) {
      const modal = window.bootstrap.Modal.getOrCreateInstance(modalElement);
      modal.show();
    }
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}
