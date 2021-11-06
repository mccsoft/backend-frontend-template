import { Links } from 'application/constants/links';
import { AppLink } from 'components/uikit/buttons/AppLink';
import { Loading } from 'components/uikit/suspense/Loading';
import React from 'react';
import { QueryFactory } from 'services/api';
import { useNavigate } from 'react-router';

export const ProductDetailsPage: React.FC<{ id: number }> = (props) => {
  const navigate = useNavigate();

  const productQuery = QueryFactory.ProductQuery.useGetQuery(props.id);

  return (
    <>
      <AppLink
        onClick={() => {
          navigate(Links.Authorized.Products);
        }}
      >
        Back
      </AppLink>
      <Loading loading={productQuery.isLoading}>
        <div>Product: {productQuery.data?.title}</div>
      </Loading>
    </>
  );
};
