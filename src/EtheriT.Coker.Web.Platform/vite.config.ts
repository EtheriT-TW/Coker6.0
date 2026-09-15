import { fileURLToPath, URL } from "node:url";
import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";

export default defineConfig(({ mode }) => ({
  root: fileURLToPath(new URL("./ClientApp", import.meta.url)),
  base: "/dist/",
  plugins: [vue()],
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
