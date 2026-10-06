import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container" *ngIf="notificationService.toasts().length > 0">
      <div
        *ngFor="let toast of notificationService.toasts()"
        class="toast"
        [ngClass]="'toast-' + toast.type"
      >
        <span>{{ toast.text }}</span>
        <button
          type="button"
          (click)="notificationService.remove(toast.id)"
          style="background: transparent; color: white; margin-left: 1rem; font-size: 1.1rem; opacity: 0.8;"
        >
          &times;
        </button>
      </div>
    </div>
  `
})
export class ToastContainerComponent {
  notificationService = inject(NotificationService);
}
