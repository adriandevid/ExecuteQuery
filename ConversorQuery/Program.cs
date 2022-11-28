using ConversorQuery.EntitiesMapping;
using QueryConversor;

var resultQuery = await ConversorQueryUtil.GerarDadosApartirDaQuery<TipoFase>("select * from academico.tb_tp_fase tpf");

foreach (var item in resultQuery)
{
    Console.WriteLine($"{item.cd_tp_fase_pk} {item.nm_fase}");
}