import {
  BooleanParam,
  StringParam,
  useQueryParams,
} from 'react-router-url-params';
import { SortingState, Updater } from '@tanstack/table-core';
import { OnChangeFn } from '@tanstack/react-table';
import { useCallback, useMemo, useRef } from 'react';

export const useSortBy: () => {
  onSortingChange: OnChangeFn<SortingState>;
  sortingState: SortingState;
} = () => {
  const [{ sortBy, desc }, setQuery] = useQueryParams({
    sortBy: StringParam,
    desc: BooleanParam,
  });

  const sortingState: SortingState = useMemo(() => {
    if (!sortBy) {
      return [];
    }
    return [{ id: sortBy, desc: !!desc }];
  }, [sortBy, desc]);
  const sortingStateRef = useRef(sortingState);
  sortingStateRef.current = sortingState;

  const onSortingChange = useCallback(
    (updaterOrValue: Updater<SortingState>) => {
      const newState =
        typeof updaterOrValue === 'function'
          ? updaterOrValue(sortingStateRef.current)
          : updaterOrValue;

      const sortByArray = newState;
      if (sortByArray.length === 0) {
        setQuery({
          sortBy: undefined,
          desc: undefined,
        });
        return;
      }
      const sortByValue = sortByArray[0];
      setQuery({
        sortBy: sortByValue.id,
        desc: sortByValue.desc,
      });
    },
    [setQuery],
  );

  return {
    sortingState: sortingState,
    onSortingChange: onSortingChange,
  };
};
