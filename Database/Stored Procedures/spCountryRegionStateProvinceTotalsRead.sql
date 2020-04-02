CREATE PROCEDURE [dbo].[spCountryRegionStateProvinceTotalsRead]
    @country NVARCHAR(100) = NULL,
    @state NVARCHAR(100) = NULL
AS

SELECT
 CountryRegion,
 StateProvince,
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
WHERE (@country IS NULL AND @state IS NULL)
 OR (@country = CountryRegion AND @state = StateProvince)
GROUP BY CountryRegion, StateProvince, FileDate
ORDER BY CountryRegion, StateProvince, FileDate

RETURN 0
