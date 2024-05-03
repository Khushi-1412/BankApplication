// Function to generate time series data for the last 30 days
function generateTimeSeriesData() {
    var today = new Date();
    var timeSeriesData = [];
    for (var i = 0; i < 30; i++) {
        var date = new Date(today);
        date.setDate(today.getDate() - i);
        timeSeriesData.push({ date: date, value: 1 });
    }
    return timeSeriesData.reverse(); // Reverse the array to plot bars from oldest to newest
}

// Function to determine the color based on the date
function getColorFromDate(date) {
    // Example condition: Green if date is April 01, Red if date is April 02, else Blue
    if (date.getDate() === 1 && date.getMonth() === 3) { // April is month 3 (0-indexed)
        return "#00FF00"; // Green
    } else if (date.getDate() === 2 && date.getMonth() === 3) {
        return "#FF0000"; // Red
    } else {
        return "#0000FF"; // Blue (or any default color)
    }
}

// Set colors for each data point based on the date
var timeSeriesData = generateTimeSeriesData().map(function(d) {
    return { date: d.date, value: d.value, color: getColorFromDate(d.date) };
});

// Create the chart
nv.addGraph(function() {
    var chart = nv.models.discreteBarChart()
        .margin({left: 100}) // Adjust margin as needed
        .x(function(d) { return d.date }) // Specify x-axis accessor
        .y(function(d) { return d.value }) // Specify y-axis accessor
        .showValues(false) // Hide bar values
        .color(function(d) { return d.color }) // Set bar colors based on date
        .forceY([0, 1]) // Force y-axis range
        .showYAxis(true) // Show y-axis
        .showXAxis(true); // Show x-axis
        
    chart.yAxis
        .axisLabel("Date")
        .tickFormat(function(d) {
            // Show date only when hovered over
            return d3.time.format('%b %d')(new Date(d));
        });

    d3.select('#chart svg')
        .datum([{ key: "API Uptime/Downtime", values: timeSeriesData }])
        .call(chart);

    nv.utils.windowResize(chart.update);

    return chart;
});

