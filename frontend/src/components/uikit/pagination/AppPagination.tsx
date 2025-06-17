import React, { FC } from 'react';
import ReactPaginate from 'react-paginate';

import PreviousPageIcon from 'assets/icons/left.svg?react';
import NextPageIcon from 'assets/icons/right.svg?react';
import styles from './AppPagination.module.scss';

// const items = [10, 20, 50];

export type Props = {
  page: number;
  perPage: number;
  totalCount: number;
  changePagination: (pagination: { page?: number; perPage?: number }) => void;
};

export const AppPagination: FC<Props> = (props) => {
  if (props.totalCount === 0) {
    return null;
  }
  const perPage = props.perPage;
  return (
    <div className={styles.container}>
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
  );
};
