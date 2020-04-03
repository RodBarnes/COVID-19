TRUNCATE TABLE CountryRegion
DBCC CHECKIDENT ('CountryRegion', RESEED, 1)
GO

TRUNCATE TABLE StateProvince
DBCC CHECKIDENT ('StateProvince', RESEED, 1)
GO

INSERT INTO StateProvince ([Name]) VALUES('')
GO

TRUNCATE TABLE CountyDistrict
DBCC CHECKIDENT ('CountyDistrict', RESEED, 1)
GO

INSERT INTO CountyDistrict ([Name]) VALUES('')
GO

TRUNCATE TABLE DailyReport
GO

TRUNCATE TABLE CountryStats
GO

