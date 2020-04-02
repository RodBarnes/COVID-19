SELECT
 CountryRegion,
 StateProvince,
 FileDate,
 SUM(TotalConfirmed) AS TotalConfirmed,
 SUM(TotalRecovered) AS TotalRecovered,
 SUM(TotalDeaths) AS TotalDeaths,
 SUM(TotalActive) AS TotalActive
FROM DailyReportAll
WHERE CountryRegion = 'US' AND NOT StateProvince = '' AND FileDate = '03-31-2020'
GROUP BY CountryRegion, StateProvince, FileDate
ORDER BY CountryRegion, StateProvince, FileDate

SELECT
 CountryRegion,
 FileDate,
 SUM(TotalConfirmed) AS TotalConfirmed,
 SUM(TotalRecovered) AS TotalRecovered,
 SUM(TotalDeaths) AS TotalDeaths,
 SUM(TotalActive) AS TotalActive
FROM DailyReportAll
WHERE CountryRegion = 'US' AND FileDate = '03-31-2020'
GROUP BY CountryRegion, FileDate
ORDER BY CountryRegion, FileDate

/*
select * from CountryRegion order by [Name]
select * from StateProvince order by [Name]
select * from CountyDistrict order by [Name]
select * from DailyReport order by FileDate
*/

exec spCountryRegionStateProvinceTotalsRead 'US', 'Washington'
exec spCountryRegionTotalsRead 'US'
exec spGlobalTotalsRead

