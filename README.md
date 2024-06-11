import json
from random import randint
from datetime import datetime, timedelta

# List of services
services = ["gpp", "gps", "gpu", "gds"]

# Generate random metrics for a day
def generate_metrics():
    return {
        "successfulcalls": randint(10, 50),
        "unsuccessfulcalls": randint(0, 40)
    }

# Generate data for 30 days for each service
data = []
for service in services:
    for i in range(30):
        date = (datetime.now() - timedelta(days=i)).strftime("%Y-%m-%d")
        metrics = generate_metrics()
        data.append({
            "service": service,
            "date": date,
            "metrics": metrics
        })

# Convert data to JSON and print
json_data = json.dumps(data, indent=4)
print(json_data)
