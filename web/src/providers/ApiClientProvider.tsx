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

export type ApiClientProps = PropsWithChildren<{}>;

const ApiClientProvider = ({ children }: ApiClientProps): React.ReactElement => {
    const { instance, accounts, inProgress } = useMsal();
    
    const GetAccessToken = async () => {
        // NOTE: This isn't great, but the MSAL library will throw an exception
        //       if silent acquisition fails, so we need to handle that
        const account = accounts[0] ?? null;
        if (account && inProgress === "none") {
            try {
                const result = await instance.acquireTokenSilent({
                    ...loginSilentRequest,
                    account,
                });
                return result.accessToken;
            } catch (err) {
                if (err instanceof InteractionRequiredAuthError) {
                    // fallback to interaction when silent call fails
                    return instance.acquireTokenPopup(loginRequest);
                }
            }
        } else if (!account && inProgress === "none") {
            return instance.acquireTokenPopup(loginRequest);
        }
        return null;
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
