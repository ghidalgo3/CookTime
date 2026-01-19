import { reactRouter } from "@react-router/dev/vite";
import { defineConfig } from "vite";
import path from "path";

const target = process.env.ASPNETCORE_HTTPS_PORT
    ? `https://localhost:${process.env.ASPNETCORE_HTTPS_PORT}`
    : process.env.ASPNETCORE_URLS
        ? process.env.ASPNETCORE_URLS.split(';')[0]
        : 'https://localhost:5001';
const port = process.env.PORT ? parseInt(process.env.PORT) : 3000;

export default defineConfig({
    plugins: [reactRouter()],
    resolve: {
        alias: {
            src: path.resolve(__dirname, "./src"),
        },
    },
    server: {
        port: port,
        host: true,  // Bind to 0.0.0.0 for Docker accessibility
        proxy: {
            "/Auth": {
                target,
                changeOrigin: true,
                secure: false,
            },
            "/js": {
                target,
                changeOrigin: true,
                secure: false,
            },
            "/lib": {
                target,
                changeOrigin: true,
                secure: false,
            },
            "/api": {
                target,
                changeOrigin: true,
                secure: false,
                headers: {
                    'X-Forwarded-Host': 'localhost:3000',
                    'X-Forwarded-Proto': 'http',
                },
            },
            "/image": {
                target,
                changeOrigin: true,
                secure: false,
            },
            "/Blog": {
                target,
                changeOrigin: true,
                secure: false,
            },
            "/openapi": {
                target,
                changeOrigin: true,
                secure: false,
            },
        },
    },
    build: {
        outDir: "build",
    },
});
