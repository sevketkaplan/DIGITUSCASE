var c3DonutChart = c3.generate({
    bindto: '#c3-donut-chart',
    data: {
        columns: surveyanalitics[0],
        type: 'donut',
        onclick: function (d, i) {
        },
        onmouseover: function (d, i) {
        },
        onmouseout: function (d, i) {
        }
    },
    color: {
        pattern: ['rgba(88,216,163,1)', 'rgba(4,189,254,0.6)', 'rgba(237,28,36,0.6)', 'rgba(235, 110, 0, 0.6)', 'rgba(0,100,230,0.6)']
    },
    padding: {
        top: 0,
        right: 0,
        bottom: 30,
        left: 0
    },
    donut: {
        title: ""
    }
});

setTimeout(function () {
    var total = surveyanalitics[1];
    for (var i = 0; i < surveyanalitics[2].length; i++) {
        var deger = '.' + surveyanalitics[2][i][0];
        var percentage1 = surveyanalitics[2][i][1] > 0 ?
            Math.ceil(((surveyanalitics[2][i][1] / total) * 100) * 100) / 100 : 0;
        $(deger).css({ "width": percentage1 + 1 + "%" }).html(percentage1 + "%");
    }
}, 1);