import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ClientService } from '../../../core/services/client.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Client } from '../../../shared/models/client.model';

@Component({
  selector: 'app-client-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="clients-page">
      <!-- Header -->
      <div class="page-header">
        <div>
          <h1>Corporate &amp; Individual Clients</h1>
          <p class="subtitle">Manage applicant corporate clients, legal assignees, and patent portfolios.</p>
        </div>
        <div class="header-actions" *ngIf="authService.canEdit()">
          <button type="button" class="btn btn-primary" (click)="openAddModal()">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <line x1="12" y1="5" x2="12" y2="19"/>
              <line x1="5" y1="12" x2="19" y2="12"/>
            </svg>
            Add New Client
          </button>
        </div>
      </div>

      <!-- Search Card -->
      <div class="card filter-card">
        <div class="search-input-wrapper">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="search-icon">
            <circle cx="11" cy="11" r="8"/>
            <line x1="21" y1="21" x2="16.65" y2="16.65"/>
          </svg>
          <input
            type="text"
            class="form-control with-icon"
            placeholder="Search clients by name, company, email, or phone..."
            [(ngModel)]="searchQuery"
            (keyup.enter)="loadClients()"
          />
          <button type="button" class="btn btn-primary btn-sm" (click)="loadClients()">Search</button>
          <button type="button" class="btn btn-secondary btn-sm" (click)="searchQuery = ''; loadClients()">Reset</button>
        </div>
      </div>

      <!-- Clients Table -->
      <div class="card table-card">
        <div *ngIf="isLoading" class="table-loading">
          <div class="spinner"></div>
          <span>Loading client directory...</span>
        </div>

        <div *ngIf="!isLoading && clients.length === 0" class="empty-state">
          <h3>No Clients Found</h3>
          <p>No client records match your search query.</p>
        </div>

        <div class="table-container" *ngIf="!isLoading && clients.length > 0">
          <table class="table">
            <thead>
              <tr>
                <th>Client Name</th>
                <th>Company</th>
                <th>Email</th>
                <th>Phone</th>
                <th>Patents Filed</th>
                <th style="text-align: right;">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let c of clients">
                <td>
                  <strong>{{ c.clientName }}</strong>
                  <div class="client-addr" *ngIf="c.address">{{ c.address }}</div>
                </td>
                <td>{{ c.company || 'Individual' }}</td>
                <td>{{ c.email }}</td>
                <td>{{ c.phone || '—' }}</td>
                <td>
                  <span class="badge badge-filed">{{ c.patentsCount }} Patents</span>
                </td>
                <td style="text-align: right;">
                  <div class="action-buttons">
                    <button
                      *ngIf="authService.canEdit()"
                      type="button"
                      class="btn btn-secondary btn-sm"
                      (click)="openEditModal(c)"
                    >
                      Edit
                    </button>
                    <button
                      *ngIf="authService.isAdmin()"
                      type="button"
                      class="btn btn-danger btn-sm"
                      (click)="confirmDelete(c)"
                    >
                      Delete
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Add / Edit Modal -->
      <div class="modal-backdrop" *ngIf="showModal">
        <div class="modal-content">
          <div class="card-header">
            <span class="card-title">{{ isEditing ? 'Edit Client Details' : 'Register New Client' }}</span>
            <button type="button" class="btn-icon" (click)="showModal = false">&times;</button>
          </div>

          <form [formGroup]="clientForm" (ngSubmit)="saveClient()">
            <div class="form-group">
              <label class="form-label" for="clientName">
                Client / Organization Name <span class="required">*</span>
              </label>
              <input
                id="clientName"
                type="text"
                class="form-control"
                [class.is-invalid]="f['clientName'].touched && f['clientName'].invalid"
                formControlName="clientName"
                placeholder="e.g. Acme Corporation Pvt Ltd"
              />
              <div class="invalid-feedback" *ngIf="f['clientName'].touched && f['clientName'].errors?.['required']">
                Client name is required.
              </div>
            </div>

            <div class="form-row">
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
                  placeholder="e.g. legal@acmecorp.com"
                />
                <div class="invalid-feedback" *ngIf="f['email'].touched && f['email'].errors?.['required']">
                  Email is required.
                </div>
              </div>

              <div class="form-group">
                <label class="form-label" for="phone">Phone Number</label>
                <input id="phone" type="text" class="form-control" formControlName="phone" placeholder="+91 98765 43210" />
              </div>
            </div>

            <div class="form-group">
              <label class="form-label" for="company">Company / Legal Entity</label>
              <input id="company" type="text" class="form-control" formControlName="company" placeholder="e.g. Acme Corporation Limited" />
            </div>

            <div class="form-group">
              <label class="form-label" for="address">Registered Office Address</label>
              <textarea id="address" rows="2" class="form-control" formControlName="address" placeholder="Physical or legal notice address"></textarea>
            </div>

            <div style="display: flex; justify-content: flex-end; gap: 0.75rem; margin-top: 1.5rem;">
              <button type="button" class="btn btn-secondary" (click)="showModal = false">Cancel</button>
              <button type="submit" class="btn btn-primary" [disabled]="clientForm.invalid || isSaving">
                <span *ngIf="!isSaving">{{ isEditing ? 'Update Client' : 'Save Client' }}</span>
                <span *ngIf="isSaving">Saving...</span>
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- Delete Confirmation Modal -->
      <div class="modal-backdrop" *ngIf="clientToDelete">
        <div class="modal-content">
          <div class="card-header">
            <span class="card-title">Confirm Client Deletion</span>
            <button type="button" class="btn-icon" (click)="clientToDelete = null">&times;</button>
          </div>
          <p style="margin-bottom: 1.25rem;">
            Are you sure you want to delete client <strong>{{ clientToDelete.clientName }}</strong>?
            <span *ngIf="clientToDelete.patentsCount > 0" style="color: #dc2626; display: block; margin-top: 0.5rem; font-weight: 600;">
              Warning: This client has {{ clientToDelete.patentsCount }} associated patents and cannot be deleted until patents are reassigned or removed.
            </span>
          </p>
          <div style="display: flex; justify-content: flex-end; gap: 0.75rem;">
            <button type="button" class="btn btn-secondary" (click)="clientToDelete = null">Cancel</button>
            <button type="button" class="btn btn-danger" (click)="deleteClient()">Confirm Delete</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .clients-page {
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
      padding: 1rem;
    }
    .search-input-wrapper {
      position: relative;
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }
    .search-icon {
      position: absolute;
      left: 0.75rem;
      color: var(--slate-400);
    }
    .with-icon {
      padding-left: 2.25rem;
    }
    .client-addr {
      font-size: 0.75rem;
      color: var(--slate-500);
      margin-top: 0.15rem;
    }
    .action-buttons {
      display: inline-flex;
      gap: 0.35rem;
    }
    .table-loading, .empty-state {
      text-align: center;
      padding: 3rem 1rem;
      color: var(--slate-500);
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
  `]
})
export class ClientListComponent implements OnInit {
  private fb = inject(FormBuilder);
  private clientService = inject(ClientService);
  private notificationService = inject(NotificationService);
  authService = inject(AuthService);

  clients: Client[] = [];
  searchQuery = '';
  isLoading = true;

  // Modal
  showModal = false;
  isEditing = false;
  editingClientId = 0;
  isSaving = false;
  clientToDelete: Client | null = null;

  clientForm: FormGroup = this.fb.group({
    clientName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    phone: [''],
    company: [''],
    address: ['']
  });

  get f() {
    return this.clientForm.controls;
  }

  ngOnInit(): void {
    this.loadClients();
  }

  loadClients(): void {
    this.isLoading = true;
    this.clientService.getClients(this.searchQuery).subscribe({
      next: (res) => {
        if (res.data) this.clients = res.data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.notificationService.error('Failed to load client records.');
      }
    });
  }

  openAddModal(): void {
    this.isEditing = false;
    this.editingClientId = 0;
    this.clientForm.reset({ clientName: '', email: '', phone: '', company: '', address: '' });
    this.showModal = true;
  }

  openEditModal(client: Client): void {
    this.isEditing = true;
    this.editingClientId = client.clientId;
    this.clientForm.patchValue({
      clientName: client.clientName,
      email: client.email,
      phone: client.phone,
      company: client.company,
      address: client.address
    });
    this.showModal = true;
  }

  saveClient(): void {
    if (this.clientForm.invalid) {
      this.clientForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    const val = this.clientForm.value;

    if (this.isEditing) {
      this.clientService.updateClient(this.editingClientId, val).subscribe({
        next: () => {
          this.notificationService.success('Client updated successfully.');
          this.isSaving = false;
          this.showModal = false;
          this.loadClients();
        },
        error: (err) => {
          this.isSaving = false;
          const msg = err.error?.message || 'Failed to update client.';
          this.notificationService.error(msg);
        }
      });
    } else {
      this.clientService.createClient(val).subscribe({
        next: () => {
          this.notificationService.success('Client created successfully.');
          this.isSaving = false;
          this.showModal = false;
          this.loadClients();
        },
        error: (err) => {
          this.isSaving = false;
          const msg = err.error?.message || 'Failed to create client.';
          this.notificationService.error(msg);
        }
      });
    }
  }

  confirmDelete(client: Client): void {
    this.clientToDelete = client;
  }

  deleteClient(): void {
    if (!this.clientToDelete) return;
    const id = this.clientToDelete.clientId;
    this.clientService.deleteClient(id).subscribe({
      next: () => {
        this.notificationService.success('Client deleted.');
        this.clientToDelete = null;
        this.loadClients();
      },
      error: (err) => {
        const msg = err.error?.message || 'Cannot delete client.';
        this.notificationService.error(msg);
      }
    });
  }
}
