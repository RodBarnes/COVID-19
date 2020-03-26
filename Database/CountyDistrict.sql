CREATE TABLE [dbo].[CountyDistrict]
(
	[CountyDistrictId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] NVARCHAR(100) NOT NULL, 
    [FIPS] INT NULL
)
