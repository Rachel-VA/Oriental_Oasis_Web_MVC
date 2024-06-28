var dataTable;

$(document).ready(function () {
    var url = window.location.search;
    var status = "all"; // Default status

    if (url.includes("inprocess")) {
        status = "inprocess";
    } else if (url.includes("completed")) {
        status = "completed";
    } else if (url.includes("pending")) {
        status = "pending";
    } else if (url.includes("approved")) {
        status = "approved";
    }

    loadDataTable(status);
});

function loadDataTable(status) {
    dataTable = $('#myTable').DataTable({
        "ajax": {
            "url": "/Customer/CustomerOrderHistory/GetCustomerOrders?status=" + status,
            "type": "GET",
            "datatype": "json",
            "error": function (xhr, error, thrown) {
                console.log(xhr.responseText);
            }
        },
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "name", "width": "15%" },
            { "data": "address", "width": "25%" },
            { "data": "phoneNumber", "width": "10%" },
            { "data": "orderStatus", "width": "10%" },
            { "data": "orderTotal", "width": "7%" },
            { "data": "email", "width": "20%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="text-center">
                            <a href="/Customer/CustomerOrderHistory/Details?orderId=${data}" class="btn btn-success text-white" style="cursor:pointer; width:60px; font-size:12px; padding:5px; margin-top:10px;">
                                View
                            </a>
                        </div>
                    `;
                },
                "width": "10%"
            }
        ]
    });
}
