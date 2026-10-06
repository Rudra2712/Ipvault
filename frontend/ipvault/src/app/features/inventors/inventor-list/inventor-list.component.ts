import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventorService } from '../../../core/services/inventor.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Inventor } from '../../../shared/models/inventor.model';

@Component({
  selector: 'app-inventor-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="inventors-page">
      <!-- Header -->
      <div class="page-header">
        <div>
          <h1>Registered Inventors &amp; Researchers</h1>
          <p class="subtitle">Directory of research scientists, inventors, and academic contributors.</p>
        </div>
        <div class="header-actions" *ngIf="authService.canEdit()">
          <button type="button" class="btn btn-primary" (click)="openAddModal()">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <line x1="12" y1="5" x2="12" y2="19"/>
              <line x1="5" y1="12" x2="19" y2="12"/>
            </svg>
            Add New Inventor
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
            placeholder="Search inventors by name, organization, or email..."
            [(ngModel)]="searchQuery"
            (keyup.enter)="loadInventors()"
          />
          <button type="button" class="btn btn-primary btn-sm" (click)="loadInventors()">Search</button>
          <button type="button" class="btn btn-secondary btn-sm" (click)="searchQuery = ''; loadInventors()">Reset</button>
        </div>
      </div>

      <!-- Inventors Table -->
      <div class="card table-card">
        <div *ngIf="isLoading" class="table-loading">
          <div class="spinner"></div>
          <span>Loading inventors...</span>
        </div>

        <div *ngIf="!isLoading && inventors.length === 0" class="empty-state">
          <h3>No Inventors Found</h3>
          <p>No inventors match your search criteria.</p>
        </div>

        <div class="table-container" *ngIf="!isLoading && inventors.length > 0">
          <table class="table">
            <thead>
              <tr>
                <th>Inventor Name</th>
                <th>Organization / Lab</th>
                <th>Email</th>
                <th>Phone</th>
                <th>Patents</th>
                <th style="text-align: right;">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let inv of inventors">
                <td>
                  <strong>{{ inv.fullName }}</strong>
                </td>
                <td>{{ inv.organization || 'Independent Researcher' }}</td>
                <td>{{ inv.email }}</td>
                <td>{{ inv.phone || '—' }}</td>
                <td>
                  <span class="badge badge-published">{{ inv.patentsCount }} Patents</span>
                </td>
                <td style="text-align: right;">
                  <div class="action-buttons">
                    <button
                      *ngIf="authService.canEdit()"
                      type="button"
                      class="btn btn-secondary btn-sm"
                      (click)="openEditModal(inv)"
                    >
                      Edit
                    </button>
                    <button
                      *ngIf="authService.isAdmin()"
                      type="button"
                      class="btn btn-danger btn-sm"
                      (click)="confirmDelete(inv)"
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
            <span class="card-title">{{ isEditing ? 'Edit Inventor Details' : 'Register New Inventor' }}</span>
            <button type="button" class="btn-icon" (click)="showModal = false">&times;</button>
          </div>

          <form [formGroup]="inventorForm" (ngSubmit)="saveInventor()">
            <div class="form-row">
              <div class="form-group">
                <label class="form-label" for="firstName">
                  First Name <span class="required">*</span>
                </label>
                <input
                  id="firstName"
                  type="text"
                  class="form-control"
                  [class.is-invalid]="f['firstName'].touched && f['firstName'].invalid"
                  formControlName="firstName"
                  placeholder="e.g. Dr. Rajesh"
                />
                <div class="invalid-feedback" *ngIf="f['firstName'].touched && f['firstName'].errors?.['required']">
                  First name is required.
                </div>
              </div>

              <div class="form-group">
                <label class="form-label" for="lastName">
                  Last Name <span class="required">*</span>
                </label>
                <input
                  id="lastName"
                  type="text"
                  class="form-control"
                  [class.is-invalid]="f['lastName'].touched && f['lastName'].invalid"
                  formControlName="lastName"
                  placeholder="e.g. Kulkarni"
                />
                <div class="invalid-feedback" *ngIf="f['lastName'].touched && f['lastName'].errors?.['required']">
                  Last name is required.
                </div>
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
                  placeholder="e.g. rajesh.k@abctech.in"
                />
                <div class="invalid-feedback" *ngIf="f['email'].touched && f['email'].errors?.['required']">
                  Email is required.
                </div>
              </div>

              <div class="form-group">
                <label class="form-label" for="phone">Phone Number</label>
                <input id="phone" type="text" class="form-control" formControlName="phone" placeholder="+91 98231 00001" />
              </div>
            </div>

            <div class="form-group">
              <label class="form-label" for="organization">Research Organization / Affiliate Lab</label>
              <input id="organization" type="text" class="form-control" formControlName="organization" placeholder="e.g. ABC Technologies AI Labs" />
            </div>

            <div style="display: flex; justify-content: flex-end; gap: 0.75rem; margin-top: 1.5rem;">
              <button type="button" class="btn btn-secondary" (click)="showModal = false">Cancel</button>
              <button type="submit" class="btn btn-primary" [disabled]="inventorForm.invalid || isSaving">
                <span *ngIf="!isSaving">{{ isEditing ? 'Update Inventor' : 'Save Inventor' }}</span>
                <span *ngIf="isSaving">Saving...</span>
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- Delete Confirmation Modal -->
      <div class="modal-backdrop" *ngIf="inventorToDelete">
        <div class="modal-content">
          <div class="card-header">
            <span class="card-title">Confirm Inventor Deletion</span>
            <button type="button" class="btn-icon" (click)="inventorToDelete = null">&times;</button>
          </div>
          <p style="margin-bottom: 1.25rem;">
            Are you sure you want to delete inventor <strong>{{ inventorToDelete.fullName }}</strong>?
          </p>
          <div style="display: flex; justify-content: flex-end; gap: 0.75rem;">
            <button type="button" class="btn btn-secondary" (click)="inventorToDelete = null">Cancel</button>
            <button type="button" class="btn btn-danger" (click)="deleteInventor()">Confirm Delete</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .inventors-page {
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
export class InventorListComponent implements OnInit {
  private fb = inject(FormBuilder);
  private inventorService = inject(InventorService);
  private notificationService = inject(NotificationService);
  authService = inject(AuthService);

  inventors: Inventor[] = [];
  searchQuery = '';
  isLoading = true;

  // Modal
  showModal = false;
  isEditing = false;
  editingInventorId = 0;
  isSaving = false;
  inventorToDelete: Inventor | null = null;

  inventorForm: FormGroup = this.fb.group({
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    phone: [''],
    organization: ['']
  });

  get f() {
    return this.inventorForm.controls;
  }

  ngOnInit(): void {
    this.loadInventors();
  }

  loadInventors(): void {
    this.isLoading = true;
    this.inventorService.getInventors(this.searchQuery).subscribe({
      next: (res) => {
        if (res.data) this.inventors = res.data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.notificationService.error('Failed to load inventor records.');
      }
    });
  }

  openAddModal(): void {
    this.isEditing = false;
    this.editingInventorId = 0;
    this.inventorForm.reset({ firstName: '', lastName: '', email: '', phone: '', organization: '' });
    this.showModal = true;
  }

  openEditModal(inv: Inventor): void {
    this.isEditing = true;
    this.editingInventorId = inv.inventorId;
    this.inventorForm.patchValue({
      firstName: inv.firstName,
      lastName: inv.lastName,
      email: inv.email,
      phone: inv.phone,
      organization: inv.organization
    });
    this.showModal = true;
  }

  saveInventor(): void {
    if (this.inventorForm.invalid) {
      this.inventorForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    const val = this.inventorForm.value;

    if (this.isEditing) {
      this.inventorService.updateInventor(this.editingInventorId, val).subscribe({
        next: () => {
          this.notificationService.success('Inventor updated successfully.');
          this.isSaving = false;
          this.showModal = false;
          this.loadInventors();
        },
        error: (err) => {
          this.isSaving = false;
          const msg = err.error?.message || 'Failed to update inventor.';
          this.notificationService.error(msg);
        }
      });
    } else {
      this.inventorService.createInventor(val).subscribe({
        next: () => {
          this.notificationService.success('Inventor created successfully.');
          this.isSaving = false;
          this.showModal = false;
          this.loadInventors();
        },
        error: (err) => {
          this.isSaving = false;
          const msg = err.error?.message || 'Failed to create inventor.';
          this.notificationService.error(msg);
        }
      });
    }
  }

  confirmDelete(inv: Inventor): void {
    this.inventorToDelete = inv;
  }

  deleteInventor(): void {
    if (!this.inventorToDelete) return;
    const id = this.inventorToDelete.inventorId;
    this.inventorService.deleteInventor(id).subscribe({
      next: () => {
        this.notificationService.success('Inventor deleted.');
        this.inventorToDelete = null;
        this.loadInventors();
      },
      error: (err) => {
        const msg = err.error?.message || 'Cannot delete inventor.';
        this.notificationService.error(msg);
      }
    });
  }
}
