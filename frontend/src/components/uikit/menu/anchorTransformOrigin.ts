import { MenuDirection } from './MenuDirection';
import { MenuProps } from '@mui/material';

export const anchorTransformOrigin: Record<
  MenuDirection,
  {
    anchorOrigin: MenuProps['anchorOrigin'];
    transformOrigin: MenuProps['transformOrigin'];
  }
> = {
  [MenuDirection.topLeftEdge]: {
    anchorOrigin: { vertical: 'top', horizontal: 'left' },
    transformOrigin: { vertical: 'bottom', horizontal: 'right' },
  },
  [MenuDirection.topCenter]: {
    anchorOrigin: { vertical: 'top', horizontal: 'center' },
    transformOrigin: { vertical: 'bottom', horizontal: 'center' },
  },
  [MenuDirection.topRightEdge]: {
    anchorOrigin: { vertical: 'top', horizontal: 'right' },
    transformOrigin: { vertical: 'bottom', horizontal: 'left' },
  },
  [MenuDirection.bottomLeftEdge]: {
    anchorOrigin: { vertical: 'bottom', horizontal: 'left' },
    transformOrigin: { vertical: 'top', horizontal: 'right' },
  },
  [MenuDirection.bottomCenter]: {
    anchorOrigin: { vertical: 'bottom', horizontal: 'center' },
    transformOrigin: { vertical: 'top', horizontal: 'center' },
  },
  [MenuDirection.bottomRightEdge]: {
    anchorOrigin: { vertical: 'bottom', horizontal: 'right' },
    transformOrigin: { vertical: 'top', horizontal: 'left' },
  },
  [MenuDirection.leftTopEdge]: {
    anchorOrigin: { vertical: 'top', horizontal: 'left' },
    transformOrigin: { vertical: 'top', horizontal: 'right' },
  },
  [MenuDirection.leftCenter]: {
    anchorOrigin: { vertical: 'center', horizontal: 'left' },
    transformOrigin: { vertical: 'center', horizontal: 'right' },
  },
  [MenuDirection.leftBottomEdge]: {
    anchorOrigin: { vertical: 'bottom', horizontal: 'left' },
    transformOrigin: { vertical: 'top', horizontal: 'right' },
  },
  [MenuDirection.rightTopEdge]: {
    anchorOrigin: { vertical: 'top', horizontal: 'right' },
    transformOrigin: { vertical: 'top', horizontal: 'left' },
  },
  [MenuDirection.rightCenter]: {
    anchorOrigin: { vertical: 'center', horizontal: 'right' },
    transformOrigin: { vertical: 'center', horizontal: 'left' },
  },
  [MenuDirection.rightBottomEdge]: {
    anchorOrigin: { vertical: 'bottom', horizontal: 'right' },
    transformOrigin: { vertical: 'top', horizontal: 'left' },
  },
};
