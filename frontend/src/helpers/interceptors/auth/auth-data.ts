import { UserClaims } from 'services/auth-client';

export type AuthData = {
  access_token: string;
  refresh_token: string;
  claims: UserClaims;
};
