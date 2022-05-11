declare module '*.css';
declare module '*.scss';
declare module '*.module.scss';
declare module '*.svg' {
  const ReactComponent: React.FC<
    React.SVGProps<SVGSVGElement> & {
      testid?: string;
      title?: string;
      tst?: string;
    }
  >;
  const content: string;

  export { ReactComponent };
  export default content;
}
