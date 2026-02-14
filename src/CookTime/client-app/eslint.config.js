import js from "@eslint/js";
import tseslint from "typescript-eslint";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";

export default tseslint.config(
    { ignores: ["build/**", "node_modules/**", ".react-router/**"] },
    js.configs.recommended,
    ...tseslint.configs.recommended,
    {
        plugins: {
            "react-hooks": reactHooks,
            "react-refresh": reactRefresh,
        },
        rules: {
            ...reactHooks.configs.recommended.rules,
            "react-refresh/only-export-components": [
                "warn",
                { allowConstantExport: true },
            ],
            // Relaxed for existing codebase — tighten over time
            "@typescript-eslint/no-explicit-any": "warn",
            "@typescript-eslint/no-unused-vars": [
                "warn",
                { argsIgnorePattern: "^_", varsIgnorePattern: "^_" },
            ],
            "@typescript-eslint/no-empty-object-type": "warn",
            "react-hooks/static-components": "warn",
            "react-hooks/set-state-in-effect": "warn",
        },
    }
);
