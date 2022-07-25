import {
  createRoute,
  RequiredNumberParam,
  StringParam,
} from 'react-router-url-params';

export const Links = {
  Unauthorized: {
    Login: createRoute('/login'),
  },
  Authorized: {
    Dashboard: createRoute('/'),
    Products: createRoute('/products'),
    ProductDetails: createRoute('/products/:id', {
      id: RequiredNumberParam,
    }),
    CreateProduct: createRoute('/products/create'),
    EditProduct: createRoute('/products/:id/edit', {
      id: RequiredNumberParam,
    }),
    UiKit: createRoute('/uikit'),
  },
};
