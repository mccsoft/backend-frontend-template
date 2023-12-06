import { jwtDecode } from 'jwt-decode';

export type FetchLoginResponse = {
  access_token: string;
  refresh_token: string;
  expires_in: number;
};

export type AuthData = {
  access_token: string;
  refresh_token: string;
  claims: UserClaims;
};

export type UserClaims = {
  id: string;
  name: string;
};

export function decodeClaimsFromToken(token: string): UserClaims {
  const claims = jwtDecode<UserClaims>(token);
  return claims;
}
