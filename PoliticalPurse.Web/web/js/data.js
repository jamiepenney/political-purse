var loading = require('./loading');
var _ = require('lodash');

function parseName(structure, query) {
    var name = structure.name;

    if (query) {
        var index = name.indexOf("{{");
        while (index >= 0) {
            var endIndex = name.indexOf("}}", index);

            var propName = name.substring(index + 2, endIndex);
            var replaceValue = query[propName];
            if (replaceValue) {
                name = name.replace("{{" + propName + "}}", replaceValue + "");
            }
            index = name.indexOf("{{");
        }
    }
    
    return name;
}

function loadData(datasource, query, element){
    var $el = $(element);

    $el.find('.js-data').empty();
    $el.find('.js-charts').empty();
    $el.find('.js-header').empty();
    $el.find('.js-sub-header').empty();
    $el.find('.js-query').empty();

    loading.start($el);

    $.ajax('/'+datasource+'/v1/' + query)
    .done(function(result){
        loading.end($el);

        var structure = result.structure;

        $el.find('.js-header').text(parseName(structure, result.query));
        if (result.query) {
            $el.find('.js-sub-header').text(result.query.description);
        }

        if (structure.query) {
            generateQueryBox($el.find('.js-query'), structure.query);
        }
        
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

function generateQueryBox($el, query) {
    $el.empty();

    var uri = new URI(!location.hash || location.hash === "#" ? "" : location.hash.substring(1));
    var uriParams = uri.search(true);

    _.forEach(query.parameters, parameter => {
        let $param = $(`<div><label for="query_${parameter.name}">${parameter.title}</label></div>`);

        switch(parameter.type) {
            case "year":
                var years = _.range(parameter.minValue, parameter.maxValue + 1)
                    .map(year => `<option value="${year}" ${uriParams[parameter.name] === year.toString() ? "selected" : ""}>${year}</option>`);
                var $select = $(`<select id="query_${parameter.name} name="${parameter.name}"><option value="">All</option></select>`).append(years).appendTo($param);
                $select.on('change', function() {
                    const newVal = $select.val();
                    var newUri = new URI(!location.hash || location.hash === "#" ? "" : location.hash.substring(1));
                    var newSearch = uri.search(true);
                    newSearch[parameter.name] = newVal;
                    newUri.search(newSearch);
                    location.hash = newUri.toString();
                });
                break;
        }
        $el.append($param);
    });
}

function drawPie($el, result, chart)
{
    // add row for pie chart
    var row = createChartRow($el);

    var datatable = getGenericDatatable(result, chart);
    var pie = new window.google.visualization.PieChart(row[0]);
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
    var bar = new window.google.visualization.ColumnChart(row[0]);
    bar.draw(datatable, {
        title: chart.title,
        width: "100%",
        height: "400px"
    });
}

function getGenericDatatable(result, chart, limit) {
    var structure = result.structure;
    var data = result.data;

    var datatable = new window.google.visualization.DataTable();

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

    var datatable = new window.google.visualization.DataTable();
    var dtProperties = _.filter(structure.properties, function(p) {
        return !_.includes(structure.datatable.skipProperties, p.key);
    });

    for(var i in dtProperties)
    {
        var item = dtProperties[i];
        datatable.addColumn(item.type, item.name);
    }

    var rows = [];
    for(var d = 0; d < data.length; d++) {
        var row = [];
        for (var j = 0; j < dtProperties.length; j++)
        {
            var prop = dtProperties[j];
            row.push(data[d][prop.key]);
        }
        rows.push(row);
    }
    datatable.addRows(rows);

    var columnIndex = 0;
    for (var key in dtProperties) {
        var property = dtProperties[key];
        console.log("checking column " + columnIndex + ": " + property.name);
        if(property.formatter) {
            var formatter = new window.google.visualization.NumberFormat({pattern: property.formatter});
            formatter.format(datatable, columnIndex);
            console.log("formatting column " + columnIndex);
        }
        columnIndex++;
    }

    var table = new window.google.visualization.Table($el.find('.js-data')[0]);
    table.draw(datatable, {
        showRowNumber: true, width: "100%", page: true,
        sortColumn: _.findIndex(structure.properties, function(p) { return p.key === structure.datatable.initialSort; }),
        sortAscending: structure.datatable.initialSortDirection === "asc"
    });
}

module.exports = {
    loadData: loadData
};