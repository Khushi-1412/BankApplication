package com.example.splunkboot.controller;

import biz.neustar.ipi.platform.ipiadmin.services.apiproxy.SplunkClient;
import biz.neustar.ipi.platform.ipiadmin.services.apiproxy.dto.ApiHealthcheck;
import org.joda.time.DateTime;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;

@RestController
public class SplunkController {

    private final SplunkClient splunkClient;

    @Autowired
    public SplunkController(SplunkClient splunkClient) {
        this.splunkClient = splunkClient;
    }

    @GetMapping("/api/healthcheck")
    public List<ApiHealthcheck> getApiHealthcheck() {
        DateTime endDate = DateTime.now();
        DateTime startDate = endDate.minusMinutes(30);

        try {
            return splunkClient.getApiHealthcheckData(startDate, endDate);
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }
}
