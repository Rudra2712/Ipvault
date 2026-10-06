import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ToastContainerComponent } from '../../../shared/components/toast-container/toast-container.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ToastContainerComponent],
  template: `
    <div class="login-wrapper">
      <div class="login-card">
        <!-- Logo & Header -->
        <div class="login-header">
          <div class="brand-logo-large">
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
              <path d="m9 12 2 2 4-4"/>
            </svg>
          </div>
          <h1>IPVault Portal</h1>
          <p class="subtitle">Intellectual Property & Patent Management System</p>
        </div>

        <!-- Login Form -->
        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="login-form">
          <div class="form-group">
            <label class="form-label" for="email">
              Email Address <span class="required">*</span>
            </label>
            <input
              id="email"
              type="email"
              class="form-control"
              [class.is-invalid]="f['email'].touched && f['email'].invalid"
              formControlName="email"
              placeholder="e.g. admin@ipvault.com"
            />
            <div class="invalid-feedback" *ngIf="f['email'].touched && f['email'].errors?.['required']">
              Email is required.
            </div>
            <div class="invalid-feedback" *ngIf="f['email'].touched && f['email'].errors?.['email']">
              Please enter a valid email address.
            </div>
          </div>

          <div class="form-group">
            <label class="form-label" for="password">
              Password <span class="required">*</span>
            </label>
            <input
              id="password"
              type="password"
              class="form-control"
              [class.is-invalid]="f['password'].touched && f['password'].invalid"
              formControlName="password"
              placeholder="Enter your password"
            />
            <div class="invalid-feedback" *ngIf="f['password'].touched && f['password'].errors?.['required']">
              Password is required.
            </div>
          </div>

          <button
            type="submit"
            class="btn btn-primary btn-block"
            [disabled]="loginForm.invalid || isLoading"
          >
            <span *ngIf="!isLoading">Sign In to Workspace</span>
            <span *ngIf="isLoading">Signing in...</span>
          </button>
        </form>

        <!-- Quick Demo Credentials Picker -->
        <div class="demo-accounts">
          <div class="demo-title">Quick Demo Login:</div>
          <div class="demo-buttons">
            <button
              type="button"
              class="btn-demo demo-admin"
              (click)="fillCredentials('admin@ipvault.com', 'Admin@123')"
            >
              <strong>Admin</strong>
              <small>Full Access</small>
            </button>
            <button
              type="button"
              class="btn-demo demo-attorney"
              (click)="fillCredentials('attorney@ipvault.com', 'Attorney@123')"
            >
              <strong>Attorney</strong>
              <small>Create / Edit</small>
            </button>
            <button
              type="button"
              class="btn-demo demo-staff"
              (click)="fillCredentials('staff@ipvault.com', 'Staff@123')"
            >
              <strong>Staff</strong>
              <small>Read Only</small>
            </button>
          </div>
        </div>

        <div class="login-footer">
          <span>Enterprise Secure Authentication &bull; ASP.NET Core &amp; EF Core</span>
        </div>
      </div>

      <app-toast-container></app-toast-container>
    </div>
  `,
  styles: [`
    .login-wrapper {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: radial-gradient(circle at top, #1e293b 0%, #0f172a 100%);
      padding: 1.5rem;
    }
    .login-card {
      background: var(--white);
      border-radius: var(--radius-xl);
      box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
      width: 100%;
      max-width: 440px;
      padding: 2.5rem;
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
      animation: fadeIn 0.4s ease-out;
    }
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(15px); }
      to { opacity: 1; transform: translateY(0); }
    }
    .login-header {
      text-align: center;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.5rem;
    }
    .brand-logo-large {
      width: 56px;
      height: 56px;
      border-radius: var(--radius-lg);
      background: var(--color-accent-gradient);
      color: var(--white);
      display: flex;
      align-items: center;
      justify-content: center;
      box-shadow: 0 8px 20px rgba(59, 130, 246, 0.4);
      margin-bottom: 0.5rem;
    }
    .login-header h1 {
      font-size: 1.625rem;
      color: var(--slate-900);
      margin-bottom: 0.1rem;
    }
    .subtitle {
      font-size: 0.85rem;
      color: var(--slate-500);
    }
    .btn-block {
      width: 100%;
      padding: 0.75rem;
      font-size: 0.95rem;
      margin-top: 0.5rem;
    }
    .demo-accounts {
      background-color: var(--slate-50);
      border: 1px dashed var(--slate-300);
      border-radius: var(--radius-md);
      padding: 1rem;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
    .demo-title {
      font-size: 0.75rem;
      font-weight: 700;
      color: var(--slate-500);
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .demo-buttons {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 0.5rem;
    }
    .btn-demo {
      padding: 0.5rem;
      border: 1px solid var(--slate-200);
      border-radius: var(--radius-sm);
      background-color: var(--white);
      text-align: center;
      cursor: pointer;
      display: flex;
      flex-direction: column;
      gap: 0.15rem;
      transition: all var(--transition-fast);
    }
    .btn-demo:hover {
      border-color: var(--color-primary);
      background-color: var(--color-primary-light);
    }
    .btn-demo strong {
      font-size: 0.8rem;
      color: var(--slate-800);
    }
    .btn-demo small {
      font-size: 0.675rem;
      color: var(--slate-500);
    }
    .login-footer {
      text-align: center;
      font-size: 0.75rem;
      color: var(--slate-400);
    }
  `]
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  isLoading = false;

  loginForm: FormGroup = this.fb.group({
    email: ['admin@ipvault.com', [Validators.required, Validators.email]],
    password: ['Admin@123', [Validators.required]]
  });

  get f() {
    return this.loginForm.controls;
  }

  fillCredentials(email: string, pass: string): void {
    this.loginForm.patchValue({ email, password: pass });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.authService.login(this.loginForm.value).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.notificationService.success(`Welcome back, ${res.data.user.fullName}!`);
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err) => {
        this.isLoading = false;
        const msg = err.error?.message || 'Login failed. Please verify your email and password.';
        this.notificationService.error(msg);
      }
    });
  }
}
