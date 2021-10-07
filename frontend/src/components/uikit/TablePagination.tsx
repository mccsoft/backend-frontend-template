import React, { FC } from 'react';
import ReactPaginate from 'react-paginate';

import { ReactComponent as PreviousPageIcon } from 'assets/icons/left.svg';
import { ReactComponent as NextPageIcon } from 'assets/icons/right.svg';
const styles = require('./TablePagination.module.scss');

// const items = [10, 20, 50];

export interface Props {
  page: number;
  perPage: number;
  totalCount: number;
  changePagination: (pagination: { page?: number; perPage?: number }) => void;
}

export const TablePagination: FC<Props> = (props) => {
  if (props.totalCount === 0) {
    return null;
  }
  const perPage = props.perPage;
  return (
    <div className={styles.container}>
      {/*<div className={styles.rowsCountContainer}>*/}
      {/*  <div className={styles.rowsCountTitle}>{props.pageLimitTitle}</div>*/}
      {/*  {items.map((perPage) => (*/}
      {/*    <div*/}
      {/*      onClick={*/}
      {/*        currentPerPage === perPage*/}
      {/*          ? undefined*/}
      {/*          : () =>*/}
      {/*              changePagination({*/}
      {/*                page: defaultPagination.page, // always render first page if Limit is changed*/}
      {/*                perPage: perPage,*/}
      {/*              })*/}
      {/*      }*/}
      {/*      className={*/}
      {/*        currentPerPage === perPage*/}
      {/*          ? styles.rowsCountSelected*/}
      {/*          : styles.rowsCount*/}
      {/*      }*/}
      {/*      key={perPage}*/}
      {/*    >*/}
      {/*      {perPage}*/}
      {/*    </div>*/}
      {/*  ))}*/}
      {/*</div>*/}
      <div>
        <ReactPaginate
          pageCount={(props.totalCount ?? 0) / perPage}
          forcePage={props.page - 1}
          pageRangeDisplayed={3}
          marginPagesDisplayed={2}
          previousLabel={<PreviousPageIcon />}
          nextLabel={<NextPageIcon />}
          breakLabel={'...'}
          breakClassName={styles.break}
          containerClassName={styles.paginationContainer}
          pageLinkClassName={styles.page}
          activeLinkClassName={styles.activePage}
          nextLinkClassName={styles.pageIcon}
          previousLinkClassName={styles.pageIcon}
          onPageChange={(e) => {
            props.changePagination({
              page: e.selected + 1,
            });
          }}
        />
      </div>
    </div>
  );
};
