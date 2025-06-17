declare module '*.svg?react' {
  const content: React.FC<
    React.SVGProps<SVGSVGElement> & {
      testId?: string;
      title?: string;
      className?: string;
    }
  >;
  export default content;
}
declare module '*.ttf' {}
