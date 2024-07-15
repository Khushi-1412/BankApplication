import org.apache.http.HttpHeaders;
import org.apache.http.HttpResponse;
import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.impl.client.HttpClients;
import org.apache.http.util.EntityUtils;
import org.springframework.stereotype.Service;

@Service
public class SplunkService {

    private static final String SPLUNK_API_URL = "https://your-splunk-api-url";
    private static final String SPLUNK_JWT_TOKEN = "your_jwt_token_here";

    public String fetchDataFromSplunk() throws Exception {
        HttpClient httpClient = HttpClients.createDefault();
        HttpGet request = new HttpGet(SPLUNK_API_URL);

        // Set JWT token in the Authorization header
        request.setHeader(HttpHeaders.AUTHORIZATION, "Bearer " + SPLUNK_JWT_TOKEN);

        HttpResponse response = httpClient.execute(request);
        int statusCode = response.getStatusLine().getStatusCode();

        if (statusCode == 200) {
            return EntityUtils.toString(response.getEntity());
        } else {
            throw new Exception("Failed to fetch data from Splunk: " + response.getStatusLine().getReasonPhrase());
        }
    }
}
