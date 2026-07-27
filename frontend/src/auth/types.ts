export type User = { id: string; email: string; displayName: string; role: string };
export type AuthResponse = { accessToken: string; refreshToken: string; accessTokenExpiresAtUtc: string; user: User };
