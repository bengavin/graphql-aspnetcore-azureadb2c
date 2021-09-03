process.env.SNOWPACK_PUBLIC_LOG_LEVEL = process.env.SNOWPACK_PUBLIC_LOG_LEVEL || "DEBUG";
process.env.SNOWPACK_PUBLIC_API_BASE_URL = process.env.SNOWPACK_PUBLIC_API_BASE_URL || "https://localhost:5001";
process.env.SNOWPACK_PUBLIC_ENV = process.env.SNOWPACK_PUBLIC_ENV || "LOCAL";
process.env.SNOWPACK_PUBLIC_B2C_CLIENT_ID = process.env.SNOWPACK_PUBLIC_B2C_CLIENT_ID || "<B2C UI CLIENT ID>";
process.env.SNOWPACK_PUBLIC_B2C_AUTHORITY = process.env.SNOWPACK_PUBLIC_B2C_AUTHORITY || "https://<B2C ORG>.b2clogin.com/<B2C ORG>.onmicrosoft.com/<B2C User Flow>";
process.env.SNOWPACK_PUBLIC_B2C_REDIRECT_URI = process.env.SNOWPACK_PUBLIC_B2C_REDIRECT_URI || "http://localhost:8080/blank.html";
process.env.SNOWPACK_PUBLIC_B2C_KNOWN_AUTHORITIES = process.env.SNOWPACK_PUBLIC_B2C_KNOWN_AUTHORITIES || "<B2C ORG>.b2clogin.com";
process.env.SNOWPACK_PUBLIC_B2C_REQUEST_SCOPES = process.env.SNOWPACK_PUBLIC_B2C_REQUEST_SCOPES || "<B2C Scopes>";

/** @type {import("snowpack").SnowpackUserConfig } */
module.exports = {
  mount: {
    public: "/",
    src: "/dist",
  },
  plugins: [
    "@snowpack/plugin-react-refresh",
    "@snowpack/plugin-sass"
  ],
  routes: [
    /* Enable an SPA Fallback in development: */
    {"match": "routes", "src": ".*", "dest": "/index.html"},
  ],
  optimize: {
    bundle: true,
    minify: true,
    target: "es2020",
  },
  alias: {
    "@components": "./src/components",
    "@features": "./src/features",
    "@global": "./src/global",
    "@helpers": "./src/helpers",
    "@providers": "./src/providers",
    "@root": "./public",
  },
  packageOptions: {
    /* ... */
  },
  devOptions: {
    /* ... */
  },
  buildOptions: {
    out: "wwwroot",
    /*sourcemap: true*/
  },
};
