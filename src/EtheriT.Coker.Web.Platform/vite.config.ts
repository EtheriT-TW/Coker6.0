import { fileURLToPath, URL } from "node:url";
import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import { mkdir, rename, rm } from "node:fs/promises";

const privateManifestDirectory = fileURLToPath(new URL("./FrontendResources", import.meta.url));
const generatedManifestDirectory = fileURLToPath(new URL("./wwwroot/dist/.vite", import.meta.url));

export default defineConfig(({ mode }) => ({
  root: fileURLToPath(new URL("./ClientApp", import.meta.url)),
  base: "/dist/",
  plugins: [vue(), {
    name: "private-backend-manifest",
    async closeBundle() {
      await mkdir(privateManifestDirectory, { recursive: true });
      await rename(`${generatedManifestDirectory}/manifest.json`, `${privateManifestDirectory}/manifest.json`);
      await rm(generatedManifestDirectory, { recursive: true, force: true });
    }
  }],
  publicDir: false,
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./ClientApp/src", import.meta.url))
    }
  },
  build: {
    outDir: fileURLToPath(new URL("./wwwroot/dist", import.meta.url)),
    emptyOutDir: true,
    manifest: true,
    minify: mode === "production",
    sourcemap: mode !== "production",
    cssCodeSplit: true
  }
}));
