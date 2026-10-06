import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PatentService } from '../../../core/services/patent.service';
import { ClientService } from '../../../core/services/client.service';
import { InventorService } from '../../../core/services/inventor.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PatentStatus } from '../../../shared/models/patent.model';
import { Client } from '../../../shared/models/client.model';
import { Inventor } from '../../../shared/models/inventor.model';

@Component({
  selector: 'app-patent-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="patent-form-page">
      <!-- Header -->
      <div class="page-header">
        <div>
          <a routerLink="/patents" class="back-link">&larr; Back to Patents</a>
          <h1>{{ isEditMode ? 'Edit Patent Application' : 'Register New Patent' }}</h1>
          <p class="subtitle">{{ isEditMode ? 'Update existing patent record details and status' : 'Enter complete specification and bibliographic details to file patent' }}</p>
        </div>
      </div>

      <div class="card form-card">
        <form [formGroup]="patentForm" (ngSubmit)="onSubmit()">
          <!-- Section 1: Bibliographic Information -->
          <div class="form-section">
            <h3 class="section-title">Bibliographic &amp; Title Information</h3>
            <div class="form-row">
              <div class="form-group">
                <label class="form-label" for="patentNumber">
                  Patent Number <span class="required">*</span>
                </label>
                <input
                  id="patentNumber"
                  type="text"
                  class="form-control"
                  [class.is-invalid]="f['patentNumber'].touched && f['patentNumber'].invalid"
                  formControlName="patentNumber"
                  placeholder="e.g. IN202611009876"
                />
                <div class="invalid-feedback" *ngIf="f['patentNumber'].touched && f['patentNumber'].errors?.['required']">
                  Patent number is required.
                </div>
              </div>

              <div class="form-group">
                <label class="form-label" for="statusId">
                  Patent Status <span class="required">*</span>
                </label>
                <select
                  id="statusId"
                  class="form-control"
                  [class.is-invalid]="f['statusId'].touched && f['statusId'].invalid"
                  formControlName="statusId"
                >
                  <option [ngValue]="null" disabled>Select patent status</option>
                  <option *ngFor="let s of statuses" [ngValue]="s.statusId">{{ s.statusName }}</option>
                </select>
                <div class="invalid-feedback" *ngIf="f['statusId'].touched && f['statusId'].errors?.['required']">
                  Status is required.
                </div>
              </div>
            </div>

            <div class="form-group">
              <label class="form-label" for="title">
                Patent Invention Title <span class="required">*</span>
              </label>
              <input
                id="title"
                type="text"
                class="form-control"
                [class.is-invalid]="f['title'].touched && f['title'].invalid"
                formControlName="title"
                placeholder="e.g. High-Efficiency Perovskite Tandem Photovoltaic Cell"
              />
              <div class="invalid-feedback" *ngIf="f['title'].touched && f['title'].errors?.['required']">
                Title is required.
              </div>
            </div>

            <div class="form-group">
              <label class="form-label" for="abstract">
                Abstract / Invention Summary
              </label>
              <textarea
                id="abstract"
                rows="4"
                class="form-control"
                formControlName="abstract"
                placeholder="Provide a concise summary of technical disclosure..."
              ></textarea>
            </div>
          </div>

          <!-- Section 2: Client & Assignee -->
          <div class="form-section">
            <h3 class="section-title">Client &amp; Applicant</h3>
            <div class="form-group">
              <label class="form-label" for="clientId">
                Applicant Client <span class="required">*</span>
              </label>
              <select
                id="clientId"
                class="form-control"
                [class.is-invalid]="f['clientId'].touched && f['clientId'].invalid"
                formControlName="clientId"
              >
                <option [ngValue]="null" disabled>Select applicant client</option>
                <option *ngFor="let c of clients" [ngValue]="c.clientId">{{ c.clientName }} ({{ c.company || 'Individual' }})</option>
              </select>
              <div class="invalid-feedback" *ngIf="f['clientId'].touched && f['clientId'].errors?.['required']">
                Client is required.
              </div>
            </div>
          </div>

          <!-- Section 3: Critical Dates -->
          <div class="form-section">
            <h3 class="section-title">Prosecution &amp; Statutory Dates</h3>
            <div class="form-row">
              <div class="form-group">
                <label class="form-label" for="filingDate">
                  Filing Date <span class="required">*</span>
                </label>
                <input
                  id="filingDate"
                  type="date"
                  class="form-control"
                  [class.is-invalid]="f['filingDate'].touched && f['filingDate'].invalid"
                  formControlName="filingDate"
                />
                <div class="invalid-feedback" *ngIf="f['filingDate'].touched && f['filingDate'].errors?.['required']">
                  Filing date is required.
                </div>
              </div>

              <div class="form-group">
                <label class="form-label" for="publicationDate">Publication Date</label>
                <input id="publicationDate" type="date" class="form-control" formControlName="publicationDate" />
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label class="form-label" for="grantDate">Grant Date</label>
                <input id="grantDate" type="date" class="form-control" formControlName="grantDate" />
              </div>

              <div class="form-group">
                <label class="form-label" for="expiryDate">Expiry Date</label>
                <input
                  id="expiryDate"
                  type="date"
                  class="form-control"
                  [class.is-invalid]="patentForm.errors?.['expiryInvalid']"
                  formControlName="expiryDate"
                />
                <div class="invalid-feedback" *ngIf="patentForm.errors?.['expiryInvalid']">
                  Expiry date cannot be earlier than filing date.
                </div>
              </div>
            </div>
          </div>

          <!-- Section 4: Inventor Assignment (On Create) -->
          <div class="form-section" *ngIf="!isEditMode">
            <h3 class="section-title">Inventor Assignment (At least 1 required)</h3>
            <p class="section-hint">Select the inventors who developed this invention. Choose the primary inventor.</p>

            <div class="inventors-selection-list">
              <div *ngFor="let inv of inventors" class="inventor-select-row">
                <label class="checkbox-label">
                  <input
                    type="checkbox"
                    [checked]="selectedInventorIds.includes(inv.inventorId)"
                    (change)="toggleInventor(inv.inventorId)"
                  />
                  <span><strong>{{ inv.fullName }}</strong> ({{ inv.organization || 'Independent' }})</span>
                </label>

                <label class="radio-label" *ngIf="selectedInventorIds.includes(inv.inventorId)">
                  <input
                    type="radio"
                    name="primaryInventor"
                    [checked]="primaryInventorId === inv.inventorId"
                    (change)="primaryInventorId = inv.inventorId"
                  />
                  <span class="primary-tag">Primary</span>
                </label>
              </div>
            </div>
            <div class="invalid-feedback" style="display: block; margin-top: 0.5rem;" *ngIf="submitted && selectedInventorIds.length === 0">
              Please select at least one inventor.
            </div>
          </div>

          <!-- Form Actions -->
          <div class="form-actions">
            <a routerLink="/patents" class="btn btn-secondary">Cancel</a>
            <button type="submit" class="btn btn-primary" [disabled]="isSubmitting">
              <span *ngIf="!isSubmitting">{{ isEditMode ? 'Save Changes' : 'Submit &amp; File Patent' }}</span>
              <span *ngIf="isSubmitting">Saving...</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .patent-form-page {
      max-width: 860px;
      margin: 0 auto;
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }
    .back-link {
      font-size: 0.85rem;
      font-weight: 600;
      color: var(--slate-500);
      margin-bottom: 0.5rem;
      display: inline-block;
    }
    .subtitle {
      font-size: 0.9rem;
      color: var(--slate-500);
    }
    .form-card {
      padding: 2rem;
    }
    .form-section {
      margin-bottom: 2rem;
      padding-bottom: 1.5rem;
      border-bottom: 1px solid var(--slate-100);
    }
    .form-section:last-of-type {
      border-bottom: none;
      margin-bottom: 1rem;
    }
    .section-title {
      font-size: 1.05rem;
      font-weight: 700;
      color: var(--slate-900);
      margin-bottom: 1rem;
    }
    .section-hint {
      font-size: 0.85rem;
      color: var(--slate-500);
      margin-bottom: 0.875rem;
    }
    .inventors-selection-list {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      max-height: 240px;
      overflow-y: auto;
      border: 1px solid var(--slate-200);
      border-radius: var(--radius-md);
      padding: 0.75rem;
      background-color: var(--slate-50);
    }
    .inventor-select-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0.45rem 0.5rem;
      border-radius: var(--radius-sm);
      background-color: var(--white);
    }
    .checkbox-label, .radio-label {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.875rem;
      cursor: pointer;
    }
    .primary-tag {
      font-size: 0.75rem;
      font-weight: 600;
      color: var(--color-primary);
    }
    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
      margin-top: 1.5rem;
    }
  `]
})
export class PatentFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private patentService = inject(PatentService);
  private clientService = inject(ClientService);
  private inventorService = inject(InventorService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  isEditMode = false;
  patentId = 0;
  isSubmitting = false;
  submitted = false;

  statuses: PatentStatus[] = [];
  clients: Client[] = [];
  inventors: Inventor[] = [];

  selectedInventorIds: number[] = [];
  primaryInventorId: number | null = null;

  patentForm: FormGroup = this.fb.group({
    patentNumber: ['', [Validators.required]],
    title: ['', [Validators.required]],
    abstract: [''],
    statusId: [null, [Validators.required]],
    clientId: [null, [Validators.required]],
    filingDate: ['', [Validators.required]],
    publicationDate: [''],
    grantDate: [''],
    expiryDate: ['']
  }, { validators: this.dateComparisonValidator });

  get f() {
    return this.patentForm.controls;
  }

  ngOnInit(): void {
    this.loadDropdownData();
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.isEditMode = true;
      this.patentId = +id;
      this.loadPatentDetails(this.patentId);
    }
  }

  loadDropdownData(): void {
    this.patentService.getStatuses().subscribe(res => {
      if (res.data) this.statuses = res.data;
    });
    this.clientService.getClients().subscribe(res => {
      if (res.data) this.clients = res.data;
    });
    this.inventorService.getInventors().subscribe(res => {
      if (res.data) this.inventors = res.data;
    });
  }

  loadPatentDetails(id: number): void {
    this.patentService.getPatentById(id).subscribe({
      next: (res) => {
        const p = res.data;
        this.patentForm.patchValue({
          patentNumber: p.patentNumber,
          title: p.title,
          abstract: p.abstract,
          statusId: p.statusId,
          clientId: p.clientId,
          filingDate: p.filingDate ? p.filingDate.substring(0, 10) : '',
          publicationDate: p.publicationDate ? p.publicationDate.substring(0, 10) : '',
          grantDate: p.grantDate ? p.grantDate.substring(0, 10) : '',
          expiryDate: p.expiryDate ? p.expiryDate.substring(0, 10) : ''
        });
      },
      error: () => {
        this.notificationService.error('Failed to load patent details.');
        this.router.navigate(['/patents']);
      }
    });
  }

  toggleInventor(id: number): void {
    if (this.selectedInventorIds.includes(id)) {
      this.selectedInventorIds = this.selectedInventorIds.filter(i => i !== id);
      if (this.primaryInventorId === id) {
        this.primaryInventorId = this.selectedInventorIds.length > 0 ? this.selectedInventorIds[0] : null;
      }
    } else {
      this.selectedInventorIds.push(id);
      if (this.selectedInventorIds.length === 1) {
        this.primaryInventorId = id;
      }
    }
  }

  dateComparisonValidator(group: FormGroup): { [key: string]: boolean } | null {
    const filing = group.get('filingDate')?.value;
    const expiry = group.get('expiryDate')?.value;

    if (filing && expiry && new Date(expiry) < new Date(filing)) {
      return { expiryInvalid: true };
    }
    return null;
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.patentForm.invalid) {
      this.patentForm.markAllAsTouched();
      return;
    }

    if (!this.isEditMode && this.selectedInventorIds.length === 0) {
      this.notificationService.error('Please assign at least one inventor.');
      return;
    }

    this.isSubmitting = true;
    const formVal = this.patentForm.value;

    if (this.isEditMode) {
      this.patentService.updatePatent(this.patentId, {
        patentNumber: formVal.patentNumber,
        title: formVal.title,
        abstract: formVal.abstract,
        filingDate: formVal.filingDate,
        publicationDate: formVal.publicationDate || null,
        grantDate: formVal.grantDate || null,
        expiryDate: formVal.expiryDate || null,
        statusId: formVal.statusId,
        clientId: formVal.clientId
      }).subscribe({
        next: () => {
          this.notificationService.success('Patent updated successfully.');
          this.router.navigate(['/patents', this.patentId]);
        },
        error: (err) => {
          this.isSubmitting = false;
          const msg = err.error?.message || 'Failed to update patent.';
          this.notificationService.error(msg);
        }
      });
    } else {
      this.patentService.createPatent({
        patentNumber: formVal.patentNumber,
        title: formVal.title,
        abstract: formVal.abstract,
        filingDate: formVal.filingDate,
        publicationDate: formVal.publicationDate || null,
        grantDate: formVal.grantDate || null,
        expiryDate: formVal.expiryDate || null,
        statusId: formVal.statusId,
        clientId: formVal.clientId,
        inventorIds: this.selectedInventorIds,
        primaryInventorId: this.primaryInventorId
      }).subscribe({
        next: (res) => {
          this.notificationService.success('Patent created and filed successfully.');
          this.router.navigate(['/patents', res.data.patentId]);
        },
        error: (err) => {
          this.isSubmitting = false;
          const msg = err.error?.message || 'Failed to create patent.';
          this.notificationService.error(msg);
        }
      });
    }
  }
}
