// Adapter for react-window
import React, { ComponentType, useCallback, useMemo, useRef } from 'react';
import styles from './StyledAutocomplete.module.scss';
import {
  FixedSizeList as _VirtualList,
  FixedSizeListProps,
  ListChildComponentProps,
} from 'react-window';
import { AutocompleteRenderOptionState } from '@mui/material';

const VirtualList = _VirtualList as ComponentType<FixedSizeListProps>;

export type VirtualizedListboxComponentProps = {
  itemSize: number;
  renderOption: (
    props: React.HTMLAttributes<HTMLLIElement>,
    option: any,
    state: AutocompleteRenderOptionState,
  ) => React.ReactNode;
} & React.HTMLAttributes<HTMLElement>;

export const VirtualizedListboxComponent = React.forwardRef<
  HTMLDivElement,
  VirtualizedListboxComponentProps
>(function ListboxComponent(props, ref) {
  const { children, itemSize, renderOption, ...other } = props;
  const items = children as [];

  const itemCount = items.length;
  const getHeight = () => {
    if (itemCount > 8) {
      return 8 * itemSize + 1;
    }
    return itemCount * itemSize + 1;
  };

  const otherRef = useRef(other);
  otherRef.current = other;
  const outerElementType = useMemo(() => {
    return React.forwardRef<HTMLDivElement>((props, ref) => {
      return <div ref={ref} {...props} {...otherRef.current} />;
    });
  }, []);

  const renderRow = useCallback(
    function (props: ListChildComponentProps) {
      const { data, index, style } = props;
      const dataSet = data[index];
      const [liProps, option, state] = dataSet;

      return renderOption(
        { ...liProps, style: { ...style, ...liProps.style } },
        option,
        state,
      ) as any;
    },
    [renderOption],
  );

  return (
    <div ref={ref} className={styles.virtualizedList}>
      <VirtualList
        itemData={items}
        height={getHeight()}
        width="100%"
        outerElementType={outerElementType}
        innerElementType="ul"
        itemSize={props.itemSize}
        overscanCount={5}
        itemCount={itemCount}
      >
        {renderRow}
      </VirtualList>
    </div>
  );
});
