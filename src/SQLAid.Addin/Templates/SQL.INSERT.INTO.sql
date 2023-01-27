
-- INSERT INTO #GridResults ({columnHeaders})
SELECT *
FROM
(
    VALUES
    {rows}
) AS T({columnHeaders})