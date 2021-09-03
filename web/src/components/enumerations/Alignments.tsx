import React from "react";

import { useFormContext } from "react-hook-form";
import { withValidatedClassName } from "@helpers/withValidatedClassName";
import { faJedi, faMinus, faMoon, faQuestionCircle, faSun } from "@fortawesome/free-solid-svg-icons";
import { faMandalorian, faSith } from "@fortawesome/free-brands-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export const alignmentsDisplay = [
    "",
    "Dark Side",
    "Neutral",
    "Light Side",
];

export const alignmentIcons = {
    "UNKNOWN": faQuestionCircle,
    "DARK": faMoon,
    "NEUTRAL": faMinus,
    "LIGHT": faSun
};

export const alignments: [string, ...string[]] = [
    "UNKNOWN",
    "DARK",
    "NEUTRAL",
    "LIGHT",
];

export const AlignmentSelect = ({name, errors = null, ...props}) => {
    const { register, formState: { errors: formErrors } } = useFormContext();
    const error = errors ? errors() : formErrors[name];

    return (
        <>
            <select {...register(name)} className={withValidatedClassName("form-select", error)} {...props}>
                {alignments.map((a, i) => <option key={a} value={a}>{alignmentsDisplay[i]}</option>)}
            </select>
            {error && <span className="invalid-feedback">{error?.message}</span>}
        </>
    );
};

export const AlignmentIcon = ({alignment, ...props}) => {
    const icon = alignmentIcons[alignment];
    const color = alignment === "DARK" ? "text-danger" : alignment === "LIGHT" ? "text-info" : "";

    return <FontAwesomeIcon icon={icon} size="2x" {...props} className={color} title={alignmentsDisplay[alignments.indexOf(alignment)]} />
}