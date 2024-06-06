To modify the provided Java code to return results in the format required by the `apihealthcheck` DTO, you'll need to adjust the aggregation pipeline and the result mapping. Here's the modified code:

```java
import com.mongodb.*;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.mongodb.core.MongoOperations;
import org.springframework.stereotype.Service;
import java.util.Date;
import java.util.ArrayList;
import java.util.List;

@Service
public class ApiUsageEntryService {

    @Autowired
    private MongoOperations mongoOps;

    private static final Logger logger = LoggerFactory.getLogger(ApiUsageEntryService.class);

    public List<ApiHealthCheck> getAggregateUsage(String licenseKey, Date startDate, Date endDate) {
        // Create our pipeline operations, first with the match
        DBObject matchObj = new BasicDBObject("licenseKey", licenseKey);
        DBObject dateRangeObj = new BasicDBObject("$gte", startDate);
        dateRangeObj.put("$lte", endDate);
        matchObj.put("startDate", dateRangeObj);

        DBObject matchOp = new BasicDBObject("$match", matchObj);

        // Group by date, sum the call counts
        DBObject groupObj = new BasicDBObject("_id", "$startDate");
        groupObj.put("successfulCalls", new BasicDBObject("$sum", "$callStatusSuccessful"));
        groupObj.put("unsuccessfulCalls", new BasicDBObject("$sum", new BasicDBObject("$add", new Object[]{"$callStatusBlocked", "$callStatusOther"})));

        DBObject groupOp = new BasicDBObject("$group", groupObj);

        // Sort by _id descending
        DBObject sortOp = new BasicDBObject("$sort", new BasicDBObject("_id", -1));

        // Run the aggregation
        AggregationOutput output = mongoOps.getCollection("apiUsageEntry").aggregate(matchOp, groupOp, sortOp);
        logger.trace("MongoDB aggregation result: {}", output.getCommandResult());

        // Convert the result to ApiHealthCheck objects
        List<ApiHealthCheck> healthChecks = new ArrayList<>();
        for (DBObject result : output.results()) {
            ApiHealthCheck healthCheck = new ApiHealthCheck();
            healthCheck.setDate((Date) result.get("_id"));
            healthCheck.setSuccessfulCalls((int) result.get("successfulCalls"));
            healthCheck.setUnsuccessfulCalls((int) result.get("unsuccessfulCalls"));
            healthChecks.add(healthCheck);
        }

        return healthChecks;
    }
}
```

In this modified code:

1. The `getAggregateUsage` method now returns a list of `ApiHealthCheck` objects instead of a single `ApiUsageEntry` object.
2. The aggregation pipeline groups documents by the `startDate` field and calculates the sum of successful and unsuccessful calls.
3. Each result from the aggregation is mapped to an `ApiHealthCheck` object, which represents the required fields: date, successful calls, and unsuccessful calls.
4. The method returns a list of `ApiHealthCheck` objects containing the aggregated data.
