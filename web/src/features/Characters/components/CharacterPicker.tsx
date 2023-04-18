import React, { useRef } from "react";

import Button from "react-bootstrap/Button";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPlus } from "@fortawesome/free-solid-svg-icons";

import { useListQuery } from "../queries/CharacterQueries";

export const CharacterPicker = ({excludeCharacters, onPickCharacter}) => {
    const { data, loading: charactersLoading } = useListQuery();
    const selectRef = useRef();

    if (charactersLoading) { return <span className="text-info">"Loading Characters..."</span>; }

    const charactersToSelect = excludeCharacters?.length ?? 0 > 0
                            ? data.characters.filter(c => !excludeCharacters.find(e => (c.id === e.friendId || c.id === e.id) && c.__typename === e.__typename ))
                            : data.characters;
    if (charactersToSelect.length <= 0) {
        return <></>;
    }

    return (
        <div className="d-flex flex-row align-items-center justify-content-between">
            <select ref={selectRef} className="form-select me-2">
                <option></option>
                {charactersToSelect.map((c, i) => <option key={i} value={i}>{c.name}</option>)}
            </select>
            <Button type="button" variant="success" 
                    onClick={() => !!selectRef.current?.value && onPickCharacter(charactersToSelect[+selectRef.current.value])}>
                <FontAwesomeIcon icon={faPlus} />
            </Button>
        </div>
    );
};
