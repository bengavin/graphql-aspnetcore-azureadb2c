import React from "react";
import Alert from "react-bootstrap/Alert";

export type SimpleInlineAlertProps = {
    variant: string,
    iconElement: React.ReactElement,
    header: string,
    content: string
};

export const SimpleInlineAlert = ({ variant, iconElement, header, content }: SimpleInlineAlertProps): React.ReactElement => {
    return (
        <Alert variant={variant} className="d-flex flex-row align-items-center">
            {iconElement}
            <div className="flex-column">
                <Alert.Heading>{header}</Alert.Heading>
                <span>{content}</span>
            </div>
        </Alert>
    );
};
