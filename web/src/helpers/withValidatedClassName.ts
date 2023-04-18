export const withValidatedClassName = (className, errorItem) => {
    return `${className} ${errorItem ? 'is-invalid' : ''}`;
};
