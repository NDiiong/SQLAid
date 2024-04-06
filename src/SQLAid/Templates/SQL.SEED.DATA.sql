
GO
IF NOT EXISTS (SELECT TOP 1 1 FROM {tableName} WHERE {key})
BEGIN
	INSERT {tableName} ({columnHeaders})
	VALUES {rows}
END