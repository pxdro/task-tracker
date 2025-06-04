import { Injectable } from '@angular/core';
import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError, catchError, switchMap } from 'rxjs';
import { Router } from '@angular/router';
import { AuthService } from './services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    const authToken = this.authService.getAuthToken();

    const clonedReq = authToken
      ? req.clone({
          setHeaders: {
            Authorization: `Bearer ${authToken}`
          }
        })
      : req;

    return next.handle(clonedReq).pipe(
      catchError((error) => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          // Invalid auth token, try refresh
          const refreshToken = this.authService.getRefreshToken();
          if (refreshToken) {
            return this.authService.refreshToken().pipe(
              switchMap(() => {
                const newToken = this.authService.getAuthToken();
                const retryReq = req.clone({
                  setHeaders: {
                    Authorization: `Bearer ${newToken}`
                  }
                });
                return next.handle(retryReq);
              }),
              catchError((refreshError) => {
                // Refresh failed, logout and redirect to login
                this.authService.logout();
                this.router.navigate(['/auth/login']);
                return throwError(() => refreshError);
              })
            );
          } else {
            // No refresh Token, logout and redirect to login
            this.authService.logout();
            this.router.navigate(['/auth/login']);
            return throwError(() => error);
          }
        } else {
          return throwError(() => error);
        }
      })
    );
  }
}
