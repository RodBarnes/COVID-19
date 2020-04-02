CREATE PROCEDURE [dbo].[spGlobalTotalsRead]
AS

SELECT
 FileDate,
 SUM(TotalConfirmed) AS TotalConfirmed,
 SUM(TotalRecovered) AS TotalRecovered,
 SUM(TotalDeaths) AS TotalDeaths,
 SUM(TotalActive) AS TotalActive,
 SUM(NewConfirmed) AS NewConfirmed,
 SUM(NewRecovered) AS NewRecovered,
 SUM(NewDeaths) AS NewDeaths,
 SUM(NewActive) AS NewActive
FROM DailyReportAll
GROUP BY FileDate
ORDER BY FileDate

RETURN 0
