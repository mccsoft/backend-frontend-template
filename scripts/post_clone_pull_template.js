#!/usr/bin/env node
import yargs from "yargs";
import fs from "fs";
import path from "path";
import {hideBin} from "yargs/helpers";
import {execSync} from 'child_process'

const args = yargs(hideBin(process.argv))
    .version("0.1")
    .option("templateFolder", {
      alias: "t",
      type: "string",
      description: "Path to clone template folder",
    })
    .demandOption(["templateFolder"])
    .help().argv;

// Here you can do anything to make structure of cloned template look like your project
console.warn('Running post_clone_pull_template script');
