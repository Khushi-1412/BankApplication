define([
    "text!templates/account/assetItemTemplate.html",
    "text!templates/account/assetDetailTemplate.html",
    "text!templates/account/reportsDetailView.html",
    "text!templates/account/totalCalls.html",
    "backbone",
    "jquery.ui",
    "jquery.daterangepicker",
    "jquery.neuDateRangePicker",
    "app/enum",
    "d3",
    "nvd3"
    
], function(tmpl, detailViewTemplate, reportsDetailView, totalCallView) {
    IPI.Portal.Views.Account.AssetItemView = Backbone.View.extend({
        tagName: "tr",
        className: "rowClass",
        dateFilters: null,
        successChart: null,
        errorChart: null,
        controlsData: {
//           "hourly" : [
//              { key: 'Hourly',  disabled: false },
//              { key: 'Daily',   disabled: true },
//              { key: 'Monthly', disabled: true }
//           ],
           "daily" : [
//               { key: 'Hourly',  disabled: true },
               { key: 'Daily',   disabled: false },
               { key: 'Monthly', disabled: true }
           ],
           "monthly" : [
//               { key: 'Hourly',  disabled: true },
               { key: 'Daily',   disabled: true },
               { key: 'Monthly', disabled: false }
           ]
        },
        events: {
            "click .asset-name-col": "onAssetClicked",
            "click .license-key-col": "onAssetClicked",
            "click .ico-col-text": "onUsageReportClicked"
        },

        initialize: function() {
            var self = this;
            this.model = this.options.model;
            IPI.Portal.Views.ViewManager.events.on("portal.cancelSubscription.success", function(msg, subscriptionModel){
                if(self.model.get("id") === subscriptionModel.id) {
                    self.model.set("status", subscriptionModel.get("status"));
                    self.model.set("cancelled", subscriptionModel.get("cancelled"));
                    self.model.set("endDate", subscriptionModel.get("endDate"));
                    self.model.get("displayAttributes").status = IPI.Portal.App.Enums.ASSET_STATUS.CANCELLED;
                    self.render();
                }
            });
            return this;
        },

        render: function() {   // to render the main pg of license tab on click of 'return to license' link.
            var self = this;
            self.$el.html(_.template(tmpl, {data: self.model.attributes}));
            return this;
        },

        renderAsset: function(){   //render the asset details and plot graphs.
            this.reportsView = $("#reports-view");
            var self = this;
            $("#reports-title").text($.i18n.prop("messages.portal.assetlist.id_" + self.model.attributes.product));
            self.reportsView.html(_.template(reportsDetailView, {data: self.model.attributes}));
            var oneDay = 1000*60*60*24;
            var _31Day = oneDay * 31;
            var _7Day = oneDay * 7;

            self.dateRangeWidget = $('#date-range-picker');
            self.dateRangeWidget.neuDateRangePicker({
                defaultValues: self.defaults,
                change: function(dateFilters,event){

                    if(self.dateFilters === null){
                        self.dateFilters = dateFilters;
                    } else if(self.dateFilters.utcStartDate !== dateFilters.utcStartDate ||
                        self.dateFilters.utcEndDate !== dateFilters.utcEndDate) {
                        self.dateFilters = dateFilters;
                        d3.selectAll("#reports-view svg  > *").remove();
                        self.reportsView.find(".totalErrorCalls").remove();
                    } else {
                        // do not proceed if similar option is selected comparing to the last selection.
                        return;
                    }


                    IPI.Portal.Views.ViewManager.showWaitIndicator();
                    
                    var fetchUrl;

                    if(self.model.get("ddsId") !== null) {
                        fetchUrl = "secure/resources/apiusage/dds/"+self.model.get("ddsId");
                        $("#key-usage-lineChart-errorCodes").hide();
                        $("#dataRefreshed").text("Data refreshed daily");
                    } else {
                        fetchUrl = "secure/resources/apiusage/keys/"+self.model.get("licenseKey");
                        $("#key-usage-lineChart-errorCodes").show();
                        $("#dataRefreshed").text("Data refreshed hourly");
                    }
                    
                    self.model.fetch({
                        url: fetchUrl,
                        data: {
                            startDate: dateFilters.utcStartDate,
                            endDate: dateFilters.utcEndDate,
                            interval: function(){
                                if (dateFilters.interval === "monthly"){
                                    return IPI.Portal.App.Enums.REPORT_INTERVAL.MONTHLY;
                                }
                                if((dateFilters.utcEndDate - dateFilters.utcStartDate) > (_31Day * 3)) {
                                    return ipiadmin.app.Enums.REPORT_INTERVAL.MONTHLY;
                                }
                                if((dateFilters.utcEndDate - dateFilters.utcStartDate) > (oneDay*2)) {
                                    return IPI.Portal.App.Enums.REPORT_INTERVAL.DAILY;
                                }

                                return IPI.Portal.App.Enums.REPORT_INTERVAL.DAILY;
                            }
                        },
                        success: function(resp){
                            IPI.Portal.Views.ViewManager.hideWaitIndicator();
                            var autoScale = true;
                            if(dateFilters.interval === IPI.Portal.App.Enums.REPORT_INTERVAL.MONTHLY) {
                                autoScale = false;
                            }else {
                                autoScale = !( dateFilters.utcEndDate - dateFilters.utcStartDate <= _31Day );
                            }
                            var overflow = ( dateFilters.utcEndDate - dateFilters.utcStartDate > _7Day );


                            var uri;
                            
                            if(self.model.get("licenseKey") !== null) {
                            self.drawSuccessErrorGraph(resp.attributes, autoScale, overflow);
                            self.drawErrorCodesGraph(resp.attributes, autoScale, overflow);      
                            
                                uri = "secure/resources/apiusage/download/keys/" +self.model.get("licenseKey")+
                                          "?startDate="+dateFilters.utcStartDate+"&endDate=" +dateFilters.utcEndDate+
                                          "&interval="+self.model.attributes.interval;                            
                        } else {
                            self.drawSuccessDDSGraph(resp.attributes, autoScale, overflow);
                            
                                uri = "secure/resources/apiusage/download/ddsId/" +self.model.get("ddsId")+
                                          "?startDate="+dateFilters.utcStartDate+"&endDate=" +dateFilters.utcEndDate+
                                          "&interval="+self.model.attributes.interval;                            
                        }
                        
                           //create a download link for csv.
                            self.reportsView.find(".download-link").on("click", function(event){
                                event.preventDefault();

                                self.reportsView.find("#dataLink").attr("href", uri)[0].click();
                            });                        
                        }
                    });
                } 
            });
            //return this;
        },

        redrawGraph: function(graph, lineData, chart, interval, autoScale, overflow){

            var self = this;
            var key = lineData[0].key;
            var dataFormat = '%b %Y'; // monthly format by default

            if(interval === IPI.Portal.App.Enums.REPORT_INTERVAL.HOURLY){
                dataFormat = '%I%p';
            }
            if(interval === IPI.Portal.App.Enums.REPORT_INTERVAL.DAILY) {
                dataFormat = '%b %d %Y';
            }
            //if(autoScale === false) {
            //    chart.xAxis.tickValues(lineData[0].xAxisTickValues);
            //}
            var ticks = this._processAxisTicks(lineData[0].xAxisTickValues);
            chart.xAxis.tickValues(ticks);

            var isRotate = false;
            if(overflow === false && lineData[0].interval ===
                IPI.Portal.App.Enums.REPORT_INTERVAL.DAILY && autoScale === false ) {
                // do not rotate x labels
                chart.xAxis.rotateLabels(0);
            } else {
                isRotate = true;
                chart.xAxis.rotateLabels(-45);
            }

            chart.options({
                rightAlignLegend: false,
                width: 1100,
                margin: {bottom: isRotate? 115 : 70,  left: 100}
            });

            chart.legend.margin({
                top: 5,
                bottom: 90,
                left: 0,
                right: 0
            });

            chart.yAxis.tickFormat(d3.format(',f'));
            chart.xAxis
                .tickFormat(function(d) {
                        return d3.time.format.utc(dataFormat)(new Date(d));
                });

            chart.lines.forceY([0,10]);

            graph.append("text")
                         .attr("x", parseInt(graph.style('width')) - 75)
                         .attr("y", parseInt(graph.style('height')) - 30)
                         .attr("text-anchor", "middle")
                         .text($.i18n.prop("messages.portal.usage_report.timezone"));

            graph.datum(lineData).transition().duration(600).call(chart);

        },

        //Graph for the Success/blocked and other calls.
        drawSuccessErrorGraph: function(data, autoScale, overflow) {
            var self = this;
            var lineData = this._processRequestData(data);
            var interval = lineData[0].interval;

            //table to generate total success/blocked and other calls
            var div = self.reportsView.find('#key-usage-lineChart').append('<div class="totalErrorCalls"></div>');
            $('.totalErrorCalls').html(_.template(totalCallView, {data: data}));


            var graph = d3.select("#key-usage-lineChart svg");

            if(self.successChart === null){

                nv.addGraph(function() {
                    self.successChart = chart = nv.models.lineChart();

                    self.redrawGraph(graph, lineData, self.successChart, interval, autoScale, overflow);
                    self.successErrorGraphElements(graph, lineData, interval);

                    nv.utils.windowResize(chart.update);

                    return chart;
                });
            } else {
                self.redrawGraph(graph, lineData, self.successChart, interval, autoScale, overflow);
                self.successErrorGraphElements(graph, lineData, interval);

                nv.utils.windowResize(chart.update);
            }
        },

        successErrorGraphElements: function(graph, lineData, interval){
            var self = this;

            // Tooltips

            // create legend for hourly/monthly and daily tickers.
            var intervalControls = graph.select('.nv-wrap g').append('g').attr('class', 'nv-intervalWrap').attr('id','nv-intervalWrapId');
            var controls = nv.models.legend().height(30).color(['#444', '#444', '#444']);
            intervalControls.datum(self.controlsData[interval])
                .attr('transform', 'translate(628,-115)')
                .call(controls);

            $('.nv-intervalWrap').find('g').find('.nv-series').find('.nv-legend-symbol,.nv-hover-icon').remove();

            $(".nv-legend-text").each(function(){
               if($(this).text() === "Others"){
                  $(this).next().attr('id','img2').attr('x', '45');
               }
            });

            //on hover show the different error rsponse codes.
            $(".nv-hover-icon").on("mouseover", function(event){
                var ele = $(this);
                var myindex =  $(this).parent().index();
                var offset = ele.offset();
                var longLabel = "";
                switch (lineData[myindex].key) {
                    case "Success": {
                        longLabel = 'Success response code: 200 OK Successfull HTTP requests';
                        break;
                    }
                    case "Blocked" :{
                        longLabel = 'Error response Code: 403 Developer is not authorized';
                        break;
                    }
                    case "Others" :{
                        longLabel = 'Error repsonse codes: 400 Bad request, 404 Not Found, 500 Internal Server Error, 503 Service Unavailable, 504 Gateway Timeout';
                        break;
                    }

                }

                nv.tooltip.show([offset.left + 111, offset.top + 90], '<p>' + longLabel.replace(/\,/g, "<br>") + '</p>');

            });

            $(".nv-hover-icon").on("mouseout", function(event){
                nv.tooltip.cleanup();
            });

        },
        
        //Graph for the breakdown of the error calls.
        drawErrorCodesGraph: function(data, autoScale, overflow){
            var self = this;
            var lineData = this._errorResponseCodes(data);
            var interval = lineData[0].interval;
            var key = lineData[0].key;

            var graph = d3.select("#key-usage-lineChart-errorCodes svg");

            if(self.errorChart === null){

                nv.addGraph(function() {
                    self.errorChart = chart = nv.models.lineChart();

                    self.redrawGraph(graph, lineData, self.errorChart, interval, autoScale, overflow);
                    self.errorCodesGraphElements(graph, interval);
                    nv.utils.windowResize(chart.update);

                    return chart;
                });
            } else {

                self.redrawGraph(graph, lineData, self.errorChart, interval, autoScale, overflow);
                self.errorCodesGraphElements(graph, interval);

                nv.utils.windowResize(chart.update);
            }
        },
        
        //Graph for the Success/blocked and other calls.
        drawSuccessDDSGraph: function(data, autoScale, overflow) {
            var self = this;
            var lineData = this._processDPData(data);
            var interval = lineData[0].interval;
            
            data.totalCallStatusSuccessful = data.totalP;
            data.totalCallStatusBlocked = 0;
            data.totalCallStatusOther = 0;

            //table to generate total success/blocked and other calls
            var div = self.reportsView.find('#key-usage-lineChart').append('<div class="totalErrorCalls"></div>');
            $('.totalErrorCalls').html(_.template(totalCallView, {data: data}));


            var graph = d3.select("#key-usage-lineChart svg");

            if(self.successChart === null){

                nv.addGraph(function() {
                    self.successChart = chart = nv.models.lineChart();

                    self.redrawGraph(graph, lineData, self.successChart, interval, autoScale, overflow);
                    self.successErrorGraphElements(graph, lineData, interval);

                    nv.utils.windowResize(chart.update);

                    return chart;
                });
            } else {
                self.redrawGraph(graph, lineData, self.successChart, interval, autoScale, overflow);
                self.successErrorGraphElements(graph, lineData, interval);

                nv.utils.windowResize(chart.update);
            }
        },        

        errorCodesGraphElements: function(graph, interval){
            var self = this;
            //add legend for tickers.
            var intervalControls = graph.select('.nv-wrap g').append('g').attr('class', 'nv-intervalWrap').attr('id','nv-intervalWrapId');
            var controls = nv.models.legend().height(30).color(['#444', '#444', '#444']);
            intervalControls.datum(self.controlsData[interval]).attr('transform', 'translate(628,-80)').call(controls);
            $('.nv-intervalWrap').find('g').find('.nv-series').find('.nv-legend-symbol,.nv-hover-icon').remove();
            $('#key-usage-lineChart-errorCodes').find('g').find('.nv-series').find('.nv-hover-icon').remove();

        },

         _processRequestData: function(data) {
            var self = this;
            var successData = [];
            var blockData = [];
            var otherData = [];
            var xAxisTickValues = [];
            
            if(data.usageData.length === 1) {  //checking if the daterange equal to 1 day. 
                var val = data.usageData[0];
                var startDate = new Date(val.startDate);
                var endDate = new Date(val.endDate);

                successData.push({x: startDate, y: val.callStatusSuccessful});
                successData.push({x: endDate, y: val.callStatusSuccessful});
                blockData.push({x: startDate, y: val.callStatusBlocked});
                blockData.push({x: endDate, y: val.callStatusBlocked});
                otherData.push({x: startDate, y: val.callStatusOther});
                otherData.push({x: endDate, y: val.callStatusOther});
                xAxisTickValues.push(startDate);
                xAxisTickValues.push(endDate);
               
             }else {
                successData = $.map(data.usageData, function(val, index){
                    var dateObj = new Date(val.startDate);

                    xAxisTickValues.push(dateObj);

                    return {x: dateObj, y: val.callStatusSuccessful};
                });
                blockData = $.map(data.usageData, function(val, index){
                    var dateObj = new Date(val.startDate);

                    //xAxisTickValues.push(dateObj);
                   
                    return {x: dateObj, y: val.callStatusBlocked};
                });
                otherData = $.map(data.usageData, function(val, index){
                    var dateObj = new Date(val.startDate);

                    //xAxisTickValues.push(dateObj);
                    
                    return {x: dateObj, y: val.callStatusOther};
                });
            }    
            return [
             {
               values: successData,
               interval: data.interval,
               xAxisTickValues: xAxisTickValues,
               image_path:  "/portal/resources/images/info-icon.png",
               key: "Success",
               color: "#6cb43a" 
             },
             {
               values: blockData,
               interval: data.interval,
               xAxisTickValues: xAxisTickValues,
               image_path:  "/portal/resources/images/info-icon.png",
               key: "Blocked",
               color: "#d11600"
               
             },
             {
               values: otherData,
               interval: data.interval,
               xAxisTickValues: xAxisTickValues,
               image_path:  "/portal/resources/images/info-icon.png" ,
               key: "Others",
               color: "#e3d200" 
             }
           ];
        },
        
        _processDPData: function(data){
            var developmentData = [];
            var productionData = [];

            var xAxisTickValues = [];

            if(data.typedUsages.length === 1){  //this case we cannot use $.map
                var date = data.startDate;
                var format = d3.time.format.utc("%Y-%m-%d");
                date = format.parse(date);
                xAxisTickValues.push(date);
                developmentData.push({x : date, y: data.totalD});
                date = data.endDate;
                date = format.parse(date);
                xAxisTickValues.push(date);
                developmentData.push({x : date, y: data.totalD});
                productionData.push({x : date, y: data.totalP});
                
            }else{
                developmentData = $.map(data.typedUsages, function(val, index){
                    var date = val.startDate;
                    var format = d3.time.format.utc("%Y-%m-%d");
                    date = format.parse(date);
                    xAxisTickValues.push(date);
                    return {x : date, y: val.totalDeveloper};
                });

                productionData = $.map(data.typedUsages, function(val, index){

                    var date = val.startDate;
                    var format = d3.time.format.utc("%Y-%m-%d");
                    date = format.parse(date);
                    return { x: date, y: val.totalProduction};
                });
            }     

            return [
                {
                    values: developmentData,
                    interval: data.interval,
                    xAxisTickValues: xAxisTickValues,
                    key: "Queries",
                    color: "#6cb43a"
                }
            ];

        },        

        _errorResponseCodes: function(data){
            var self = this;
            var badReqData = [];
            var notAuthoData = [];
            var forbiddenData = [];
            var serviceUnavailableAPIData = [];
            var serviceUnavailableProxyData = [];
            var gatewayData = [];
            var notFoundData = [];
            var overLimitData = [];	  
            var qpsData = [];	
            var inactiveData = [];	
            var internalServerData = [];
            var xAxisTickValues = [];
            
            if(data.usageData.length === 1) { //checking if date range is equal to 1 day
                var val = data.usageData[0];
                var startDate = new Date(val.startDate);
                var endDate = new Date(val.endDate);
                var mid = new Date(val.endDate);

                badReqData.push({x: startDate, y: val.badRequestAPI400});
                badReqData.push({x: endDate, y: val.badRequestAPI400});

                notAuthoData.push({x: startDate, y: val.notAuthorizedProxy403});
                notAuthoData.push({x: endDate, y: val.notAuthorizedProxy403});

                forbiddenData.push({x:startDate, y: val.forbidden403});
                forbiddenData.push({x: endDate, y:val.forbidden403});

                notFoundData.push({x: startDate, y: val.notFoundAPI404});
                notFoundData.push({x: endDate, y: val.notFoundAPI404});

                internalServerData.push({x: startDate, y: val.internalServerErrorAPI500});
                internalServerData.push({x: endDate, y: val.internalServerErrorAPI500});

                serviceUnavailableProxyData.push({x: startDate, y: val.serviceUnavailableProxy503});
                serviceUnavailableProxyData.push({x: endDate, y: val.serviceUnavailableProxy503});

                serviceUnavailableAPIData.push({x: startDate, y: val.serviceUnavailableAPI503});
                serviceUnavailableAPIData.push({x: endDate, y: val.serviceUnavailableAPI503});

                gatewayData.push({x: startDate, y: val.gatewayTimeoutProxy504});
                gatewayData.push({x: endDate, y: val.gatewayTimeoutProxy504});
                
                overLimitData.push({x: startDate, y: val.developerOverLimitProxy403});
                overLimitData.push({x: endDate, y: val.developerOverLimitProxy403});
	       
                qpsData.push({x: startDate, y: val.developerOverQPSProxy403});
                qpsData.push({x: endDate, y: val.developerOverQPSProxy403});

                inactiveData.push({x: startDate, y: val.inactiveUnknownKeyProxy403});
                inactiveData.push({x: endDate, y: val.inactiveUnknownKeyProxy403});
                
                xAxisTickValues.push(startDate);
                xAxisTickValues.push(endDate);

            }else {
                badReqData = $.map(data.usageData, function(val, index){
                    var dateObj = new Date(val.startDate);

                    xAxisTickValues.push(dateObj);

                    return {x: dateObj, y: val.badRequestAPI400};
                });
                notAuthoData = $.map(data.usageData, function(val, index){
                    var dateObj = new Date(val.startDate);

                    return {x: dateObj, y: val.notAuthorizedProxy403};
                });
                forbiddenData = $.map(data.usageData, function(val, index){
                    var dateObj = new Date(val.startDate);

                    return {x: dateObj, y: val.forbidden403};
                });
                notFoundData = $.map(data.usageData, function(val, index){
                    var dateObj = new Date(val.startDate);

                    return {x: dateObj, y: val.notFoundAPI404};
                });
                internalServerData = $.map(data.usageData, function(val, index){
                    var dateObj = new Date(val.startDate);

                    return {x: dateObj, y: val.internalServerErrorAPI500};
                });
                serviceUnavailableProxyData = $.map(data.usageData, function(val, index){
                    var dateObj = new Date(val.startDate);

                    return {x: dateObj, y: val.serviceUnavailableProxy503};
                });
                serviceUnavailableAPIData = $.map(data.usageData, function(val, index){
                    var dateObj = new Date(val.startDate);

                    return {x: dateObj, y: val.serviceUnavailableAPI503};
                });
                gatewayData = $.map(data.usageData, function(val, index){
                    var dateObj = new Date(val.startDate);

                    return {x: dateObj, y: val.gatewayTimeoutProxy504};
                });
                overLimitData = $.map(data.usageData, function(val, index){
                    var dateObj = new Date(val.startDate);

                    return {x: dateObj, y: val.developerOverLimitProxy403};
                });
                qpsData = $.map(data.usageData, function(val, index){
                    var dateObj = new Date(val.startDate);

                    return {x: dateObj, y: val.developerOverQPSProxy403};
                });
                inactiveData = $.map(data.usageData, function(val, index){
                    var dateObj = new Date(val.startDate);

                    return {x: dateObj, y: val.inactiveUnknownKeyProxy403};
                });
            }

            return [
                {
                    values: badReqData,
                    interval: data.interval,
                    xAxisTickValues: xAxisTickValues,
                    key: "400 Bad Request",
                    color: "#CE7C7C"
                },
                {
                    values: notAuthoData,
                    interval: data.interval,
                    xAxisTickValues: xAxisTickValues,
                    key: "403 Not Authorized",
                    color: "#e2d200"

                },
                {
                    values: forbiddenData,
                    interval: data.interval,
                    xAxisTickValues: xAxisTickValues,
                    key: "403 Forbidden (API)",
                    color: "#8c830b"
                },
	        {
                   values: overLimitData,
                   interval: data.interval,
                   xAxisTickValues: xAxisTickValues,
                   key: "403 Account Over Rate Limit",
                   color: "#9BD6A7"

                },
	        {
                   values: inactiveData,
                   interval: data.interval,
                   xAxisTickValues: xAxisTickValues,
                   key: "403 Account Inactive",
                   color: "#3BC6F7"

                },
	        {
                   values: qpsData,
                   interval: data.interval,
                   xAxisTickValues: xAxisTickValues,
                   key: "403 Account Over Queries Per Second Limit",
                   color: "#FBD988"

                },
                {
                    values: notFoundData,
                    interval: data.interval,
                    xAxisTickValues: xAxisTickValues,
                    key: "404 Not Found",
                    color: "#9AD6F7"

                },
                {
                    values: internalServerData,
                    interval: data.interval,
                    xAxisTickValues: xAxisTickValues,
                    key: "500 Internal Server Error",
                    color: "#51222D"

                },
                {
                    values: serviceUnavailableProxyData,
                    interval: data.interval,
                    xAxisTickValues: xAxisTickValues,
                    key: "503 Service Unavailable (Proxy)",
                    color: "#b11f24"

                },
                {
                    values: serviceUnavailableAPIData,
                    interval: data.interval,
                    xAxisTickValues: xAxisTickValues,
                    key: "503 Service Unavailable (API)",
                    color: "#b28d1f"

                },
                {
                    values: gatewayData,
                    interval: data.interval,
                    xAxisTickValues: xAxisTickValues,
                    key: "504 Gateway Timeout",
                    color: "#F79AF5"

                }
            ];
        },

        _processAxisTicks: function(ticks){

            if(ticks.length > 60){
                var newTicks = $.map(ticks, function(val, i){
                    if(i % 2 != 0 && i != ticks.length-1){
                        return "";
                    }
                    else{
                        return val;
                    }
                });
                return newTicks;
            }
            else{
                return ticks;
            }
        },
        
        onAssetClicked: function(evt) {
            evt.preventDefault();
            // get the link from the cell which was clicked
            var $element = $(evt.currentTarget);

            var options = {
                header: $element.closest(".asset-table").data("productdisplayname")
            };

            //debug.debug("this.model.attributes", this.model.attributes);

            $.fancybox(_.template(detailViewTemplate, {data: $.extend(options, this.model.attributes)}), {
                    helpers : {
                        overlay : {
                            opacity: 0.6,
                            css : {
                                'background-color' : '#000000'
                            }
                        }
                    },
                    'modal': true,
                    'autoScale': false,
                    'autoDimensions': false,
                    'hideOnOverlayClick': false,
                    'scrolling': false,
                    'minWidth': 878,
                    //'minHeight': 255,
                    'padding': 0,
                    'margin': 0,
                    'wrapCSS': 'neuportal detail-asset-popover',

                    'beforeShow': function(opts) {
                        // hide the fancybox
                        $(".neuportal.fancybox-wrap").css("visibility", "hidden");
                        var items = $('.neuportal .detail-asset-popover .list-container-box ul').find('li');
                        var count = items.length;

                        var columns = 3;
                        if(count <= 19) {
                            columns = 2;
                        }

                        $('.columnize').columnize({
                            'columns': columns,
                            'lastNeverTallest': true
                        });
                    },
                    'afterShow': function(evt) {
                        $("#attr-list").niceScroll(".scroll-content");
                        $(".fancybox-skin").append('<div class="fancybox-item fancybox-close" title="Close"></div>');
                        $(".fancybox-skin .fancybox-close").on("click", function(evt) {
                            evt.preventDefault();
                            $.fancybox.close( true );
                        });
                    },
                    'onUpdate': function() {
                        // show the fancybox - do it in update because the dimensions are calculated
                        // before this entrypoint is called
                        $(".neuportal.fancybox-wrap").css("visibility", "visible");
                    }
                }
            );
        },

        onUsageReportClicked: function(evt) {
            debug.debug("reportsclicked.begin");
  
            evt.preventDefault();
            var self = this;
            IPI.Portal.Routers.AppRouter.navigate(evt.currentTarget.hash, {trigger: true});

        }

    });

});
