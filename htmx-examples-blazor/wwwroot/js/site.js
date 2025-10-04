// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Call the dataTables jQuery plugin
$(document).ready(function () {
    $('#dataTable').DataTable();
    $('#dataTableInvoices').DataTable();
    $('#dataTableInvoiceLines').DataTable();
});

$(document).ready(function () {
    $("#dataTableInvoices tbody tr").click(function () {
        var selected = $(this).hasClass("highlight");
        $("#dataTableInvoices tr").removeClass("highlight");
        if (!selected)
            $(this).addClass("highlight");
    });
});