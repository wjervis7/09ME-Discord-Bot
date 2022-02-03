import { initializeDataTable, replaceEpochWithLocalFormat, formatDate } from "../site.js";

const loadPage = (data) => {
    data = data.map(d => {
        const args = JSON.parse(d.args);
        d.args = args;
        d.report = replaceEpochWithLocalFormat(d.report);
        d.startTimeFormatted = formatDate(d.startTime);
        d.endTimeFormatted = formatDate(d.endTime);
        return d;
    });

    const app = initializeDataTable([
            {
                text: "Report Type",
                align: "start",
                value: "reportType"
            },
            { text: "Initiator", value: "initiator" },
            { text: "Start Time", value: "startTimeFormatted" },
            { text: "End Time", value: "endTimeFormatted" }
        ],
        data.map(d => {
            const args = d.args;
            d.args = Object.entries(args).map(entry => {
                return {
                    key: entry[0],
                    value:
                        entry[1]
                };
            });
            return d;
        }),
        {
            sortTable(items, [sortField], [isDesc]) {
                if (!sortField && typeof isDesc === "undefined") {
                    this.sortBy = this.lastSortBy;
                    sortField = this.sortBy;
                    this.sortDesc = !this.lastSortDesc;
                    isDesc = this.sortDesc;
                }
                switch (sortField) {
                case "startTimeFormatted":
                    items = items.sort((a, b) => a.startTime - b.startTime);
                    break;
                case "endTimeFormatted":
                    items = items.sort((a, b) => a.endTime - b.endTime);
                    break;
                default:
                    items = items.sort((a, b) => a[sortField].localeCompare(b[sortField], "en", { sensitivity: "base", numeric: true }));
                    break;
                }
                this.lastSortBy = sortField;
                this.lastSortDesc = isDesc;
                return isDesc ? items.reverse() : items;
            }
        },
        { singleExpand: false },
        true,
        { sortBy: "endTimeFormatted", sortDesc: true }
    );

    app.$mount("#app");
};

export default loadPage;
