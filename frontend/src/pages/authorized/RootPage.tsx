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
import { logOut } from 'helpers/auth/auth-interceptor';
import styles from './RootPage.module.scss';
import Logger from 'js-logger';
import { ProductDetailsPage } from './products/details/ProductDetailsPage';
import { EditProductPage } from './products/edit/EditProductPage';

export const RootPage: React.FC = () => {
  const i18n = useTranslation();
  return (
    <>
      <div className={styles.container} data-test-id="main-page-container">
        <div className={styles.content}>
          <Routes>
            <Route
              path={Links.Authorized.UiKit.route}
              element={<UiKitPage />}
            />
            <Route
              path={Links.Authorized.CreateProduct.route}
              element={<CreateProductPage />}
            />
            <Route
              path={Links.Authorized.EditProduct.route}
              element={<EditProductPage />}
            />
            <Route
              path={Links.Authorized.Products.route}
              element={<ProductListPage />}
            />
            <Route
              path={Links.Authorized.ProductDetails.route}
              element={<ProductDetailsPage />}
            />
            <Route path={'/*'} element={<ProductListPage />} />
          </Routes>
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
