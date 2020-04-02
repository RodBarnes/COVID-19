select *
from DailyReportAll
where FileDate >= '3/21/2020' and FileDate <= '3/24/2020'
 and CountryRegion = 'US' and StateProvince = 'Washington'
order by FileDate, CountryRegion, StateProvince

select min(FileDate), max(FileDate) from DailyReportAll

exec spCountryRegionStateProvinceTotalsRead 'US','Washington'
