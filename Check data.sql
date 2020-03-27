SELECT cr.[Name] as CountryRegion, sp.[Name] as StateProvince, cd.[Name] as CountyDistrict,
dr.FileDate, dr.LastUpdate, dr.TotalConfirmed, dr.TotalRecovered, dr.TotalDeaths, dr.TotalActive, dr.Latitude, dr.Longitude,
dr.NewConfirmed, dr.NewRecovered, dr.NewDeaths
FROM DailyReport dr
JOIN CountryRegion cr ON cr.CountryRegionId=dr.CountryRegionId
JOIN StateProvince sp ON sp.StateProvinceId=dr.StateProvinceId
JOIN CountyDistrict cd ON cd.CountyDistrictId=dr.CountyDistrictId
order by dr.TotalRecovered desc

select cr.[Name] as CountryRegion, sp.[Name] as StateProvince, cd.[Name] as CountyDistrict,
 dr.FileDate, dr.LastUpdate
from DailyReport dr
 join CountryRegion cr on cr.CountryRegionId = dr.CountryRegionId
 join StateProvince sp on sp.StateProvinceId = dr.StateProvinceId
 join CountyDistrict cd on cd.CountyDistrictId = dr.CountyDistrictId
where cr.[Name] = 'US'
order by
 cr.[Name], sp.[Name], cd.[Name]

SELECT COUNT(*) as Count
FROM DailyReport dr
 JOIN CountryRegion cr ON cr.CountryRegionId = dr.countryRegionId
 JOIN StateProvince sp ON sp.StateProvinceId = dr.StateProvinceId
 JOIN CountyDistrict cd ON cd.CountyDistrictId = dr.CountyDistrictId
WHERE cr.[Name]='US' AND sp.[Name]='' AND cd.[Name]=''

select * from CountryRegion order by [Name]
select * from StateProvince order by [Name]
select * from CountyDistrict order by [Name]

select * from DailyReport where CountryRegionId = 115 and StateProvinceId = 1 and CountyDistrictId = 1
(115, 1, 1, Mar 11 2020 12:00AM)