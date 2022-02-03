const initializeDataTable = (headers, data, methods = {}, expand, search, sort) => {
    var vueData = {
        headers,
        data
    };

    if (expand) {
        vueData.expanded = [];
        vueData.singleExpand = expand.singleExpand;
    }

    if (search) {
        vueData.search = "";
    }

    if (sort) {
        vueData.sortBy = sort.sortBy;
        vueData.sortDesc = sort.sortDesc;
        vueData.lastSortBy = sort.sortBy;
        vueData.lastSortDesc = sort.sortDesc;
    }

    return new Vue({
        data() {
            return vueData;
        },
        methods,
        vuetify: new window.Vuetify()
    });
};

const _timeRegex = /(?<=\<t\:)(\d*)(?=\:f\>)/g;

const replaceEpochWithLocalFormat = report => {
    let newReport = report;
    const matches = report.match(_timeRegex);
    if (!matches) {
        return newReport;
    }
    for (const match of matches) {
        newReport = newReport.replace(`<t:${match}:f>`, formatDate(+match));
    }
    return newReport;
};

const formatDate = dateTime => moment.unix(dateTime).local().format("llll");

export {
    initializeDataTable,
    replaceEpochWithLocalFormat,
    formatDate
};
