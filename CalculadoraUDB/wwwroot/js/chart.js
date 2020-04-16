$(document).ready(function () {
    $.ajax({
        url: "/Expediente/GetChartData",
        type: "post",
        dataType: "json",
        success: function (chart) {
            LoadChart(chart);
        },
        error: function (chart) {
            console.log("Problema al llamar al action /Expediente/GetChartData");
        }
    });
});

function LoadChart(chart) {
    console.log(chart);
    Highcharts.chart('container', {
        title: {
            text: 'Rendimiento academico / años'
        },
        subtitle: {
            text: 'Actualizado según consolidado de materias'
        },
        xAxis: {
            categories: chart.anios
        },
        yAxis: {
            title: {
                text: 'Materias'
            },
            plotLines: [{
                value: 0,
                width: 1,
                color: '#808080'
            }]
        },
        tooltip: {
            valueSuffix: ' Materias'
        },
        legend: {
            title: {
                text: 'Clasificación<br /><span style="font-size: 9px; color: #666; font-weight: normal">(Escojer deseado)</span>',
                style: {
                    fontStyle: 'italic'
                }
            },
            layout: 'vertical',
            align: 'right',
            verticalAlign: 'top',
            x: -10,
            y: 100
        },
        series: [{
            name: 'Aprobadas',
            color: '#38c300',
            data: chart.Aprobadas
        }, {
            name: 'Retiradas',
            color: '#ffe700',
            data: chart.Retiradas
        }, {
            name: 'Reprobadas',
            color: '#FF0000',
            data: chart.Reprobadas
        }]
    });
}