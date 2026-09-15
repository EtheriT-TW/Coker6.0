import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { spawnSync } from "node:child_process";

const projectDirectory = path.resolve(
  path.dirname(fileURLToPath(import.meta.url)),
  ".."
);
const licensePath = path.join(
  projectDirectory,
  "ClientApp",
  "src",
  "devextreme-license.ts"
);

if (fs.existsSync(licensePath)) {
  console.log("[DevExtreme] Modular runtime license already exists.");
  process.exit(0);
}

console.log("[DevExtreme] Modular runtime license not found. Generating it now...");

const npmCliPath = process.env.npm_execpath;
const command = npmCliPath
  ? process.execPath
  : process.platform === "win32"
    ? process.env.ComSpec ?? "cmd.exe"
    : "npm";
const args = npmCliPath
  ? [npmCliPath, "run", "generate-devextreme-license"]
  : process.platform === "win32"
    ? ["/d", "/s", "/c", "npm run generate-devextreme-license"]
    : ["run", "generate-devextreme-license"];
const generation = spawnSync(command, args, {
  cwd: projectDirectory,
  stdio: "inherit"
});

if (generation.error) {
  console.error(`[DevExtreme] Unable to start license generation: ${generation.error.message}`);
  process.exit(1);
}

if (generation.status !== 0 || !fs.existsSync(licensePath)) {
  console.error("[DevExtreme] A modular runtime license could not be generated. Confirm that this developer has registered a compatible DevExpress license on this computer.");
  process.exit(generation.status || 1);
}

console.log("[DevExtreme] Modular runtime license generated successfully.");
