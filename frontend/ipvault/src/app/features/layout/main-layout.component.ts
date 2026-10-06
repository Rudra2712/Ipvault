import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ToastContainerComponent } from '../../shared/components/toast-container/toast-container.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, ToastContainerComponent],
  template: `
    <div class="layout-wrapper">
      <!-- Top Navigation Bar -->
      <header class="top-nav">
        <div class="nav-brand">
          <div class="brand-logo">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
              <path d="m9 12 2 2 4-4"/>
            </svg>
          </div>
          <div class="brand-text">
            <span class="brand-title">IPVault</span>
            <span class="brand-tagline">Intellectual Property System</span>
          </div>
        </div>

        <div class="nav-user">
          <div class="user-meta" *ngIf="authService.currentUser() as user">
            <div class="user-avatar">{{ getInitials(user.fullName) }}</div>
            <div class="user-details">
              <span class="user-name">{{ user.fullName }}</span>
              <span class="user-role badge badge-role">{{ user.role }}</span>
            </div>
          </div>
          <button type="button" class="btn btn-secondary btn-sm" (click)="authService.logout()">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
              <polyline points="16 17 21 12 16 7"/>
              <line x1="21" y1="12" x2="9" y2="12"/>
            </svg>
            Sign Out
          </button>
        </div>
      </header>

      <!-- Main Layout with Sidebar -->
      <div class="layout-body">
        <aside class="sidebar">
          <nav class="sidebar-nav">
            <div class="nav-section-title">MAIN NAVIGATION</div>

            <a routerLink="/dashboard" routerLinkActive="active" class="nav-item">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <rect width="7" height="9" x="3" y="3" rx="1"/>
                <rect width="7" height="5" x="14" y="3" rx="1"/>
                <rect width="7" height="9" x="14" y="12" rx="1"/>
                <rect width="7" height="5" x="3" y="16" rx="1"/>
              </svg>
              <span>Dashboard</span>
            </a>

            <a routerLink="/patents" routerLinkActive="active" class="nav-item">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M14.5 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7.5L14.5 2z"/>
                <polyline points="14 2 14 8 20 8"/>
                <line x1="16" y1="13" x2="8" y2="13"/>
                <line x1="16" y1="17" x2="8" y2="17"/>
                <line x1="10" y1="9" x2="8" y2="9"/>
              </svg>
              <span>Patents</span>
            </a>

            <a routerLink="/clients" routerLinkActive="active" class="nav-item">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/>
                <circle cx="9" cy="7" r="4"/>
                <path d="M22 21v-2a4 4 0 0 0-3-3.87"/>
                <path d="M16 3.13a4 4 0 0 1 0 7.75"/>
              </svg>
              <span>Clients</span>
            </a>

            <a routerLink="/inventors" routerLinkActive="active" class="nav-item">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M15 14c.2-1 .7-1.7 1.5-2.5 1-.9 1.5-2.2 1.5-3.5A6 6 0 0 0 6 8c0 1 .2 2.2 1.5 3.5.7.7 1.3 1.5 1.5 2.5"/>
                <path d="M9 18h6"/>
                <path d="M10 22h4"/>
              </svg>
              <span>Inventors</span>
            </a>
          </nav>

          <div class="sidebar-footer">
            <div class="system-status">
              <span class="status-dot"></span>
              <span>API Connected (.NET 8)</span>
            </div>
          </div>
        </aside>

        <!-- Main Content Area -->
        <main class="main-content">
          <router-outlet></router-outlet>
        </main>
      </div>

      <!-- Toast notifications container -->
      <app-toast-container></app-toast-container>
    </div>
  `,
  styles: [`
    .layout-wrapper {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
      background-color: #f8fafc;
    }
    .top-nav {
      height: 64px;
      background-color: var(--white);
      border-bottom: 1px solid var(--slate-200);
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0 1.5rem;
      position: sticky;
      top: 0;
      z-index: 100;
      box-shadow: var(--shadow-xs);
    }
    .nav-brand {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }
    .brand-logo {
      width: 38px;
      height: 38px;
      border-radius: var(--radius-md);
      background: var(--color-accent-gradient);
      color: var(--white);
      display: flex;
      align-items: center;
      justify-content: center;
      box-shadow: 0 4px 10px rgba(59, 130, 246, 0.3);
    }
    .brand-text {
      display: flex;
      flex-direction: column;
    }
    .brand-title {
      font-family: var(--font-heading);
      font-weight: 800;
      font-size: 1.15rem;
      color: var(--slate-900);
      line-height: 1.1;
    }
    .brand-tagline {
      font-size: 0.725rem;
      color: var(--slate-500);
      font-weight: 500;
    }
    .nav-user {
      display: flex;
      align-items: center;
      gap: 1.25rem;
    }
    .user-meta {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }
    .user-avatar {
      width: 36px;
      height: 36px;
      border-radius: var(--radius-full);
      background: #e2e8f0;
      color: var(--slate-700);
      font-weight: 700;
      font-size: 0.875rem;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .user-details {
      display: flex;
      flex-direction: column;
      gap: 0.1rem;
    }
    .user-name {
      font-size: 0.875rem;
      font-weight: 600;
      color: var(--slate-800);
    }
    .user-role {
      align-self: flex-start;
      font-size: 0.7rem;
      padding: 0.1rem 0.45rem;
    }
    .layout-body {
      display: flex;
      flex: 1;
    }
    .sidebar {
      width: 240px;
      background-color: var(--white);
      border-right: 1px solid var(--slate-200);
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      padding: 1.25rem 0.75rem;
    }
    .nav-section-title {
      font-size: 0.7rem;
      font-weight: 700;
      color: var(--slate-400);
      letter-spacing: 0.08em;
      padding: 0.5rem 0.75rem;
      margin-bottom: 0.25rem;
    }
    .sidebar-nav {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }
    .nav-item {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.625rem 0.875rem;
      border-radius: var(--radius-md);
      color: var(--slate-600);
      font-weight: 600;
      font-size: 0.875rem;
      transition: all var(--transition-fast);
    }
    .nav-item:hover {
      background-color: var(--slate-50);
      color: var(--slate-900);
    }
    .nav-item.active {
      background: var(--color-primary-light);
      color: var(--color-primary-dark);
    }
    .sidebar-footer {
      padding: 0.75rem;
      border-top: 1px solid var(--slate-100);
    }
    .system-status {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.75rem;
      color: var(--slate-500);
    }
    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: var(--radius-full);
      background-color: #10b981;
    }
    .main-content {
      flex: 1;
      padding: 2rem;
      overflow-y: auto;
      max-width: 1400px;
      margin: 0 auto;
      width: 100%;
    }
    @media (max-width: 768px) {
      .sidebar {
        width: 64px;
        padding: 1rem 0.25rem;
      }
      .nav-item span, .nav-section-title, .brand-text, .user-details, .system-status span {
        display: none;
      }
      .main-content {
        padding: 1rem;
      }
    }
  `]
})
export class MainLayoutComponent {
  authService = inject(AuthService);

  getInitials(name: string): string {
    if (!name) return 'U';
    const parts = name.split(' ');
    if (parts.length > 1) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }
}
