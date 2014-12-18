
﻿if (navigator.userAgent.match(/IEMobile\/10\.0/)) {
	var msViewportStyle = document.createElement("style")
	msViewportStyle.appendChild(
		document.createTextNode(
			"@-ms-viewport{width:auto!important}"
		)
	)
	
	document.getElementsByTagName("head")[0].appendChild(msViewportStyle)
}
﻿
$(document).ready(function () {
    $("[rel='tooltip']").tooltip({ html: true });
});

$(document).on("click", ".table-clickable > tbody > tr", function (event) {
    var href = $(this).find("a").attr("href");
    if (href) {
        window.location = href;
        event.stopPropagation();
    }
});


function findWinner() {
    var setsNeeded = $("#SetsNeededToWin").val();
    var setsCount = 0;
    var found = false;
    var homewins = 0;
    var awaywins = 0;
    var draws = 0;
    var homeCount = 0;
    var homeTeam = "";
    var awayCount = 0;
    var awayTeam = "";

    $(".checkbox-home:checked").each(function () {
        var name = $(this).data("name");
        homeTeam = homeTeam + name + ", ";
        homeCount++;
    });
    homeTeam = homeTeam.substring(0, homeTeam.length - 2)

    $(".checkbox-away:checked").each(function () {
        var name = $(this).data("name");
        awayTeam = awayTeam + name + ", ";
        awayCount++;
    });
    awayTeam = awayTeam.substring(0, awayTeam.length - 2)

    $(".result-home").each(function (index) {
        var self = $(this);

        if (self.val() && $('.' + self.data('ref')).val()) {
            setsCount++;

            var homescore = Number(self.val());
            var awayscore = Number($('.' + self.data('ref')).val());

            if (homescore > awayscore)
                homewins++;
            else if (awayscore > homescore)
                awaywins++;
            else if (homescore == awayscore)
                draws++;
        }
    });

    if (homewins >= setsNeeded || awaywins >= setsNeeded || draws >= setsNeeded)
        found = true;

    if (homeCount == 0 || awayCount == 0)
        found = false;

    if (found) {
        var text = homewins == awaywins ? "draws" : homewins > awaywins ? "wins against" : "looses to";

        //var textArray = ['{0} wins {1}', '{0} beats {1}', '{0} kicks {1} asses', '{0} takes a piss on {1}'];
        //var randomNumber = Math.floor(Math.random()*textArray.length);

        $('.player-home').html(homeTeam);
        $('.player-away').html(awayTeam);
        $('.text-winloose').html(text);

        $('.result-winner').fadeIn();
        $('.btn-submit').prop("disabled", false);
    }
    else {
        $('.result-winner').fadeOut();
        $('.btn-submit').prop("disabled", true);
    }
}
