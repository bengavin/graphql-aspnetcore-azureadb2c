import React from "react";

export interface ErrorContainerProps {
    /** The primary error message to display */
    title?: string;
    /** Additional error details */
    children?: React.ReactNode;
    /** Custom display CSS class */
    className?: string;
}

export const ErrorContainer = ({
    title = "Something unexpectedly went wrong! 😢",
    children = undefined,
    className = "",
}: ErrorContainerProps) => {
    return (
        <section className={"content-container bg-light rounded shadow-sm " + className}>
            {title && <h1>{title}</h1>}
            {children}
        </section>
    );
};
