import { PersistConfig } from 'redux-persist/es/types';
import { createMigrate } from 'redux-persist';
import { migrations } from './migrations';
import storage from 'redux-persist/lib/storage';

export const persistConfig: PersistConfig<any> = {
  key: 'root',
  version: 0,
  storage: storage,
  whitelist: ['auth'],
  migrate: createMigrate(migrations, { debug: false }),
};
