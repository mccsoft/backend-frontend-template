import { appVersion } from 'application/constants/env-variables';
import { Links } from 'application/constants/links';
import { useAppDispatch } from 'application/redux-store/root-store';
import { Button } from 'components/uikit/buttons/Button';
import { CreateProductPage } from 'pages/authorized/products/create/CreateProductPage';
import { ProductListPage } from 'pages/authorized/products/ProductListPage';
import React from 'react';
import { Route, Routes } from 'react-router';
import { UiKitPage } from './uikit/UiKitPage';
import { DropDownInput } from '../../components/uikit/inputs/dropdown/DropDownInput';
import {
  changeLanguage,
  getCurrentSupportedLanguage,
} from '../../application/localization/localization';
import { Language, languages } from '../../application/localization/locales';
import { useTranslation } from 'react-i18next';
import { setAuthData } from '../../helpers/interceptors/auth/auth-interceptor';
const styles = require('./RootPage.module.scss');

export const RootPage: React.FC = () => {
  const dispatch = useAppDispatch();
  const i18n = useTranslation();
  return (
    <>
      <div className={styles.container}>
        <div className={styles.content}>
          <Routes>
            <Route path={Links.Authorized.UiKit} element={<UiKitPage />} />
            <Route
              path={Links.Authorized.CreateProduct}
              element={<CreateProductPage />}
            />
            <Route
              path={Links.Authorized.ProductDetails()}
              element={<CreateProductPage />}
            />
            <Route
              path={Links.Authorized.Products}
              element={<ProductListPage />}
            />
            <Route path={'/*'} element={<ProductListPage />} />
          </Routes>
        </div>
        <div className={styles.bottomNavigation}>
          <div>Version: {appVersion()}</div>
          <div>
            <DropDownInput
              options={languages}
              required={true}
              value={getCurrentSupportedLanguage(i18n.i18n)}
              onSelectedOptionChanged={(x) => {
                changeLanguage(x as Language);
              }}
            />
          </div>
          <div className={styles.logOutWrapper}>
            <Button
              onClick={() => {
                setAuthData(null);
              }}
              title={'Log Out'}
            />
          </div>
        </div>
      </div>
    </>
  );
};
