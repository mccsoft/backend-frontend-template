import { PersistConfig } from 'redux-persist/es/types';
import { createMigrate } from 'redux-persist';
import { migrations } from './migrations';
import storage from 'redux-persist/lib/storage';
import { GlobalState } from '../types';

export const persistConfig: PersistConfig<any> = {
  key: 'root',
  version: 0,
  storage: storage,
  whitelist: ['theme'] as (keyof GlobalState)[],
  migrate: createMigrate(migrations, { debug: false }),
};
