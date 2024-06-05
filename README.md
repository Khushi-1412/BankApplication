import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import java.util.*;

@RestController
public class CallController {
    @Autowired
    private CallService callService;

    private static final String CONSTANT_LICENSE_KEY = "BC";

    @GetMapping("/api/call-stats")
    public Map<String, List<CallStats>> getCallStats(@RequestParam List<String> licenseKeys) {
        Date startDate = new Date();
        Calendar calendar = Calendar.getInstance();
        calendar.setTime(startDate);
        calendar.add(Calendar.DATE, 30);
        Date endDate = calendar.getTime();

        Map<String, List<CallStats>> results = new HashMap<>();

        // Add constant license key stats
        List<CallStats> constantKeyStats = callService.getCallStats(CONSTANT_LICENSE_KEY, startDate, endDate);
        results.put(CONSTANT_LICENSE_KEY, constantKeyStats);

        // Add dynamic license key stats
        for (String licenseKey : licenseKeys) {
            List<CallStats> stats = callService.getCallStats(licenseKey, startDate, endDate);
            results.put(licenseKey, stats);
        }
        return results;
    }
}
