package com.example.splunkboot.service;

import com.splunk.*;
import com.example.splunkboot.config.SplunkConfigProperties;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import javax.annotation.PostConstruct;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

@Service
public class SplunkService {

    private final SplunkConfigProperties splunkConfigProperties;
    private Service splunkService;

    @Autowired
    public SplunkService(SplunkConfigProperties splunkConfigProperties) {
        this.splunkConfigProperties = splunkConfigProperties;
    }

    @PostConstruct
    public void init() {
        Map<String, Object> connectionArgs = new HashMap<>();
        connectionArgs.put("username", splunkConfigProperties.getUsername());
        connectionArgs.put("password", splunkConfigProperties.getPassword());
        connectionArgs.put("host", splunkConfigProperties.getHost());
        connectionArgs.put("port", splunkConfigProperties.getPort());

        splunkService = Service.connect(connectionArgs);
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
}
