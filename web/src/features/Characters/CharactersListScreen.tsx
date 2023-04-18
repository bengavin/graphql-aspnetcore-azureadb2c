import React from "react";
import { Link } from "react-router-dom";

import Spinner from "react-bootstrap/Spinner";
import Table from "react-bootstrap/Table";
import DropdownButton from "react-bootstrap/DropdownButton";
import Dropdown from "react-bootstrap/Dropdown";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faEdit, faExclamationTriangle, faPlus } from "@fortawesome/free-solid-svg-icons";

import { SimpleInlineAlert } from "~components/alerts/SimpleInlineAlert";
import { AlignmentIcon } from "~components/enumerations/Alignments";

import { useListQuery } from "./queries/CharacterQueries";
import { CharacterTypeIcon } from "./components/CharacterTypeDisplay";

export const CharactersListScreen = () => {
    const { loading, error, data } = useListQuery();

    const loadingIconElement = <Spinner animation="border" className="me-3" />;
    
    const errorIconElement = <FontAwesomeIcon icon={faExclamationTriangle} size="2x" className="me-3" />;
    const errorContent = `We were unable to load your characters: ${error}`;

    return (
        <div>
            <div className="d-flex justify-content-between align-items-center">
                <h1>Characters</h1>
                <DropdownButton variant="primary" drop="down" align="end" id="addCharacterButton" title={<><FontAwesomeIcon icon={faPlus} className="me-1" />Add a Character</>}>
                    <Dropdown.Item>
                        <Link to="/characters/Human/new" className="dropdown-item">Humanoid</Link>
                    </Dropdown.Item>
                    <Dropdown.Item>
                        <Link to="/characters/Droid/new" className="dropdown-item">Droid</Link>
                    </Dropdown.Item>
                </DropdownButton>
            </div>
            {loading && <SimpleInlineAlert variant="info" iconElement={loadingIconElement} header="One moment" content="Loading your characters..." />}
            {error && <SimpleInlineAlert variant="danger" iconElement={errorIconElement} header="Sorry" content={errorContent} />}
            {data && data.characters && <Table striped bordered hover>
                <thead>
                    <tr>
                        <th></th>
                        <th>Name</th>
                        <th>Alignment</th>
                    </tr>
                </thead>
                <tbody>
                    {data.characters.map(item => {
                        return (<tr key={item.id}>
                            <td style={{ width: "2em", fontSize: "1.25em" }}><Link to={`/characters/${item.__typename}/${item.id}`}><FontAwesomeIcon icon={faEdit} /></Link></td>
                            <td className="align-center w-100">
                                <CharacterTypeIcon type={item.__typename} className="me-1" />
                                {item.name}
                            </td>
                            <td className="text-center"><AlignmentIcon alignment={item.alignment} /></td>
                        </tr>);
                    })}
                </tbody>
            </Table>}
        </div>
    );
}
