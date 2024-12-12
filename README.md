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
        
        // Re
        try (Inpu resultsStream = job.getResults()) {
            ResultsReaderJson resultsReader = new ResultsReaderJson(resultsStream);
            Map<String, String> event;
            while ((event = resultsReader.getNextEvent()) != null) {
                for (Map.Entry<String, String> entry : event.entrySet()) {
                    System.out.println(entry.getKey() + ": " + entry.getValue());
                }
            }
        } catch (Exception e) {
            e.printStackTrace();



------------------

I Say What I Need to Say
While working on the “Add Customers List” feature, there were many open questions regarding the database and the current customer list. I ensured effective communication by raising these concerns and seeking clarification. I proactively spoke up to understand the requirements and resolve any uncertainties,



I Own It
While working on the feature for color transformation across the application, I took full ownership of the task. I ensured that all color changes were consistent and aligned with the project requirements. I proactively identified areas where the color scheme needed to be updated and worked efficiently to complete the transformation across all applications. 



I Think Like a Customer
I prioritized the customer experience by identifying and addressing inconsistencies. For instance, I noticed discrepancies in the application’s color codes during the migration and worked with the product team to standardize them, ensuring a cohesive and user-friendly interface.






In 2024, transitioning to a new team was initially challenging but allowed me to expand my technical skills by working with Java and SpringBoot. I gained valuable experience with JavaScript, contributing to both frontend and backend development. I also familiarized myself with the customer and admin portal codebase and had the opportunity to explore the various types of customers our application supports. I contributed to key features such as the Gpod Healthcheck Dashboard, the ForgeRock-to-Ping migration, and the “Add Customers List” feature.

Additionally, I completed a LinkedIn Learning course in the Full Stack Developer path, which has laid a strong foundation for my growth.

Looking ahead to 2025, I am excited to deepen my knowledge in frontend technologies, particularly React, and further explore cloud technologies, especially AWS. I plan to achieve these goals by enrolling in relevant courses, obtaining certifications, and contributing more to team projects as new opportunities arise.

---
I worked on the ForgeRock to Ping migration, ensuring I met the PI commitments and completed tasks as quickly as possible. I successfully resolved all the bugs assigned to me within the given deadlines for the migration project. Additionally, I provided support to the team whenever needed, contributing to the overall success of the project.
