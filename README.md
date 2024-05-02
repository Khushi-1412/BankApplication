
define([
	"jquery",
	"underscore",
    "text!templates/healthCheckPage.html",
     "d3",
    "nvd3",
	"backbone"
],
 function(healthCheckTemplate,d3,nv,Backbone)
{
	IPI.Portal.Views.HealthCheckView = Backbone.View.extend({
		template:_.template(healthCheckTemplate),
		render:function()
		{
			this.$el.html(this.template());
			this.renderGraph();
			console.log("cjdbcbd33455");
			return this;
		},
		renderGraph : function()
		{
			var data=[];
    		 for(var i=0;i<=30;i++)
{
 
 
var isUptime=(Math,random()<0.5);
 
data.push({
day:"Day"+i,
value:Math.floor(Math,random()*100),
isUptime : isUptime
});
}
 
var chart=nv.models.discreteBarChart()
.x(function(d){return d.day;})
.y(function(d){return d.value;})
.color(function(d){return d.isUptime?"green" : "red";})
.tooltip(function(key,x,y,e,graph)
{
return "<h3>" +key + "</h3><p>" + y + "hours</p>";
});
 
d3.select("#graphical").dataum(data).call(chart);
 
nv.utils.windowResize(chart.update);
 
return this;
}
		
				
			});
		
	console.log("cjdbcbd");
	return IPI.Portal.Views.HealthCheckView;
	
});



