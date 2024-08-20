

DROP TABLE IF EXISTS public.expenses_backup;
CREATE TABLE public.expenses_backup AS TABLE public.expenses;

-- Step 1: Add the new column with a nullable string type
ALTER TABLE expenses ADD COLUMN trip_status VARCHAR(255);

-- Step 2: Update the new column based on the existing boolean field
UPDATE expenses
SET trip_status = CASE
                      WHEN trip = TRUE THEN 'unknown'
                      WHEN trip = FALSE THEN NULL
    END;

-- Step 3: Drop the old boolean column if needed
ALTER TABLE expenses DROP COLUMN trip;

-- Step 4: Optionally, rename the new column to 'trip' if desired
ALTER TABLE expenses RENAME COLUMN trip_status TO trip;

