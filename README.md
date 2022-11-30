# ExecuteQuery is library open source
This is a library open source for execute queries in postgresql database.

## Execute Mode

First execute the some procedures, is essential create a ConnectionString in appsettings with name **DB** or create a environment variable with name **"DB"**.

```json
{
    "ConnectionStrings": {
        "DB": "{connectionString}"
    }
}
```

For execute this library is essential create query with or whithout brackets, an example:

```sql
select 
    [rowExample].[columnExample] as [columnNameForMapping]
    [rowExample].[columnExample] 
from [schem].[table] [rowExample]
```

With the query created, in next level of execution, our let's go create model of return query.

```csharp
class Model {
    public int [namePropertie] { get; set; }
    public int [namePropertie] { get; set; }
}
```

Now let's create a instance of object **ExecuteQueryNpgsqlUtil**:

```csharp
using ExecuteQuery.Npgsql;

const instanceObject = new ExecuteQueryNpgsqlUtil();

var resultQuery = await instanceObject.ExecuteQuery<Model>([queryCreated]);
```

By: Adrian Devid Menezes Santos.

Site of creator: [adrian devid menezes santos](https://www.adriandevid.com/)


<div align="center"  class="icons-social" style="margin-left: 10px;">
        <a style="margin-left: 10px;"  target="_blank" href="https://www.linkedin.com/in/adrian-devid-menezes-santos-ba584017b/">
			<img src="https://img.icons8.com/doodle/40/000000/linkedin--v2.png"></a>
        <a style="margin-left: 10px;" target="_blank" href="https://github.com/adriandevid">
		<img src="https://img.icons8.com/doodle/40/000000/github--v1.png"></a>
        <a style="margin-left: 10px;" target="_blank" href="https://instagram.com/adrian_devid_iii">
			<img src="https://img.icons8.com/doodle/40/000000/instagram-new--v2.png"></a>
      </div>
</p>