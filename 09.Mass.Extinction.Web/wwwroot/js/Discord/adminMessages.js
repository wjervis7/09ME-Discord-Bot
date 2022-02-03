import { initializeDataTable, formatDate } from "../site.js";

const loadPage = (data) => {
    data = data.map(d => {
        d.dateSentFormatted = formatDate(d.dateSent);
        d.bodyTrunc = d.body.length <= 100 
            ? d.body 
            : `${d.body.substr(0, 100)}...`;
        d.isAnonymous = d.isAnonymous ? "Yes" : "No";
        d.canExpand = d.body !== d.bodyTrunc;
        return d;
    });

    const app = initializeDataTable([
            { text: "Sender", value: "sender" },
            { text: "Body", value: "bodyTrunc" },
            { text: "Date Sent", value: "dateSentFormatted" },
            { text: "Is Anonymous", value: "isAnonymous" }
        ],
        data,
        {
            sortTable(items, [sortField], [isDesc]) {
                if (!sortField && typeof isDesc === "undefined") {
                    this.sortBy = this.lastSortBy;
                    sortField = this.sortBy;
                    this.sortDesc = !this.lastSortDesc;
                    isDesc = this.sortDesc;
                }
                switch (sortField) {
                case "dateSentFormatted":
                    items = items.sort((a, b) => a.dateSent - b.dateSent);
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
        { sortBy: "dateSentFormatted", sortDesc: true }
    );

    app.$mount("#app");
};

export default loadPage;
