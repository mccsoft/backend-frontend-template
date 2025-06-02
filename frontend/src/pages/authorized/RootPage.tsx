import { appVersion } from 'application/constants/env-variables';
import { Button } from 'components/uikit/buttons/Button';
import React from 'react';
import { Outlet } from 'react-router';
import { useTranslation } from 'react-i18next';
import { logOut } from 'helpers/auth/auth-interceptor';
import styles from './RootPage.module.scss';
import Logger from 'js-logger';
import { DropDownInput } from 'components/uikit/inputs/dropdown/DropDownInput';
import {
  changeLanguage,
  getCurrentSupportedLanguage,
} from 'application/localization/localization';
import { Language, languages } from 'application/localization/locales';

export const RootPage: React.FC = () => {
  const i18n = useTranslation();

  return (
    <>
      <div className={styles.container} data-test-id="main-page-container">
        <div className={styles.content}>
          <Outlet />
        </div>
        <div className={styles.bottomNavigation}>
          <div>
            Version: {appVersion()}, {import.meta.env.TST}
          </div>
          <div>
            <DropDownInput
              className={styles.languageSwitcher}
              variant={'normal'}
              options={languages}
              required={true}
              value={getCurrentSupportedLanguage(i18n.i18n)}
              onValueChanged={(x) => {
                changeLanguage(x as Language).catch((e) => Logger.error(e));
              }}
            />
          </div>
          <div className={styles.logOutWrapper}>
            <Button
              onClick={() => {
                logOut().catch((e) => Logger.error(e));
              }}
              title={'Log Out'}
            />
          </div>
        </div>
      </div>
    </>
  );
};
