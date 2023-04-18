import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: [
      { find: "~components", replacement: "./src/components" },
      { find: "~features", replacement: "./src/features" },
      { find: "~global", replacement: "./src/global" },
      { find: "~helpers", replacement: "./src/helpers" },
      { find: "~providers", replacement: "./src/providers" },
      { find: "~root", replacement: "./public" },
    ]
  },
  server: {
    port: 8080
  }
})
