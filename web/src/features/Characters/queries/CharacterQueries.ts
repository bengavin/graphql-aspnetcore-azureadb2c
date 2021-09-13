import { gql, useMutation, useQuery } from "@apollo/client";
import { logger, loggingLevel } from "~helpers/logger";

const CHARACTER_CORE_FIELDS_FRAGMENT = gql`
fragment CoreFields on Character {
    id
    name
    alignment
    __typename
}`;

const CHARACTER_LIST_QUERY = gql`
${CHARACTER_CORE_FIELDS_FRAGMENT}
query characterList {
    characters {
        ... on Character {
            ...CoreFields
        }
        ... on Human {
            homePlanet
        }
        ... on Droid {
            primaryFunction
        }
    }
}
`;

export const useListQuery = () => {
    const {
        loading,
        error,
        data
    } = useQuery(CHARACTER_LIST_QUERY);

    return {
        loading,
        error,
        data
    };
};

const GET_HUMAN_QUERY = gql`
${CHARACTER_CORE_FIELDS_FRAGMENT}
query getHuman($id: ID!) {
    human(id: $id) {
        ...CoreFields
        appearsIn
        friends {
            ...CoreFields
        }
        homePlanet
    }
}
`;

const GET_DROID_QUERY = gql`
${CHARACTER_CORE_FIELDS_FRAGMENT}
query getDroid($id: ID!) {
    droid(id: $id) {
        ...CoreFields
        appearsIn
        friends {
            ...CoreFields
        }
        primaryFunction
    }
}
`;

export const useGetCharacter = (characterType, id) => {
    if (id === "new") {
        logger.log('useGetCharacter', { characterType, id }, loggingLevel.DEBUG);
        return characterType === "Human"
             ? {
                data: {
                    human: {
                        characterType: "Human",
                        name: "",
                        alignment: "UNKNOWN",
                        appearsIn: [],
                        friends: [],
                        homePlanet: ""
                    }
                },
                error: false,
                loading: false
            }
            : {
                data: {
                    droid: {
                        characterType: "Droid",
                        name: "",
                        alignment: "UNKNOWN",
                        appearsIn: [],
                        friends: [],
                        primaryFunction: ""
                    }
                },
                error: false,
                loading: false 
            };
    }

    const { data, loading, error } = useQuery(characterType === "Human" ? GET_HUMAN_QUERY : GET_DROID_QUERY, { variables: { "id": id } });
    return {
        data,
        loading,
        error
    };
}

const ADD_HUMAN_MUTATION = gql`
${CHARACTER_CORE_FIELDS_FRAGMENT}
mutation addHuman($human: HumanInput!) {
    createHuman(human: $human) {
        ...CoreFields
    }
}`;

const ADD_DROID_MUTATION = gql`
${CHARACTER_CORE_FIELDS_FRAGMENT}
mutation addDroid($droid: DroidInput!) {
    createDroid(droid: $droid) {
        ...CoreFields
    }
}`;

export const useAddCharacter = (type, onComplete) => {
    const [addCharacter, { data, loading, error }] = useMutation(type === "Human" ? ADD_HUMAN_MUTATION : ADD_DROID_MUTATION, {
        onCompleted: onComplete,
        refetchQueries: [
            CHARACTER_LIST_QUERY,
            'characterList',
        ],
    });

    return {
        addCharacter: (character) => addCharacter({ variables: { "human": character, "droid": character } }),
        data,
        loading,
        error
    };
};

const UPDATE_HUMAN_MUTATION = gql`
${CHARACTER_CORE_FIELDS_FRAGMENT}
mutation updateHuman($id: ID!, $human: HumanInput!) {
    updateHuman(id: $id, human: $human) {
        ...CoreFields
    }
}`;

const UPDATE_DROID_MUTATION = gql`
${CHARACTER_CORE_FIELDS_FRAGMENT}
mutation updateDroid($id: ID!, $droid: DroidInput!) {
    updateDroid(id: $id, droid: $droid) {
        ...CoreFields
    }
}`;

export const useUpdateCharacter = (type, onComplete) => {
    const [updateCharacter, { data, loading, error }] = useMutation(type === "Human" ? UPDATE_HUMAN_MUTATION : UPDATE_DROID_MUTATION, {
        onCompleted: onComplete,
        refetchQueries: [
            CHARACTER_LIST_QUERY,
            'characterList',
            type === "Human" ? GET_HUMAN_QUERY : GET_DROID_QUERY,
            `get${type}`
        ]
    });

    return {
        updateCharacter: (id, character) => updateCharacter({ variables: { "id": id, "human": character, "droid": character } }),
        data,
        loading,
        error
    };
};