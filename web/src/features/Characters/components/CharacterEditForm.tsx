import React from "react";
import { Link, useNavigate } from "react-router-dom";

import { useForm, FormProvider, useFieldArray } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";

import Button from "react-bootstrap/Button";
import Spinner from "react-bootstrap/Spinner";
import Table from "react-bootstrap/Table";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faExclamationTriangle, faTrash } from "@fortawesome/free-solid-svg-icons";

import { episodes, episodesDisplay } from "~components/enumerations/Episodes";
import { AlignmentIcon, alignments, AlignmentSelect } from "~components/enumerations/Alignments";
import { logger, loggingLevel } from "~helpers/logger";
import { withValidatedClassName } from "~helpers/withValidatedClassName";

import { useAddCharacter, useUpdateCharacter } from "../queries/CharacterQueries";
import { CharacterTypeSelect } from "./CharacterTypeSelect";
import { CharacterTypeIcon } from "./CharacterTypeDisplay";
import { CharacterPicker } from "./CharacterPicker";

const characterSchema = z.object({
    name: z.string().min(3, "Name must be at least 3 characters").max(50, "Name must be no more than 50 characters"),
    alignment: z.enum(alignments),
    appearsIn: z.array(z.object({
        value: z.enum(episodes),
        selected: z.boolean()
    })),
    friends: z.array(z.object({
        friendId: z.string(),
        name: z.string()
    })),
    homePlanet: z.string().optional(),
    primaryFunction: z.string().optional(),
    characterType: z.enum(["Human", "Droid"]),
}).refine(({ characterType, homePlanet }) => characterType === "Droid" || homePlanet?.length <= 50, { message: "Home Planet must be less than 50 characters", path: ["homePlanet"] });

const asCharacterInput = (data) => {
    // convert to [Human,Driod]Input form
    return data.characterType === "Human"
         ? {
            name: data.name,
            alignment: data.alignment,
            appearsIn: data.appearsIn?.filter(x => x.selected).map(i => i.value) || [],
            friends: data.friends?.map(f => f.id) || [],
            homePlanet: data.homePlanet,
        }
        : {
            name: data.name,
            alignment: data.alignment,
            appearsIn: data.appearsIn?.filter(x => x.selected).map(i => i.value) || [],
            friends: data.friends?.map(f => f.friendId) || [],
            primaryFunction: data.primaryFunction,
        };
};

const asFriendField = (friend) => {
    return {
        friendId: friend.id,
        name: friend.name,
        alignment: friend.alignment,
        __typename: friend.__typename
    };
};

const asFormValues = (data) => {
    if (!data) { return data; }
    console.log("data", data);

    // convert from CharacterGraphType form

    const appearsIn = episodes.map(e => {
        return {
            value: e,
            selected: !!data.appearsIn?.find(x => x === e)
        }
    });
    const friends = data.friends.map(asFriendField);

    return data.characterType === "Human"
        ? {
            characterType: "Human",
            name: data.name,
            alignment: data.alignment || alignments[0],
            appearsIn: appearsIn,
            friends: friends,
            homePlanet: data.homePlanet
        }
        : {
            characterType: "Droid",
            name: data.name,
            alignment: data.alignment,
            appearsIn: appearsIn,
            friends: friends,
            primaryFunction: data.primaryFunction
        };
};

export type CharacterEditFormProps = {
    character: any;
    id?: string;
    isNew: boolean;
};

export const CharacterEditForm = ({character, id, isNew, ...props}) => {
    const defaultFormValues = asFormValues(character);
    const formMethods = useForm({
        mode: "onTouched",
        resolver: zodResolver(characterSchema),
        defaultValues: defaultFormValues
    });
    const { register, handleSubmit, watch, reset, formState: { errors } } = formMethods;
    const { fields: appearsIn } = useFieldArray({ control: formMethods.control, name: "appearsIn" });
    const { fields: friends, append: addFriend, remove: removeFriend } = useFieldArray({ control: formMethods.control, name: "friends" });
    
    const characterType = watch("characterType");

    const navigate = useNavigate();
    const { addCharacter, loading: addLoading, error: addError } = useAddCharacter(characterType, (data) => navigate("/characters", { replace: true }));
    const { updateCharacter, loading: updateLoading, error: updateError } = useUpdateCharacter(characterType, (id, data) => navigate("/characters", { replace: true }));

    const onFormSubmit = (data) => {
        // convert this to the appropriate graph type
        const characterData = asCharacterInput(data);
        
        if (isNew) {
            addCharacter(characterData);
        }
        else {
            updateCharacter(id, characterData);
        }
    };

    return (
        <FormProvider {...formMethods}>
            <form onSubmit={handleSubmit(onFormSubmit)} className="ms-5">
                <div className="row mb-2">
                    <label htmlFor="name" className="col-form-label col-md-3">Name:</label>
                    <div className="col-md-5">
                        <input {...register("name")} className={withValidatedClassName("form-control", errors?.name)} />
                        {errors?.name && <span className="invalid-feedback">{errors?.name?.message}</span>}
                    </div>
                </div>
                <div className="row mb-2">
                    <label htmlFor="alignment" className="col-form-label col-md-3">Alignment:</label>
                    <div className="col-md-3">
                        <AlignmentSelect name="alignment" />
                    </div>
                </div>
                <div className="row mb-2">
                    <label htmlFor="characterType" className="col-form-label col-md-3">Character Type:</label>
                    <div className="col-md-3">
                        <CharacterTypeSelect name="characterType" />
                    </div>
                </div>
                {characterType === "Human" && <div className="row mb-2">
                    <label htmlFor="homePlanet" className="col-form-label col-md-3">Home Planet:</label>
                    <div className="col-md-3">
                        <input {...register("homePlanet")} className={withValidatedClassName("form-control", errors?.homePlanet)} />
                        {errors?.homePlanet && <span className="invalid-feedback">{errors?.homePlanet?.message}</span>}
                    </div>
                </div>}
                {characterType === "Droid" && <div className="row mb-2">
                    <label htmlFor="primaryFunction" className="col-form-label col-md-3">Primary Function:</label>
                    <div className="col-md-3">
                        <input {...register("primaryFunction")} className={withValidatedClassName("form-control", errors?.primaryFunction)} />
                        {errors?.primaryFunction && <span className="invalid-feedback">{errors?.primaryFunction?.message}</span>}
                    </div>
                </div>}
                <div className="row mb-2">
                    <label htmlFor="appearsIn" className="col-form-label col-md-3">Appears In:</label>
                    <div className="col-md-3 flex-column">
                        {episodes.map((e, i) => {
                            const selected = defaultFormValues.appearsIn.find(x => x.value === e)?.selected ?? false;
                            return(
                                <div key={i} className="form-check-inline">
                                    <input className="form-check-inline me-2"
                                        type="checkbox"
                                        {...register(`appearsIn.${i}.selected`)}
                                        defaultChecked={selected} />
                                    <label htmlFor={`appearsIn.${i}.selected`} className="form-check-label">{episodesDisplay[i]}</label>
                                </div>);
                        })}
                    </div>
                </div>
                <div className="row mb-2">
                    <div className="col-md-3">
                        <label htmlFor="friends" className="col-form-label">Friends:</label>
                    </div>
                    <div className="col-md-4">
                        {!!friends.length &&
                            <Table borderless striped>
                                <tbody>
                                    {friends.map((field, index) =>
                                    {
                                        logger.log('fieldData:', { field, index }, loggingLevel.DEBUG);
                                        return (
                                            <tr key={`${field.id}`} className="align-middle">
                                                <td className="w-100">
                                                    <CharacterTypeIcon type={field.__typename} className="me-2" />
                                                    {field.name}
                                                </td>
                                                <td className="text-center"><AlignmentIcon alignment={field.alignment} className="mx-2" /></td>
                                                <td>
                                                    <Button type="button" onClick={() => removeFriend(index)} variant="danger">
                                                        <FontAwesomeIcon icon={faTrash} />
                                                    </Button>
                                                </td>
                                            </tr>);
                                    })}
                                </tbody>
                                <tfoot>
                                    <tr>
                                        <td colSpan={3} className="text-right">
                                            <CharacterPicker excludeCharacters={[character, ...friends]} onPickCharacter={(c) => addFriend(asFriendField(c))} />
                                        </td>
                                    </tr>
                                </tfoot>
                            </Table>}
                            {!friends.length && <CharacterPicker excludeCharacters={[character]} onPickCharacter={(c) => addFriend(asFriendField(c))} />}
                    </div>
                </div>
                <div className="d-flex flex-row align-items-center justify-content-end">
                    <Button variant="secondary" as={Link} to="/characters" className="me-2">Cancel</Button>
                    {isNew && <>
                        <button type="submit" className="btn btn-primary me-2">Add Character</button>
                        {addLoading && <Spinner animation="border" />}
                        {addError && <FontAwesomeIcon icon={faExclamationTriangle} size="2x" />}
                    </>}
                    {!isNew && <>
                        <button type="submit" className="btn btn-primary me-2">Save Character</button>
                        {updateLoading && <Spinner animation="border" />}
                        {updateError && <FontAwesomeIcon icon={faExclamationTriangle} size="2x" />}
                    </>}
                </div>
            </form>
        </FormProvider>
    );
};
