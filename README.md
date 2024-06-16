package biz.neustar.ipi.platform.ipiadmin.services.apiproxy;

import biz.neustar.ipi.platform.ipiadmin.services.ipi.dto.SplunkUsage;
import com.splunk.HttpService;
import com.splunk.JobResultsArgs;
import com.splunk.ResultsReaderJson;
import com.splunk.SSLSecurityProtocol;
import com.splunk.Service;
import com.splunk.ServiceArgs;
import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.concurrent.Callable;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import org.apache.log4j.Logger;
import org.joda.time.DateTime;
import org.joda.time.DateTimeZone;
import org.joda.time.format.DateTimeFormatter;
import org.joda.time.format.ISODateTimeFormat;
import org.springframework.beans.factory.annotation.Value;

/**
 * Splunk client to call Splunk's api's. Does not do any data processing.
 *
 *
 */
public class SplunkClient implements ISplunkClient {

    private static final Logger logger = Logger.getLogger(SplunkClient.class);

    private final String baseUrl;
    private final int apiPort;
    private final String apiUser;
    private final String apiPassword;
    private final String searchHost;
    private final String searchIndex;

    @Value("${splunk.search.log}")
    private String searchLog;

    @Value("${splunk.search.thread}")
    private int splunkThreadsCount;

    private static final String SEARCH_STRING = "search index=%s host=%s %s (ipscore OR realtime OR ipinfo OR rvgp) ";
    private static final int SPLUNK_TIME_INTERVAL = 3;

    public SplunkClient(String baseUrl, int apiPort, String apiUser, String apiPassword,
        String searchHost, String searchIndex) {
        this.baseUrl = baseUrl;
        this.apiPort = apiPort;
        this.apiUser = apiUser;
        this.apiPassword = apiPassword;
        this.searchHost = searchHost;
        this.searchIndex = searchIndex;
    }

    /**
     * Get the usage for a service for a date range. Start date is inclusive,
     * while end date is NOT inclusive.
     *
     * @param startDate Start date is inclusive. Use ISO8601 format
     * @param endDate End date is NOT inclusive. Use ISO8601 format
     * @return
     * @throws IOException
     */
    @Override
    public List<SplunkUsage> callsDeveloperActivityForService(DateTime startDate, DateTime endDate)
        throws Exception {
        return getCallsDeveloperActivityForServiceUri(startDate, endDate);
    }

    private List<SplunkUsage> getCallsDeveloperActivityForServiceUri(DateTime startDate, DateTime endDate) throws Exception {

        DateTimeFormatter fmt = ISODateTimeFormat.dateTime();

        List<SplunkUsage> splunkUsageList = new ArrayList<>();

        HttpService.setSslSecurityProtocol(SSLSecurityProtocol.TLSv1_2);

        // Create a map of arguments and add login parameters
        ServiceArgs loginArgs = new ServiceArgs();
        loginArgs.setUsername(this.apiUser);
        loginArgs.setPassword(this.apiPassword);
        loginArgs.setHost(this.baseUrl);
        loginArgs.setPort(this.apiPort);

        ExecutorService executor = Executors.newFixedThreadPool(splunkThreadsCount);

        List<Future<List<SplunkUsage>>> futureList = new ArrayList<>();

        while (startDate.isBefore(endDate)) {

            // Set the parameters for the search:
            JobResultsArgs oneshotSearchArgs = new JobResultsArgs();

            // calling splunk with 3 minutes interval within an hour with 20 api calls (00 to 03, ... , 57 to 60)
            // splunk configuration has 500,000 max events per api call
            // call will fail if within a 3 minutes interval with over 500,000 events
            // default to 3 mins interval call now, update properties file (e.g. 2 minutes) if over 500,000 events within 3 minutes interval
            oneshotSearchArgs.put("earliest_time", fmt.print(startDate.toDateTime(DateTimeZone.UTC)));
            oneshotSearchArgs.put("latest_time", fmt.print(startDate.plusMinutes(SPLUNK_TIME_INTERVAL).toDateTime(DateTimeZone.UTC)));
            oneshotSearchArgs.setOutputMode(JobResultsArgs.OutputMode.JSON);
            String[] fieldList = new String[]{"request_http_parameter_apikey", "response_http_status", "response_mainpart_size"};
            oneshotSearchArgs.setFieldList(fieldList);
            oneshotSearchArgs.setCount(0); // returns all results

            String oneshotSearchQuery = String.format(SEARCH_STRING, searchIndex, searchHost, searchLog);

            // Create a Callable object of anonymous class for use in threading
            Callable<List<SplunkUsage>> aCallable = (new Callable<List<SplunkUsage>>() {
                private ServiceArgs loginArgs;
                private JobResultsArgs oneshotSearchArgs;
                private Service service;
                private String oneshotSearchQuery;
                private Logger logger;

                @Override
                public List<SplunkUsage> call() throws Exception {

                    int total = 0;
                    List<SplunkUsage> suList = new ArrayList<SplunkUsage>();
                    this.service = Service.connect(this.loginArgs);

                    logger.info(String.format("Splunk search : %s %s %s", this.oneshotSearchArgs.get("earliest_time"), this.oneshotSearchArgs.get("latest_time"), this.oneshotSearchQuery));

                    InputStream results_oneshot = service.oneshotSearch(oneshotSearchQuery, oneshotSearchArgs);

                    ResultsReaderJson resultsReader = new ResultsReaderJson(results_oneshot);

                    HashMap<String, String> event;

                    while ((event = resultsReader.getNextEvent()) != null) {
                        total++;
                        
                        if (event.get("request_http_parameter_apikey") != null && event.get("request_http_parameter_apikey").contains(".") && event.get("response_http_status") != null && event.get("response_mainpart_size") != null) {
                            suList.add(new SplunkUsage(event.get("request_http_parameter_apikey"), event.get("response_http_status"), event.get("response_mainpart_size")));
                        }
                    }

                    resultsReader.close();
                    results_oneshot.close();

                    service.logout();

                    logger.info("Total events from Splunk: " + total);

                    return suList;
                }

                private Callable<List<SplunkUsage>> initialize(ServiceArgs loginArgs, JobResultsArgs oneshotSearchArgs,
                    String oneshotSearchQuery, Logger logger) {
                    this.loginArgs = loginArgs;
                    this.oneshotSearchArgs = oneshotSearchArgs;
                    this.oneshotSearchQuery = oneshotSearchQuery;
                    this.logger = logger;

                    return this;
                }
            }).initialize(loginArgs, oneshotSearchArgs, oneshotSearchQuery, logger);

            futureList.add(executor.submit(aCallable));

            startDate = startDate.plusMinutes(SPLUNK_TIME_INTERVAL);
        }

        if (!futureList.isEmpty()) {
            try {
                for (Future<List<SplunkUsage>> result : futureList) {
                    splunkUsageList.addAll(result.get());
                }

            } catch (Exception e) {
                logger.error("Error calling Splunk API: " + e);
                //clean up
                futureList = null;

                throw e;
            } finally {
                //shutdown executor after all threads are done
                executor.shutdown();
            }
        }

        return splunkUsageList;

    }

}
