# JBHiFi-Challenge

![image](https://user-images.githubusercontent.com/36960366/179854769-bf9075c0-5ba7-46e6-be03-4ddf5aa085fb.png)

This Weather Describer app works as follows:
- Enter a city and its country code to receive a brief description of the weather
- Only 5 requests per hour

How to build/run:
- For API service, open the <b>WeatherData</b> folder and run <code>dotnet build</code> in your terminal.
- Once built, navigate to <b>WeatherData\bin\Debug</b> to find the exe to run.
- Alternatively open the WeatherData.sln in your IDE to run the app.
- For the UI, open the <b>weatherdata.ui</b> folder and run <code>npm start</code> in your terminal.

Technical and testing notes:
- Rate limited by string API key. 5 keys total with 1 being used by UI client.
- Rate limit counts are stored in an in-memory cache.
- Rate limit count resets after 60 mins without making a request.
- Requests made 60 mins within each other add to the count.
- To reset rate count, simply restart the API app.
- Appsettings.json configs which include rate limit config and specifies the valid API keys.


The following <b>unit tests</b> have also been created for the backend weather service:

![image](https://user-images.githubusercontent.com/36960366/179856724-3f0b4690-5dbe-47ce-a665-f3a3ce596362.png)


Improvements I would make on the app as next steps:
- Global exception handling.
- Logging information and errors.
- Implement options pattern for configuration.
