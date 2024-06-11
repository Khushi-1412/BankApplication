import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.mongodb.core.MongoTemplate;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;

@RestController
@RequestMapping("/api/healthcheck")
public class ServiceHealthCheckController {

    @Autowired
    private MongoTemplate mongoTemplate; // Autowire MongoTemplate

    @GetMapping("/data")
    public ResponseEntity<List<ServiceHealthCheckDTO>> getHealthCheckData() {
        List<ServiceHealthCheckDTO> data = mongoTemplate.findAll(ServiceHealthCheckDTO.class, "apihealthcheck");
        return new ResponseEntity<>(data, HttpStatus.OK);
    }
}

