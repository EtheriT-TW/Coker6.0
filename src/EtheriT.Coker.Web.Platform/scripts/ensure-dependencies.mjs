import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { spawnSync } from "node:child_process";

const projectDirectory = path.resolve(
  path.dirname(fileURLToPath(import.meta.url)),
  ".."
);
const packagePath = path.join(projectDirectory, "package.json");
const lockPath = path.join(projectDirectory, "package-lock.json");
const nodeModulesPath = path.join(projectDirectory, "node_modules");

const packageJson = readJson(packagePath);
const expectedDependencies = packageJson.dependencies ?? {};
const expectedDevDependencies = packageJson.devDependencies ?? {};
const expectedPackages = {
  ...expectedDependencies,
  ...expectedDevDependencies
};

if (dependenciesAreReady()) {
  process.exit(0);
}

console.log("Frontend dependencies are missing or outdated. Running npm install...");

const npmCliPath = process.env.npm_execpath;
const installCommand = npmCliPath
  ? process.execPath
  : process.platform === "win32"
    ? process.env.ComSpec ?? "cmd.exe"
    : "npm";
const installArguments = npmCliPath
  ? [npmCliPath, "install"]
  : process.platform === "win32"
    ? ["/d", "/s", "/c", "npm install"]
    : ["install"];
const install = spawnSync(installCommand, installArguments, {
  cwd: projectDirectory,
  stdio: "inherit"
});

if (install.error) {
  console.error(`Unable to start npm install: ${install.error.message}`);
  process.exit(1);
}

process.exit(install.status ?? 1);

function dependenciesAreReady() {
  if (!fs.existsSync(lockPath) || !fs.existsSync(nodeModulesPath)) {
    return false;
  }

  let packageLock;
  try {
    packageLock = readJson(lockPath);
  }
  catch {
    return false;
  }

  const rootLock = packageLock.packages?.[""];
  if (!rootLock ||
      !sameDependencyMap(rootLock.dependencies, expectedDependencies) ||
      !sameDependencyMap(rootLock.devDependencies, expectedDevDependencies)) {
    return false;
  }

  return Object.keys(expectedPackages).every((packageName) => {
    const installedManifestPath = path.join(
      nodeModulesPath,
      ...packageName.split("/"),
      "package.json"
    );
    const lockedPackage = packageLock.packages?.[`node_modules/${packageName}`];

    if (!lockedPackage?.version || !fs.existsSync(installedManifestPath)) {
      return false;
    }

    try {
      return readJson(installedManifestPath).version === lockedPackage.version;
    }
    catch {
      return false;
    }
  });
}

function sameDependencyMap(actual = {}, expected = {}) {
  const actualEntries = Object.entries(actual).sort(([left], [right]) => left.localeCompare(right));
  const expectedEntries = Object.entries(expected).sort(([left], [right]) => left.localeCompare(right));
  return JSON.stringify(actualEntries) === JSON.stringify(expectedEntries);
}

function readJson(filePath) {
  return JSON.parse(fs.readFileSync(filePath, "utf8"));
}
