import React, { PropsWithChildren } from "react";
import { useMsal } from "@azure/msal-react";
import { InteractionRequiredAuthError } from "@azure/msal-browser";

import { ApolloClient, createHttpLink, InMemoryCache } from '@apollo/client';
import { setContext } from '@apollo/client/link/context';
import { ApolloProvider } from "@apollo/client/react";
import { keys } from "@global/config";

// Setup required scopes for supporting the required GraphQL queries
export const loginSilentRequest = {
    scopes: keys.B2C_REQUEST_SCOPES.split(','),
};

export const loginRequest = {
    ...loginSilentRequest,
    prompt: "select_account",
};

let loginPopupPromise = null;
const acquireTokenPopup = (instance) => {
    if (!loginPopupPromise) {
        loginPopupPromise = instance.acquireTokenPopup(loginRequest);

        // once this completes, set the modular value to null so future requests
        // will generate a new popup
        loginPopupPromise.finally(r => loginPopupPromise = null);
    }

    return loginPopupPromise;
}

let loginSilentPromise = null;
const acquireTokenSilent = (account, instance) => {
    if (!loginSilentPromise) {
        loginSilentPromise = instance.acquireTokenSilent({
            ...loginSilentRequest,
            account
        }).then(response => response.accessToken);

        loginSilentPromise.finally(r => loginSilentPromise = null);
    }

    return loginSilentPromise;
};

export type ApiClientProps = PropsWithChildren<{}>;

const ApiClientProvider = ({ children }: ApiClientProps): React.ReactElement => {
    const { instance, accounts, inProgress } = useMsal();
    
    const GetAccessToken = async () => {
        // NOTE: This isn't great, but the MSAL library will throw an exception
        //       if silent acquisition fails, so we need to handle that
        const account = accounts[0] ?? null;
        return account 
             ? acquireTokenSilent(account, instance)
               .catch(async (error) => 
                    error instanceof InteractionRequiredAuthError
                    ? acquireTokenPopup(instance)
                    : null)
             : acquireTokenPopup(instance);

    };

    const httpLink = createHttpLink({
        uri: `${keys.API_BASE_URL}/graphql`,
    });

    const authLink = setContext(async (_, { headers }) => {
        // get the authentication token from our retrieved access token
        const token = await GetAccessToken();

        // return the headers to the context so httpLink can read them
        return {
            headers: {
                ...headers,
                authorization: token ? `Bearer ${token}` : "",
            }
        }
    });

    const client = new ApolloClient({
        link: authLink.concat(httpLink),
        cache: new InMemoryCache({
            possibleTypes: {
                Character: ["Human", "Droid"]
            }
        })
    });

    return (
        <ApolloProvider client={client}>
            {children}
        </ApolloProvider>
    );
};

export default ApiClientProvider;
