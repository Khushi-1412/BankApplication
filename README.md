Service service = null;
        try {
            // Set up login parameters
            ServiceArgs loginArgs = new ServiceArgs();
            loginArgs.setHost(HOST);
            loginArgs.setPort(PORT);
            loginArgs.setScheme(SCHEME);
            loginArgs.setToken(TOKEN);

            // Initialize the SDK client
            service = Service.connect(loginArgs);

            // Perform a search query
            Job job = service.getJobs().create("search index=_internal | head 1");

            // Wait for the search to finish
            job.waitForCompletion();

            // Print out the results
            ResultsReaderXml resultsReader = new ResultsReaderXml(job.getResults());
            ResultsReader.Xml resultMap;
            while ((resultMap = resultsReader.getNextEvent()) != null) {
                for (String key : resultMap.keySet()) {
                    System.out.println(key + ": " + resultMap.get(key));
                }
            }

        } catch (Exception e) {
            e.printStackTrace();
        } finally {
            // Always close the service when done
            if (service != null) {
                service.logout();
            }
        }
    
