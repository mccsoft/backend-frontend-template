export type AuthState =
  | ({
      type: 'authorized';
    } & TokenData)
  | {
      type: 'unauthorized';
    };

export type TokenData = {
  userId: string;
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  acquiredAt: Date;
};
