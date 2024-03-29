﻿CREATE TABLE [dbo].[DailyReport]
(
	[CountryRegionId] INT NOT NULL, 
    [StateProvinceId] INT NOT NULL, 
    [CountyDistrictId] INT NOT NULL, 
    [FileDate] DATETIME NOT NULL,
    [LastUpdate] DATETIME NULL, 
    [TotalConfirmed] INT NULL DEFAULT 0, 
    [TotalRecovered] INT NULL DEFAULT 0, 
    [TotalDeaths] INT NULL DEFAULT 0, 
    [TotalActive] INT NULL DEFAULT 0,
    [NewConfirmed] INT NULL DEFAULT 0, 
    [NewRecovered] INT NULL DEFAULT 0, 
    [NewDeaths] INT NULL DEFAULT 0, 
    [NewActive] INT DEFAULT ((0)) NULL,
    [Latitude] DECIMAL(10, 4) NULL DEFAULT 0, 
    [Longitude] DECIMAL(10, 4) NULL DEFAULT 0, 
    [FIPS] INT DEFAULT ((0)) NULL,
    CONSTRAINT [PK_DailyReport] PRIMARY KEY ([CountryRegionId], [StateProvinceId], [CountyDistrictId], [FileDate]), 
)
