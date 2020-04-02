CREATE VIEW [dbo].[DailyReportAll]
	AS

SELECT
 dr.FileDate,
 cr.[Name] AS CountryRegion, dr.CountryRegionId,
 sp.[Name] AS StateProvince, dr.StateProvinceId,
 cd.[Name] AS CountyDistrict, dr.CountyDistrictId,
 dr.FIPS,
 dr.LastUpdate,
 dr.TotalConfirmed,
 dr.TotalRecovered,
 dr.TotalDeaths,
 dr.TotalActive,
 dr.NewConfirmed,
 dr.NewRecovered,
 dr.NewDeaths,
 dr.NewActive,
 dr.Latitude,
 dr.Longitude
FROM DailyReport dr
JOIN CountryRegion cr ON cr.CountryRegionId = dr.CountryRegionId
JOIN StateProvince sp ON sp.StateProvinceId = dr.StateProvinceId
JOIN CountyDistrict cd ON cd.CountyDistrictId = dr.CountyDistrictId
