// Adapter for react-window
import React, { ComponentType, useCallback, useMemo } from 'react';
import styles from './StyledAutocomplete.module.scss';
import {
  FixedSizeList as _VirtualList,
  FixedSizeListProps,
  ListChildComponentProps,
} from 'react-window';

const VirtualList = _VirtualList as ComponentType<FixedSizeListProps>;

export type VirtualizedListboxComponentProps = {
  itemSize: number;
} & React.HTMLAttributes<HTMLElement>;

export const VirtualizedListboxComponent = React.forwardRef<
  HTMLDivElement,
  VirtualizedListboxComponentProps
>(function ListboxComponent(props, ref) {
  const { children, itemSize, ...other } = props;
  const items = children as [];

  const itemCount = items.length;
  const getHeight = () => {
    if (itemCount > 8) {
      return 8 * itemSize + 1;
    }
    return itemCount * itemSize + 1;
  };

  const outerElementType = useMemo(() => {
    return React.forwardRef<HTMLDivElement>((props, ref) => {
      return <div ref={ref} {...props} {...other} />;
    });
  }, []);

  const renderRow = useCallback(function (props: ListChildComponentProps) {
    const { data, index, style } = props;

    return <div style={style}>{data[index]}</div>;
  }, []);

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
