$(function () {
    //$.connection.hub.logging = true;
    var integrationProgress = $.connection.integrationProgress;

    integrationProgress.client.broadcastMessage = function (data) {
        var isActive = data.Active;
        console.log(data);
        var procent = (data.ElapsedCount / data.TotalCount) * 100;
        var uuid = data.Uuid;
        console.log(procent);
        $("#progress-" + uuid).val(procent);
        $("#elapsed-" + uuid).html(data.ElapsedCount);
        $("#total-" + uuid).html(data.TotalCount);
        $("#procent-" + uuid).html(parseInt(procent));
        $(".startdate-" + uuid).html(data.StartDate);
        $("#elapsedtime-" + uuid).html(data.ElapsedTime);
        if (isActive) {
            $(".active-" + uuid).addClass("active");
            $(".active-" + uuid).removeClass("inactive");
        } else {
            $(".active-" + uuid).addClass("inactive");
            $(".active-" + uuid).removeClass("active");
        }
        

        var showhideprogressinfo = ".showhide-progressinfo-" + uuid;
        if (data.Active) {
            if (!$(showhideprogressinfo).hasClass("show-progressinfo")) {
                $(showhideprogressinfo).addClass("show-progressinfo");
            }
            $(showhideprogressinfo).removeClass("hide-progressinfo");
        } else {
            if (!$(showhideprogressinfo).hasClass("hide-progressinfo")) {
                $(showhideprogressinfo).addClass("hide-progressinfo");
            }
            $(showhideprogressinfo).removeClass("show-progressinfo");
        }
    };

    $.connection.hub.start().done(function() {
        $('#sendmessage').click(function() {
            // Call the Send method on the hub.
        });
    });
});