

import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { tap, map, catchError } from 'rxjs/operators';
import { Observable, throwError, of } from 'rxjs';
import {
  ResultDto,
  UserRequestDto,
  UserReturnDto,
  TokensDto
} from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly API = '/api/auth';
  private readonly TOKEN_KEY = 'auth_token';
  private readonly REFRESH_KEY = 'refresh_token';

  constructor(private http: HttpClient) {}

  register(dto: UserRequestDto): Observable<UserReturnDto> {
    return this.http
      .post<HttpResponse<ResultDto<UserReturnDto>>>(
        this.API + '/register',
        dto,
        { observe: 'response' }
      )
      .pipe(
        map(resp => {
          const body = resp.body?.body!;
          if (body.errorMessage) {
            throw new Error(body.errorMessage);
          }
          return body.data!;
        }),
        catchError(err => throwError(() => err))
      );
  }

  login(dto: UserRequestDto): Observable<UserReturnDto> {
    return this.http
      .post<HttpResponse<ResultDto<TokensDto>>>(
        this.API + '/login',
        dto,
        { observe: 'response' }
      )
      .pipe(
        tap(resp => {
          const tokens = resp.body?.body?.data;
          if (tokens) {
            const authHeader = resp.headers.get('X-Auth-Token');
            const refreshHeader = resp.headers.get('X-Refresh-Token');
            this.setTokens(
              authHeader ?? tokens.authToken,
              refreshHeader ?? tokens.refreshToken
            );
          }
        }),
        map(resp => {
          const body = resp.body!.body!;
          if (body.errorMessage) {
            throw new Error(body.errorMessage);
          }
          // Retorna apenas o e-mail
          return { email: dto.email };
        }),
        catchError(err => throwError(() => err))
      );
  }

  refreshToken(): Observable<void> {
    const payload: TokensDto = {
      authToken: this.getRefreshToken()!,
      refreshToken: this.getRefreshToken()!
    };
    return this.http
      .post<HttpResponse<ResultDto<TokensDto>>>(
        this.API + '/refresh',
        payload,
        { observe: 'response' }
      )
      .pipe(
        tap(resp => {
          const tokens = resp.body?.body?.data;
          if (tokens) {
            const authHeader = resp.headers.get('X-Auth-Token');
            const refreshHeader = resp.headers.get('X-Refresh-Token');
            this.setTokens(
              authHeader ?? tokens.authToken,
              refreshHeader ?? tokens.refreshToken
            );
          }
        }),
        map(() => {}),
        catchError(err => throwError(() => err))
      );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
  }

  isLoggedIn(): boolean {
    return !!this.getAuthToken();
  }

  getAuthToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_KEY);
  }

  private setTokens(authToken: string, refreshToken: string) {
    localStorage.setItem(this.TOKEN_KEY, authToken);
    localStorage.setItem(this.REFRESH_KEY, refreshToken);
  }
}