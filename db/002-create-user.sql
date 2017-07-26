SELECT run_migration(2, $$

    CREATE TABLE "user" (
        "id" BIGSERIAL PRIMARY KEY,
        "email" TEXT NOT NULL,
        "name" TEXT NOT NULL,
        "password_hash" TEXT NOT NULL,
    );

$$);

-- rollback_migration(2, $$
--  DROP TABLE "user";
-- $$);