import org.apache.http.HttpHeaders;
import org.apache.http.HttpHost;
import org.apache.http.HttpResponse;
import org.apache.http.client.HttpClient;
import org.apache.http.client.config.RequestConfig;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.impl.client.HttpClients;
import org.apache.http.util.EntityUtils;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

@Service
public class SplunkService {

    @Value("${splunk.api.url}")
    private String splunkApiUrl;

    @Value("${splunk.jwt.token}")
    private String splunkJwtToken;

    public String fetchDataFromSplunk() throws Exception {
        HttpHost proxy = new HttpHost("proxy.example.com", 8080, "http"); // Replace with your proxy details if needed
        RequestConfig config = RequestConfig.custom()
                .setConnectTimeout(5000)     // 5 seconds connect timeout
                .setSocketTimeout(5000)      // 5 seconds socket timeout
                .setProxy(proxy)             // Optional proxy configuration
                .build();

        HttpClient httpClient = HttpClients.custom()
                .setDefaultRequestConfig(config)
                .setMaxConnPerRoute(10)      // Maximum number of concurrent connections per route
                .setMaxConnTotal(20)         // Maximum number of concurrent connections in total
                .build();

        try {
            HttpGet request = new HttpGet(splunkApiUrl);
            request.setHeader(HttpHeaders.AUTHORIZATION, "Bearer " + splunkJwtToken);

            HttpResponse response = httpClient.execute(request);
            int statusCode = response.getStatusLine().getStatusCode();

            if (statusCode == 200) {
                return EntityUtils.toString(response.getEntity());
            } else {
                throw new Exception("Failed to fetch data from Splunk: " + response.getStatusLine().getReasonPhrase());
            }
        } finally {
            // Ensure resources are properly released
            httpClient.close();
        }
    }
