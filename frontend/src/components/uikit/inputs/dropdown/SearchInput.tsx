import { Input, Props } from '../Input';
import { ReactComponent as SearchIcon } from 'assets/icons/search.svg';

export const SearchInput: React.FC<Props> = (props) => {
  return <Input endAdornment={<SearchIcon />} variant={'normal'} {...props} />;
};
