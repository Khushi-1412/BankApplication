# Server port
server.port=8080

# Logging configuration
logging.level.root=INFO
logging.level.org.springframework=INFO
logging.level.com.example.splunkboot=DEBUG
logging.file=splunkboot.log

# Splunk configuration
splunk.base-url=http://your-splunk-url
splunk.api-port=8089
splunk.api-user=admin
splunk.api-password=admin
splunk.search-host=your-search-host
splunk.search-index=your-search-index
splunk.search-log=your-search-log

# Splunk thread count
splunk.search.thread=5
