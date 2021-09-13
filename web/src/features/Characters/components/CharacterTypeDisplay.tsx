import React from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faUser, faRobot, faQuestion, IconDefinition } from "@fortawesome/free-solid-svg-icons";

const mapping = {
    "Human": {
        icon: faUser,
        text: "Humanoid"
    },
    "Droid": {
        icon: faRobot,
        text: "Droid"
    }
};

const getIcon = (type: String): IconDefinition => {
    return mapping[type]?.icon ?? faQuestion;
};

const getText = (type: String): String => {
    return mapping[type]?.text ?? "Unknown";
};

export const CharacterTypeDisplay = ({ type }) => {
    const iconDef = getIcon(type);

    return (
        <div>
            <FontAwesomeIcon icon={iconDef} className="me-1" />
            <span>{getText(type)}</span>
        </div>
    );
};

export const CharacterTypeIcon = ({type, ...props}) => {
    const iconDef = getIcon(type);

    return (
        <FontAwesomeIcon icon={iconDef} {...props} />
    );
};


