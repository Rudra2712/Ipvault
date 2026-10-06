import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DashboardService } from '../../core/services/dashboard.service';
import { AuthService } from '../../core/services/auth.service';
import { DashboardSummary } from '../../shared/models/dashboard.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="dashboard-page">
      <!-- Header banner -->
      <div class="page-header">
        <div>
          <h1>Executive IP Dashboard</h1>
          <p class="subtitle">Overview of patent filings, prosecution statuses, deadlines, and corporate portfolios.</p>
        </div>
        <div class="header-actions" *ngIf="authService.canEdit()">
          <a routerLink="/patents/new" class="btn btn-primary">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <line x1="12" y1="5" x2="12" y2="19"/>
              <line x1="5" y1="12" x2="19" y2="12"/>
            </svg>
            File New Patent
          </a>
        </div>
      </div>

      <!-- Loading State -->
      <div *ngIf="isLoading" class="loading-state">
        <div class="spinner"></div>
        <p>Loading analytics and patent metrics...</p>
      </div>

      <!-- Dashboard Data Grid -->
      <ng-container *ngIf="!isLoading && summary">
        <!-- 6 KPI Metric Cards -->
        <div class="kpi-grid">
          <div class="kpi-card">
            <div class="kpi-icon icon-blue">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M14.5 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7.5L14.5 2z"/>
                <polyline points="14 2 14 8 20 8"/>
              </svg>
            </div>
            <div class="kpi-info">
              <span class="kpi-label">Total Patents</span>
              <span class="kpi-value">{{ summary.totalPatents }}</span>
            </div>
          </div>

          <div class="kpi-card">
            <div class="kpi-icon icon-green">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                <polyline points="22 4 12 14.01 9 11.01"/>
              </svg>
            </div>
            <div class="kpi-info">
              <span class="kpi-label">Granted Patents</span>
              <span class="kpi-value text-green">{{ summary.grantedPatents }}</span>
            </div>
          </div>

          <div class="kpi-card">
            <div class="kpi-icon icon-amber">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="12" cy="12" r="10"/>
                <polyline points="12 6 12 12 16 14"/>
              </svg>
            </div>
            <div class="kpi-info">
              <span class="kpi-label">Pending / In-Review</span>
              <span class="kpi-value text-amber">{{ summary.pendingPatents }}</span>
            </div>
          </div>

          <div class="kpi-card">
            <div class="kpi-icon icon-gray">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="12" cy="12" r="10"/>
                <line x1="4.93" y1="4.93" x2="19.07" y2="19.07"/>
              </svg>
            </div>
            <div class="kpi-info">
              <span class="kpi-label">Expired Patents</span>
              <span class="kpi-value text-gray">{{ summary.expiredPatents }}</span>
            </div>
          </div>

          <div class="kpi-card">
            <div class="kpi-icon icon-indigo">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/>
                <circle cx="9" cy="7" r="4"/>
              </svg>
            </div>
            <div class="kpi-info">
              <span class="kpi-label">Corporate Clients</span>
              <span class="kpi-value">{{ summary.totalClients }}</span>
            </div>
          </div>

          <div class="kpi-card">
            <div class="kpi-icon icon-purple">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M15 14c.2-1 .7-1.7 1.5-2.5 1-.9 1.5-2.2 1.5-3.5A6 6 0 0 0 6 8c0 1 .2 2.2 1.5 3.5.7.7 1.3 1.5 1.5 2.5"/>
                <path d="M9 18h6"/>
                <path d="M10 22h4"/>
              </svg>
            </div>
            <div class="kpi-info">
              <span class="kpi-label">Active Inventors</span>
              <span class="kpi-value">{{ summary.totalInventors }}</span>
            </div>
          </div>
        </div>

        <!-- Middle Charts Section -->
        <div class="analytics-grid">
          <!-- Status Distribution -->
          <div class="card">
            <div class="card-header">
              <span class="card-title">Patents by Prosecution Status</span>
            </div>
            <div class="status-bars">
              <div *ngFor="let item of summary.patentCountByStatus" class="status-bar-item">
                <div class="bar-header">
                  <span class="status-name">{{ item.statusName }}</span>
                  <span class="status-count"><strong>{{ item.count }}</strong> ({{ getPercentage(item.count, summary.totalPatents) }}%)</span>
                </div>
                <div class="progress-track">
                  <div
                    class="progress-fill"
                    [ngClass]="getStatusFillClass(item.statusName)"
                    [style.width.%]="getPercentage(item.count, summary.totalPatents)"
                  ></div>
                </div>
              </div>
            </div>
          </div>

          <!-- Filing Year Activity -->
          <div class="card">
            <div class="card-header">
              <span class="card-title">Patents Filed by Year</span>
            </div>
            <div class="years-chart">
              <div *ngFor="let y of summary.patentsFiledPerYear" class="year-bar-col">
                <div class="year-count">{{ y.count }}</div>
                <div
                  class="year-bar-pillar"
                  [style.height.px]="getMaxYearHeight(y.count)"
                ></div>
                <div class="year-label">{{ y.year }}</div>
              </div>
            </div>
          </div>
        </div>

        <!-- Lower Section: Client Distribution & Upcoming Expiries -->
        <div class="bottom-grid">
          <!-- Client Distribution -->
          <div class="card">
            <div class="card-header">
              <span class="card-title">Top Client Portfolios</span>
              <a routerLink="/clients" class="btn btn-secondary btn-sm">View All Clients</a>
            </div>
            <div class="client-list-preview">
              <div *ngFor="let c of summary.patentCountByClient" class="client-row">
                <div class="client-info">
                  <span class="client-name">{{ c.clientName }}</span>
                </div>
                <div class="client-patents-count">
                  <span class="badge badge-filed">{{ c.count }} Patents</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Upcoming Expiries & Deadlines -->
          <div class="card">
            <div class="card-header">
              <span class="card-title">Patent Expiry &amp; Renewal Watch</span>
              <span class="badge badge-examination">Active Alerts</span>
            </div>
            <div *ngIf="summary.upcomingExpiries.length === 0" class="empty-notice">
              No patents expiring in the near term.
            </div>
            <div class="table-container" *ngIf="summary.upcomingExpiries.length > 0">
              <table class="table">
                <thead>
                  <tr>
                    <th>Patent #</th>
                    <th>Title</th>
                    <th>Expiry Date</th>
                    <th>Client</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let exp of summary.upcomingExpiries">
                    <td>
                      <a [routerLink]="['/patents', exp.patentId]" class="patent-link">
                        {{ exp.patentNumber }}
                      </a>
                    </td>
                    <td class="text-truncate" style="max-width: 200px;">{{ exp.title }}</td>
                    <td>{{ exp.expiryDate | date:'mediumDate' }}</td>
                    <td>{{ exp.clientName }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </ng-container>
    </div>
  `,
  styles: [`
    .dashboard-page {
      display: flex;
      flex-direction: column;
      gap: 1.75rem;
    }
    .page-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      flex-wrap: wrap;
      gap: 1rem;
    }
    .subtitle {
      font-size: 0.9rem;
      color: var(--slate-500);
      margin-top: 0.2rem;
    }
    .kpi-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(190px, 1fr));
      gap: 1.25rem;
    }
    .kpi-card {
      background-color: var(--white);
      border: 1px solid var(--slate-200);
      border-radius: var(--radius-lg);
      padding: 1.25rem;
      display: flex;
      align-items: center;
      gap: 1rem;
      box-shadow: var(--shadow-sm);
      transition: transform var(--transition-fast), box-shadow var(--transition-fast);
    }
    .kpi-card:hover {
      transform: translateY(-2px);
      box-shadow: var(--shadow-md);
    }
    .kpi-icon {
      width: 48px;
      height: 48px;
      border-radius: var(--radius-md);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }
    .icon-blue { background: #eff6ff; color: #2563eb; }
    .icon-green { background: #ecfdf5; color: #059669; }
    .icon-amber { background: #fffbeb; color: #d97706; }
    .icon-gray { background: #f1f5f9; color: #64748b; }
    .icon-indigo { background: #e0e7ff; color: #4338ca; }
    .icon-purple { background: #f5f3ff; color: #7c3aed; }
    .kpi-info {
      display: flex;
      flex-direction: column;
    }
    .kpi-label {
      font-size: 0.8rem;
      font-weight: 600;
      color: var(--slate-500);
      text-transform: uppercase;
      letter-spacing: 0.03em;
    }
    .kpi-value {
      font-size: 1.75rem;
      font-weight: 800;
      color: var(--slate-900);
      font-family: var(--font-heading);
      line-height: 1.1;
    }
    .text-green { color: #059669; }
    .text-amber { color: #d97706; }
    .text-gray { color: #64748b; }

    .analytics-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(420px, 1fr));
      gap: 1.5rem;
    }
    .status-bars {
      display: flex;
      flex-direction: column;
      gap: 0.875rem;
    }
    .status-bar-item {
      display: flex;
      flex-direction: column;
      gap: 0.35rem;
    }
    .bar-header {
      display: flex;
      justify-content: space-between;
      font-size: 0.85rem;
      color: var(--slate-700);
    }
    .progress-track {
      height: 8px;
      background-color: var(--slate-100);
      border-radius: var(--radius-full);
      overflow: hidden;
    }
    .progress-fill {
      height: 100%;
      border-radius: var(--radius-full);
      transition: width 0.6s ease;
    }
    .fill-granted { background-color: #10b981; }
    .fill-examination { background-color: #f59e0b; }
    .fill-filed { background-color: #3b82f6; }
    .fill-published { background-color: #8b5cf6; }
    .fill-draft { background-color: #94a3b8; }
    .fill-rejected { background-color: #ef4444; }
    .fill-expired { background-color: #64748b; }
    .fill-default { background-color: #3b82f6; }

    .years-chart {
      display: flex;
      align-items: flex-end;
      justify-content: space-around;
      gap: 0.75rem;
      height: 180px;
      padding-top: 1.5rem;
    }
    .year-bar-col {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.35rem;
      flex: 1;
    }
    .year-count {
      font-size: 0.75rem;
      font-weight: 700;
      color: var(--slate-700);
    }
    .year-bar-pillar {
      width: 100%;
      max-width: 38px;
      background: var(--color-accent-gradient);
      border-radius: var(--radius-sm) var(--radius-sm) 0 0;
      min-height: 8px;
      transition: height 0.5s ease;
    }
    .year-label {
      font-size: 0.75rem;
      font-weight: 600;
      color: var(--slate-500);
    }

    .bottom-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(420px, 1fr));
      gap: 1.5rem;
    }
    .client-list-preview {
      display: flex;
      flex-direction: column;
      gap: 0.6rem;
    }
    .client-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0.5rem 0.75rem;
      border-radius: var(--radius-md);
      background-color: var(--slate-50);
      font-size: 0.875rem;
    }
    .client-name {
      font-weight: 600;
      color: var(--slate-800);
    }
    .patent-link {
      font-weight: 700;
      color: var(--color-primary);
    }
    .loading-state {
      text-align: center;
      padding: 4rem 1rem;
      color: var(--slate-500);
    }
    .spinner {
      width: 36px;
      height: 36px;
      border: 3px solid var(--slate-200);
      border-top-color: var(--color-primary);
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
      margin: 0 auto 1rem auto;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
    .text-truncate {
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
  `]
})
export class DashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  authService = inject(AuthService);

  summary: DashboardSummary | null = null;
  isLoading = true;

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.isLoading = true;
    this.dashboardService.getSummary().subscribe({
      next: (res) => {
        this.summary = res.data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  getPercentage(count: number, total: number): number {
    if (!total || total === 0) return 0;
    return Math.round((count / total) * 100);
  }

  getStatusFillClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'granted': return 'fill-granted';
      case 'under examination': return 'fill-examination';
      case 'filed': return 'fill-filed';
      case 'published': return 'fill-published';
      case 'draft': return 'fill-draft';
      case 'rejected': return 'fill-rejected';
      case 'expired': return 'fill-expired';
      default: return 'fill-default';
    }
  }

  getMaxYearHeight(count: number): number {
    if (!this.summary?.patentsFiledPerYear?.length) return 20;
    const max = Math.max(...this.summary.patentsFiledPerYear.map(y => y.count));
    const maxHeight = 120;
    return max > 0 ? Math.max(12, Math.round((count / max) * maxHeight)) : 20;
  }
}
