declare module 'node-replace' {
  export default function replace(options: {
    regex: string | RegExp;
    replacement: string;
    paths: string[];
    recursive?: boolean;
    silent?: boolean;
    exclude?: string | string[];
  }) {}
}
