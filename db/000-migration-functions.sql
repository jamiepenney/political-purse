-- Migration stuff

CREATE TABLE IF NOT EXISTS "migration" (
	migration_id integer PRIMARY KEY
);

CREATE OR REPLACE FUNCTION has_migration_run(id integer)
RETURNS boolean AS
$body$
BEGIN
	RETURN EXISTS (
		SELECT * FROM migration WHERE migration_id=id
	);
END;
$body$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION migration_finished(id integer)
RETURNS boolean AS
$body$
DECLARE
	"run" boolean;
BEGIN
	"run" := has_migration_run(id);
	INSERT INTO migration (migration_id)
	SELECT id
	WHERE NOT EXISTS (
		SELECT * FROM migration WHERE migration_id=id
	);
	RETURN run;
END;
$body$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION rollback_migration(id integer, ddl text)
RETURNS VOID AS
$body$
BEGIN
	IF has_migration_run(id) THEN
		EXECUTE ddl;
		DELETE FROM migration where migration_id=id;
	END IF;
END;
$body$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION run_migration(id integer, ddl text) RETURNS boolean AS $$
BEGIN
	IF has_migration_run(id) THEN
		RETURN FALSE;
	ELSE
		EXECUTE ddl;
		RETURN migration_finished(id) = FALSE;
	END IF;
END;
$$ LANGUAGE plpgsql STRICT;