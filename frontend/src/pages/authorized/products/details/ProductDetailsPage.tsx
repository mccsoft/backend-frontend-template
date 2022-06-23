import { Links } from 'application/constants/links';
import { AppLink } from 'components/uikit/buttons/AppLink';
import { Loading } from 'components/uikit/suspense/Loading';
import React from 'react';
import { QueryFactory } from 'services/api';

export const ProductDetailsPage: React.FC = (props) => {
  const p2 = Links.Authorized.ProductDetails.useParams();
  console.log('qweqwe', p2);
  const productQuery = QueryFactory.ProductQuery.useGetQuery(parseInt(p2.id!));

  return (
    <>
      <AppLink to={Links.Authorized.Products.link()}>Back</AppLink>
      <Loading loading={productQuery.isLoading}>
        <div>Product: {productQuery.data?.title}</div>
      </Loading>
    </>
  );
};
