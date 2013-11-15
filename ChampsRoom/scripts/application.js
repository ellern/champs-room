
$(document).ready(function () {
    console.log("document ready");

    $("[rel='tooltip']").tooltip({ html: true });
});

$(document).on("click", ".table-clickable > tbody > tr", function (event) {
    var href = $(this).find("a").attr("href");
    if (href) {
        window.location = href;
        event.stopPropagation();
    }
});
