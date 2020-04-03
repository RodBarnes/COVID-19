CREATE TABLE [dbo].[CountryStats] (
    [CountryId] INT IDENTITY (1, 1) NOT NULL,
	[Rank] INT NOT NULL,
	[Country] NVARCHAR (100) NOT NULL,
	[Population] INT NOT NULL,
	[PctChange] DECIMAL(5, 2) NULL,
	[NetChange] INT NULL,
	[Density] INT NULL,
	[Area] INT NULL,
	[Migrants] INT NULL,
	[FertilityRate] FLOAT NULL,
	[MedianAge] INT NULL,
	[PctUrban] DECIMAL(5, 2) NULL,
	[PctWorld] DECIMAL(5, 2) NULL
    PRIMARY KEY CLUSTERED ([CountryId] ASC)
);
