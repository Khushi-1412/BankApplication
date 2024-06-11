import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.mongodb.core.MongoTemplate;
import org.springframework.data.mongodb.core.query.Criteria;
import org.springframework.data.mongodb.core.query.Query;
import org.springframework.stereotype.Service;

import java.time.LocalDate;
import java.util.List;

@Service
public class ServiceHealthCheckService {

    @Autowired
    private MongoTemplate mongoTemplate;

    public List<ServiceHealthCheckDTO> getHealthCheckDataForService(String service) {
        LocalDate endDate = LocalDate.now();
        LocalDate startDate = endDate.minusDays(29); // 30 days ago

        Query query = new Query(Criteria.where("service").is(service)
                .and("date").gte(startDate.toString())
                .lte(endDate.toString()));
        return mongoTemplate.find(query, ServiceHealthCheckDTO.class, "apihealthcheck");
    }
}
