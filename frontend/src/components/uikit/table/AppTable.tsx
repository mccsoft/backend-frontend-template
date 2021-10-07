import { TableInstance } from 'react-table';
import React, { useRef } from 'react';
import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@material-ui/core';

export const emptyArray: any[] = [];
export function AppTable<D extends object = object>(props: {
  table: TableInstance<D>;
}) {
  const tableObj = props.table;
  const tableRef = useRef();
  return (
    <TableContainer
      ref={tableRef}
      // className={styles.tableContainer}
      component={Paper}
    >
      <Table
        stickyHeader
        aria-label="sticky table"
        {...(tableObj.getTableProps() as any)}
      >
        <TableHead>
          {tableObj.headerGroups.map((headerGroup) => (
            <TableRow {...(headerGroup.getHeaderGroupProps() as any)}>
              {headerGroup.headers.map((column: any) => (
                <TableCell
                  {...column.getHeaderProps(column.getSortByToggleProps())}
                >
                  {column.render('Header')}
                  <span>
                    {column.isSorted
                      ? column.isSortedDesc
                        ? ' 🔽'
                        : ' 🔼'
                      : ''}
                  </span>
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableHead>
        <TableBody {...(tableObj.getTableBodyProps() as any)}>
          {tableObj.rows.map((row) => {
            tableObj.prepareRow(row);
            return (
              <TableRow {...(row.getRowProps() as any)}>
                {row.cells.map((cell) => {
                  return (
                    <TableCell {...(cell.getCellProps() as any)}>
                      {cell.render('Cell')}
                    </TableCell>
                  );
                })}
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
