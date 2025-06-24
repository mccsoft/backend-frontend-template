import { createBrowserRouter } from 'react-router-dom';
import { ServerSideLoginPage } from './unauthorized/ServerSideLoginPage';
import { RootPage } from './authorized/RootPage';
import { Links } from 'application/constants/links';
import { UiKitPage } from './authorized/uikit/UiKitPage';
import { CreateProductPage } from './authorized/products/create/CreateProductPage';
import { EditProductPage } from './authorized/products/edit/EditProductPage';
import { ProductListPage } from './authorized/products/ProductListPage';
import { ProductDetailsPage } from './authorized/products/details/ProductDetailsPage';
import { ReactRouterErrorBoundary } from './ReactRouterErrorBoundary';
import { LoginPage } from './unauthorized/LoginPage';
import { UseCookieAuth } from 'helpers/auth/auth-settings';

export const authorizedRoutes = () =>
  createBrowserRouter([
    {
      path: '/',
      element: <RootPage />,
      children: [
        { path: Links.Authorized.UiKit.route, element: <UiKitPage /> },
        {
          path: Links.Authorized.CreateProduct.route,
          element: <CreateProductPage />,
        },
        {
          path: Links.Authorized.EditProduct.route,
          element: <EditProductPage />,
        },
        {
          path: Links.Authorized.Products.route,
          element: <ProductListPage />,
        },
        {
          path: Links.Authorized.ProductDetails.route,
          element: <ProductDetailsPage />,
        },
        { path: '', element: <ProductListPage /> },
      ],
      ErrorBoundary: ReactRouterErrorBoundary,
    },
  ]);
export const anonymousRoutes = () =>
  createBrowserRouter([
    {
      path: '/*',
      element: UseCookieAuth ? <LoginPage /> : <ServerSideLoginPage />,
      ErrorBoundary: ReactRouterErrorBoundary,
    },
    {
      path: Links.Unauthorized.Login.route,
      element: <LoginPage />,
    },
  ]);
