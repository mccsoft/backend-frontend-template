// Adapter for react-window
import React, { ComponentType, useCallback, useMemo } from 'react';
import styles from './StyledAutocomplete.module.scss';
import {
  FixedSizeList as _VirtualList,
  FixedSizeListProps,
  ListChildComponentProps,
} from 'react-window';
import { CustomOption } from './types';
import clsx from 'clsx';
const VirtualList = _VirtualList as ComponentType<FixedSizeListProps>;

export const VirtualizedListboxComponent = React.forwardRef<
  HTMLDivElement,
  {
    itemSize: number;
    customOptions?: CustomOption[];
    optionClasses?: string;
  } & React.HTMLAttributes<HTMLElement>
>(function ListboxComponent(props, ref) {
  const { children, itemSize, customOptions, optionClasses, ...other } = props;
  const items = children as [];

  const itemCount = items.length + (customOptions?.length ?? 0);
  const customOptionsNonNullable = customOptions ?? [];
  const getHeight = () => {
    if (itemCount > 8) {
      return 8 * itemSize;
    }
    return itemCount * itemSize;
  };

  const outerElementType = useMemo(() => {
    return (props: any) => <div {...props} {...other} />;
  }, []);

  const renderRow = useCallback(function (props: ListChildComponentProps) {
    const { data, index, style } = props;
    const customOption = customOptionsNonNullable[index];
    const liElement = customOption ? (
      <li
        className={clsx('MuiAutocomplete-option', optionClasses)}
        onClick={customOption.onClick}
      >
        {customOption.label}
      </li>
    ) : (
      data[index - customOptionsNonNullable.length]
    );

    return <li style={style}>{liElement}</li>;
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
