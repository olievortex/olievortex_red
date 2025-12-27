# olievortex_red
This is a POC exploring Azure Functions and Azure Cosmos DB. We are supporting the back-end of a Severe Weather History website, where a user can navigate to a specific severe weather day, view storm reports, look at satellite images, read the Storm Prediction Center outlooks, watches and mesoscale discussions. Our backend must acquire the necessary data, process it, and make it consumable via the web.

# Architecture
All resources to be consumed by website users are stored in a public Azure Blob container. There is an Azure Container Instance that hosts a linux instance that runs the python code to generate the maps. The Cosmos DB instance is used to index all the data and is utilized by the web frontend. The Azure Service Bus is used to communicate satellite images that need to be processed by the Python process.

# Azure Function
There is one Azure Function application responsible for scheduling all the data collection and processing.

## TimerDailyStormDownload
This function is on a timer trigger, once per day. It will reach out to the Storm Prediction Center and find both new and updated storm reports. The text is loaded into Cosmos DB and the related images are stored in Blob Storage.

Next, we index (but down't download) the available satellite imagery and radar for the past day from an AWS bucket. The radar indexing is limited to only those radars that had a severe weather event nearby.

### Database initial load
When seeding the database, there is a secondary method of downloading storm events from the Storm Events database. These are spreadsheets that contain all weather related reports for an entire year. This is a much more efficient process than importing years of individual daily reports.

## TimerSatellitePosters
This is a timer function that triggers once per day after the TimerDailyStormDownload. If there was a severe weather event in the prior day, the severe weather reports are ranked in order of significance. The timestamp from the most significant event is seleted. We then download the raw satellite data (NetCDF format) from the AWS bucket into Azure Blob Storage. Satellite metadata is then enqueued into the Azure Service Bus to facilitate image processing that must be done in Python. After all the metadata is enqueued, an Azure Container Instance gets kicks off. The container is a Linux image that executes a Python script and then shuts down.

The Python function dequeues metadata from the Azure Service Bus and uses libraries to process the NetCDF source, and generate a graphic image using matplotlib. The image is then uploaded to the Azure Blob Storage. Finally, the HttpSatelliteRequest HTTP Trigger Function is executed to update the database image inventory.

The Python code that runs on the Azure Container Instance is located in the olievortex_purple repository.

## HttpSatelliteRequest
This is an HTTP trigger function that is called from the Python code in the Azure Container Instance. It is called after the native satellite file is converted to an image and stored in Blob Storage. This updates the file index in CosmosDB so the imagery can appear on the website.

# Thoughts on Azure Functions
I utilized Linux instances for better pricing. I found deployment is flaky, sometimes requiring me to delete the instance in Azure, recreate it, and redeploy. Deploys would sometimes hang. Never felt very solid to me. Nothing frustrates me greater than a black box that just decides to stop working.

# Thougts on Cosmos DB
I enjoyed using Cosmos DB and may use it again in the future. It works very well as a document repository for the SPC Outlook, Watches, and MesoscaleDiscussion content.

I used Cosmos DB for everything, including the tracking and indexing of content to be consumed by the website. In the future, I will do this in MySQL or Azure SQL Server because it is much more appropriate for a relational database. I felt very limited in my querying ability with Cosmos DB. Queries that are simple in SQL aren't so simple in Cosmos DB.

The different ways that data is queried requires different containers (tables) specially indexed to the query. This causes substantial duplication of data, and is a pain to keep everything in synch when updating.

# Thoughts on the project
I vastly underestimated the quantity and size of the data to obtain the years of history I desired. We're talking close to a terrabyte of blob storage. This already factors in the fact I am only collecting data on those days with a significant severe weather event. There are other sources for history like the Iowa Mesonet, although they appear to be hiding or obsuring their source files, forcing the user to go through their user interface, which I fund cumbersom.

The other issuse is there is probably only 100 people in the entire United States that care enough about severe weather to utuilize the service I envisioned, which would be geared towards storm chasers, not data scientists. I do not see enough need in the market to move forward with this project.

