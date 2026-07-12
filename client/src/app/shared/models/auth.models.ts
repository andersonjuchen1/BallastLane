// Request/response contracts for the auth endpoints (mirror the .NET DTOs).

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

/** Returned by both register (auto-login) and login. */
export interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
}

/** Returned by GET /auth/me. */
export interface UserResponse {
  id: string;
  username: string;
  email: string;
  createdAt: string;
}
