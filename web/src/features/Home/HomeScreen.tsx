import React from "react";
import { useIsAuthenticated } from "@azure/msal-react";

export const HomeScreen = () => {
    const isAuthenticated = useIsAuthenticated();

  return (
    <div>
      <h1>Welcome {isAuthenticated ? 'back' : ''} to the Securing GraphQL with Azure AD B2C Demo!</h1>
      <p>
        This app represents a demonstration of managing a set of Star Wars&trade; characters using
        a GraphQL API, and secured with Azure AD B2C.  Please see <a href="https://www.virtual-olympus.com/2021/05/securing-graphql-azureadb2c-1">this blog post</a> for
        additional details.  Enjoy!
      </p>
    </div>
  );
}
