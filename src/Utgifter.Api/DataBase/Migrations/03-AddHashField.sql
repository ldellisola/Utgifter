-- Ensure pgcrypto extension is installed
CREATE EXTENSION IF NOT EXISTS pgcrypto;

DROP TABLE IF EXISTS public.expenses_backup;
CREATE TABLE public.expenses_backup AS TABLE public.expenses;

-- 2. Add a new column for the hash field
ALTER TABLE public.expenses ADD COLUMN hash VARCHAR(44);

-- 3. Compute the hash values and store them in the new column
UPDATE public.expenses
SET hash = encode(
        digest(
                date::text || '.' || person || '.' || store || '.' || amount::text,
                'sha256'
        ),
        'base64'
           );

-- 4. Create an index on the `hash` column
CREATE INDEX idx_expenses_hash ON public.expenses(hash);

