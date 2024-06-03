To retrieve the aggregated data in reverse order, from the current date to the last month, you can add an additional `$sort` stage to the aggregation pipeline. Here's the modified pipeline:

```json
[
  {
    "$match": {
      "serviceId": /^220/,
      "startDate": {
        "$gte": new Date(new Date().setMonth(new Date().getMonth() - 1)),
        "$lte": new Date()
      }
    }
  },
  {
    "$group": {
      "_id": {
        "year": { "$year": "$startDate" },
        "month": { "$month": "$startDate" },
        "day": { "$dayOfMonth": "$startDate" }
      },
      "totalSuccessfulCalls": { "$sum": "$callStatusSuccessful" },
      "totalBlockedCalls": { "$sum": "$callStatusBlocked" }
    }
  },
  {
    "$sort": {
      "_id.year": -1,
      "_id.month": -1,
      "_id.day": -1
    }
  }
]
```

In this pipeline, the additional `$sort` stage sorts the aggregated data in descending order based on the year, month, and day fields, effectively arranging the results from the current date to the last month
