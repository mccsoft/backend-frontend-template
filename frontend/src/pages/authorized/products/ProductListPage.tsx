import { Links } from 'application/constants/links';
import { AppTable } from 'components/uikit/table/AppTable';
import { AppLink } from 'components/uikit/buttons/AppLink';
import { ButtonColor } from 'components/uikit/buttons/Button';
import { Input } from 'components/uikit/inputs/Input';
import { Loading } from 'components/uikit/suspense/Loading';
import {
  pagingSortingQueryParams,
  pagingSortingToBackendRequest,
} from 'helpers/pagination-helper';
import { useSortBy } from 'components/uikit/table/updateSortByInUrl';
import React, { useMemo } from 'react';
import {
  getCoreRowModel,
  getSortedRowModel,
  useReactTable,
} from '@tanstack/react-table';
import { QueryFactory } from 'services/api';
import { ProductListItemDto } from 'services/api/api-client';
import { localFormat } from '../../../helpers/date-helpers';
import { useScopedTranslation } from '../../../application/localization/useScopedTranslation';
import styles from './ProductListPage.module.scss';
import { StringParam, useQueryParams } from 'react-router-url-params';
import { DotMenu } from 'components/uikit/menu/DotMenu';
import { useNavigate } from 'react-router';
import { AppPagination } from 'components/uikit/pagination/AppPagination';
import { MenuDirection } from '../../../components/uikit/menu/MenuDirection';
import { emptyArray } from 'helpers/empty-array';
import { useModal } from 'components/uikit/modal/useModal';

export const ProductListPage: React.FC = () => {
  const i18n = useScopedTranslation('Page.Products.list');
  const navigate = useNavigate();
  const modal = useModal();
  const [queryParams, setQueryParams] = useQueryParams({
    search: StringParam,
    ...pagingSortingQueryParams(2),
  });
  const productsQuery = QueryFactory.ProductQuery.useSearchQuery({
    search: queryParams.search,
    ...pagingSortingToBackendRequest(queryParams),
  });

  const sortBy = useSortBy();
  const table = useReactTable<ProductListItemDto>({
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    manualSorting: true,
    onSortingChange: sortBy.onSortingChange,
    state: {
      sorting: sortBy.sortingState,
    },
    data: productsQuery.data?.data ?? emptyArray(),
    columns: useMemo(() => {
      return [
        {
          accessorKey: 'title',
          enableSorting: true,
          cell: ({ row }) => (
            <div>
              <AppLink
                to={Links.Authorized.ProductDetails.link({
                  id: row.original.id,
                })}
              >
                {row.original.id}. {row.original.title}
              </AppLink>{' '}
              ({row.original.productType}) -{' '}
              {localFormat(row.original.lastStockUpdatedAt, 'P')}
            </div>
          ),
          size: 5000,
          header: i18n.t('column_title'),
        },
        {
          id: 'menu',
          cell: ({ row }) => {
            return (
              <DotMenu
                menuItems={[
                  {
                    key: 'edit',
                    text: 'Edit',
                    onClick: () =>
                      navigate(
                        Links.Authorized.EditProduct.link({
                          id: row.original.id,
                        }),
                      ),
                  },
                  {
                    key: 'delete',
                    text: 'Delete',
                    isDanger: true,
                    onClick: async () =>
                      await modal.showConfirm({
                        text: 'Are you sure you want to delete this product?',
                        title: 'Delete confirmation',
                      }),
                  },
                ]}
                direction={MenuDirection.bottomLeftEdge}
              />
            );
          },
          size: 40,
          maxSize: 40,
          header: '',
        },
      ];
    }, [i18n.i18n.language]),
  });

  return (
    <div>
      <div className={styles.navigation}>
        <AppLink
          color={ButtonColor.Primary}
          to={Links.Authorized.CreateProduct.link()}
        >
          Create product
        </AppLink>
        <AppLink color={ButtonColor.Primary} to={Links.Authorized.UiKit.link()}>
          UiKit
        </AppLink>
      </div>
      <Input
        placeholder={'search'}
        defaultValue={queryParams.search ?? undefined}
        onChange={(e) => setQueryParams({ search: e.target.value, page: 1 })}
      />

      <Loading loading={productsQuery.isInitialLoading}>
        <AppTable table={table} />
        <AppPagination
          page={queryParams.page}
          perPage={queryParams.perPage}
          totalCount={productsQuery.data?.totalCount ?? 0}
          changePagination={setQueryParams}
        />
      </Loading>
    </div>
  );
};
