## Process new Storm Events Database

In CosmosDb, find the corresponding record in stormEventsDatabaseInventory. Set the IsReadyToProcess flag to True.

In Azure Portal, fa-olievortex-red, disable the TimerDailyStormDownload and TimerSatellitePosters jobs.

In Visual Studio, run the OlievortexRed.CLI "eventsdatabase" command

Should the CLI throw an error, look at the error message for the failure date. In CosmosDb, delete all records from that
date from the stormEventsDailyDetails container.

In Visual Studio, run the TimerDailyStormDownload job to inventory AWS Satellite.

In Azure Portal, fa-olievortex-red, manually run the TimerSatellitePosters job.

For efficiency, start the container instance olievortex-purple-2 in rg-container.

Monitor the progress of the sb-olievortex service bus "satellite_aws_posters" queue. One drained, the container
instances will shut down after 5 minutes.

In Azure Portal, manual run the TimerSatellitePosters job. This will create the thumbnails.