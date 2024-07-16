import org.springframework.http.*;
import org.springframework.web.client.RestTemplate;

public class SplunkService {

    private final String splunkUrl = "https://your-splunk-url:port";
    private final String accessToken = "your-access-token";

    public String querySplunk(String searchQuery) {
        HttpHeaders headers = new HttpHeaders();
        headers.setBearerAuth(accessToken);
        headers.setContentType(MediaType.APPLICATION_JSON);

        HttpEntity<String> entity = new HttpEntity<>(searchQuery, headers);

        RestTemplate restTemplate = new RestTemplate();
        ResponseEntity<String> response = restTemplate.exchange(
                splunkUrl + "/services/search/jobs/export",
                HttpMethod.POST,
                entity,
                String.class
        );

        return response.getBody();
    }

    public static void main(String[] args) {
        SplunkService splunkService = new SplunkService();
        
        // Example query: Retrieve the first 10 results from index=main
        String searchQuery = "search index=main | head 10";
        String result = splunkService.querySplunk(searchQuery);
        
        System.out.println("Response from Splunk:");
        System.out.println(result);
    }
}
