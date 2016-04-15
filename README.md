# random-marten-hacks

Summary:

* No major problems
* Marten uses Roslyn so start up time is long enough to be noticable. There is a workaround: https://github.com/JasperFx/marten/issues/289
* JSON support in PostgreSQL is excellent so if something can't be done via Marten it can be done directly via NpgsqlCommand and raw SQL.
* Searching using date ranges is a bit painful but it is a problem with JSON that simply does not have date/time data type.
* As of now LINQ queries are not complied and cached
* Performance of concurrent inserts is not great: 

