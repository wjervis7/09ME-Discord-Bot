const sleep = (ms) => new Promise(resolve => {
    setTimeout(resolve, ms);
});

modules.export = {
    sleep
};
