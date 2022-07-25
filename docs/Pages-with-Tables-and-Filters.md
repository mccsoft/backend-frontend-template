# How to handle pages with filters and data tables

Examples: Patient list, Product list, etc.

**Idea**: all filters are stored in URL. URL is the source of truth.

**Why**: That way we will be sure that after F5 the user will see the same results. Also if URL is sent & opened by colleague, he/she will see the same results.

## How to implement it

1. Copy&paste the following example
1. Adjust the filter definition in `useQueryParams` function (by default there's a single `search` parameter and paging/sorting).
1. To change filter parameters call `setQueryParams({ search: 'qwe' })` (or pass any other filter/paging parameter you like).
1. All current filter values (e.g. for passing to react-query) should be taken from `queryParams` (e.g. `queryParams.search`).
```TS
    import { StringParam, useQueryParams } from 'react-router-url-params';


    const [queryParams, setQueryParams] = useQueryParams({
      search: StringParam,
      ...pagingSortingQueryParams(2),
    });
    const productsQuery = QueryFactory.ProductQuery.useSearchQuery({
      search: queryParams.search,
      ...pagingSortingToBackendRequest(queryParams),
    });

```
