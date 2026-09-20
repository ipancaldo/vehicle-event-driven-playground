using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AutoAuctionPlayground.Infrastructure.Extensions
{
    public static class ModelBuilderExtensions
    {
        // Postgres convention. Runs after the entity configurations, and only renames what they
        // did not name explicitly, so owned-type columns (vin, price_amount, ...) keep their names.
        public static void UseSnakeCaseNaming(this ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                if (!entity.IsOwned() && entity.FindAnnotation(RelationalAnnotationNames.TableName) is null)
                    entity.SetTableName((entity.GetTableName() ?? entity.ClrType.Name).ToSnakeCase());

                foreach (var property in entity.GetProperties())
                {
                    // An owned type's key is a shadow FK that must share the owner's PK column;
                    // renaming it breaks table splitting.
                    if (entity.IsOwned() && property.IsKey())
                        continue;

                    if (property.FindAnnotation(RelationalAnnotationNames.ColumnName) is null)
                        property.SetColumnName(property.Name.ToSnakeCase());
                }

                foreach (var key in entity.GetKeys())
                    key.SetName(key.GetName()?.ToSnakeCase());

                foreach (var foreignKey in entity.GetForeignKeys())
                    foreignKey.SetConstraintName(foreignKey.GetConstraintName()?.ToSnakeCase());

                foreach (var index in entity.GetIndexes())
                    index.SetDatabaseName(index.GetDatabaseName()?.ToSnakeCase());
            }
        }
    }
}
