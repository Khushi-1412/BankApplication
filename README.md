public class SplunkLogin {

    static Service service = null;

    /**
     * Authentication Token.
     * Actual token length would be longer than this token length.
     */
    static String token = "1k_Ostpl6NBe4iVQ5d6I3Ohla_U5";
    
    public static void main(String args[]) {
        // Initialize the SDK client
        connectToSplunk();
        
        // Define your search query
        String query = "search index=your_index_name sourcetype=your_sourcetype | stats count by field_name";

        // Run the search query
        runSearchQuery(query);
    }

    private static void connectToSplunk() {
        ServiceArgs loginArgs = new ServiceArgs();
        loginArgs.setPort(8089);
        loginArgs.setHost("localhost");
        loginArgs.setScheme("https");
        loginArgs.setToken(String.format("Bearer %s", token));

        service = Service.connect(loginArgs);
    }

    private static void runSearchQuery(String query) {
        JobArgs jobArgs = new JobArgs();
        jobArgs.setExecutionMode(JobArgs.ExecutionMode.BLOCKING);
        
        // Create a job to run the query
        Job job = service.getJobs().create(query, jobArgs);
        
        // Retrieve results
        try (InputStream resultsStream = job.getResults()) {
            ResultsReaderJson resultsReader = new ResultsReaderJson(resultsStream);
            Map<String, String> event;
            while ((event = resultsReader.getNextEvent()) != null) {
                for (Map.Entry<String, String> entry : event.entrySet()) {
                    System.out.println(entry.getKey() + ": " + entry.getValue());
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
