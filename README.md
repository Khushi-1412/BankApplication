// Define your time series data
var timeSeriesData = [
    { x: new Date("2024-04-01"), y: 100 },
    { x: new Date("2024-04-02"), y: 110 },
    { x: new Date("2024-04-03"), y: 120 },
    // Add more data points...
];

// Create the chart
nv.addGraph(function() {
    var chart = nv.models.lineChart()
        .margin({left: 100}) // Adjust margin as needed
        .useInteractiveGuideline(true) // Enable interactive guideline
        .x(function(d) { return d.x }) // Specify x-axis accessor
        .y(function(d) { return d.y }) // Specify y-axis accessor
        .showLegend(true) // Show legend
        .showYAxis(true) // Show y-axis
        .showXAxis(true) // Show x-axis
        .forceX([new Date("2024-04-01"), new Date("2024-04-30")]) // Force x-axis range
        .forceY([0, 150]) // Force y-axis range
        .forceY([0, 100]); // Force y-axis range
        
    chart.xAxis
        .axisLabel("Date")
        .tickFormat(function(d) { return d3.time.format('%b %d')(new Date(d)) });

    chart.yAxis
        .axisLabel("Value")
        .tickFormat(d3.format(',.1f'));

    // Customize line colors and thickness based on API uptime and downtime
    chart.lines.forceY([0, 100]); // Force each line to cover 100% of y-axis height
    chart.lines.width(5); // Set line width to 5px

    // Set line colors based on uptime and downtime
    chart.lines.color(function(d, i) {
        return d.data.color;
    });

    d3.select('#chart svg')
        .datum([{ values: timeSeriesData }])
        .call(chart);

    nv.utils.windowResize(chart.update);

    return chart;
});
