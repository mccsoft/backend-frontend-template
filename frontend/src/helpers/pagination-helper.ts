import { SortOrder } from 'services/api/api-client';
import {
  BooleanParam,
  NumberParam,
  StringParam,
  withDefault,
} from 'react-router-url-params';

export const sortingQueryParams = (defaultDesc?: boolean) => ({
  sortBy: StringParam,
  desc: withDefault(BooleanParam, defaultDesc ?? false),
});
export const pagingQueryParams = (defaultPerPage: number) => ({
  page: withDefault(NumberParam, 1),
  perPage: withDefault(NumberParam, defaultPerPage),
});
export const pagingSortingQueryParams = (defaultPerPage: number) => ({
  ...pagingQueryParams(defaultPerPage),
  ...sortingQueryParams(),
});
export function pagingSortingToBackendRequest(query: {
  sortBy?: string | null;
  desc?: boolean;
  page: number;
  perPage: number;
}): {
  offset?: number | null;
  limit?: number | null;
  sortBy?: string | null;
  sortOrder?: SortOrder | undefined;
} {
  return {
    offset: (query.page - 1) * query.perPage,
    limit: query.perPage,
    sortBy: query.sortBy ?? undefined,
    sortOrder: query.desc ? SortOrder.Desc : SortOrder.Asc,
  };
}
