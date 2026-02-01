import type { Config } from "@react-router/dev/config";

export default {
    ssr: false,  // SPA mode for static hosting
    appDirectory: "src",
    buildDirectory: "build",
} satisfies Config;
