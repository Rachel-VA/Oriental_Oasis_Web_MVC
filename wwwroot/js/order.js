var dataTable; // Variable to reload the table after a product is deleted

$(document).ready(function () {
    var url = window.location.search;

    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    } else if (url.includes("completed")) {
        loadDataTable("completed");
    } else if (url.includes("pending")) {
        loadDataTable("pending");
    } else if (url.includes("approved")) {
        loadDataTable("approved");
    } else {
        loadDataTable("all");
    }
});

function loadDataTable(status) {
    dataTable = $('#myTable').DataTable({
        "ajax": {
            "url": "/Admin/Order/GetAll?status=" + status,
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
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="text-center">
                            <a href="/Admin/Order/Details?orderId=${data}" class="btn btn-success text-white" style="cursor:pointer; width:60px; font-size:12px; padding:5px; margin-top:10px;">
                                Edit
                            </a>
                        </div>
                    `;
                },
                "width": "10%"
            }
        ]
    });
}

