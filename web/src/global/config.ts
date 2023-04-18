export const keys = {
    LOG_LEVEL: import.meta.env.VITE_PUBLIC_LOG_LEVEL,
    IS_PRODUCTION: import.meta.env.VITE_PUBLIC_ENV === "PROD",
    NODE_ENV: import.meta.env.NODE_ENV,
    API_BASE_URL: import.meta.env.VITE_PUBLIC_API_BASE_URL,
    ENV: import.meta.env.VITE_PUBLIC_ENV as Environment,
    B2C_CLIENT_ID: import.meta.env.VITE_PUBLIC_B2C_CLIENT_ID,
    B2C_AUTHORITY: import.meta.env.VITE_PUBLIC_B2C_AUTHORITY,
    B2C_REDIRECT_URI: import.meta.env.VITE_PUBLIC_B2C_REDIRECT_URI,
    B2C_KNOWN_AUTHORITIES: import.meta.env.VITE_PUBLIC_B2C_KNOWN_AUTHORITIES,
    B2C_REQUEST_SCOPES: import.meta.env.VITE_PUBLIC_B2C_REQUEST_SCOPES,
};
