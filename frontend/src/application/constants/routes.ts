export const Routes = {
  Unauthorized: {
    Login: '/login',
  },
  Authorized: {
    Dashboard: '/',
    Products: '/products',
    ProductDetails: (id?: number) => `/products/${id ?? ':id'}`,
    CreateProduct: '/products/create',
    UiKit: '/uikit',
  },
};
