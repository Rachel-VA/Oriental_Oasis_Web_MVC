var dataTable; //var to reload the table after a product is deleted

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#myTable').DataTable({
        "ajax": {
            "url": "/admin/product/getall",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "itemName", "width": "15%" },
            { "data": "itemNumber", "width": "10%" },
            { "data": "listPrice", "width": "8%" },
            { "data": "origin", "width": "10%" },
            { "data": "category.name", "width": "12%" },
            { "data": "description", "width": "25%" },
            {
                "data": "imageURL",
                "render": function (data) {
                    return `<img src="${data}" class="table-img" />`;
                },
                "width": "10%"
            },
            {
                "data": "productId",
                "render": function (data) {
                    return `
                        <div class="text-center" style="display: flex; flex-direction: column; align-items: center; gap: 2px;">
                            <a href="/Admin/Product/UpSert/${data}" class="btn btn-success text-yellow" style="cursor:pointer; width:80px; font-size:12px; padding:5px;margin-top:10px;">
                                Edit
                            </a>
                            <a onClick="Delete('/Admin/Product/Delete/${data}')" class="btn btn-danger mx-2" style="cursor:pointer; width:80px; font-size:12px; padding:5px;margin-top:10px;">
                                Delete
                            </a>
                        </div>
                    `;
                },
                "width": "10%"
            }
        ]
    });
}

// Using SweetAlert2 for delete confirmation
function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: 'DELETE',
                url: url,
                success: function (data) {
                    if (data.success) {
                        Swal.fire(
                            'Deleted!',
                            'Your product has been deleted.',
                            'success'
                        );
                        dataTable.ajax.reload(); //call the reload var
                    } else {
                        Swal.fire(
                            'Error!',
                            data.message,
                            'error'
                        );
                    }
                }
            });
        }
    });
}
