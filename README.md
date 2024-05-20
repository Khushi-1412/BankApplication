
define([
	"text!templates/healthCheckPage.html",
	"jquery",
	"underscore",
	"d3",
	"nvd3",
	"backbone"
], function(healthCheckPage) {


	IPI.Portal.Views.HealthCheckPage = Backbone.View.extend({
		template: _.template(healthCheckPage),

		initialize: function() {
			$('a[href="#healthcheck"]').on('click', function() {

				window.location.reload(true);


				console.log("initialize graph");
				/*this.render();*/

			}.bind(this));

		},

		render: function() {
			var self = this;
			console.log("rendering start");
			self.$el.html(this.template());
			console.log("rendering graph");
			this.rendergraph();

			console.log("Finished rendering");
			return this;
		},


		rendergraph: function() {

			nv.addGraph(function() {
				var chart = nv.models.discreteBarChart()
					.margin({ left: 100 })
					.x(function(d) { return d.date })
					.y(function(d) { return 1; })

					.showValues(false)
					.color(function(d) { return getColorFromDate(d.successfulcalls, d.unsuccessfulcalls) })
					.forceY([0, 1])
					.showYAxis(false)
					.showXAxis(false)
					.tooltipContent(function(key, x, y, e, graph) {
						return '<h3>' + x + '</h3>' +
							'<p>' + " Success Rate: " + ((e.point.successfulcalls / (e.point.successfulcalls + e.point.unsuccessfulcalls)) * 100).toFixed(2) + "%" + '</p>'
					})
					
				



				var dataAPi = {
					"2024-05-15": { "successfulcalls": 48, "unsuccessfulcalls": 2 },
					"2024-05-14": { "successfulcalls": 25, "unsuccessfulcalls": 25 },
					"2024-05-13": { "successfulcalls": 17, "unsuccessfulcalls": 33 },
					"2024-05-12": { "successfulcalls": 35, "unsuccessfulcalls": 15 },
					"2024-05-10": { "successfulcalls": 45, "unsuccessfulcalls": 5 },
					"2024-05-09": { "successfulcalls": 30, "unsuccessfulcalls": 20 },
					"2024-04-17": { "successfulcalls": 10, "unsuccessfulcalls": 40 },
					"2024-05-07": { "successfulcalls": 35, "unsuccessfulcalls": 15 },
					"2024-04-20": { "successfulcalls": 23, "unsuccessfulcalls": 27 },
					"2024-04-23": { "successfulcalls": 30, "unsuccessfulcalls": 20 },
					"2024-04-25": { "successfulcalls": 28, "unsuccessfulcalls": 22 },

				};

				function generateTimeSeriesData() {
					var today = new Date();
					var timeSeriesData = [];
					for (var i = 0; i < 30; i++) {
						var date = new Date(today);

						date.setDate(today.getDate() - i);
						var options = { month: 'long', day: 'numeric', year: 'numeric' };
						var dateString = date.toLocaleDateString('en-US', options);
						var apiData = dataAPi[date.toISOString().split('T')[0]] || { successfulcalls: 50, unsuccessfulcalls: 4 };
						timeSeriesData.push({ date: dateString, successfulcalls: apiData.successfulcalls, unsuccessfulcalls: apiData.unsuccessfulcalls });

					}
					return timeSeriesData.reverse();
				}

			
				function getColorFromDate(successfulcalls, unsuccessfulcalls) {

					if (successfulcalls > unsuccessfulcalls ) {
						return "#5daf5f"; //green
					}
					else if (Math.abs(unsuccessfulcalls - successfulcalls) < 10) {
						return "#E6CE66"; //yellow
					}
					else {
						return "#FF0000"; //red
					}

				}
			

				var timeSeriesData = generateTimeSeriesData();


				d3.select('#api-uptime-downtime-chart svg')
					.datum([{ key: "API Uptime/Downtime", values: timeSeriesData }])
					.call(chart);
					
			    d3.select('#api-uptime-downtime-chart-1 svg')
					.datum([{ key: "API Uptime/Downtime", values: timeSeriesData }])
					.call(chart);
 			  

				d3.select('#api-uptime-downtime-chart svg')
					.append("text")
					.attr("x", 115)
					.attr("y", 120)
					.style("text-anchor", "middle")
					.text("30 days ago")
					.attr("class", "x-axis label");

				d3.select('#api-uptime-downtime-chart svg')
					.append("text")
					.attr("x", 900)
					.attr("y", 120)
					.style("text-anchor", "middle")
					.text("Today")
					.attr("class", "x-axis label");

				nv.utils.windowResize(chart.update);

				return chart;
			});
		}




	});

});

