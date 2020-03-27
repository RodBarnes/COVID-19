CREATE TABLE [dbo].[DailyReport]
(
	[CountryRegionId] INT NOT NULL, 
    [StateProvinceId] INT NOT NULL, 
    [CountyDistrictId] INT NOT NULL, 
    [FileDate] DATETIME NOT NULL,
    [LastUpdate] DATETIME NULL, 
    [TotalActive] INT NULL DEFAULT 0,
    [TotalConfirmed] INT NULL DEFAULT 0, 
    [TotalRecovered] INT NULL DEFAULT 0, 
    [TotalDeaths] INT NULL DEFAULT 0, 
    [Latitude] FLOAT NULL DEFAULT 0, 
    [Longitude] FLOAT NULL DEFAULT 0, 
    [NewConfirmed] INT NULL DEFAULT 0, 
    [NewRecovered] INT NULL DEFAULT 0, 
    [NewDeaths] INT NULL DEFAULT 0, 
    CONSTRAINT [PK_DailyReport] PRIMARY KEY ([CountryRegionId], [StateProvinceId], [CountyDistrictId], [FileDate]), 
)
