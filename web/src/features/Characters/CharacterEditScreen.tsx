import React from "react";
import { useParams } from "react-router-dom";

import { faExclamationTriangle } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

import Spinner from "react-bootstrap/Spinner";

import { useGetCharacter } from "./queries/CharacterQueries";

import { SimpleInlineAlert } from "~components/alerts/SimpleInlineAlert";
import { PageHeader } from "~components/PageHeader";
import { logger, loggingLevel } from "~helpers/logger";
import { CharacterEditForm } from "./components/CharacterEditForm";

export const CharacterEditScreen = () => {
    let { type, id } = useParams();
    const isNew = id === "new";

    const { data: characterData, loading: characterLoading, error: characterError } = useGetCharacter(type, id);

    const pageTitle = isNew ? "New Character" : "Edit Character";
    const loadingIconElement = <Spinner animation="border" className="me-3" />;
    if (characterLoading) {
        return (<>
            <PageHeader backLink="/characters" pageTitle={pageTitle} />
            <SimpleInlineAlert variant="info" iconElement={loadingIconElement} header="One moment" content="Loading character..." />
        </>);
    }

    const errorIconElement = <FontAwesomeIcon icon={faExclamationTriangle} size="2x" className="mr-3" />;
    const errorContent = `We were unable to load the character: ${characterError}`;
    if (characterError) {
        return (<>
            <PageHeader backLink="/characters" pageTitle={pageTitle} />
            <SimpleInlineAlert variant="danger" iconElement={errorIconElement} header="Sorry" content={errorContent} />
        </>);
    }

    logger.log("characterInfo:", characterData, loggingLevel.DEBUG);
    return (
        <>
            <PageHeader backLink="/characters" pageTitle={pageTitle} />
            <CharacterEditForm character={characterData.human ?? characterData.droid} 
                               id={id}
                               isNew={isNew} />
        </>
    )
};
