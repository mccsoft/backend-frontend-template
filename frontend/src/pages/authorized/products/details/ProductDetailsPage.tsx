import { Routes } from 'application/constants/routes';
import { AppLink } from 'components/uikit/buttons/AppLink';
import { ButtonColor } from 'components/uikit/buttons/Button';
import { Loading } from 'components/uikit/suspense/Loading';
import { useHistory } from 'react-router';
import React from 'react';
import { QueryFactory } from 'services/api';

export const ProductDetailsPage: React.FC<{ id: number }> = (props) => {
  const history = useHistory();

  const productQuery = QueryFactory.ProductQuery.useGetQuery(props.id);

  return (
    <>
      <AppLink
        onClick={() => {
          history.push(Routes.Authorized.Products);
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
