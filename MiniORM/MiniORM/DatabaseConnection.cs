using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace MiniORM
{
    internal class DatabaseConnection
    {
        private readonly SqlConnection connection;

        private SqlTransaction transaction;

        public DatabaseConnection(string connectionString) { 
            this.connection=new SqlConnection(connectionString);
        }

        private SqlCommand CreateCommand(string queryText, params SqlParameter[] parameters) {
            var command = new SqlCommand(queryText, this.connection, this.transaction);
            foreach (var param in parameters)
            {
                  command.Parameters.Add(param);
            }
            return command;
        }

        public int ExecuteNonQuery(string queryText, params SqlParameter[] parameters) {
            using (var query = CreateCommand(queryText, parameters)) {
                var result = query.ExecuteNonQuery();
                return result;
            }
        }

        public IEnumerable<string> FetchColumnNames(string tableName) { 
            var rows= new List<string>();
            var queryText = $@"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=`{tableName}`";
            using (var query = CreateCommand(queryText)) {
                using (var reader=query.ExecuteReader()) {
                    while (reader.Read())
                    {
                       var column=reader.GetString(0);
                        rows.Add(column);
                    }
                }
            }

            return rows;
        }
        public void InsertEntities<T>(IEnumerable<T> entities, string tableName, string[] columns) {
            var identityColumns = GetIdentityColumns(tableName);
            var columnsToInsert=columns.Except(identityColumns).ToArray();
            var escapeColumns = columnsToInsert.Select(EscapeColumn).ToArray();
            var rowValues = entities
                .Select(entity => columnsToInsert
                .Select(
                    c => entity.GetType().GetProperty(c).GetValue(entity)
                 ).ToArray())
                .ToArray();
            var rowParameterNames = Enumerable.Range(1, rowValues.Length)
                .Select(i=>columnsToInsert.Select(c=>c+1).ToArray())
                .ToArray();
            var sqlColumns = string.Join(", ", escapeColumns);
            var sqlRows = string.Join(", ", rowParameterNames.Select(p =>
                string.Format("({0})", string.Join(", ", p.Select(a => $"@{a}")))
            ));
            var query = string.Format("INSERT INTO {0} ({1}) VALUES {2}", tableName, sqlColumns, sqlRows);
            var parameters = rowParameterNames
                .Zip(rowValues,(@params,values)=>
                    @params.Zip(values,(param,value)=>
                        new SqlParameter(param,value ?? DBNull.Value)
                    )
                ).SelectMany(p => p)
                .ToArray();
            var insertedRows=this.ExecuteNonQuery(query,parameters);
            if (insertedRows != entities.Count()) {
                throw new InvalidOperationException($"Could not insert {entities.Count()} {insertedRows} rows!");
            }
        }

        public void UpdateEntities<T>(IEnumerable<T> modifiedEntities, string tableName, string[] columns)
        {
            var identityColumns = GetIdentityColumns(tableName);
            var columnsToUpdate = columns.Except(identityColumns).ToArray();

            var primaryKeyProperties = typeof(T).GetProperties()
                .Where(pi => pi.HasAttribute<KeyAttribute>())
                .ToArray();

            foreach (var entity in modifiedEntities)
            {
                var primaryKeyValues = primaryKeyProperties
                    .Select(c => c.GetValue(entity))
                    .ToArray();

                var primaryKeyParameters = primaryKeyProperties
                    .Zip(primaryKeyValues, (param, value) => new SqlParameter(param.Name, value))
                    .ToArray();

                var setClauses = columnsToUpdate
                    .Select(c => $"{EscapeColumn(c)} = @{c}")
                    .ToArray();

                var query = $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE " +
                            string.Join(" AND ", primaryKeyProperties.Select(pk => $"{EscapeColumn(pk.Name)} = @{pk.Name}"));

                var parameters = columnsToUpdate
                    .Select(c => new SqlParameter(c, typeof(T).GetProperty(c).GetValue(entity) ?? DBNull.Value))
                    .Concat(primaryKeyParameters)
                    .ToArray();

                ExecuteNonQuery(query, parameters);
            }
        }

        public void DeleteEntities<T>(IEnumerable<T> entitiesToDelete, string tableName, string[] columns)
        {
            var primaryKeyProperties = typeof(T).GetProperties()
                .Where(pi => pi.HasAttribute<KeyAttribute>())
                .ToArray();

            foreach (var entity in entitiesToDelete)
            {
                var whereClauses = primaryKeyProperties
                    .Select(pk => $"{EscapeColumn(pk.Name)} = @{pk.Name}")
                    .ToArray();

                var parameters = primaryKeyProperties
                    .Select(pk => new SqlParameter(pk.Name, pk.GetValue(entity)))
                    .ToArray();

                var query = $"DELETE FROM {tableName} WHERE {string.Join(" AND ", whereClauses)}";
                ExecuteNonQuery(query, parameters);
            }
        }

        private IEnumerable<string> GetIdentityColumns(string tableName) {
            var identityCols = new List<string>();
            var query = $@"SELECT COLUMN_NAME 
                           FROM INFORMATION_SCHEMA.COLUMNS 
                           WHERE TABLE_NAME = '{tableName}' AND COLUMNPROPERTY(object_id(TABLE_NAME), COLUMN_NAME, 'IsIdentity') = 1";

            using (var command = CreateCommand(query))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    identityCols.Add(reader.GetString(0));
                }
            }

            return identityCols;
        }

        public SqlTransaction StartTransaction() {
            if (this.transaction == null)
            {
                this.transaction = this.connection.BeginTransaction();
            }
            return this.transaction;
        }

        public void Open() => this.connection.Open();

        public void Close() => this.connection.Close();

        private static string EscapeColumn(string c) {
            return $"[{c}]";
        }

        private static T MapColumnsToObject<T>(string columnName, object[] columns) {
            var obj = Activator.CreateInstance<T>();
            var props = typeof(T).GetProperties();

            for (int i = 0; i < columns.Length && i < props.Length; i++)
            {
                props[i].SetValue(obj, columns[i] == DBNull.Value ? null : columns[i]);
            }

            return obj;
        }
    }
}
