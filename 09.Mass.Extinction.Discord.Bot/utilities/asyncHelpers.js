const to = (promise, errorExt) => {
    return Promise.resolve(promise)
        .then(data => {
            return [null, data]
        })
        .catch(error => {
            if (errorExt) {
                const parsedError = Object.assign({}, error, errorExt);
                return [parsedError, undefined];
            }
            return [error, undefined];
        });
};

module.exports = {
    to
};