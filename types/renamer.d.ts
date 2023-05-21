declare module 'renamer' {
  export default class Renamer {
    public rename(options: {
      files: string | string[];
      find: string | RegExp;
      replace: strint;
      dryRun?: boolean;
    }) {}
  }
}
