SELECT cr.[Name] as CountryRegion, sp.[Name] as StateProvince, cd.[Name] as CountyDistrict,
dr.RecordDate, dr.TotalConfirmed, dr.TotalRecovered, dr.TotalDeaths, dr.TotalActive, dr.Latitude, dr.Longitude,
dr.NewConfirmed, dr.NewRecovered, dr.NewDeaths
FROM DailyReport dr
JOIN CountryRegion cr ON cr.CountryRegionId=dr.CountryRegionId
JOIN StateProvince sp ON sp.StateProvinceId=dr.StateProvinceId
JOIN CountyDistrict cd ON cd.CountyDistrictId=dr.CountyDistrictId
order by dr.TotalRecovered desc
