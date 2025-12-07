using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace SocialMap.Repository.Data;

/// <summary>
/// Database migration helper - EnsureCreated() yeni kolonları eklemediği için
/// manuel olarak kolonları ekler.
/// </summary>
public static class DatabaseMigrationHelper
{
    public static async Task EnsurePostLocationColumnsExistAsync(ApplicationDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            var sql = @"
                -- Latitude kolonu
                DO $$ 
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Posts' AND column_name = 'Latitude'
                    ) THEN
                        ALTER TABLE ""Posts"" ADD COLUMN ""Latitude"" double precision;
                    END IF;
                END $$;

                -- Longitude kolonu
                DO $$ 
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Posts' AND column_name = 'Longitude'
                    ) THEN
                        ALTER TABLE ""Posts"" ADD COLUMN ""Longitude"" double precision;
                    END IF;
                END $$;

                -- CommentsCount kolonu
                DO $$ 
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Posts' AND column_name = 'CommentsCount'
                    ) THEN
                        ALTER TABLE ""Posts"" ADD COLUMN ""CommentsCount"" integer NOT NULL DEFAULT 0;
                    END IF;
                END $$;

                -- Index'ler
                CREATE INDEX IF NOT EXISTS ""IX_Posts_Geohash"" ON ""Posts"" (""Geohash"");
                CREATE INDEX IF NOT EXISTS ""IX_Posts_Latitude_Longitude"" ON ""Posts"" (""Latitude"", ""Longitude"");

                -- Mevcut CommentsCount'u güncelle
                UPDATE ""Posts"" 
                SET ""CommentsCount"" = (
                    SELECT COUNT(*) 
                    FROM ""Comments"" 
                    WHERE ""Comments"".""PostId"" = ""Posts"".""Id""
                )
                WHERE ""CommentsCount"" = 0 OR ""CommentsCount"" IS NULL;

                -- Mevcut postların koordinatlarını Place'den kopyala
                UPDATE ""Posts"" 
                SET 
                    ""Latitude"" = ""Places"".""Latitude"",
                    ""Longitude"" = ""Places"".""Longitude""
                FROM ""Places""
                WHERE ""Posts"".""PlaceId"" = ""Places"".""Id""
                  AND ""Places"".""Latitude"" IS NOT NULL 
                  AND ""Places"".""Longitude"" IS NOT NULL
                  AND (""Posts"".""Latitude"" IS NULL OR ""Posts"".""Longitude"" IS NULL);
            ";

            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            // PostgreSQL bağlantı hatası veya başka bir hata
            Console.WriteLine($"[MigrationHelper] Kolon ekleme hatası (normal olabilir): {ex.Message}");
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}

