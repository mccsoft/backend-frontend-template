import { Routes } from 'application/constants/routes';
import {AppTable, emptyArray} from 'components/uikit/table/AppTable';
import { AppLink } from 'components/uikit/buttons/AppLink';
import { ButtonColor } from 'components/uikit/buttons/Button';
import { Input } from 'components/uikit/inputs/Input';
import { Loading } from 'components/uikit/suspense/Loading';
import { TablePagination } from 'components/uikit/TablePagination';
import {
  pagingSortingQueryParams,
  pagingSortingToBackendRequest,
} from 'helpers/pagination-helper';
import { useUpdateSortByInUrl } from 'components/uikit/table/useUpdateSortByInUrl';
import React, { useMemo } from 'react';
import { useHistory } from 'react-router';
import { useSortBy, useTable } from 'react-table';
import { QueryFactory } from 'services/api';
import { ProductListItemDto } from 'services/api/api-client';
import { StringParam, useQueryParams } from 'use-query-params';

export const ProductListPage: React.FC = () => {
  const history = useHistory();

  const [queryParams, setQueryParams] = useQueryParams({
    search: StringParam,
    ...pagingSortingQueryParams(2),
  });
  const productsQuery = QueryFactory.ProductQuery.useSearchQuery({
    search: queryParams.search,
    ...pagingSortingToBackendRequest(queryParams),
  });

  const table = useTable<ProductListItemDto>(
    {
      data: productsQuery.data?.data ?? emptyArray,
      columns: useMemo(() => {
        return [
          {
            accessor: 'title',
            Cell: ({ row }) => (
              <div>
                {row.original.id}. {row.original.title}
              </div>
            ),
            width: 'auto',
            Header: 'Title',
          },
        ];
      }, []),
      manualSortBy: true,
      initialState: useMemo(
        () => ({
          sortBy: queryParams.sortBy
            ? [
                {
                  id: queryParams.sortBy,
                  desc: queryParams.desc,
                },
              ]
            : [],
        }),
        [],
      ),
    },
    useSortBy,
  );
  useUpdateSortByInUrl(table.state.sortBy);

  return (
    <div>
      <AppLink
        color={ButtonColor.Primary}
        onClick={() => {
          history.push(Routes.Authorized.CreateProduct);
        }}
      >
        Create product
      </AppLink>
      <Input
        placeholder={'search'}
        defaultValue={queryParams.search ?? undefined}
        onChange={(e) => setQueryParams({ search: e.target.value, page: 1 })}
      />

      <Loading loading={productsQuery.isLoading}>
        <AppTable table={table} />
        <TablePagination
          page={queryParams.page}
          perPage={queryParams.perPage}
          totalCount={productsQuery.data?.totalCount ?? 0}
          changePagination={setQueryParams}
        />
      </Loading>
    </div>
  );
};
