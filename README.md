// Function to generate time series data for the last 30 days
function generateTimeSeriesData() {
    var today = new Date();
    var timeSeriesData = [];
    for (var i = 0; i < 30; i++) {
        var date = new Date(today);
        date.setDate(today.getDate() - i);
        timeSeriesData.push({ x: date });
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
    return { x: d.x, color: getColorFromDate(d.x) };
});

// Create the chart
nv.addGraph(function() {
    var chart = nv.models.lineChart()
        .margin({left: 100}) // Adjust margin as needed
        .useInteractiveGuideline(true) // Enable interactive guideline
        .x(function(d) { return d.x }) // Specify x-axis accessor
        .y(function(d) { return 1 }) // Set a constant y-value for bars
        .showLegend(true) // Show legend
        .showYAxis(false) // Hide y-axis labels
        .showXAxis(true); // Show x-axis
        
    chart.xAxis
        .axisLabel("Date")
        .tickFormat(function(d) {
            // Show date only when hovered over
            return d3.time.format('%b %d')(new Date(d));
        });

    // Customize line colors and thickness based on API uptime and downtime
    chart.lines.width(10); // Set line width to 10px

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

