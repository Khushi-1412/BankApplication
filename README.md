package biz.neustar.ipi.platform.ipiadmin.services.apiproxy;

import biz.neustar.ipi.platform.ipiadmin.services.apiproxy.dto.ApiHealthcheck;
import com.splunk.HttpService;
import com.splunk.JobResultsArgs;
import com.splunk.ResultsReaderJson;
import com.splunk.SSLSecurityProtocol;
import com.splunk.Service;
import com.splunk.ServiceArgs;
import org.apache.log4j.Logger;
import org.joda.time.DateTime;
import org.joda.time.DateTimeZone;
import org.joda.time.format.DateTimeFormatter;
import org.joda.time.format.ISODateTimeFormat;

import java.io.InputStream;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.concurrent.Callable;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;

public class SplunkClient {

    private static final Logger logger = Logger.getLogger(SplunkClient.class);

    // Hardcoded Splunk configuration details
    private static final String BASE_URL = "your-splunk-url"; // replace with your Splunk URL
    private static final int API_PORT = 8089; // replace with your Splunk API port
    private static final String API_USER = "your-username"; // replace with your Splunk username
    private static final String API_PASSWORD = "your-password"; // replace with your Splunk password
    private static final String SEARCH_HOST = "your-search-host"; // replace with your search host
    private static final String SEARCH_INDEX = "exm_apim_prod"; // replace with your search index
    private static final String SEARCH_LOG = "response_mainpart size=\"*\""; // replace with your search log
    private static final int SPLUNK_THREADS_COUNT = 10; // Adjust as necessary
    private static final int SPLUNK_TIME_INTERVAL = 30; // Interval in minutes

    private static final String SEARCH_STRING = "search index=%s host=%s %s | stats count(eval(response_http_status)) AS Hits BY response_http_status";

    public List<ApiHealthcheck> getApiHealthcheckData(DateTime startDate, DateTime endDate) throws Exception {
        return executeSplunkQueries(startDate, endDate);
    }

    private List<ApiHealthcheck> executeSplunkQueries(DateTime startDate, DateTime endDate) throws Exception {
        DateTimeFormatter fmt = ISODateTimeFormat.dateTime();
        List<ApiHealthcheck> apiHealthcheckList = new ArrayList<>();

        // Set SSL security protocol
        HttpService.setSslSecurityProtocol(SSLSecurityProtocol.TLSv1_2);

        // Create a map of arguments and add login parameters
        ServiceArgs loginArgs = new ServiceArgs();
        loginArgs.setUsername(API_USER);
        loginArgs.setPassword(API_PASSWORD);
        loginArgs.setHost(BASE_URL);
        loginArgs.setPort(API_PORT);

        // Create a thread pool
        ExecutorService executor = Executors.newFixedThreadPool(SPLUNK_THREADS_COUNT);
        List<Future<List<ApiHealthcheck>>> futureList = new ArrayList<>();

        while (startDate.isBefore(endDate)) {
            JobResultsArgs oneshotSearchArgs = new JobResultsArgs();
            oneshotSearchArgs.put("earliest_time", fmt.print(startDate.toDateTime(DateTimeZone.UTC)));
            oneshotSearchArgs.put("latest_time", fmt.print(startDate.plusMinutes(SPLUNK_TIME_INTERVAL).toDateTime(DateTimeZone.UTC)));
            oneshotSearchArgs.setOutputMode(JobResultsArgs.OutputMode.JSON);
            oneshotSearchArgs.setCount(0);

            String oneshotSearchQuery = String.format(SEARCH_STRING, SEARCH_INDEX, SEARCH_HOST, SEARCH_LOG);

            Callable<List<ApiHealthcheck>> aCallable = (new Callable<List<ApiHealthcheck>>() {
                private ServiceArgs loginArgs;
                private JobResultsArgs oneshotSearchArgs;
                private Service service;
                private String oneshotSearchQuery;
                private Logger logger;

                @Override
                public List<ApiHealthcheck> call() throws Exception {
                    List<ApiHealthcheck> apiHealthcheckList = new ArrayList<>();
                    this.service = Service.connect(this.loginArgs);

                    logger.info(String.format("Splunk search: %s %s %s", this.oneshotSearchArgs.get("earliest_time"), this.oneshotSearchArgs.get("latest_time"), this.oneshotSearchQuery));

                    InputStream resultsOneshot = service.oneshotSearch(oneshotSearchQuery, oneshotSearchArgs);
                    ResultsReaderJson resultsReader = new ResultsReaderJson(resultsOneshot);

                    HashMap<String, String> event;
                    while ((event = resultsReader.getNextEvent()) != null) {
                        String statusCode = event.get("response_http_status");
                        int hits = Integer.parseInt(event.get("Hits"));

                        if (statusCode != null) {
                            apiHealthcheckList.add(new ApiHealthcheck(statusCode, hits));
                        }
                    }

                    resultsReader.close();
                    resultsOneshot.close();
                    service.logout();

                    logger.info("Total events from Splunk: " + apiHealthcheckList.size());

                    return apiHealthcheckList;
                }

                private Callable<List<ApiHealthcheck>> initialize(ServiceArgs loginArgs, JobResultsArgs oneshotSearchArgs,
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
                for (Future<List<ApiHealthcheck>> result : futureList) {
                    apiHealthcheckList.addAll(result.get());
                }
            } catch (Exception e) {
                logger.error("Error calling Splunk API: " + e);
                throw e;
            } finally {
                executor.shutdown();
            }
        }

        return apiHealthcheckList;
    }
}

