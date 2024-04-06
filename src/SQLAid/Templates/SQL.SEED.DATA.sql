
GO
IF NOT EXISTS (SELECT TOP 1 1 FROM #tempTable WHERE {key})
BEGIN
	INSERT #tempTable ({columnHeaders})
	VALUES {rows}
END