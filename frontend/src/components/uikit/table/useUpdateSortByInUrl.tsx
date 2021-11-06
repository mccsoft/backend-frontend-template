import { SortingRule } from 'react-table';
import { BooleanParam, StringParam, useQueryParams } from 'use-query-params';
import { useEffect, useRef } from 'react';

export function useUpdateSortByInUrl(sortBy: Array<SortingRule<any>>) {
  const [query, setQuery] = useQueryParams({
    sortBy: StringParam,
    desc: BooleanParam,
  });
  const currentQueryParamsRef = useRef(query);
  useEffect(() => {
    currentQueryParamsRef.current = query;
  }, [query]);

  useEffect(
    function updateSortingState() {
      const sortByArray = sortBy;
      if (sortByArray.length === 0) {
        if (
          currentQueryParamsRef.current.sortBy !== undefined ||
          currentQueryParamsRef.current.desc !== undefined
        ) {
          setQuery({
            sortBy: undefined,
            desc: undefined,
          });
        }
        return;
      }
      const sortByValue = sortByArray[0];
      if (
        currentQueryParamsRef.current.sortBy !== sortByValue.id ||
        currentQueryParamsRef.current.desc !== sortByValue.desc
      ) {
        setQuery({
          sortBy: sortByValue.id,
          desc: sortByValue.desc,
        });
      }
    },
    [sortBy],
  );
}
