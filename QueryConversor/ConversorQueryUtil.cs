using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;

namespace ExecuteQuery
{
    public class ExecuteQuery
    {
        /// <summary>
        /// Gera o mapeamento dos parametros, além de executar uma consulta performatica de forma sincrona.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public async static Task<IList<EntityType>> GerarDadosApartirDaQuery<EntityType>(string query) where EntityType : class
        {
            ValidarEstruturaDaQuery(query);

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var selectSemDestaqueDasColunas = LimparColunasDestacadas(query);
            List<EntityType> armazenamentoDeDadosDeserializados = new List<EntityType>();

            using (var conexao = new NpgsqlConnection(configuration.GetConnectionString("DB")))
            {
                await conexao.OpenAsync();
                using (var comando = new NpgsqlCommand(selectSemDestaqueDasColunas, conexao))
                {
                    using (NpgsqlDataReader reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            List<string> colunas = ExtrairColunasDoSelect(query);
                            Dictionary<string, dynamic> mapeamento = new Dictionary<string, dynamic>();

                            var resultColumns = reader.GetColumnSchema();

                            for (var counterOfColumntable = 0; counterOfColumntable < resultColumns.Count; ++counterOfColumntable) 
                            {
                                if (colunas.Count == 0) {
                                    mapeamento.Add(resultColumns[counterOfColumntable].ColumnName, reader.GetValue(counterOfColumntable));
                                } else {
                                    foreach (var colunaQuery in colunas)
                                    {
                                        if (colunaQuery == resultColumns[counterOfColumntable].ColumnName)
                                        {
                                            mapeamento.Add(colunaQuery, reader.GetValue(counterOfColumntable));
                                        }
                                        else {
                                            mapeamento.Add(resultColumns[counterOfColumntable].ColumnName, reader.GetValue(counterOfColumntable));
                                        }
                                    }
                                }
                            }

                            
                            var serializacao = JsonConvert.SerializeObject(mapeamento);
                            var deserializacao = JsonConvert.DeserializeObject<EntityType>(serializacao);

                            armazenamentoDeDadosDeserializados.Add(deserializacao);
                        }
                    }
                }
                await conexao.CloseAsync();
            }

            return armazenamentoDeDadosDeserializados;
        }

        /// <summary>
        /// Obtem todas as colunas do select informado.
        /// obs: para que os parametros sejam encontrados é necessario que o nome da coluna esteja entre colchetes.
        /// ex: [noma da coluna]
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static List<string> ExtrairColunasDoSelect(string sql)
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
        /// Limpa a query para que se possa ser executada com eficiência e sem erros de sintaxe.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static string LimparColunasDestacadas(string sql)
        {
            return sql.Replace("[", "").Replace("]", "");
        }

        private static void ValidarEstruturaDaQuery(string query) 
        {
            if (query.IndexOf("select") == -1) throw new Exception("Adicione uma clausula select");
            if (query.IndexOf("from") == -1) throw new Exception("Adicione uma clausula from");
        }
    }

}
