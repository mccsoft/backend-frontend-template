import { SortingRule } from 'react-table';
import { BooleanParam, StringParam, useQueryParams } from 'use-query-params';
import { useEffect } from 'react';

export function useUpdateSortByInUrl(sortBy: Array<SortingRule<any>>) {
  const [, setQuery] = useQueryParams({
    field: StringParam,
    desc: BooleanParam,
  });
  useEffect(
    function updateSortingState() {
      const sortByArray = sortBy;
      if (sortByArray.length === 0) {
        setQuery({
          field: undefined,
          desc: undefined,
        });
        return;
      }
      const sortByValue = sortByArray[0];
      setQuery({
        field: sortByValue.id,
        desc: sortByValue.desc,
      });
    },
    [sortBy],
  );
}
