$(function () {
    $("#accordion").accordion({ collapsible: true, heightStyle: "content", active: false });
    $(".import-process-uuid").each(function () {
        importprocess($(this).attr("value"));
    });
    
});

function importprocess(uuid) {
    console.log(uuid);
    $.ajax({
        type: "POST",
        url: "/Integration/IntegrationAdmin/GetProcessInfo",
        data: { uuid: uuid },
        success: function (data) {
            console.log(data);
            if (data != null && data.Running) {
                if (data.ItemsLeft != 1) {
                    $(".import-process-text-" + uuid).parent().show();
                }
                $(".import-process-text-" + uuid).html("<span>" + data.ItemsLeft + "/" + data.ItemsTotal + " " + data.Procent + "%" + "</span>");
                $(".import-process-text-" + uuid).siblings(".import-process-status").width(data.Procent + "%");
                setTimeout(function() {
                    importprocess(uuid);
                }, 2000);
            } else {
                $(".import-process-text-" + uuid).parent().hide();
            }
        },
    });
    
}