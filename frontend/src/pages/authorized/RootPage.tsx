import { appVersion } from 'application/constants/env-variables';
import { Routes } from 'application/constants/routes';
import { AuthActions } from 'application/redux-store/auth/auth-reducer';
import { useAppDispatch } from 'application/redux-store/root-store';
import { Button } from 'components/uikit/buttons/Button';
import { CreateProductPage } from 'pages/authorized/products/create/CreateProductPage';
import { ProductListPage } from 'pages/authorized/products/ProductListPage';
import React from 'react';
import { Route, Switch } from 'react-router';
import { UiKitPage } from './uikit/UiKitPage';
const styles = require('./RootPage.module.scss');

export const RootPage: React.FC = () => {
  const dispatch = useAppDispatch();
  return (
    <>
      <div className={styles.container}>
        <div className={styles.content}>
          <Switch>
            <Route path={Routes.Authorized.UiKit} component={UiKitPage} />
            <Route
              path={Routes.Authorized.CreateProduct}
              component={CreateProductPage}
            />
            <Route
              path={Routes.Authorized.ProductDetails()}
              component={CreateProductPage}
            />

            <Route component={ProductListPage} />
          </Switch>
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
