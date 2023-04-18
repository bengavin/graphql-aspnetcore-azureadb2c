import React from 'react'
import ReactDOM from 'react-dom/client'

import { MsalProvider } from "@azure/msal-react";
import { Configuration, PublicClientApplication } from "@azure/msal-browser";

import { App } from "~global/App";
import { keys } from "~global/config";
import ApiClientProvider from "~providers/ApiClientProvider";

// MSAL configuration
const configuration: Configuration = {
    auth: {
        clientId: keys.B2C_CLIENT_ID,
        authority: keys.B2C_AUTHORITY,
        redirectUri: keys.B2C_REDIRECT_URI,
        knownAuthorities: keys.B2C_KNOWN_AUTHORITIES.split(',')
    }
};

const pca = new PublicClientApplication(configuration);

ReactDOM.createRoot(document.getElementById('root') as HTMLElement).render(
  <React.StrictMode>
    <MsalProvider instance={pca}>
        <ApiClientProvider>
            <App />
        </ApiClientProvider>
    </MsalProvider>
  </React.StrictMode>,
);
