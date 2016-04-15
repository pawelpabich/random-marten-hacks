# random-marten-hacks

Summary:

* Marten uses Roslyn so start up time is long enough to be noticable. There is a workaround: https://github.com/JasperFx/marten/issues/289
* JSON support in PostgreSQL is excellent so if something can't be done via Marten it can be done directly via NpgsqlCommand and raw SQL.
* Searching using date ranges is a bit painful but it is a problem with JSON that simply doesn't have date/time data type.
* As of now LINQ queries are not complied and cached and not all operators are implemented. Check docs for details.
* Performance of concurrent inserts is not great when the default UpsertMode is left in place: https://github.com/JasperFx/marten/issues/296. When UpsertMode is changed to Standard then I get the following numbers on my laptop (i7 + SSD + 50 connections):
	* 10k concurrent users, each user persists 1 document. 714 documents / second
	* 10k concurrent users, each user persists 10 documents. 2777 documents / second
	* 1k concurrent users, each user persists 10 documents. 3333 documents / second
* Performance of concurrent queries is fine.
* API is easy to use and is based on RavenDB.
* PGAdmin is a decent tool but nowhere close as good as Sql Management Studio.

