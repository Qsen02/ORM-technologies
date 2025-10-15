using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace MiniORM
{
    public abstract class DbContext
    {
        private readonly DatabaseConnection connection;

        private readonly Dictionary<Type, PropertyInfo> dbSetProperties;

        internal static readonly Type[] AllowedSqlTypes = new Type[]
        {
            typeof(string), typeof(int), typeof(uint), typeof(long), typeof(ulong),
            typeof(decimal), typeof(bool), typeof(DateTime)
        };

        protected DbContext(string connectionString) {
            this.connection = new DatabaseConnection(connectionString);
            this.dbSetProperties = DiscoverDbSets();
            InitializeDbSets();
        }

        public void SaveChanges() {
            using (var manager = new ConnectionManager(this.connection))
            {
                using (var transaction = this.connection.StartTransaction())
                {
                    foreach (var dbSetProperty in this.dbSetProperties.Values)
                    {
                        var entityType = dbSetProperty.PropertyType.GetGenericArguments().First();
                        var persistMethod = typeof(DbContext)
                            .GetMethod("Persist", BindingFlags.NonPublic | BindingFlags.Instance)
                            .MakeGenericMethod(entityType);

                        persistMethod.Invoke(this, new object[] { dbSetProperty.GetValue(this) });
                    }

                    transaction.Commit();
                }
            }
        }

        private void Persist<TEntity>(DbSet<TEntity> dbSet) where TEntity : class, new()
        {
            var tableName = GetTableName(typeof(TEntity));
            var columns = this.connection.FetchColumnNames(tableName).ToArray();

            // INSERT
            var addedEntities = dbSet.ChangeTracker.Added;
            if (addedEntities.Any())
            {
                this.connection.InsertEntities(addedEntities, tableName, columns);
            }

            // UPDATE
            var modifiedEntities = dbSet.ChangeTracker.GetModifiedEntities(dbSet);
            if (modifiedEntities.Any())
            {
                this.connection.UpdateEntities(modifiedEntities, tableName, columns);
            }

            // DELETE
            var removedEntities = dbSet.ChangeTracker.Removed;
            if (removedEntities.Any())
            {
                this.connection.DeleteEntities(removedEntities, tableName, columns);
            }
        }

        private void InitializeDbSets() {
            foreach (var dbSetProperty in this.dbSetProperties)
            {
                var entityType = dbSetProperty.Key;
                var populateMethod = typeof(DbContext)
                    .GetMethod(nameof(PopulateDbSet), BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(entityType);

                populateMethod.Invoke(this, new object[] { dbSetProperty.Value });
            }
        }

        private void PopulateDbSet<TEntity>(PropertyInfo dbSet) where TEntity : class, new()
        {
            var entities = LoadTableEntities<TEntity>().ToList();
            var dbSetInstance = new DbSet<TEntity>(entities);
            dbSet.SetValue(this, dbSetInstance);
        }

        private void MapAllRelations() {
            foreach (var dbSetProp in this.dbSetProperties.Values)
            {
                var entityType = dbSetProp.PropertyType.GetGenericArguments().First();
                var mapMethod = typeof(DbContext)
                    .GetMethod(nameof(MapRelations), BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(entityType);

                var dbSetInstance = dbSetProp.GetValue(this);
                mapMethod.Invoke(this, new[] { dbSetInstance });
            }
        }

        private void MapRelations<TEntity>(DbSet<TEntity> dbSet) where TEntity : class, new()
        {
            MapNavigationProperties(dbSet);

            var collectionProperties = typeof(TEntity)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p =>
                    p.PropertyType.IsGenericType &&
                    (p.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                     p.PropertyType.GetInterfaces().Any(i =>
                         i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>))))
                .ToArray();

            foreach (var collectionProp in collectionProperties)
            {
                var childType = collectionProp.PropertyType.GetGenericArguments().First();
                var mapCollectionMethod = typeof(DbContext)
                    .GetMethod(nameof(MapCollection), BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(typeof(TEntity), childType);

                mapCollectionMethod.Invoke(this, new object[] { dbSet, collectionProp });
            }
        }

        private void MapCollection<TDbSet, TCollection>(DbSet<TDbSet> dbSet, PropertyInfo collectionProperty)
            where TDbSet : class, new()
            where TCollection : class, new()
        {
            if (!this.dbSetProperties.TryGetValue(typeof(TCollection), out var childDbSetProp))
                return;

            var childDbSet = (DbSet<TCollection>)childDbSetProp.GetValue(this);
            var parentType = typeof(TDbSet);
            var childType = typeof(TCollection);

            var parentKeyProp = parentType.GetProperties().FirstOrDefault(p => p.HasAttribute<KeyAttribute>());
            if (parentKeyProp == null) return;

            var candidateFkNames = new[]
            {
                parentType.Name + "Id",
                parentKeyProp.Name,
                parentType.Name + "_" + parentKeyProp.Name,
                parentType.Name + "ID"
            };

            foreach (var parentEntity in dbSet.Entities)
            {
                var parentKeyValue = parentKeyProp.GetValue(parentEntity);

                var fkProp = childType.GetProperties()
                    .FirstOrDefault(p => candidateFkNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase)
                                         && AllowedSqlTypes.Contains(p.PropertyType));

                if (fkProp == null)
                {
                    fkProp = childType.GetProperties()
                        .FirstOrDefault(p => p.PropertyType == parentType);
                }

                IEnumerable<TCollection> children;
                if (fkProp != null && AllowedSqlTypes.Contains(fkProp.PropertyType))
                {
                    children = childDbSet.Entities.Where(child => {
                        var fkValue = fkProp.GetValue(child);
                        return Equals(fkValue, parentKeyValue);
                    }).ToList();
                }
                else
                {
                    var navProp = childType.GetProperties().FirstOrDefault(p => p.PropertyType == parentType);
                    if (navProp != null)
                    {
                        children = childDbSet.Entities.Where(child => {
                            var navValue = navProp.GetValue(child);
                            if (navValue == null) return false;

                            var parentKeyOfNav = parentType.GetProperties().FirstOrDefault(p => p.HasAttribute<KeyAttribute>());
                            var navKeyValue = parentKeyOfNav?.GetValue(navValue);
                            return Equals(navKeyValue, parentKeyValue);
                        }).ToList();
                    }
                    else
                    {
                        children = Enumerable.Empty<TCollection>();
                    }
                }

                var listInstance = (object)children.ToList();

                collectionProperty.SetValue(parentEntity, listInstance);

                var backingFieldNames = new[]
                {
                    $"<{collectionProperty.Name}>k__BackingField",
                    $"<{collectionProperty.Name}>k_BackingField",
                    $"_{char.ToLower(collectionProperty.Name[0]) + collectionProperty.Name.Substring(1)}",
                    $"m_{collectionProperty.Name}"
                };

                var field = parentType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                    .FirstOrDefault(f => backingFieldNames.Contains(f.Name));

                if (field != null)
                {
                    field.SetValue(parentEntity, listInstance);
                }
            }
        }

        private void MapNavigationProperties<TEntity>(DbSet<TEntity> dbSet) where TEntity : class, new()
        {
            var entityType = typeof(TEntity);
            var navProps = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p =>
                    !AllowedSqlTypes.Contains(p.PropertyType) &&
                    !(p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)))
                .ToArray();

            foreach (var entity in dbSet.Entities)
            {
                foreach (var navProp in navProps)
                {
                    var targetType = navProp.PropertyType;
                    if (!this.dbSetProperties.TryGetValue(targetType, out var targetDbSetProp))
                        continue;

                    var targetDbSet = targetDbSetProp.GetValue(this);
                    var targetEntitiesProp = targetDbSetProp.PropertyType.GetProperty(nameof(DbSet<object>.Entities));
                    var targetEntities = (IEnumerable<object>)targetEntitiesProp.GetValue(targetDbSet);

                    var candidateFkNames = new[] { navProp.Name + "Id", targetType.Name + "Id", "Id", navProp.Name + "_Id" };

                    PropertyInfo fkProp = null;
                    object fkValue = null;

                    foreach (var fkName in candidateFkNames)
                    {
                        var p = entityType.GetProperty(fkName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (p != null && AllowedSqlTypes.Contains(p.PropertyType))
                        {
                            fkProp = p;
                            fkValue = p.GetValue(entity);
                            break;
                        }
                    }

                    object found = null;
                    if (fkProp != null)
                    {
                        var targetKeyProp = targetType.GetProperties().FirstOrDefault(p => p.HasAttribute<KeyAttribute>());
                        if (targetKeyProp != null)
                        {
                            found = targetEntities.FirstOrDefault(te =>
                            {
                                var val = targetKeyProp.GetValue(te);
                                return Equals(val, fkValue);
                            });
                        }
                    }
                    else
                    {
                        var targetKeyProp = targetType.GetProperties().FirstOrDefault(p => p.HasAttribute<KeyAttribute>());
                        if (targetKeyProp != null)
                        {
                            found = null;
                        }
                    }

                    if (found != null)
                    {
                        try
                        {
                            navProp.SetValue(entity, found);
                        }
                        catch
                        {
                            var backingFieldNames = new[]
                            {
                                $"<{navProp.Name}>k__BackingField",
                                $"<{navProp.Name}>k_BackingField",
                                $"_{char.ToLower(navProp.Name[0]) + navProp.Name.Substring(1)}",
                                $"m_{navProp.Name}"
                            };

                            var field = entityType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                                .FirstOrDefault(f => backingFieldNames.Contains(f.Name));

                            if (field != null)
                            {
                                field.SetValue(entity, found);
                            }
                        }
                    }
                }
            }
        }

        private static bool isObjectValid(object e) {
            var validationContext = new ValidationContext(e);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(e, validationContext, results, true);
            return isValid;
        }

        private IEnumerable<TEntity> LoadTableEntities<TEntity>() {
            var tableName = GetTableName(typeof(TEntity));
            var columns = this.connection.FetchColumnNames(tableName).ToArray();

            var entities = new List<TEntity>();
            var query = $"SELECT * FROM {tableName}";

            using (var command = new SqlCommand(query, new SqlConnection()))
            {
                this.connection.Open();
                using (var sqlCommand = new SqlCommand(query, (SqlConnection)typeof(DatabaseConnection)
                    .GetField("connection", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(this.connection)))
                {
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var entity = Activator.CreateInstance<TEntity>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var prop = typeof(TEntity).GetProperty(reader.GetName(i));
                                if (prop != null && AllowedSqlTypes.Contains(prop.PropertyType))
                                {
                                    var value = reader.GetValue(i);
                                    if (value is DBNull) value = null;
                                    prop.SetValue(entity, value);
                                }
                            }
                            entities.Add(entity);
                        }
                    }
                }
            }

            return entities;
        }

        private string GetTableName(Type table) {
            var tableName = table.Name;
            return tableName;
        }

        private Dictionary<Type, PropertyInfo> DiscoverDbSets() {
            var dbSets = this.GetType()
               .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
               .Where(p => p.PropertyType.IsGenericType &&
                           p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
               .ToDictionary(p => p.PropertyType.GetGenericArguments().First(), p => p);

            return dbSets;
        }

        private string GetEntityColumnNames(Type table) {
            var tableName = GetTableName(table);
            var cols = this.connection.FetchColumnNames(tableName);
            var escaped = cols.Select(c => $"[{c}]");
            return string.Join(", ", escaped);
        }
    }
}
