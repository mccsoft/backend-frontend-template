import { flexRender, Table as TanTable } from '@tanstack/react-table';
import React, { useRef } from 'react';
import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@mui/material';
import styles from './AppTable.module.scss';

export function AppTable<D extends object = object>(props: {
  table: TanTable<D>;
}) {
  const { table } = props;
  const tableRef = useRef<HTMLDivElement>(null);
  return (
    <TableContainer ref={tableRef} component={Paper}>
      <Table stickyHeader aria-label="sticky table">
        <TableHead>
          {table.getHeaderGroups().map((headerGroup) => (
            <TableRow key={headerGroup.id}>
              {headerGroup.headers.map((header) => (
                <TableCell
                  key={header.id}
                  {...{
                    className: header.column.getCanSort()
                      ? styles.sortableColumnHeader
                      : undefined,
                    onClick: header.column.getToggleSortingHandler(),
                  }}
                  style={{ width: header.column.getSize() }}
                >
                  <>
                    {flexRender(
                      header.column.columnDef.header,
                      header.getContext(),
                    )}
                    {{
                      asc: ' 🔼',
                      desc: ' 🔽',
                    }[header.column.getIsSorted() as string] ?? null}
                  </>
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableHead>
        <TableBody>
          {table.getRowModel().rows.map((row) => {
            return (
              <TableRow key={row.id}>
                {row.getVisibleCells().map((cell) => {
                  return (
                    <TableCell key={cell.id}>
                      {flexRender(
                        cell.column.columnDef.cell,
                        cell.getContext(),
                      )}
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
