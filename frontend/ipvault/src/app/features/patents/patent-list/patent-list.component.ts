import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { PatentService } from '../../../core/services/patent.service';
import { ClientService } from '../../../core/services/client.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { PatentFilter, PatentListItem, PatentStatus } from '../../../shared/models/patent.model';
import { Client } from '../../../shared/models/client.model';

@Component({
  selector: 'app-patent-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, PaginationComponent],
  template: `
    <div class="patents-page">
      <!-- Page Header -->
      <div class="page-header">
        <div>
          <h1>Patents Portfolio</h1>
          <p class="subtitle">Search, filter, and track intellectual property filings and legal status.</p>
        </div>
        <div class="header-actions" *ngIf="authService.canEdit()">
          <a routerLink="/patents/new" class="btn btn-primary">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <line x1="12" y1="5" x2="12" y2="19"/>
              <line x1="5" y1="12" x2="19" y2="12"/>
            </svg>
            Add New Patent
          </a>
        </div>
      </div>

      <!-- Filters Card -->
      <div class="card filter-card">
        <div class="filter-grid">
          <!-- Search Input -->
          <div class="filter-item search-box">
            <label class="form-label">Search</label>
            <div class="search-input-wrapper">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="search-icon">
                <circle cx="11" cy="11" r="8"/>
                <line x1="21" y1="21" x2="16.65" y2="16.65"/>
              </svg>
              <input
                type="text"
                class="form-control with-icon"
                placeholder="Search by patent #, title, or abstract..."
                [(ngModel)]="filter.search"
                (keyup.enter)="applyFilter()"
              />
            </div>
          </div>

          <!-- Status Dropdown -->
          <div class="filter-item">
            <label class="form-label">Status</label>
            <select class="form-control" [(ngModel)]="filter.status" (change)="applyFilter()">
              <option value="">All Statuses</option>
              <option *ngFor="let s of statuses" [value]="s.statusName">{{ s.statusName }}</option>
            </select>
          </div>

          <!-- Client Dropdown -->
          <div class="filter-item">
            <label class="form-label">Client</label>
            <select class="form-control" [(ngModel)]="filter.clientId" (change)="applyFilter()">
              <option [ngValue]="undefined">All Clients</option>
              <option *ngFor="let c of clients" [ngValue]="c.clientId">{{ c.clientName }}</option>
            </select>
          </div>

          <!-- Filter Actions -->
          <div class="filter-actions">
            <button type="button" class="btn btn-primary btn-sm" (click)="applyFilter()">
              Apply Filters
            </button>
            <button type="button" class="btn btn-secondary btn-sm" (click)="resetFilter()">
              Reset
            </button>
          </div>
        </div>
      </div>

      <!-- Patents Table -->
      <div class="card table-card">
        <div *ngIf="isLoading" class="table-loading">
          <div class="spinner"></div>
          <span>Loading patents...</span>
        </div>

        <div *ngIf="!isLoading && patents.length === 0" class="empty-state">
          <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
            <path d="M14.5 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7.5L14.5 2z"/>
            <polyline points="14 2 14 8 20 8"/>
          </svg>
          <h3>No Patents Found</h3>
          <p>Try adjusting your search criteria or add a new patent record.</p>
        </div>

        <div class="table-container" *ngIf="!isLoading && patents.length > 0">
          <table class="table">
            <thead>
              <tr>
                <th>Patent Number</th>
                <th>Title</th>
                <th>Client</th>
                <th>Primary Inventor</th>
                <th>Status</th>
                <th>Filing Date</th>
                <th>Expiry Date</th>
                <th style="text-align: right;">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let p of patents">
                <td>
                  <a [routerLink]="['/patents', p.patentId]" class="patent-num-link">
                    {{ p.patentNumber }}
                  </a>
                </td>
                <td>
                  <div class="patent-title" [title]="p.title">{{ p.title }}</div>
                </td>
                <td>{{ p.clientName }}</td>
                <td>{{ p.primaryInventorName }}</td>
                <td>
                  <span class="badge" [ngClass]="getBadgeClass(p.statusName)">
                    {{ p.statusName }}
                  </span>
                </td>
                <td>{{ p.filingDate | date:'mediumDate' }}</td>
                <td>
                  <span *ngIf="p.expiryDate">{{ p.expiryDate | date:'mediumDate' }}</span>
                  <span *ngIf="!p.expiryDate" class="text-muted">—</span>
                </td>
                <td style="text-align: right;">
                  <div class="action-buttons">
                    <a [routerLink]="['/patents', p.patentId]" class="btn btn-secondary btn-sm" title="View details">
                      View
                    </a>
                    <a
                      *ngIf="authService.canEdit()"
                      [routerLink]="['/patents', p.patentId, 'edit']"
                      class="btn btn-secondary btn-sm"
                      title="Edit patent"
                    >
                      Edit
                    </a>
                    <button
                      *ngIf="authService.isAdmin()"
                      type="button"
                      class="btn btn-danger btn-sm"
                      (click)="confirmDelete(p)"
                      title="Delete patent"
                    >
                      Delete
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <app-pagination
          *ngIf="!isLoading && totalItems > 0"
          [currentPage]="filter.page || 1"
          [pageSize]="filter.pageSize || 10"
          [totalItems]="totalItems"
          [totalPages]="totalPages"
          (pageChange)="onPageChange($event)"
        ></app-pagination>
      </div>

      <!-- Delete Confirmation Modal -->
      <div class="modal-backdrop" *ngIf="patentToDelete">
        <div class="modal-content">
          <div class="card-header">
            <span class="card-title">Confirm Patent Deletion</span>
            <button type="button" class="btn-icon" (click)="patentToDelete = null">&times;</button>
          </div>
          <p style="margin-bottom: 1.25rem;">
            Are you sure you want to delete patent <strong>{{ patentToDelete.patentNumber }}</strong> ({{ patentToDelete.title }})? This action cannot be undone.
          </p>
          <div style="display: flex; justify-content: flex-end; gap: 0.75rem;">
            <button type="button" class="btn btn-secondary" (click)="patentToDelete = null">Cancel</button>
            <button type="button" class="btn btn-danger" (click)="deletePatent()">Confirm Delete</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .patents-page {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
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
    .filter-card {
      padding: 1.25rem;
    }
    .filter-grid {
      display: grid;
      grid-template-columns: 2fr 1fr 1.5fr auto;
      align-items: flex-end;
      gap: 1rem;
    }
    .search-input-wrapper {
      position: relative;
      display: flex;
      align-items: center;
    }
    .search-icon {
      position: absolute;
      left: 0.75rem;
      color: var(--slate-400);
    }
    .with-icon {
      padding-left: 2.25rem;
    }
    .filter-actions {
      display: flex;
      gap: 0.5rem;
    }
    .patent-num-link {
      font-weight: 700;
      color: var(--color-primary);
    }
    .patent-title {
      max-width: 280px;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      font-weight: 500;
    }
    .action-buttons {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
    }
    .table-loading, .empty-state {
      text-align: center;
      padding: 3.5rem 1rem;
      color: var(--slate-500);
    }
    .empty-state svg {
      color: var(--slate-300);
      margin-bottom: 0.75rem;
    }
    .spinner {
      width: 32px;
      height: 32px;
      border: 3px solid var(--slate-200);
      border-top-color: var(--color-primary);
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
      margin: 0 auto 0.75rem auto;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
    .text-muted {
      color: var(--slate-400);
    }
    @media (max-width: 900px) {
      .filter-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class PatentListComponent implements OnInit {
  private patentService = inject(PatentService);
  private clientService = inject(ClientService);
  private notificationService = inject(NotificationService);
  authService = inject(AuthService);

  patents: PatentListItem[] = [];
  statuses: PatentStatus[] = [];
  clients: Client[] = [];

  filter: PatentFilter = {
    search: '',
    status: '',
    clientId: undefined,
    page: 1,
    pageSize: 10
  };

  totalItems = 0;
  totalPages = 1;
  isLoading = true;
  patentToDelete: PatentListItem | null = null;

  ngOnInit(): void {
    this.loadMetadata();
    this.loadPatents();
  }

  loadMetadata(): void {
    this.patentService.getStatuses().subscribe(res => {
      if (res.data) this.statuses = res.data;
    });
    this.clientService.getClients().subscribe(res => {
      if (res.data) this.clients = res.data;
    });
  }

  loadPatents(): void {
    this.isLoading = true;
    this.patentService.getPatents(this.filter).subscribe({
      next: (res) => {
        if (res.data) {
          this.patents = res.data.items;
          this.totalItems = res.data.totalItems;
          this.totalPages = res.data.totalPages;
        }
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.notificationService.error('Failed to load patent records.');
      }
    });
  }

  applyFilter(): void {
    this.filter.page = 1;
    this.loadPatents();
  }

  resetFilter(): void {
    this.filter = {
      search: '',
      status: '',
      clientId: undefined,
      page: 1,
      pageSize: 10
    };
    this.loadPatents();
  }

  onPageChange(page: number): void {
    this.filter.page = page;
    this.loadPatents();
  }

  confirmDelete(patent: PatentListItem): void {
    this.patentToDelete = patent;
  }

  deletePatent(): void {
    if (!this.patentToDelete) return;
    const id = this.patentToDelete.patentId;
    this.patentService.deletePatent(id).subscribe({
      next: () => {
        this.notificationService.success('Patent deleted successfully.');
        this.patentToDelete = null;
        this.loadPatents();
      },
      error: (err) => {
        const msg = err.error?.message || 'Failed to delete patent.';
        this.notificationService.error(msg);
      }
    });
  }

  getBadgeClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'granted': return 'badge-granted';
      case 'under examination': return 'badge-examination';
      case 'filed': return 'badge-filed';
      case 'published': return 'badge-published';
      case 'draft': return 'badge-draft';
      case 'rejected': return 'badge-rejected';
      case 'expired': return 'badge-expired';
      case 'abandoned': return 'badge-abandoned';
      default: return 'badge-draft';
    }
  }
}
