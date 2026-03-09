using System.Data;
using Microsoft.EntityFrameworkCore;
using UgnayDesktop.Models;

namespace UgnayDesktop.Data;

public static class DbInitializer
{
    private const string ProductVersion = "10.0.3";
    private const string InitialCreateMigrationId = "20260226010809_InitialCreate";
    private const string BootstrapSchemaMigrationId = "20260309151203_BootstrapSchema";
    private const string AddMustChangePasswordMigrationId = "20260309151337_AddMustChangePassword";

    public static void Seed()
    {
        using var db = new AppDbContext();

        EnsureLegacyMigrationHistory(db);
        db.Database.Migrate();
        SeedUsers(db);
    }

    private static void EnsureLegacyMigrationHistory(AppDbContext db)
    {
        if (!TableExists(db, "Users"))
        {
            return;
        }

        CreateMigrationHistoryTable(db);

        if (!MigrationHistoryContains(db, InitialCreateMigrationId))
        {
            InsertMigrationHistoryRow(db, InitialCreateMigrationId);
        }

        if (!MigrationHistoryContains(db, BootstrapSchemaMigrationId)
            && HasAnyBootstrapSchemaArtifacts(db))
        {
            EnsureLegacyBootstrapSchema(db);
            InsertMigrationHistoryRow(db, BootstrapSchemaMigrationId);
        }

        if (!MigrationHistoryContains(db, AddMustChangePasswordMigrationId)
            && ColumnExists(db, "Users", "MustChangePassword"))
        {
            InsertMigrationHistoryRow(db, AddMustChangePasswordMigrationId);
        }
    }

    private static bool HasAnyBootstrapSchemaArtifacts(AppDbContext db)
    {
        return TableExists(db, "SensorReadings")
            || TableExists(db, "AlertOutboxMessages")
            || ColumnExists(db, "Users", "Email")
            || ColumnExists(db, "Users", "TeacherPhoneNumber")
            || ColumnExists(db, "Users", "DeviceId")
            || ColumnExists(db, "Users", "Age")
            || ColumnExists(db, "Users", "Sex")
            || ColumnExists(db, "Users", "ThemePreference")
            || ColumnExists(db, "SensorReadings", "HandGesture")
            || ColumnExists(db, "SensorReadings", "HandGestureConfidence")
            || ColumnExists(db, "SensorReadings", "HandTracked");
    }

    private static void EnsureLegacyBootstrapSchema(AppDbContext db)
    {
        EnsureUsersBootstrapColumns(db);
        EnsureSensorReadingsSchema(db);
        EnsureAlertOutboxSchema(db);
    }

    private static void EnsureUsersBootstrapColumns(AppDbContext db)
    {
        TryExecuteSql(db, "ALTER TABLE Users ADD COLUMN Email TEXT NULL;");
        TryExecuteSql(db, "ALTER TABLE Users ADD COLUMN TeacherPhoneNumber TEXT NULL;");
        TryExecuteSql(db, "ALTER TABLE Users ADD COLUMN DeviceId TEXT NULL;");
        TryExecuteSql(db, "ALTER TABLE Users ADD COLUMN Age INTEGER NULL;");
        TryExecuteSql(db, "ALTER TABLE Users ADD COLUMN Sex TEXT NULL;");
        TryExecuteSql(db, "ALTER TABLE Users ADD COLUMN ThemePreference TEXT NOT NULL DEFAULT 'Light';");

        TryExecuteSql(db, @"
            UPDATE Users
            SET ThemePreference = 'Light'
            WHERE ThemePreference IS NULL OR TRIM(ThemePreference) = '';");

        TryExecuteSql(db, "CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_Username ON Users (Username);");
    }

    private static void EnsureSensorReadingsSchema(AppDbContext db)
    {
        TryExecuteSql(db, @"
            CREATE TABLE IF NOT EXISTS SensorReadings (
                Id INTEGER NOT NULL CONSTRAINT PK_SensorReadings PRIMARY KEY AUTOINCREMENT,
                DeviceId TEXT NOT NULL,
                ReceivedAtUtc TEXT NOT NULL,
                HandGesture TEXT NULL,
                HandGestureConfidence REAL NULL,
                HandTracked INTEGER NULL,
                AccelX REAL NULL,
                AccelY REAL NULL,
                AccelZ REAL NULL,
                GyroX REAL NULL,
                GyroY REAL NULL,
                GyroZ REAL NULL,
                HeartRate REAL NULL,
                Spo2 REAL NULL,
                GsrValue REAL NULL,
                BodyTemperatureC REAL NULL,
                RawJson TEXT NOT NULL
            );");

        TryExecuteSql(db, "ALTER TABLE SensorReadings ADD COLUMN HandGesture TEXT NULL;");
        TryExecuteSql(db, "ALTER TABLE SensorReadings ADD COLUMN HandGestureConfidence REAL NULL;");
        TryExecuteSql(db, "ALTER TABLE SensorReadings ADD COLUMN HandTracked INTEGER NULL;");

        TryExecuteSql(db, @"
            DELETE FROM SensorReadings
            WHERE Id NOT IN (
                SELECT MIN(Id)
                FROM SensorReadings
                GROUP BY DeviceId, ReceivedAtUtc, RawJson
            );");

        TryExecuteSql(db, @"
            CREATE UNIQUE INDEX IF NOT EXISTS IX_SensorReadings_DeviceId_ReceivedAtUtc_RawJson
            ON SensorReadings (DeviceId, ReceivedAtUtc, RawJson);");

        TryExecuteSql(db, @"
            CREATE INDEX IF NOT EXISTS IX_SensorReadings_ReceivedAtUtc
            ON SensorReadings (ReceivedAtUtc);");
    }

    private static void EnsureAlertOutboxSchema(AppDbContext db)
    {
        TryExecuteSql(db, @"
            CREATE TABLE IF NOT EXISTS AlertOutboxMessages (
                Id INTEGER NOT NULL CONSTRAINT PK_AlertOutboxMessages PRIMARY KEY AUTOINCREMENT,
                CreatedAtUtc TEXT NOT NULL,
                UpdatedAtUtc TEXT NOT NULL,
                Source TEXT NOT NULL,
                DeviceId TEXT NOT NULL,
                Severity TEXT NOT NULL,
                Message TEXT NOT NULL,
                DedupKey TEXT NOT NULL,
                Status TEXT NOT NULL,
                AttemptCount INTEGER NOT NULL DEFAULT 0,
                LastAttemptAtUtc TEXT NULL,
                NextAttemptAtUtc TEXT NULL,
                SentAtUtc TEXT NULL,
                LastError TEXT NULL,
                RecipientPhoneNumber TEXT NULL,
                ProviderMessageId TEXT NULL,
                PayloadJson TEXT NOT NULL DEFAULT '{}'
            );");

        TryExecuteSql(db, "ALTER TABLE AlertOutboxMessages ADD COLUMN RecipientPhoneNumber TEXT NULL;");
        TryExecuteSql(db, "ALTER TABLE AlertOutboxMessages ADD COLUMN ProviderMessageId TEXT NULL;");

        TryExecuteSql(db, @"
            CREATE UNIQUE INDEX IF NOT EXISTS IX_AlertOutboxMessages_DedupKey
            ON AlertOutboxMessages (DedupKey);");

        TryExecuteSql(db, @"
            CREATE INDEX IF NOT EXISTS IX_AlertOutboxMessages_Status_NextAttemptAtUtc
            ON AlertOutboxMessages (Status, NextAttemptAtUtc);");
    }

    private static void CreateMigrationHistoryTable(AppDbContext db)
    {
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS __EFMigrationsHistory (
                MigrationId TEXT NOT NULL CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY,
                ProductVersion TEXT NOT NULL
            );");
    }

    private static void InsertMigrationHistoryRow(AppDbContext db, string migrationId)
    {
        db.Database.ExecuteSqlRaw(
            "INSERT OR IGNORE INTO __EFMigrationsHistory(MigrationId, ProductVersion) VALUES ({0}, {1});",
            migrationId,
            ProductVersion);
    }

    private static bool MigrationHistoryContains(AppDbContext db, string migrationId)
    {
        if (!TableExists(db, "__EFMigrationsHistory"))
        {
            return false;
        }

        return ExecuteScalarInt(
            db,
            "SELECT COUNT(1) FROM __EFMigrationsHistory WHERE MigrationId = $migrationId;",
            ("$migrationId", migrationId)) > 0;
    }

    private static void SeedUsers(AppDbContext db)
    {
        var changed = false;

        var admin = db.Users.FirstOrDefault(user => user.Username == "admin");
        if (admin == null)
        {
            db.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Email = "admin@local.test",
                Role = "Admin",
                FullName = "System Administrator",
                MustChangePassword = true,
            });
            changed = true;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(admin.Email))
            {
                admin.Email = "admin@local.test";
                changed = true;
            }

            if (!admin.MustChangePassword && ShouldForceAdminPasswordReset(admin))
            {
                admin.MustChangePassword = true;
                changed = true;
            }
        }

        var teacher = db.Users.FirstOrDefault(user => user.Username == "teacher");
        if (teacher == null)
        {
            db.Users.Add(new User
            {
                Username = "teacher",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("teacher"),
                Role = "Teacher",
                FullName = "Default Teacher",
                MustChangePassword = false,
            });
            changed = true;
        }

        if (changed)
        {
            db.SaveChanges();
        }
    }

    private static bool ShouldForceAdminPasswordReset(User admin)
    {
        if (!string.Equals(admin.Username, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(admin.PasswordHash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify("admin123", admin.PasswordHash);
        }
        catch
        {
            return false;
        }
    }

    private static bool TableExists(AppDbContext db, string tableName)
    {
        return ExecuteScalarInt(
            db,
            "SELECT COUNT(1) FROM sqlite_master WHERE type = 'table' AND lower(name) = lower($name);",
            ("$name", tableName)) > 0;
    }

    private static bool ColumnExists(AppDbContext db, string tableName, string columnName)
    {
        if (!TableExists(db, tableName))
        {
            return false;
        }

        var escapedTable = tableName.Replace("'", "''");
        var sql = $"SELECT COUNT(1) FROM pragma_table_info('{escapedTable}') WHERE lower(name) = lower($name);";
        return ExecuteScalarInt(db, sql, ("$name", columnName)) > 0;
    }

    private static void TryExecuteSql(AppDbContext db, string sql)
    {
        try
        {
            db.Database.ExecuteSqlRaw(sql);
        }
        catch
        {
            // Legacy-schema repair is best effort for upgraded databases.
        }
    }

    private static int ExecuteScalarInt(AppDbContext db, string sql, params (string Name, object Value)[] parameters)
    {
        var connection = db.Database.GetDbConnection();
        var openedHere = connection.State != ConnectionState.Open;

        if (openedHere)
        {
            connection.Open();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;

            foreach (var (name, value) in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = value;
                command.Parameters.Add(parameter);
            }

            var result = command.ExecuteScalar();

            if (result == null || result == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToInt32(result);
        }
        finally
        {
            if (openedHere)
            {
                connection.Close();
            }
        }
    }
}
