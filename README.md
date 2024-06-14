package com.example.splunkboot.service;

import com.splunk.*;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import javax.annotation.PostConstruct;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

@Service
public class SplunkService {

    private final String splunkUrl;
    private final String username;
    private final String password;
    private Service splunkService;

    public SplunkService(
            @Value("${splunk.url}") String splunkUrl,
            @Value("${splunk.username}") String username,
            @Value("${splunk.password}") String password) {
        this.splunkUrl = splunkUrl;
        this.username = username;
        this.password = password;
    }

    @PostConstruct
    public void init() {
        ServiceArgs loginArgs = new ServiceArgs();
        loginArgs.setHost(getHostFromUrl(splunkUrl));
        loginArgs.setPort(getPortFromUrl(splunkUrl));
        loginArgs.setUsername(username);
        loginArgs.setPassword(password);

        splunkService = Service.connect(loginArgs);
    }

    public List<Map<String, String>> search(String query) {
        List<Map<String, String>> results = new ArrayList<>();
        Job job = splunkService.getJobs().create(query);

        // Wait for the job to complete
        while (!job.isDone()) {
            try {
                Thread.sleep(500);
            } catch (InterruptedException e) {
                Thread.currentThread().interrupt();
                break;
            }
        }

        JobResultsArgs resultsArgs = new JobResultsArgs();
        resultsArgs.setOutputMode(JobResultsArgs.OutputMode.JSON);

        try (InputStream resultsStream = job.getResults(resultsArgs);
             ResultsReaderJson resultsReader = new ResultsReaderJson(resultsStream)) {
            for (Map<String, Object> result : resultsReader) {
                results.add((Map<String, String>) (Map) result);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }

        return results;
    }

    private String getHostFromUrl(String url) {
        return url.split("://")[1].split(":")[0];
    }

    private int getPortFromUrl(String url) {
        String[] parts = url.split("://")[1].split(":");
        return parts.length > 1 ? Integer.parseInt(parts[1].split("/")[0]) : 8089; // Default port
    }
}
