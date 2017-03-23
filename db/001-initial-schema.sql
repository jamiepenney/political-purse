SELECT run_migration(1, $$

    CREATE TABLE "donation" (
        "id" BIGSERIAL PRIMARY KEY,
        "party" TEXT NOT NULL,
        "year" INTEGER NOT NULL,
        "donee" TEXT NOT NULL,
        "number_of_donations" INTEGER,
        "number_of_donees" INTEGER,
        "donee_address" TEXT,
        "postcode" TEXT,
        "amount" NUMERIC(17, 2)
    );

$$);

-- rollback_migration(1, $$
--  DROP TABLE "donation";
-- $$);