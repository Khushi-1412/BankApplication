
define([
	"text!templates/healthCheckPage.html",
	"jquery",
	"underscore",
    "d3",
    "nvd3",
	"backbone"
],function(healthCheckPage)
{

	
	IPI.Portal.Views.HealthCheckView = Backbone.View.extend({
		template:_.template(healthCheckPage),
		render:function()
		{
			this.$el.html(this.template());
			console.log("rendering graph");
			this.rendergraph();
		
			console.log("Finished rendering");
			return this;
		},
		

		rendergraph:function () {
			
			var dataAPi={
				"2024-04-07":{"successfulcalls":16,"unsuccessfulcalls":2},
				"2024-04-08":{"successfulcalls":7,"unsuccessfulcalls":8},
				"2024-04-09":{"successfulcalls":17,"unsuccessfulcalls":8},
				
			};
	
			function generateTimeSeriesData() {
			    var today = new Date();
			    var timeSeriesData = [];
			    for (var i = 0; i < 30; i++) {
			       var date = new Date(today);
			        
			       date.setDate(today.getDate() - i);
			       var options={month:'long',day:'numeric',year:'numeric'};
			       var dateString= date.toLocaleDateString('en-US',options);
			       var apiData=dataAPi[date.toISOString().split('T')[0]]|| {successfulcalls:0,unsuccessfulcalls:0};
			       timeSeriesData.push({ date: dateString, successfulcalls:apiData.successfulcalls,unsuccessfulcalls:apiData.unsuccessfulcalls});
			    }
			    return timeSeriesData.reverse();
			}
			
			// Function to determine the color based on the date
			function getColorFromDate(successfulcalls,unsuccessfulcalls) {
			
			    if (successfulcalls>unsuccessfulcalls) { // April is month 3 (0-indexed)
			        return "#00FF00"; // Green
			    } 
			    else if(successfulcalls<unsuccessfulcalls)
			   {
			        return "#FF0000";
			        }
			        else
			        {
			        return "#00FF00";
			        }
			    
			}
			
			// Set colors for each data point based on the date
			var timeSeriesData = generateTimeSeriesData();
			
					
			nv.addGraph(function() {
			    var chart = nv.models.discreteBarChart()
			        .margin({left: 100}) 
			        .x(function(d) { return d.date }) 
			        .y(function(d) { return 1 }) 
			        .showValues(false) 
			        .color(function(d) { return getColorFromDate(d.successfulcalls, d.unsuccessfulcalls) }) 
			        .forceY([0, 1]) 
			        .showYAxis(false)
			        .showXAxis(false);
			       
			        
			       
			 /*       
			    chart.yAxis
			        .axisLabel("Response");
			        
			    chart.xAxis
			        .axisLabel("30 days Records");
			      */
			   
			    /*chart.tooltip(function(d)
			    {
					var data=d.data;
					var successfulcalls=data.successfulcalls;
					var unsuccessfulcalls= data.unsuccessfulcalls;
					var content="<div>";
					content += "<p>successfulcalls: " + successfulcalls +"</p>";
					content += "<p>unsuccessfulcalls: " + unsuccessfulcalls +"</p>";
					content+="</div";
					return content;
					
				});*/
			
			    d3.select('#api-uptime-downtime-chart svg')
			        .datum([{ key: "API Uptime/Downtime", values: timeSeriesData }])
			        .call(chart);
			
			    nv.utils.windowResize(chart.update);
			
			    return chart;
			});
          }	
          
         
    

	});
	
});
		
