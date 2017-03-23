var loading = require('./loading');
var _ = require('lodash')

function loadData(type, element){
    var $el = $(element);

    $el.find('.js-data').empty();
    $el.find('.js-charts').empty();

    loading.start($el);

    $.ajax('/donations/v1/' + type)
    .done(function(result){
        loading.end($el);

        var structure = result.structure;

        $el.find('.js-header').text(structure.name);

        drawTable($el, result);

        for(var i = 0; i < structure.charts.length; i++) {
            var chart = structure.charts[i];
            switch(chart.type)
            {
                case "pie":
                    drawPie($el, result, chart);
                    break;
                case "bar":
                    drawBar($el, result, chart);
                    break;
            }
        }
    });
}

function drawPie($el, result, chart)
{
    // add row for pie chart
    var row = createChartRow($el);

    var datatable = getGenericDatatable(result, chart);
    var pie = new google.visualization.PieChart(row[0]);
    pie.draw(datatable, {
        title: chart.title,
        pieHole: 0.4,
        width: "100%",
        height: "400px",
        sliceVisibilityThreshold: .08
    });
}

function drawBar($el, result, chart)
{
    // add row for bar chart
    var row = createChartRow($el);

    var datatable = getGenericDatatable(result, chart, 7);
    var bar = new google.visualization.ColumnChart(row[0]);
    bar.draw(datatable, {
        title: chart.title,
        width: "100%",
        height: "400px"
    });
}

function getGenericDatatable(result, chart, limit) {
    var structure = result.structure;
    var data = result.data;

    var datatable = new google.visualization.DataTable();

    var labelProperty = _.find(structure.properties, {key: chart.label});
    datatable.addColumn(labelProperty.type, labelProperty.name);

    var dataProperty = _.find(structure.properties, {key: chart.data});
    datatable.addColumn(dataProperty.type, dataProperty.name);

    var rows = [];
    for(var i = 0; i < data.length; i++) {
        var d = data[i];
        // can't put negative values in a pie chart
        if(chart.type === "pie" && d[chart.data] < 0) {
            continue;
        }
        rows.push([d[chart.label], d[chart.data]]);
    }

    if(limit){
        rows = _.chain(rows).sortBy(r => r[1]).reverse().take(limit).value();
    }

    datatable.addRows(rows);
    return datatable;
}

function createChartRow($el) {
    var col = $('<div class="col"></div>');
    var row = $('<div class="row" style="height: 400px"></div>');
    col = col.appendTo(row);
    row.appendTo($el.find('.js-charts').eq(0));
    return col;
}

function drawTable($el, result)
{
    var structure = result.structure;
    var data = result.data;

    var datatable = new google.visualization.DataTable();
    var dtProperties = _.filter(structure.properties, function(p) {
        return !_.includes(structure.datatable.skipProperties, p.key);
    });

    for(var i in dtProperties)
    {
        var item = dtProperties[i];
        datatable.addColumn(item.type, item.name);
    }

    var rows = [];
    for(var i = 0; i < data.length; i++) {
        var row = [];
        for(var key in dtProperties)
        {
            var prop = dtProperties[key];
            row.push(data[i][prop.key]);
        }
        rows.push(row);
    }
    datatable.addRows(rows);

    var columnIndex = 0;
    for(var key in structure.properties) {
        var property = structure.properties[key];
        if(property.formatter) {
            var formatter = new google.visualization.NumberFormat({pattern: property.formatter});
            formatter.format(datatable, columnIndex);
        }
        columnIndex++;
    }

    var table = new google.visualization.Table($el.find('.js-data')[0]);
    table.draw(datatable, {
        showRowNumber: true, width: "100%", page: true,
        sortColumn: _.findIndex(structure.properties, function(p) { return p.key === structure.datatable.initialSort }),
        sortAscending: structure.datatable.initialSortDirection === "asc",
    });
}

module.exports = {
    loadData: loadData
}