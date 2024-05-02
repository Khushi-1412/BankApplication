// healthcheckpage.js

// Assuming you have already defined other necessary variables and functions...

// Define your API uptime and downtime data
var apiData = [
    { date: "2024-04-01", uptime: 23, downtime: 1 },
    { date: "2024-04-02", uptime: 22, downtime: 2 },
    // Add data for the remaining days...
];

// Function to render the discrete bar chart for API uptime and downtime
function renderUptimeDowntimeChart(data) {
    nv.addGraph(function() {
        var chart = nv.models.discreteBarChart()
            .x(function(d) { return d.date; })
            .y(function(d) { return d.uptime; })
            .staggerLabels(true)
            .tooltips(false)
            .showValues(true);

        chart.yAxis.axisLabel('Hours');
        chart.xAxis.axisLabel('Date');

        d3.select('#api-uptime-downtime-chart svg')
            .datum(data)
            .call(chart);

        nv.utils.windowResize(chart.update);

        return chart;
    });
}

// Call the function to render the chart with the API data
renderUptimeDowntimeChart(apiData);

