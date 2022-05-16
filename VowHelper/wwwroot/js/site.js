// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

import { createApp } from "/node_modules/vue/dist/vue.esm-browser.js";
import { symbols } from "./symbols.js";

const getSymbol = symbol => symbols.find(s => s.symbol === symbol);

const defaultSymbol = {
    symbol: "Select a symbol",
    image: "",
    location: "",
    description: ""
};

const app = createApp({
    data() {
        return {
            symbols,
            symbol1Val: null,
            symbol2Val: null,
            symbol3Val: null
        }
    },
    computed: {
        selectedSymbols() {
            const selectedSymbols = [];
            if (this.symbol1Val) {
                selectedSymbols.push(getSymbol(this.symbol1Val));
            }
            if (this.symbol2Val) {
                selectedSymbols.push(getSymbol(this.symbol2Val));
            }
            if (this.symbol3Val) {
                selectedSymbols.push(getSymbol(this.symbol3Val));
            }
            return selectedSymbols.sort((a, b) => a.order - b.order);
        }
    }
});

app.config.errorHandler = (err, instance, info) => {
    console.error(err, instance, info);
};

export {app};