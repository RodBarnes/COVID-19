CREATE TABLE [dbo].[DailyReport]
(
	[CountryRegionId] INT NOT NULL, 
    [StateProvinceId] INT NOT NULL, 
    [CountyDistrictId] INT NOT NULL, 
    [RecordDate] DATETIME NOT NULL,
    [TotalActive] INT NULL,
    [TotalConfirmed] INT NULL, 
    [TotalRecovered] INT NULL, 
    [TotalDeaths] INT NULL, 
    [Latitude] FLOAT NULL, 
    [Longitude] FLOAT NULL, 
    [NewConfirmed] INT NULL, 
    [NewRecovered] INT NULL, 
    [NewDeaths] INT NULL, 
    CONSTRAINT [PK_DailyReport] PRIMARY KEY ([CountryRegionId], [StateProvinceId], [CountyDistrictId], [RecordDate]), 
)
