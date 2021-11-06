import { appVersion } from 'application/constants/env-variables';
import { Links } from 'application/constants/links';
import { AuthActions } from 'application/redux-store/auth/auth-reducer';
import { useAppDispatch } from 'application/redux-store/root-store';
import { Button } from 'components/uikit/buttons/Button';
import { CreateProductPage } from 'pages/authorized/products/create/CreateProductPage';
import { ProductListPage } from 'pages/authorized/products/ProductListPage';
import React from 'react';
import { Route, Routes } from 'react-router';
import { UiKitPage } from './uikit/UiKitPage';
const styles = require('./RootPage.module.scss');

export const RootPage: React.FC = () => {
  const dispatch = useAppDispatch();
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
          <div className={styles.logOutWrapper}>
            <Button
              onClick={() => {
                dispatch(AuthActions.logoutAction());
              }}
              title={'Log Out'}
            />
          </div>
        </div>
      </div>
    </>
  );
};
