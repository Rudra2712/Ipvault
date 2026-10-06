import { Injectable, signal } from '@angular/core';

export interface ToastMessage {
  id: string;
  type: 'success' | 'error' | 'info' | 'warning';
  text: string;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private toastsSignal = signal<ToastMessage[]>([]);
  readonly toasts = this.toastsSignal.asReadonly();

  success(text: string): void {
    this.addToast('success', text);
  }

  error(text: string): void {
    this.addToast('error', text);
  }

  info(text: string): void {
    this.addToast('info', text);
  }

  warning(text: string): void {
    this.addToast('warning', text);
  }

  remove(id: string): void {
    this.toastsSignal.update(toasts => toasts.filter(t => t.id !== id));
  }

  private addToast(type: ToastMessage['type'], text: string): void {
    const id = Math.random().toString(36).substring(2, 9);
    const toast: ToastMessage = { id, type, text };
    this.toastsSignal.update(toasts => [...toasts, toast]);

    setTimeout(() => {
      this.remove(id);
    }, 4500);
  }
}
