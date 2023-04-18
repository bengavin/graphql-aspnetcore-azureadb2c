import React from "react";
import { Link } from "react-router-dom";

import { faArrowLeft } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export type PageHeaderProps = {
    backLink?: string,
    pageTitle: string
};

export const PageHeader = ({backLink, pageTitle}: PageHeaderProps) => {
    return (
        <div className="d-flex justify-contents-left align-items-center">
            {backLink && <Link to={backLink} className="me-3">
                <FontAwesomeIcon icon={faArrowLeft} size="2x" />
            </Link>}
            <h1>{pageTitle}</h1>
        </div>
    );
};
