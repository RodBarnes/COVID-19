select
 CountryRegion,
 StateProvince,
 FileDate,
 sum(TotalConfirmed) as TotalConfirmed,
 sum(TotalRecovered) as TotalRecovered,
 sum(TotalDeaths) as TotalDeaths,
 sum(TotalActive) as TotalActive
from DailyReportAll
where CountryRegion = 'US' and FileDate = '03-31-2020'
group by CountryRegion, StateProvince, FileDate
order by CountryRegion, StateProvince, FileDate

select * from CountryRegion order by [Name]
select * from StateProvince order by [Name]
select * from CountyDistrict order by [Name]
select * from DailyReport order by FileDate