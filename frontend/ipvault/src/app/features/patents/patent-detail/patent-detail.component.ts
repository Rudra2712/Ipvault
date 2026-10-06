import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PatentService } from '../../../core/services/patent.service';
import { InventorService } from '../../../core/services/inventor.service';
import { DocumentService } from '../../../core/services/document.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PatentDetail } from '../../../shared/models/patent.model';
import { Inventor } from '../../../shared/models/inventor.model';

@Component({
  selector: 'app-patent-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="patent-detail-page" *ngIf="patent">
      <!-- Top Bar Header -->
      <div class="page-header">
        <div>
          <a routerLink="/patents" class="back-link">&larr; Back to Patents List</a>
          <div class="header-title-row">
            <h1>{{ patent.patentNumber }}</h1>
            <span class="badge" [ngClass]="getBadgeClass(patent.statusName)">
              {{ patent.statusName }}
            </span>
          </div>
          <p class="subtitle">{{ patent.title }}</p>
        </div>

        <div class="header-actions">
          <a
            *ngIf="authService.canEdit()"
            [routerLink]="['/patents', patent.patentId, 'edit']"
            class="btn btn-secondary"
          >
            Edit Patent
          </a>
          <button
            *ngIf="authService.isAdmin()"
            type="button"
            class="btn btn-danger"
            (click)="showDeletePatentModal = true"
          >
            Delete Patent
          </button>
        </div>
      </div>

      <!-- Main Layout Grid -->
      <div class="detail-grid">
        <!-- Left Main Column -->
        <div class="main-column">
          <!-- Abstract & Description Card -->
          <div class="card">
            <div class="card-header">
              <span class="card-title">Invention Disclosure &amp; Abstract</span>
            </div>
            <p class="patent-abstract" *ngIf="patent.abstract">
              {{ patent.abstract }}
            </p>
            <p class="text-muted" *ngIf="!patent.abstract">
              No abstract text provided for this patent filing.
            </p>
          </div>

          <!-- Inventors Card -->
          <div class="card">
            <div class="card-header">
              <span class="card-title">Inventors ({{ patent.inventors.length }})</span>
              <button
                *ngIf="authService.canEdit()"
                type="button"
                class="btn btn-secondary btn-sm"
                (click)="openAssignInventorModal()"
              >
                + Assign Inventor
              </button>
            </div>

            <div class="inventors-list">
              <div *ngFor="let inv of patent.inventors" class="inventor-card">
                <div class="inventor-info">
                  <div class="inventor-name-row">
                    <strong>{{ inv.fullName }}</strong>
                    <span *ngIf="inv.isPrimaryInventor" class="badge badge-role">Primary Inventor</span>
                  </div>
                  <div class="inventor-meta">
                    <span>{{ inv.organization || 'Independent' }}</span> &bull;
                    <span>{{ inv.email }}</span>
                  </div>
                </div>

                <div class="inventor-actions" *ngIf="authService.canEdit()">
                  <button
                    *ngIf="!inv.isPrimaryInventor"
                    type="button"
                    class="btn btn-secondary btn-sm"
                    (click)="setPrimaryInventor(inv.inventorId)"
                    title="Set as primary inventor"
                  >
                    Set Primary
                  </button>
                  <button
                    type="button"
                    class="btn btn-danger btn-sm"
                    (click)="removeInventor(inv.inventorId)"
                    title="Remove from patent"
                  >
                    Remove
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- Documents Card -->
          <div class="card">
            <div class="card-header">
              <span class="card-title">Patent Documents &amp; Exhibits ({{ patent.documents.length }})</span>
              <button
                *ngIf="authService.canEdit()"
                type="button"
                class="btn btn-primary btn-sm"
                (click)="showUploadModal = true"
              >
                + Upload Document
              </button>
            </div>

            <div *ngIf="patent.documents.length === 0" class="empty-docs">
              No legal documents uploaded yet.
            </div>

            <div class="table-container" *ngIf="patent.documents.length > 0">
              <table class="table">
                <thead>
                  <tr>
                    <th>Document Name</th>
                    <th>Type</th>
                    <th>Uploaded</th>
                    <th>By</th>
                    <th style="text-align: right;">Action</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let doc of patent.documents">
                    <td>
                      <div class="doc-name-cell">
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                          <path d="M14.5 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7.5L14.5 2z"/>
                          <polyline points="14 2 14 8 20 8"/>
                        </svg>
                        <span>{{ doc.documentName }}</span>
                      </div>
                    </td>
                    <td>
                      <span class="badge badge-filed">{{ doc.documentType }}</span>
                    </td>
                    <td>{{ doc.uploadedAt | date:'mediumDate' }}</td>
                    <td><small>{{ doc.uploadedBy }}</small></td>
                    <td style="text-align: right;">
                      <div class="action-buttons">
                        <button
                          type="button"
                          class="btn btn-secondary btn-sm"
                          (click)="downloadDoc(doc.documentId, doc.documentName)"
                        >
                          Download
                        </button>
                        <button
                          *ngIf="authService.canEdit()"
                          type="button"
                          class="btn btn-danger btn-sm"
                          (click)="deleteDoc(doc.documentId)"
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
        </div>

        <!-- Right Side Column: Applicant & Dates -->
        <div class="side-column">
          <!-- Client Card -->
          <div class="card">
            <div class="card-header">
              <span class="card-title">Applicant / Client</span>
            </div>
            <div class="client-details">
              <div class="meta-row">
                <span class="meta-label">Client Name</span>
                <span class="meta-value"><strong>{{ patent.clientName }}</strong></span>
              </div>
              <div class="meta-row" *ngIf="patent.clientCompany">
                <span class="meta-label">Company</span>
                <span class="meta-value">{{ patent.clientCompany }}</span>
              </div>
              <div class="meta-row" *ngIf="patent.clientEmail">
                <span class="meta-label">Email</span>
                <span class="meta-value">{{ patent.clientEmail }}</span>
              </div>
              <div class="meta-row" *ngIf="patent.clientPhone">
                <span class="meta-label">Phone</span>
                <span class="meta-value">{{ patent.clientPhone }}</span>
              </div>
            </div>
          </div>

          <!-- Important Dates Card -->
          <div class="card">
            <div class="card-header">
              <span class="card-title">Important Dates</span>
            </div>
            <div class="timeline-list">
              <div class="timeline-item">
                <div class="timeline-bullet bullet-blue"></div>
                <div class="timeline-content">
                  <span class="date-label">Filing Date</span>
                  <span class="date-val">{{ patent.filingDate | date:'longDate' }}</span>
                </div>
              </div>

              <div class="timeline-item">
                <div class="timeline-bullet" [class.bullet-purple]="patent.publicationDate" [class.bullet-gray]="!patent.publicationDate"></div>
                <div class="timeline-content">
                  <span class="date-label">Publication Date</span>
                  <span class="date-val" *ngIf="patent.publicationDate">{{ patent.publicationDate | date:'longDate' }}</span>
                  <span class="date-val text-muted" *ngIf="!patent.publicationDate">Pending</span>
                </div>
              </div>

              <div class="timeline-item">
                <div class="timeline-bullet" [class.bullet-green]="patent.grantDate" [class.bullet-gray]="!patent.grantDate"></div>
                <div class="timeline-content">
                  <span class="date-label">Grant Date</span>
                  <span class="date-val text-green" *ngIf="patent.grantDate">{{ patent.grantDate | date:'longDate' }}</span>
                  <span class="date-val text-muted" *ngIf="!patent.grantDate">Pending examination</span>
                </div>
              </div>

              <div class="timeline-item">
                <div class="timeline-bullet" [class.bullet-amber]="patent.expiryDate" [class.bullet-gray]="!patent.expiryDate"></div>
                <div class="timeline-content">
                  <span class="date-label">Expiry Date</span>
                  <span class="date-val text-amber" *ngIf="patent.expiryDate">{{ patent.expiryDate | date:'longDate' }}</span>
                  <span class="date-val text-muted" *ngIf="!patent.expiryDate">Unspecified</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Assign Inventor Modal -->
      <div class="modal-backdrop" *ngIf="showAssignModal">
        <div class="modal-content">
          <div class="card-header">
            <span class="card-title">Assign Inventor to Patent</span>
            <button type="button" class="btn-icon" (click)="showAssignModal = false">&times;</button>
          </div>
          <div class="form-group">
            <label class="form-label">Select Inventor</label>
            <select class="form-control" [(ngModel)]="selectedInventorIdToAssign">
              <option [ngValue]="null" disabled>Choose inventor</option>
              <option *ngFor="let inv of availableInventors" [ngValue]="inv.inventorId">
                {{ inv.fullName }} ({{ inv.organization }})
              </option>
            </select>
          </div>
          <div class="form-group">
            <label class="checkbox-label">
              <input type="checkbox" [(ngModel)]="assignAsPrimary" />
              <span>Set as Primary Inventor</span>
            </label>
          </div>
          <div style="display: flex; justify-content: flex-end; gap: 0.75rem; margin-top: 1rem;">
            <button type="button" class="btn btn-secondary" (click)="showAssignModal = false">Cancel</button>
            <button
              type="button"
              class="btn btn-primary"
              [disabled]="!selectedInventorIdToAssign"
              (click)="saveAssignInventor()"
            >
              Assign
            </button>
          </div>
        </div>
      </div>

      <!-- Upload Document Modal -->
      <div class="modal-backdrop" *ngIf="showUploadModal">
        <div class="modal-content">
          <div class="card-header">
            <span class="card-title">Upload Patent Document</span>
            <button type="button" class="btn-icon" (click)="showUploadModal = false">&times;</button>
          </div>

          <div class="form-group">
            <label class="form-label">Document Type</label>
            <select class="form-control" [(ngModel)]="uploadDocType">
              <option value="Application">Application (Form 1 / Provisional)</option>
              <option value="Specification">Complete Specification (Form 2)</option>
              <option value="Drawing">Drawings / Technical Figures</option>
              <option value="Certificate">Letters Patent Certificate</option>
              <option value="LegalDocument">Power of Attorney / Legal Assignment</option>
              <option value="Other">Other Document</option>
            </select>
          </div>

          <div class="form-group">
            <label class="form-label">Select File</label>
            <input type="file" class="form-control" (change)="onFileSelected($event)" />
          </div>

          <div style="display: flex; justify-content: flex-end; gap: 0.75rem; margin-top: 1rem;">
            <button type="button" class="btn btn-secondary" (click)="showUploadModal = false">Cancel</button>
            <button
              type="button"
              class="btn btn-primary"
              [disabled]="!fileToUpload || isUploading"
              (click)="uploadDocument()"
            >
              <span *ngIf="!isUploading">Upload File</span>
              <span *ngIf="isUploading">Uploading...</span>
            </button>
          </div>
        </div>
      </div>

      <!-- Delete Confirmation Modal -->
      <div class="modal-backdrop" *ngIf="showDeletePatentModal">
        <div class="modal-content">
          <div class="card-header">
            <span class="card-title">Confirm Patent Deletion</span>
            <button type="button" class="btn-icon" (click)="showDeletePatentModal = false">&times;</button>
          </div>
          <p style="margin-bottom: 1.25rem;">
            Are you sure you want to permanently delete patent <strong>{{ patent.patentNumber }}</strong>? All associated document metadata and inventor links will also be removed.
          </p>
          <div style="display: flex; justify-content: flex-end; gap: 0.75rem;">
            <button type="button" class="btn btn-secondary" (click)="showDeletePatentModal = false">Cancel</button>
            <button type="button" class="btn btn-danger" (click)="deletePatent()">Delete Patent</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .patent-detail-page {
      display: flex;
      flex-direction: column;
      gap: 1.75rem;
    }
    .back-link {
      font-size: 0.85rem;
      font-weight: 600;
      color: var(--slate-500);
      margin-bottom: 0.5rem;
      display: inline-block;
    }
    .header-title-row {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin-bottom: 0.25rem;
    }
    .page-header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      flex-wrap: wrap;
      gap: 1rem;
    }
    .subtitle {
      font-size: 1.05rem;
      font-weight: 600;
      color: var(--slate-700);
    }
    .header-actions {
      display: flex;
      gap: 0.75rem;
    }
    .detail-grid {
      display: grid;
      grid-template-columns: 2fr 1fr;
      gap: 1.5rem;
    }
    .main-column, .side-column {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }
    .patent-abstract {
      font-size: 0.95rem;
      line-height: 1.7;
      color: var(--slate-700);
    }
    .inventors-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }
    .inventor-card {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0.875rem;
      border-radius: var(--radius-md);
      background-color: var(--slate-50);
      border: 1px solid var(--slate-200);
    }
    .inventor-name-row {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-bottom: 0.25rem;
    }
    .inventor-meta {
      font-size: 0.8rem;
      color: var(--slate-500);
    }
    .inventor-actions {
      display: flex;
      gap: 0.5rem;
    }
    .doc-name-cell {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-weight: 600;
      color: var(--slate-800);
    }
    .action-buttons {
      display: inline-flex;
      gap: 0.35rem;
    }
    .client-details {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }
    .meta-row {
      display: flex;
      flex-direction: column;
      gap: 0.15rem;
    }
    .meta-label {
      font-size: 0.75rem;
      font-weight: 600;
      color: var(--slate-400);
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .meta-value {
      font-size: 0.9rem;
      color: var(--slate-800);
    }
    .timeline-list {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
      position: relative;
      padding-left: 0.5rem;
    }
    .timeline-item {
      display: flex;
      align-items: flex-start;
      gap: 1rem;
    }
    .timeline-bullet {
      width: 12px;
      height: 12px;
      border-radius: 50%;
      margin-top: 0.25rem;
      flex-shrink: 0;
    }
    .bullet-blue { background-color: #3b82f6; }
    .bullet-purple { background-color: #8b5cf6; }
    .bullet-green { background-color: #10b981; }
    .bullet-amber { background-color: #f59e0b; }
    .bullet-gray { background-color: #cbd5e1; }
    .timeline-content {
      display: flex;
      flex-direction: column;
    }
    .date-label {
      font-size: 0.75rem;
      font-weight: 600;
      color: var(--slate-500);
    }
    .date-val {
      font-size: 0.9rem;
      font-weight: 600;
      color: var(--slate-800);
    }
    .empty-docs {
      padding: 1.5rem;
      text-align: center;
      color: var(--slate-500);
      font-size: 0.875rem;
    }
    @media (max-width: 992px) {
      .detail-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class PatentDetailComponent implements OnInit {
  private patentService = inject(PatentService);
  private inventorService = inject(InventorService);
  private documentService = inject(DocumentService);
  private notificationService = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  authService = inject(AuthService);

  patent: PatentDetail | null = null;
  patentId = 0;

  // Modals
  showAssignModal = false;
  showUploadModal = false;
  showDeletePatentModal = false;

  allInventors: Inventor[] = [];
  selectedInventorIdToAssign: number | null = null;
  assignAsPrimary = false;

  fileToUpload: File | null = null;
  uploadDocType = 'Application';
  isUploading = false;

  get availableInventors(): Inventor[] {
    if (!this.patent) return this.allInventors;
    const assignedIds = this.patent.inventors.map(i => i.inventorId);
    return this.allInventors.filter(i => !assignedIds.includes(i.inventorId));
  }

  ngOnInit(): void {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.patentId = +id;
      this.loadPatent();
      this.inventorService.getInventors().subscribe(res => {
        if (res.data) this.allInventors = res.data;
      });
    }
  }

  loadPatent(): void {
    this.patentService.getPatentById(this.patentId).subscribe({
      next: (res) => {
        this.patent = res.data;
      },
      error: () => {
        this.notificationService.error('Failed to load patent.');
        this.router.navigate(['/patents']);
      }
    });
  }

  openAssignInventorModal(): void {
    this.selectedInventorIdToAssign = null;
    this.assignAsPrimary = false;
    this.showAssignModal = true;
  }

  saveAssignInventor(): void {
    if (!this.selectedInventorIdToAssign) return;
    this.patentService.assignInventor(this.patentId, this.selectedInventorIdToAssign, this.assignAsPrimary).subscribe({
      next: () => {
        this.notificationService.success('Inventor assigned successfully.');
        this.showAssignModal = false;
        this.loadPatent();
      },
      error: (err) => {
        const msg = err.error?.message || 'Failed to assign inventor.';
        this.notificationService.error(msg);
      }
    });
  }

  setPrimaryInventor(inventorId: number): void {
    this.patentService.assignInventor(this.patentId, inventorId, true).subscribe({
      next: () => {
        this.notificationService.success('Primary inventor updated.');
        this.loadPatent();
      },
      error: (err) => {
        const msg = err.error?.message || 'Failed to update primary inventor.';
        this.notificationService.error(msg);
      }
    });
  }

  removeInventor(inventorId: number): void {
    this.patentService.removeInventor(this.patentId, inventorId).subscribe({
      next: () => {
        this.notificationService.success('Inventor unassigned.');
        this.loadPatent();
      },
      error: (err) => {
        const msg = err.error?.message || 'Failed to remove inventor.';
        this.notificationService.error(msg);
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.fileToUpload = input.files[0];
    }
  }

  uploadDocument(): void {
    if (!this.fileToUpload) return;
    this.isUploading = true;
    this.documentService.uploadDocument(this.patentId, this.fileToUpload, this.uploadDocType).subscribe({
      next: () => {
        this.notificationService.success('Document uploaded successfully.');
        this.isUploading = false;
        this.showUploadModal = false;
        this.fileToUpload = null;
        this.loadPatent();
      },
      error: (err) => {
        this.isUploading = false;
        const msg = err.error?.message || 'Failed to upload document.';
        this.notificationService.error(msg);
      }
    });
  }

  downloadDoc(documentId: number, fileName: string): void {
    this.documentService.downloadDocument(documentId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.notificationService.error('Failed to download document.');
      }
    });
  }

  deleteDoc(documentId: number): void {
    this.documentService.deleteDocument(documentId).subscribe({
      next: () => {
        this.notificationService.success('Document deleted.');
        this.loadPatent();
      },
      error: (err) => {
        const msg = err.error?.message || 'Failed to delete document.';
        this.notificationService.error(msg);
      }
    });
  }

  deletePatent(): void {
    this.patentService.deletePatent(this.patentId).subscribe({
      next: () => {
        this.notificationService.success('Patent deleted.');
        this.router.navigate(['/patents']);
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
