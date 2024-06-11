define([
    "text!templates/healthCheckPage.html",
    "jquery",
    "underscore",
    "d3",
    "nvd3",
    "backbone"
], function(healthCheckPage, $, _, d3, nv, Backbone) {

    IPI.Portal.Views.HealthCheckPage = Backbone.View.extend({

        template: _.template(healthCheckPage),

        initialize: function() {
            document.addEventListener('visibilitychange', () => {
                console.log(document.visibilityState);
                window.dispatchEvent(new Event('resize'));
            });
        },

        render: function() {
            var self = this;
            console.log("rendering start");
            self.$el.html(this.template());
            console.log("rendering graphs");

            this.fetchAndRenderGraph('gpp-chart', 'gpp');
            this.fetchAndRenderGraph('gpu-chart', 'gpu');
            this.fetchAndRenderGraph('gps-chart', 'gps');
            this.fetchAndRenderGraph('ipr-chart', 'ipr');

            console.log("Finished rendering");
            return this;
        },

        fetchAndRenderGraph: function(chartId, service) {
            var self = this;

            $.ajax({
                url: '/data/' + service,
                method: 'GET',
                success: function(response) {
                    self.renderGraph(chartId, response);
                },
                error: function(error) {
                    console.error("Error fetching data for service: " + service, error);
                }
            });
        },

        renderGraph: function(chartId, dataApi) {
            nv.addGraph(function() {
                function generateTimeSeriesData(dataApi) {
                    var today = new Date();
                    var timeSeriesData = [];
                    for (var i = 0; i < 30; i++) {
                        var date = new Date(today);
                        date.setDate(today.getDate() - i);
                        var options = { month: 'long', day: 'numeric', year: 'numeric' };
                        var dateString = date.toLocaleDateString('en-US', options);
                        var dateKey = date.toISOString().split('T')[0];
                        var apiData = dataApi[dateKey] || { successfulcalls: 0, unsuccessfulcalls: 0 };
                        timeSeriesData.push({ date: dateString, successfulcalls: apiData.successfulcalls, unsuccessfulcalls: apiData.unsuccessfulcalls });
                    }
                    return timeSeriesData.reverse();
                }

                function getColorFromDate(successfulcalls, unsuccessfulcalls) {
                    if (successfulcalls > unsuccessfulcalls) {
                        return "#5daf5f"; // green
                    } else if (Math.abs(unsuccessfulcalls - successfulcalls) < 10) {
                        return "#E6CE66"; // yellow
                    } else {
                        return "#FF0000"; // red
                    }
                }

                var timeSeriesData = generateTimeSeriesData(dataApi);

                var chart = nv.models.discreteBarChart()
                    .margin({ left: 100 })
                    .x(function(d) { return d.date })
                    .y(function(d) { return 1; })
                    .showValues(false)
                    .color(function(d) { return getColorFromDate(d.successfulcalls, d.unsuccessfulcalls) })
                    .forceY([0, 1])
                    .showYAxis(false)
                    .showXAxis(false)
                    .width(900)
                    .tooltipContent(function(key, x, y, e, graph) {
                        return '<h3>' + x + '</h3>' +
                            '<p>' + " Success Rate: " + ((e.point.successfulcalls / (e.point.successfulcalls + e.point.unsuccessfulcalls)) * 100).toFixed(2) + "%" + '</p>' +
                            '<p>' + " Failure Rate: " + ((e.point.unsuccessfulcalls / (e.point.successfulcalls + e.point.unsuccessfulcalls)) * 100).toFixed(2) + "%" + '</p>';
                    });

                d3.select('#' + chartId + ' svg')
                    .datum([{ key: "API Uptime/Downtime", values: timeSeriesData }])
                    .call(chart);

                d3.select('#' + chartId + ' svg')
                    .append("text")
                    .attr("x", 115)
                    .attr("y", 90)
                    .style("text-anchor", "middle")
                    .text("30 days ago")
                    .attr("class", "x-axis label");

                d3.select('#' + chartId + ' svg')
                    .append("text")
                    .attr("x", 880)
                    .attr("y", 90)
                    .style("text-anchor", "middle")
                    .text("Today")
                    .attr("class", "x-axis label");

                nv.utils.windowResize(chart.update);

                return chart;
            });
        }
    });
});
