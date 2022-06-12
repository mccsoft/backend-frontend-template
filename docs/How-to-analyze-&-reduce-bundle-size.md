# The goal
Your bundle size is too big, which causes initial load of your SPA to be too long. You want to optimize the initial loading time by using code-splitting, to only load essential JS initially, and load other JS on demand.
E.g. in case of Unicorn (before optimizations), you could see the following picture in Network tab of Chrome:

You see, that initial bundle size is N megabytes. Obviously, that's too much.

# How to do proper code-splitting
Let's consider an example. You have a relatively big SPA with many pages and you want to speed up the initial loading of your page.
To speed it up, you have to split your application to parts. The most common technique is to split by pages (some pages are imported via [React.lazy](https://reactjs.org/docs/code-splitting.html#code-splitting), not via regular imports), so that the code for the page will only be loaded when the page is opened.

That especially makes sense for pages, that are opened rarely, or only by Admin users, so it doesn't make sense to always load them for all users.
Hint: usually your own code for pages is not that big. It's 3rd party packages (node_modules) that make your bundle big. So, it makes sense to split the pages that use heavy 3rd party libraries (if these libraries are specific for these pages).

So, how shall we decide what exactly to split? Let's proceed with step by step instructions.

# Step by step instructions
1. ##Prepare the tools and gather the numbers for analisys
    1. Install the following development packages to help analyze the issue `yarn add --dev source-map-explorer @statoscope/webpack-plugin` (if you are using a MccSoft template, they are alredy installed).
    1. Add 2 scripts to your `package.json` (again, in a template this is already done):
       ```
       "analyze-statoscope": "STATS=1 react-app-rewired build",
       "analyze-sourcemap-size": "source-map-explorer build/static/js/*.js"
       ```
    1. Add Statoscope webpack plugin to your `config-overrides.js` (already done in template :) go read the statoscope docs, if you are not using a template)
    1. Run `yarn analyze-statoscope` and then `yarn analyze-sourcemap-size`.
    Each yarn script will open a page in your browser. Let's analyze them.

1. ## Analyze the numbers
    1. Open the Statoscope page, and find the numbers in Green. These are your primary metrics. `Initial size` is the size of chunks that needs to be loaded, before page starts to show anything.
`Total size` is the size of all js modules your app requires (including 3rd party dependencies).
If you do not use code-splitting at all, these numbers will be equal.
When you start code splitting, you will generally reduce the `Initial size`, but `Total size` will probably grow. It happens, because some code will be duplicated between code-splitted chunks.

        Example: two big pages of your SPA use lodash. Initially these 2 pages are all bundled together, so there's a single copy of lodash. You decided to code-split these two pages. So now the chunks for these two pages will be loaded only when user visits these pages. Yay, you reduced the `Initial size`, since you are not loading these 2 pages (and not loading lodash as well)!
But now both splitted pages will include their own copy of lodash! This makes `Total size` grow. Though, if the growth is not significant, while `Initial size` win is big, you are probably ok.


Ideally, you'd want to reduce both of themcheck the size of your initial chunk. Most probably you will be interested in `vendor` chunk. This is the chunk that contains bundled 3rd party node_modules.
![](images/statoscope-1.png)
    1.
