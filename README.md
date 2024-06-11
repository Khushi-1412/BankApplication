public class ServiceHealthCheckDTO {
    private String service;
    private String date;
    private Metrics metrics;

    // Getters and setters

    public String getService() {
        return service;
    }

    public void setService(String service) {
        this.service = service;
    }

    public String getDate() {
        return date;
    }

    public void setDate(String date) {
        this.date = date;
    }

    public Metrics getMetrics() {
        return metrics;
    }

    public void setMetrics(Metrics metrics) {
        this.metrics = metrics;
    }

    public static class Metrics {
        private int successfulcalls;
        private int unsuccessfulcalls;

        // Getters and setters

        public int getSuccessfulcalls() {
            return successfulcalls;
        }

        public void setSuccessfulcalls(int successfulcalls) {
            this.successfulcalls = successfulcalls;
        }

        public int getUnsuccessfulcalls() {
            return unsuccessfulcalls;
        }

        public void setUnsuccessfulcalls(int unsuccessfulcalls) {
            this.unsuccessfulcalls = unsuccessfulcalls;
        }
    }
}
