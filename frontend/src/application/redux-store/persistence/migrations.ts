import { MigrationManifest, PersistedState } from 'redux-persist/es/types';

export const migrations: MigrationManifest = {
  // @ts-ignore
  '0': (state: PersistedState) => {
    return {
      ...state,
      auth: undefined,
    };
  },
};
