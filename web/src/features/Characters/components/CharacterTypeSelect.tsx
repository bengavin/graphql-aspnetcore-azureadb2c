import React from "react";
import { useFormContext } from "react-hook-form";

import { withValidatedClassName } from "@helpers/withValidatedClassName";

export const CharacterTypeSelect = ({name, errors = null, ...props}) => {
    const { register, getValues, formState: { errors: formErrors } } = useFormContext();
    const error = errors ? errors() : formErrors[name];

    return (
        <>
            <select {...register(name)} 
                    className={withValidatedClassName("form-select", error)} 
                    defaultValue={getValues(name)} 
                    {...props}>
                <option value="Human">Humanoid</option>
                <option value="Droid">Droid</option>
            </select>
            {error && <span className="invalid-feedback">{error?.message}</span>}
        </>
    );
};
