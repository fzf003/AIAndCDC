import { defineConfig } from "npm:vite@^5.0.0";
import vue from "npm:@vitejs/plugin-vue@^5.0.0";

export default defineConfig({
  plugins: [vue() as any],
  server: {
    port: 5173,
    proxy: {
      "/api": {
        target: "http://localhost:8889",
        changeOrigin: true,
      },
      "/cdc": {
        target: "http://localhost:8889",
        changeOrigin: true,
      },
    },
  },
  build: {
    outDir: "dist",
    sourcemap: true,
  },
});
