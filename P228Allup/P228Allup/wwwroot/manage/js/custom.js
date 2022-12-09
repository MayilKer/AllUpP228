$(document).ready(function () {


    $("#deleteImage").click(function (e) {
        e.preventDefault();
        let url = $(this).attr("href");

        fetch(url).then(data => data.text())
            .then(res => {
                $(".proImg").html(res);
            })
    });









    let isMain = $("#IsMain").is(":checked");
    console.log(isMain);

    if (isMain) {
        $("#parentList").addClass("d-none");
        $("#mainImage").removeClass("d-none");
    } else {
        $("#parentList").removeClass("d-none");
        $("#mainImage").addClass("d-none");
    }

    $("#IsMain").click(function () {
        let isMain = $(this).is(":checked");
        console.log(isMain);

        if (isMain) {
            $("#parentList").addClass("d-none");
            $("#mainImage").removeClass("d-none");
        } else {
            $("#parentList").removeClass("d-none");
            $("#mainImage").addClass("d-none");
        }
    })

        let toster = $("#toster").val();

        Command: toastr["error"](toster)



    toastr.options = {
        "closeButton": false,
        "debug": false,
        "newestOnTop": false,
        "progressBar": false,
        "positionClass": "toast-top-right",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }
})