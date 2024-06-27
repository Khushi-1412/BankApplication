 Map<String, Object> dateToString = new HashMap<>();
        dateToString.put("format", "%Y-%m-%d");
        dateToString.put("date", "$startDate");

        Map<String, Object> addFieldsMap = new HashMap<>();
        addFieldsMap.put("date", new Document("$dateToString", dateToString));

