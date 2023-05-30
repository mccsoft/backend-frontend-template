# What is `pull-from-template`

Imagine you created your project from this template a year ago. Since then, a lot was added in template, a lot of libraries got updated, maybe even new .NET was shipped. You could get all these changes in your project automatically! Just run `yarn pull-from-template` and watch for log messages and/or errors.

# How to make your project updatable

In order to make your project updatable (i.e. to `yarn pull-from-template` to run smoothly without conflicts and overwriting your code) please follow the following rules while developing your code:

1. [Backend] Do not modify any existing _.cs files in [Lib](/webapi/Lib/) folder. Instead, create your own files and put your code in them. If you strictly need to add some methods to existing files, add `_.partial.cs` file next to existing one and add your functionality there.

   Reasoning: all \*.cs files in Lib folder are overwritten during update. So, if you add something, your changes will be removed during update.

1. [Backend] Do not modify any existing files in [Setup](/webapi/src/MccSoft.TemplateApp.App/Setup) folder. See the previous point for the reasons and workarounds (existing files in Setup folder are overwritten as well).
