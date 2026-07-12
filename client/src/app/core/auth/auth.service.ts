import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  UserResponse,
} from '../../shared/models/auth.models';

const TOKEN_KEY = 'tm.accessToken';
const EXPIRY_KEY = 'tm.expiresAtUtc';

/**
 * Holds authentication state as signals. The access token lives in memory
 * (a signal) and is mirrored to localStorage so a page refresh keeps the
 * session. Ownership/identity always come from the API via /auth/me.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  private readonly _token = signal<string | null>(this.readStoredToken());
  private readonly _currentUser = signal<UserResponse | null>(null);

  /** The authenticated user's profile, or null when not loaded/logged out. */
  readonly currentUser = this._currentUser.asReadonly();

  /** True while a non-expired token is held. */
  readonly isAuthenticated = computed(() => this._token() !== null);

  get token(): string | null {
    return this._token();
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/auth/register`, request)
      .pipe(tap((response) => this.storeSession(response)));
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/auth/login`, request)
      .pipe(tap((response) => this.storeSession(response)));
  }

  /** Loads the current user's profile into the currentUser signal. */
  loadCurrentUser(): Observable<UserResponse> {
    return this.http
      .get<UserResponse>(`${this.baseUrl}/auth/me`)
      .pipe(tap((user) => this._currentUser.set(user)));
  }

  logout(): void {
    this._token.set(null);
    this._currentUser.set(null);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EXPIRY_KEY);
  }

  private storeSession(response: AuthResponse): void {
    this._token.set(response.accessToken);
    localStorage.setItem(TOKEN_KEY, response.accessToken);
    localStorage.setItem(EXPIRY_KEY, response.expiresAtUtc);
  }

  private readStoredToken(): string | null {
    const token = localStorage.getItem(TOKEN_KEY);
    const expiry = localStorage.getItem(EXPIRY_KEY);
    if (!token || !expiry) {
      return null;
    }
    // Drop an already-expired token so the app starts logged out.
    if (new Date(expiry).getTime() <= Date.now()) {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(EXPIRY_KEY);
      return null;
    }
    return token;
  }
}
