import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.mongodb.core.MongoTemplate;
import org.springframework.data.mongodb.core.aggregation.Aggregation;
import org.springframework.data.mongodb.core.aggregation.AggregationResults;
import org.springframework.data.mongodb.core.query.Criteria;
import org.springframework.stereotype.Service;

import java.util.Date;
import java.util.List;

@Service
public class CallService {
    @Autowired
    private MongoTemplate mongoTemplate;

    public List<CallStats> getCallStats(String licenseKey, Date startDate, Date endDate) {
        Aggregation aggregation = Aggregation.newAggregation(
            Aggregation.match(
                Criteria.where("licenseKey").is(licenseKey)
                    .and("startDate").gte(startDate).lte(endDate)
            ),Aggregation.project()
                .and(DateOperators.dateOf("startDate").toString("%Y-%m-%d")).as("formattedDate")
                .andInclude("callStatusSuccessful")
                .andExpression("callStatusBlocked + callStatusOther").as("unsuccessfulCalls"),
            Aggregation.group(
                Aggregation.dateAsFormattedString("startDate", "%Y-%m-%d")
            )
            .sum("callStatusSuccessful").as("successfulCalls")
            .sum(Aggregation.add("callStatusBlocked", "callStatusOther")).as("unsuccessfulCalls"),
            Aggregation.sort(Aggregation.sort().descending("_id"))
        );

        AggregationResults<CallStats> results = mongoTemplate.aggregate(aggregation, "your_collection_name", CallStats.class);
        return results.getMappedResults();
    }
}
