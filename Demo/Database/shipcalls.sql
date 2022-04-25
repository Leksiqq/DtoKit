USE [Marina]
GO

/****** Object:  Table [dbo].[ShipCalls]    Script Date: 25.04.2022 13:06:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ShipCalls](
	[ID_SHIPCALL] [int] NOT NULL,
	[ID_LINE] [nchar](10) NOT NULL,
	[ID_ROUTE] [int] NOT NULL,
	[ID_PORT] [nchar](10) NOT NULL,
	[Voyage] [nvarchar](50) NOT NULL,
	[Arrival] [datetime] NULL,
	[Departure] [datetime] NULL,
	[PrevID_SHIPCALL] [int] NULL,
 CONSTRAINT [PK_ShipCalls] PRIMARY KEY CLUSTERED 
(
	[ID_SHIPCALL] ASC,
	[ID_LINE] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ShipCalls]  WITH CHECK ADD  CONSTRAINT [FK_ShipCalls_Routes] FOREIGN KEY([ID_ROUTE], [ID_LINE])
REFERENCES [dbo].[Routes] ([ID_ROUTE], [ID_LINE])
GO

ALTER TABLE [dbo].[ShipCalls] CHECK CONSTRAINT [FK_ShipCalls_Routes]
GO

ALTER TABLE [dbo].[ShipCalls]  WITH CHECK ADD  CONSTRAINT [FK_ShipCalls_ShipCalls] FOREIGN KEY([PrevID_SHIPCALL], [ID_LINE])
REFERENCES [dbo].[ShipCalls] ([ID_SHIPCALL], [ID_LINE])
GO

ALTER TABLE [dbo].[ShipCalls] CHECK CONSTRAINT [FK_ShipCalls_ShipCalls]
GO

