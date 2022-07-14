import { Links } from 'application/constants/links';
import { AppLink } from 'components/uikit/buttons/AppLink';
import { Loading } from 'components/uikit/suspense/Loading';
import React from 'react';
import { QueryFactory } from 'services/api';
import { parseIntOrThrow } from '../../../../application/constants/create-link';

export const ProductDetailsPage: React.FC = (props) => {
  const { id } = Links.Authorized.ProductDetails.useParams();
  const productId = parseIntOrThrow(id);
  const productQuery = QueryFactory.ProductQuery.useGetQuery(productId);

  return (
    <>
      <AppLink to={Links.Authorized.Products.link()}>Back</AppLink>
      <Loading loading={productQuery.isLoading}>
        <div>Product: {productQuery.data?.title}</div>
        <AppLink to={Links.Authorized.EditProduct.link({ id: productId })}>
          Edit
        </AppLink>
      </Loading>
    </>
  );
};
