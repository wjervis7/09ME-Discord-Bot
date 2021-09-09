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