import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="pagination-wrapper" *ngIf="totalPages > 1">
      <div class="pagination-info">
        Showing <strong>{{ (currentPage - 1) * pageSize + 1 }}</strong> to
        <strong>{{ getEndIndex() }}</strong> of <strong>{{ totalItems }}</strong> entries
      </div>
      <div class="pagination-controls">
        <button
          type="button"
          class="btn btn-secondary btn-sm"
          [disabled]="currentPage === 1"
          (click)="onPageChange(currentPage - 1)"
        >
          &larr; Previous
        </button>

        <ng-container *ngFor="let p of getPages()">
          <button
            type="button"
            class="btn btn-sm page-num"
            [class.btn-primary]="p === currentPage"
            [class.btn-secondary]="p !== currentPage"
            (click)="onPageChange(p)"
          >
            {{ p }}
          </button>
        </ng-container>

        <button
          type="button"
          class="btn btn-secondary btn-sm"
          [disabled]="currentPage === totalPages"
          (click)="onPageChange(currentPage + 1)"
        >
          Next &rarr;
        </button>
      </div>
    </div>
  `,
  styles: [`
    .pagination-wrapper {
      display: flex;
      align-items: center;
      justify-content: space-between;
      flex-wrap: wrap;
      gap: 1rem;
      padding: 1rem 0 0.5rem 0;
    }
    .pagination-info {
      font-size: 0.875rem;
      color: var(--slate-600);
    }
    .pagination-controls {
      display: flex;
      align-items: center;
      gap: 0.35rem;
    }
    .page-num {
      min-width: 2rem;
    }
  `]
})
export class PaginationComponent {
  @Input() currentPage = 1;
  @Input() pageSize = 10;
  @Input() totalItems = 0;
  @Input() totalPages = 1;

  @Output() pageChange = new EventEmitter<number>();

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.pageChange.emit(page);
    }
  }

  getEndIndex(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalItems);
  }

  getPages(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);

    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }
}
