// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
(($, moment) => {
    $("[data-datetime]").each((_, el) => {
        const dateTime = $(el).data("datetime");
        const m = moment.utc(dateTime);
        $(el).text(m.local().format("llll"));
    });
})(jQuery, moment);

const initializeDataTable = (headers, data) => new Vue({
    data() {
        return {
            expanded: [],
            singleExpand: false,
            headers,
            data
        };
    },
    vuetify: new Vuetify()
});

// activity reports
(() => {
    const initializeActivityReports = (data) => {
        data = data.map(d => {
            const args = JSON.parse(d.args);
            d.args = args;
            return d;
        });

        const app = initializeDataTable([
                {
                    text: "Report Type",
                    align: "start",
                    value: "reportType"
                },
                { text: "Initiator", value: "initiator" },
                { text: "Start Time", value: "startTime" },
                { text: "End Time", value: "End Time" }
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
            }));

        app.$mount("#app");
    };

    window.initializeActivityReports = initializeActivityReports;
})();
