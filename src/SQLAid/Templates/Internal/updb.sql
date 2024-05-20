DECLARE @id_control BIGINT
DECLARE @batchSize BIGINT
DECLARE @results BIGINT

SET @results = 1
SET @batchSize = 1000
SET @id_control = 0

WHILE (@results > 0)
BEGIN

   UPDATE <alias>
   SET <alias>.<column_name> = <value>
   FROM <table_name> <alias>
   WHERE <alias>.id > @id_control
   AND <alias>.id <= @id_control + @batchSize

   -- very important to obtain the latest rowcount to avoid infinite loops
   SET @results = @@ROWCOUNT

   -- next batch
   SET @id_control = @id_control + @batchSize

  -- sprint 
   RAISERROR (@id_control, 0, 1) WITH NOWAIT 
END