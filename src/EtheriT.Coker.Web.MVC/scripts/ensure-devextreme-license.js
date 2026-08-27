"use strict";

const fs = require("fs");
const path = require("path");
const { spawnSync } = require("child_process");

const projectRoot = path.resolve(__dirname, "..");
const runtimeLicensePath = path.join(
    projectRoot,
    "wwwroot",
    "js",
    "devextreme",
    "devextreme-license.js"
);

if (fs.existsSync(runtimeLicensePath)) {
    console.log("[DevExtreme] Runtime license already exists.");
    process.exit(0);
}

console.log("[DevExtreme] Runtime license not found. Generating it now...");

const npmCommand = process.platform === "win32" ? "npm.cmd" : "npm";
const generation = spawnSync(
    npmCommand,
    ["run", "generate-devextreme-license"],
    {
        cwd: projectRoot,
        encoding: "utf8",
        stdio: ["inherit", "pipe", "pipe"],
        shell: process.platform === "win32"
    }
);

if (generation.stdout) process.stdout.write(generation.stdout);
if (generation.stderr) process.stderr.write(generation.stderr);

if (generation.error) {
    console.error("[DevExtreme] Failed to start license generation.");
    console.error(generation.error.message);
    process.exit(1);
}

if (generation.status !== 0) {
    process.exit(generation.status || 1);
}

const generationOutput = `${generation.stdout || ""}\n${generation.stderr || ""}`;
if (/\bDX100[01]\b/.test(generationOutput)) {
    console.error("[DevExtreme] A valid developer license was not found. Bundle creation stopped.");
    process.exit(1);
}

if (!fs.existsSync(runtimeLicensePath)) {
    console.error("[DevExtreme] License generation completed without producing the runtime file.");
    process.exit(1);
}

console.log("[DevExtreme] Runtime license generated successfully.");
