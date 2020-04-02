CREATE PROCEDURE [dbo].[spCountryRegionTotalsRead]
    @country NVARCHAR(100) = NULL
AS

SELECT
 CountryRegion,
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
WHERE (@country IS NULL OR @country = CountryRegion)
GROUP BY CountryRegion, FileDate
ORDER BY CountryRegion ASC, FileDate ASC

RETURN 0
