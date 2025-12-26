using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using OlievortexRed.Lib.Entities;

namespace OlievortexRed.Lib.Services;

[ExcludeFromCodeCoverage]
public class CosmosContext(DbContextOptions<CosmosContext> options) : DbContext(options)
{
    #region RadarInventory

    public DbSet<RadarInventoryEntity> RadarInventory { get; init; }

    private static Action<ModelBuilder> AddRadarInventory => m =>
    {
        m.Entity<RadarInventoryEntity>()
            .HasPartitionKey(k => new
            {
                k.EffectiveDate,
                k.BucketName
            })
            .ToContainer("radarInventories");
    };

    #endregion

    #region RadarSite

    public DbSet<RadarSiteEntity> RadarSite { get; init; }

    private static Action<ModelBuilder> AddRadarSite => m =>
    {
        m.Entity<RadarSiteEntity>()
            .HasPartitionKey(k => k.Id)
            .ToContainer("radarSites");
    };

    #endregion

    #region SatelliteAwsInventory

    public DbSet<SatelliteAwsInventoryEntity> SatelliteAwsInventory { get; init; }

    private static Action<ModelBuilder> AddSatelliteAwsInventory => m => m
        .Entity<SatelliteAwsInventoryEntity>()
        .HasPartitionKey(k => new
        {
            k.EffectiveDate,
            k.Channel,
            k.DayPart
        })
        .ToContainer("satelliteAwsInventories");

    #endregion

    #region SatelliteAwsProduct

    public DbSet<SatelliteAwsProductEntity> SatelliteAwsProduct { get; init; }

    private static Action<ModelBuilder> AddSatelliteAwsProduct => m => m
        .Entity<SatelliteAwsProductEntity>()
        .HasPartitionKey(k => k.EffectiveDate)
        .ToContainer("satelliteAwsProducts");

    #endregion

    #region SpcMesoProduct

    public DbSet<SpcMesoProductEntity> SpcMesoProduct { get; init; }

    private static Action<ModelBuilder> AddSpcMesoProduct => m => m
        .Entity<SpcMesoProductEntity>()
        .HasPartitionKey(k => k.EffectiveDate)
        .ToContainer("spcMesoProducts");

    #endregion

    #region StormEventsDailyDetail

    public DbSet<StormEventsDailyDetailEntity> StormEventsDailyDetail { get; init; }

    private static Action<ModelBuilder> AddStormEventsDailyDetail => m => m
        .Entity<StormEventsDailyDetailEntity>()
        .HasPartitionKey(k => new
        {
            k.DateFk,
            k.SourceFk
        })
        .ToContainer("stormEventsDailyDetails");

    #endregion

    #region StormEventsDailySummary

    public DbSet<StormEventsDailySummaryEntity> StormEventsDailySummary { get; init; }

    private static Action<ModelBuilder> AddStormEventsDailySummary => m => m
        .Entity<StormEventsDailySummaryEntity>()
        .HasPartitionKey(k => new
        {
            k.Year,
            k.SourceFk
        })
        .ToContainer("stormEventsDailySummaries");

    #endregion

    #region StormEventsDatabaseInventory

    public DbSet<StormEventsDatabaseInventoryEntity> StormEventsDatabaseInventory { get; init; }

    private static Action<ModelBuilder> AddStormEventsDatabaseInventory => m => m
        .Entity<StormEventsDatabaseInventoryEntity>()
        .HasPartitionKey(k => k.Year)
        .ToContainer("stormEventsDatabaseInventories");

    #endregion

    #region StormEventsSpcInventory

    public DbSet<StormEventsSpcInventoryEntity> StormEventsSpcInventory { get; init; }

    private static Action<ModelBuilder> AddStormEventsSpcInventory => m => m
        .Entity<StormEventsSpcInventoryEntity>()
        .HasPartitionKey(k => k.EffectiveDate)
        .ToContainer("stormEventsSpcInventories");

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        AddRadarInventory(modelBuilder);
        AddRadarSite(modelBuilder);
        AddSatelliteAwsInventory(modelBuilder);
        AddSatelliteAwsProduct(modelBuilder);
        AddSpcMesoProduct(modelBuilder);
        AddStormEventsDailyDetail(modelBuilder);
        AddStormEventsDailySummary(modelBuilder);
        AddStormEventsDatabaseInventory(modelBuilder);
        AddStormEventsSpcInventory(modelBuilder);
    }
}