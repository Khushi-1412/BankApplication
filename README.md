function generateTimeSeriesData(dataApi) {
                    var today = new Date();
                    var timeSeriesData = [];
                    for (var i = 0; i < 30; i++) {
                        var date = new Date(today);
                        date.setDate(today.getDate() - i);
                        var options = { month: 'long', day: 'numeric', year: 'numeric' };
                        var dateString = date.toLocaleDateString('en-US', options);
                        var dateKey = date.toISOString().split('T')[0];
                        var apiData = dataApi.find(item => item.date === dateKey) || { metrics: { successfulcalls: 0, unsuccessfulcalls: 0 } };
                        timeSeriesData.push({
                            date: dateString, 
                            successfulcalls: apiData.metrics.successfulcalls, 
                            unsuccessfulcalls: apiData.metrics.unsuccessfulcalls 
                        });
                    }
                    return timeSeriesData.reverse();
                }
