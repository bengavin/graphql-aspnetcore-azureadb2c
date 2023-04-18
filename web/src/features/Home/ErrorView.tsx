import React from "react";

import { logger } from "~helpers/logger";
import Button from "react-bootstrap/Button";
import { ErrorContainer } from "~components/alerts/ErrorContainer";

const LoggedOutError = () => {
    return (
        <main>
            <ErrorContainer title="Your logged in session has expired.">
                <p>Please log in again.</p>
                <Button variant="filled" onClick={() => window.location.reload()}>
                    Log in
                </Button>
            </ErrorContainer>
        </main>
    );
};

export const ErrorView = ({ error, resetErrorBoundary }) => {
    // don't log the 401s as errors
    if (error?.status === 401) {
        return <LoggedOutError />;
    }

    logger.error(error);
    return (
        <main>
            <ErrorContainer>
                <p>This issue has been logged.</p>
                <pre>{JSON.stringify(error, null, 2).replace(/\\n/g, "\n")}</pre>
                <button className="btn btn-primary" onClick={resetErrorBoundary}>
                    Try again
                </button>
            </ErrorContainer>
        </main>
    );
};
