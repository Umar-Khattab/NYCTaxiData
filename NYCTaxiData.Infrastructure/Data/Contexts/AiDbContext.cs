    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using NYCTaxiData.Domain.EntitiesAi;
    using NYCTaxiData.Infrastructure.Domain.EntitiesAi;

    namespace NYCTaxiData.Infrastructure.Data.Contexts;

    public partial class AiDbContext : DbContext
    {
        public AiDbContext()
        {
        }

        public AiDbContext(DbContextOptions<AiDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Demand15min> Demand15mins { get; set; }

        public virtual DbSet<Demandfeature> Demandfeatures { get; set; }

        public virtual DbSet<Etum> Eta { get; set; }

        public virtual DbSet<Revenuefeature> Revenuefeatures { get; set; }

        public virtual DbSet<Stockoutfeature> Stockoutfeatures { get; set; }

    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasPostgresEnum("auth", "aal_level", new[] { "aal1", "aal2", "aal3" })
                .HasPostgresEnum("auth", "code_challenge_method", new[] { "s256", "plain" })
                .HasPostgresEnum("auth", "factor_status", new[] { "unverified", "verified" })
                .HasPostgresEnum("auth", "factor_type", new[] { "totp", "webauthn", "phone" })
                .HasPostgresEnum("auth", "oauth_authorization_status", new[] { "pending", "approved", "denied", "expired" })
                .HasPostgresEnum("auth", "oauth_client_type", new[] { "public", "confidential" })
                .HasPostgresEnum("auth", "oauth_registration_type", new[] { "dynamic", "manual" })
                .HasPostgresEnum("auth", "oauth_response_type", new[] { "code" })
                .HasPostgresEnum("auth", "one_time_token_type", new[] { "confirmation_token", "reauthentication_token", "recovery_token", "email_change_token_new", "email_change_token_current", "phone_change_token" })
                .HasPostgresEnum("realtime", "action", new[] { "INSERT", "UPDATE", "DELETE", "TRUNCATE", "ERROR" })
                .HasPostgresEnum("realtime", "equality_op", new[] { "eq", "neq", "lt", "lte", "gt", "gte", "in" })
                .HasPostgresEnum("storage", "buckettype", new[] { "STANDARD", "ANALYTICS", "VECTOR" })
                .HasPostgresExtension("extensions", "pg_stat_statements")
                .HasPostgresExtension("extensions", "pgcrypto")
                .HasPostgresExtension("extensions", "uuid-ossp")
                .HasPostgresExtension("vault", "supabase_vault");

            modelBuilder.Entity<Demand15min>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("demand15min");

                entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week");
                entity.Property(e => e.Hour).HasColumnName("hour");
                entity.Property(e => e.IsRain).HasColumnName("is_rain");
                entity.Property(e => e.IsWeekend).HasColumnName("is_weekend");
                entity.Property(e => e.Lag1).HasColumnName("lag_1");
                entity.Property(e => e.Lag4).HasColumnName("lag_4");
                entity.Property(e => e.Lag96).HasColumnName("lag_96");
                entity.Property(e => e.Minute).HasColumnName("minute");
                entity.Property(e => e.Month).HasColumnName("month");
                entity.Property(e => e.PickupCnt).HasColumnName("pickup_cnt");
                entity.Property(e => e.PuLocationId).HasColumnName("pu_location_id");
                entity.Property(e => e.RainMm)
                    .HasPrecision(5, 2)
                    .HasColumnName("rain_mm");
                entity.Property(e => e.RollMean1h)
                    .HasPrecision(10, 4)
                    .HasColumnName("roll_mean_1h");
                entity.Property(e => e.RollMean3h)
                    .HasPrecision(10, 4)
                    .HasColumnName("roll_mean_3h");
                entity.Property(e => e.TempC)
                    .HasPrecision(5, 2)
                    .HasColumnName("temp_c");
                entity.Property(e => e.WeatherCode).HasColumnName("weather_code");
            });

            modelBuilder.Entity<Demandfeature>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("demandfeatures");

                entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week");
                entity.Property(e => e.IsHoliday).HasColumnName("is_holiday");
                entity.Property(e => e.IsRain).HasColumnName("is_rain");
                entity.Property(e => e.IsWeekend).HasColumnName("is_weekend");
                entity.Property(e => e.Lag16h).HasColumnName("lag_1_6h");
                entity.Property(e => e.Lag26h).HasColumnName("lag_2_6h");
                entity.Property(e => e.Lag46h).HasColumnName("lag_4_6h");
                entity.Property(e => e.PickupCount).HasColumnName("pickup_count");
                entity.Property(e => e.PickupHour).HasColumnName("pickup_hour");
                entity.Property(e => e.PuLocationId).HasColumnName("pu_location_id");
                entity.Property(e => e.RainMm)
                    .HasPrecision(5, 2)
                    .HasColumnName("rain_mm");
                entity.Property(e => e.RollingMean24h)
                    .HasPrecision(10, 4)
                    .HasColumnName("rolling_mean_24h");
                entity.Property(e => e.TempC)
                    .HasPrecision(5, 2)
                    .HasColumnName("temp_c");
                entity.Property(e => e.TimeBucket6h).HasColumnName("time_bucket_6h");
                entity.Property(e => e.WeatherCode).HasColumnName("weather_code");
            });

            modelBuilder.Entity<Etum>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("eta");

                entity.Property(e => e.DistMedianDuration).HasColumnName("dist_median_duration");
                entity.Property(e => e.DistanceBucketLabel)
                    .HasMaxLength(10)
                    .HasColumnName("distance_bucket_label");
                entity.Property(e => e.DistanceProxy)
                    .HasPrecision(10, 2)
                    .HasColumnName("distance_proxy");
                entity.Property(e => e.DoLocationId).HasColumnName("do_location_id");
                entity.Property(e => e.DurationSec)
                    .HasPrecision(12, 2)
                    .HasColumnName("duration_sec");
                entity.Property(e => e.IsRushHour).HasColumnName("is_rush_hour");
                entity.Property(e => e.IsWeekend).HasColumnName("is_weekend");
                entity.Property(e => e.OdHourMedianDuration)
                    .HasPrecision(12, 2)
                    .HasColumnName("od_hour_median_duration");
                entity.Property(e => e.Pickup15minBucket).HasColumnName("pickup_15min_bucket");
                entity.Property(e => e.PickupDow).HasColumnName("pickup_dow");
                entity.Property(e => e.PickupHour).HasColumnName("pickup_hour");
                entity.Property(e => e.PickupMinute).HasColumnName("pickup_minute");
                entity.Property(e => e.PickupMonth).HasColumnName("pickup_month");
                entity.Property(e => e.PuHourSlowdownIndex)
                    .HasPrecision(10, 4)
                    .HasColumnName("pu_hour_slowdown_index");
                entity.Property(e => e.PuLocationId).HasColumnName("pu_location_id");
                entity.Property(e => e.RainMm)
                    .HasPrecision(5, 2)
                    .HasColumnName("rain_mm");
                entity.Property(e => e.TempC)
                    .HasPrecision(5, 2)
                    .HasColumnName("temp_c");
                entity.Property(e => e.WeatherCode).HasColumnName("weather_code");
            });

            modelBuilder.Entity<Revenuefeature>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("revenuefeatures");

                entity.Property(e => e.AvgFare)
                    .HasPrecision(20, 2)
                    .HasColumnName("avg_fare");
                entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week");
                entity.Property(e => e.IsHoliday).HasColumnName("is_holiday");
                entity.Property(e => e.IsRain).HasColumnName("is_rain");
                entity.Property(e => e.IsWeekend).HasColumnName("is_weekend");
                entity.Property(e => e.Lag16h).HasColumnName("lag_1_6h");
                entity.Property(e => e.Lag26h).HasColumnName("lag_2_6h");
                entity.Property(e => e.Lag46h).HasColumnName("lag_4_6h");
                entity.Property(e => e.PickupHour).HasColumnName("pickup_hour");
                entity.Property(e => e.PuLocationId).HasColumnName("pu_location_id");
                entity.Property(e => e.RainMm)
                    .HasPrecision(20, 2)
                    .HasColumnName("rain_mm");
                entity.Property(e => e.RevLag16h)
                    .HasPrecision(20, 2)
                    .HasColumnName("rev_lag_1_6h");
                entity.Property(e => e.RevLag1Week)
                    .HasPrecision(20, 2)
                    .HasColumnName("rev_lag_1_week");
                entity.Property(e => e.RevRollingMean30d)
                    .HasPrecision(20, 2)
                    .HasColumnName("rev_rolling_mean_30d");
                entity.Property(e => e.RevRollingMean7d)
                    .HasPrecision(20, 2)
                    .HasColumnName("rev_rolling_mean_7d");
                entity.Property(e => e.RollingMean24h)
                    .HasPrecision(20, 2)
                    .HasColumnName("rolling_mean_24h");
                entity.Property(e => e.TempC)
                    .HasPrecision(20, 2)
                    .HasColumnName("temp_c");
                entity.Property(e => e.TipRate)
                    .HasPrecision(20, 2)
                    .HasColumnName("tip_rate");
                entity.Property(e => e.WeatherCode).HasColumnName("weather_code");
            });

            modelBuilder.Entity<Stockoutfeature>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToTable("stockoutfeatures");

                entity.Property(e => e.ActivityRatio)
                    .HasPrecision(20, 2)
                    .HasColumnName("activity_ratio");
                entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week");
                entity.Property(e => e.DropoffCount).HasColumnName("dropoff_count");
                entity.Property(e => e.Hour).HasColumnName("hour");
                entity.Property(e => e.IsHoliday).HasColumnName("is_holiday");
                entity.Property(e => e.IsRain).HasColumnName("is_rain");
                entity.Property(e => e.IsWeekend).HasColumnName("is_weekend");
                entity.Property(e => e.Lag1Dropoff).HasColumnName("lag_1_dropoff");
                entity.Property(e => e.Lag1NetFlow).HasColumnName("lag_1_net_flow");
                entity.Property(e => e.Lag1Pickup).HasColumnName("lag_1_pickup");
                entity.Property(e => e.NetFlow).HasColumnName("net_flow");
                entity.Property(e => e.PickupCount).HasColumnName("pickup_count");
                entity.Property(e => e.RainMm)
                    .HasPrecision(20, 2)
                    .HasColumnName("rain_mm");
                entity.Property(e => e.TempC)
                    .HasPrecision(20, 2)
                    .HasColumnName("temp_c");
                entity.Property(e => e.TimeBucket6h).HasColumnName("time_bucket_6h");
                entity.Property(e => e.WeatherCode).HasColumnName("weather_code");
                entity.Property(e => e.ZoneId).HasColumnName("zone_id");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
