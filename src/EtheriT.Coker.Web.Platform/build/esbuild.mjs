import path from "node:path";
import { fileURLToPath } from "node:url";
import * as esbuild from "esbuild";

const buildDirectory = path.dirname(fileURLToPath(import.meta.url));
const projectDirectory = path.resolve(buildDirectory, "..");
const production = process.argv.includes("--production");
const watch = process.argv.includes("--watch");

const options = {
  absWorkingDir: projectDirectory,
  entryPoints: {
    platform: "wwwroot/js/modules/platform.js",
    "pages/companies": "wwwroot/js/modules/pages/companies.js",
    "pages/websites": "wwwroot/js/modules/pages/websites.js"
  },
  outdir: "wwwroot/dist/js",
  entryNames: "[dir]/[name]",
  chunkNames: "chunks/[name]-[hash]",
  assetNames: "assets/[name]-[hash]",
  bundle: true,
  splitting: true,
  format: "esm",
  platform: "browser",
  target: ["es2022"],
  minify: production,
  sourcemap: !production,
  logLevel: "info"
};

if (watch) {
  const context = await esbuild.context(options);
  await context.watch();
  console.log("Watching Platform ES modules...");
} else {
  await esbuild.build(options);
}
