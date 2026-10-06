import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const notificationService = inject(NotificationService);
  const token = authService.getToken();

  let authReq = req;
  if (token) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        notificationService.warning('Your session has expired. Please sign in again.');
        authService.logout();
      } else if (error.status === 403) {
        notificationService.error('Access forbidden: You do not have permission to perform this action.');
      } else if (error.status === 0) {
        notificationService.error('Unable to connect to the backend server. Please verify the API is running.');
      }
      return throwError(() => error);
    })
  );
};
