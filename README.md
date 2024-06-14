splunk.host=splunk.example.com
splunk.port=8089
splunk.username=your-username
splunk.password=your-password

------------------

package com.example.splunkboot.config;

import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.context.annotation.Configuration;

@Configuration
@ConfigurationProperties(prefix = "splunk")
public class SplunkConfigProperties {

    private String host;
    private int port;
    private String username;
    private String password;

    // getters and setters

    public String getHost() {
        return host;
    }

    public void setHost(String host) {
        this.host = host;
    }

    public int getPort() {
        return port;
    }

    public void setPort(int port) {
        this.port = port;
    }

    public String getUsername() {
        return username;
    }

    public void setUsername(String username) {
        this.username = username;
    }

    public String getPassword() {
        return password;
    }

    public void setPassword(String password) {
        this.password = password;
    }
}
-----------------


package com.example.splunkboot.service;

import com.splunk.*;
import com.example.splunkboot.config.SplunkConfigProperties;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import javax.annotation.PostConstruct;
import java.io.InputStream;
import java.util.ArrayList;
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
        ServiceArgs loginArgs = new ServiceArgs();
        loginArgs.setUsername(splunkConfigProperties.getUsername());
        loginArgs.setPassword(splunkConfigProperties.getPassword());
        loginArgs.setHost(splunkConfigProperties.getHost());
        loginArgs.setPort(splunkConfigProperties.getPort());

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
}

--------------------
package com.example.splunkboot.controller;

import com.example.splunkboot.service.SplunkService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;
import java.util.Map;

@RestController
@RequestMapping("/splunk")
public class SplunkController {

    private final SplunkService splunkService;

    @Autowired
    public SplunkController(SplunkService splunkService) {
        this.splunkService = splunkService;
    }

    @GetMapping("/search")
    public List<Map<String, String>> search() {
        String query = "index=exm_apim_prod response_mainpart size=\"*\" | stats count(eval(response_http_status)) AS Hits BY response_http_status";
        return splunkService.search(query);
    }
}
