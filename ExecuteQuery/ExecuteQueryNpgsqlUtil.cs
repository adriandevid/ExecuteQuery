using ExecuteQuery.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;

namespace ExecuteQuery.Npgsql
{
    public class ExecuteQueryNpgsqlUtil : IExecuteQueryUtil
    {
        /// <summary>
        /// Generating result of query informing 'sql'.
        /// 
        /// </summary>
        /// <typeparam name="EntityType">Type of return query</typeparam>
        /// <param name="query">Query string with option to select the columns that will be mapped, example: table.column as [columnNameExample]</param>
        /// <param name="isEnvironmentVariable">Specify if your connection string is coming from environment variable</param>
        /// <returns></returns>
        public async Task<IList<EntityType>> ExecuteQuery<EntityType>(string query, bool isEnvironmentVariable = false) where EntityType : class
        {
            ValidateStructureOfQuery(query);
            IConfiguration? configuration = null;

            if (!isEnvironmentVariable) {
                configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            }

            var connectionString = isEnvironmentVariable ? Environment.GetEnvironmentVariable("DB") : configuration.GetConnectionString("DB");

            var queryNoClousureColumns = CleanColumnsInformed(query);
            List<EntityType> datasSerialized = new List<EntityType>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(queryNoClousureColumns, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Dictionary<string, dynamic> mappings = MappingValuesOfResultQuery(reader, query);

                            var serialize = JsonConvert.SerializeObject(mappings);
                            var deserialized = JsonConvert.DeserializeObject<EntityType>(serialize);

                            if (deserialized != null) datasSerialized.Add(deserialized);
                        }
                    }
                }
                await connection.CloseAsync();
            }

            return datasSerialized;
        }

        /// <summary>
        /// Extract will columns of query
        /// 
        /// obs: For the parameters to be found, the column name must be enclosed in square brackets.
        /// ex: [nameOfColumn]
        /// </summary>
        /// <param name="sql">Query string</param>
        /// <returns></returns>
        private List<string> ExtractColumnsOfSqlQuery(string sql)
        {
            var index = 0;
            List<string> colunas = new List<string>();
            int counter = 0;
            for (int i = 0; i < sql.Length; i++)
            {
                if (index == 0 && sql[i] == '[')
                {
                    index = i;
                }

                if (index != 0 && sql[i] == ']' && (sql[i + 1] == ',' || sql[i + 1] == ' ' || sql[i + 2] == ' '))
                {
                    colunas.Add(sql.Substring((index + 1), (i - index - 1)));
                    index = 0;
                    counter += 1;
                }
            }

            return colunas;
        }

        /// <summary>
        /// Clean a query string for execute, removing brackets.
        /// </summary>
        /// <param name="sql">Query string</param>
        /// <returns></returns>
        private string CleanColumnsInformed(string sql)
        {
            return sql.Replace("[", "").Replace("]", "");
        }

        /// <summary>
        /// Validate the sintaxy of query
        /// </summary>
        /// <param name="query">Query string</param>
        /// <returns></returns>
        private void ValidateStructureOfQuery(string query) 
        {
            if (query.IndexOf("select") == -1) throw new Exception("append \"select\" clausure in are query");
            if (query.IndexOf("from") == -1) throw new Exception("append \"from\" clausure in are query");
        }

        /// <summary>
        /// Mapping the Values of result query 
        /// </summary>
        /// <param name="query">Query string</param>
        /// <returns></returns>
        private Dictionary<string, dynamic> MappingValuesOfResultQuery(NpgsqlDataReader reader, string query)
        {
            List<string> columns = ExtractColumnsOfSqlQuery(query);
            Dictionary<string, dynamic> mappings = new Dictionary<string, dynamic>();

            var resultColumns = reader.GetColumnSchema();

            for (var counterOfColumnTable = 0; counterOfColumnTable < resultColumns.Count; ++counterOfColumnTable)
            {
                if (columns.Count == 0)
                {
                    mappings.Add(resultColumns[counterOfColumnTable].ColumnName, reader.GetValue(counterOfColumnTable));
                }
                else
                {
                    foreach (var colunaQuery in columns)
                    {
                        if (colunaQuery == resultColumns[counterOfColumnTable].ColumnName)
                        {
                            mappings.Add(colunaQuery, reader.GetValue(counterOfColumnTable));
                        }
                        else
                        {
                            mappings.Add(resultColumns[counterOfColumnTable].ColumnName, reader.GetValue(counterOfColumnTable));
                        }
                    }
                }
            }

            return mappings;
        }
    }

}
