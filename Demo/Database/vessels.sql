USE [Marina]
GO

/****** Object:  Table [dbo].[Vessels]    Script Date: 25.04.2022 11:59:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Vessels](
	[ID_VESSEL] [nchar](10) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ID_PORT] [nchar](10) NOT NULL,
	[Length] [float] NULL,
	[Width] [float] NULL,
	[Height] [float] NULL,
	[Brutto] [float] NULL,
	[Netto] [float] NULL,
	[CallSign] [nchar](10) NULL,
 CONSTRAINT [PK_Vessels] PRIMARY KEY CLUSTERED 
(
	[ID_VESSEL] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Vessels]  WITH CHECK ADD  CONSTRAINT [FK_Vessels_Ports] FOREIGN KEY([ID_PORT])
REFERENCES [dbo].[Ports] ([ID_PORT])
GO

ALTER TABLE [dbo].[Vessels] CHECK CONSTRAINT [FK_Vessels_Ports]
GO

