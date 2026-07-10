USE [master]
GO
/****** Object:  Database [CloneEbayDB]    Script Date: 11/15/2025 8:41:55 AM ******/
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CloneEbayDB')
BEGIN
CREATE DATABASE [CloneEbayDB]
END
GO
ALTER DATABASE [CloneEbayDB] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [CloneEbayDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [CloneEbayDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [CloneEbayDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [CloneEbayDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [CloneEbayDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [CloneEbayDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [CloneEbayDB] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [CloneEbayDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [CloneEbayDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [CloneEbayDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [CloneEbayDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [CloneEbayDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [CloneEbayDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [CloneEbayDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [CloneEbayDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [CloneEbayDB] SET  ENABLE_BROKER 
GO
ALTER DATABASE [CloneEbayDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [CloneEbayDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [CloneEbayDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [CloneEbayDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [CloneEbayDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [CloneEbayDB] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [CloneEbayDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [CloneEbayDB] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [CloneEbayDB] SET  MULTI_USER 
GO
ALTER DATABASE [CloneEbayDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [CloneEbayDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [CloneEbayDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [CloneEbayDB] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [CloneEbayDB] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [CloneEbayDB] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [CloneEbayDB] SET QUERY_STORE = ON
GO
ALTER DATABASE [CloneEbayDB] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [CloneEbayDB]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Address]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Address](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[userId] [int] NULL,
	[fullName] [nvarchar](100) NULL,
	[phone] [nvarchar](20) NULL,
	[street] [nvarchar](100) NULL,
	[city] [nvarchar](50) NULL,
	[state] [nvarchar](50) NULL,
	[country] [nvarchar](50) NULL,
	[isDefault] [bit] NULL,
 CONSTRAINT [PK__Address__3213E83F58794D6D] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Bid]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bid](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[productId] [int] NULL,
	[bidderId] [int] NULL,
	[amount] [decimal](10, 2) NULL,
	[bidTime] [datetime] NULL,
 CONSTRAINT [PK__Bid__3213E83F51E40AD4] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Category]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Category](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](100) NULL,
 CONSTRAINT [PK__Category__3213E83FFBFAA45E] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Coupon]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Coupon](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[code] [nvarchar](50) NULL,
	[discountPercent] [decimal](5, 2) NULL,
	[startDate] [datetime] NULL,
	[endDate] [datetime] NULL,
	[maxUsage] [int] NULL,
	[productId] [int] NULL,
 CONSTRAINT [PK__Coupon__3213E83F5322616E] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DetailFeedback]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DetailFeedback](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[DeliveryOnTime] [int] NULL,
	[ExactSame] [int] NULL,
	[Communication] [int] NULL,
	[feedbackId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Dispute]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Dispute](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[orderId] [int] NULL,
	[raisedBy] [int] NULL,
	[description] [nvarchar](max) NULL,
	[status] [nvarchar](20) NULL,
	[resolution] [nvarchar](max) NULL,
	[submittedDate] [datetime] NULL,
	[solvedDate] [datetime] NULL,
	[comment] [nvarchar](500) NULL,
 CONSTRAINT [PK__Dispute__3213E83FC7A98713] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Feedback]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Feedback](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[sellerId] [int] NULL,
	[averageRating] [decimal](3, 2) NULL,
	[totalReviews] [int] NULL,
	[positiveRate] [decimal](5, 2) NULL,
	[OrdersId] [int] NULL,
	[comment] [nvarchar](max) NULL,
 CONSTRAINT [PK__Feedback__3213E83F7404CC3B] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Inventory]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Inventory](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[productId] [int] NULL,
	[quantity] [int] NULL,
	[lastUpdated] [datetime] NULL,
 CONSTRAINT [PK__Inventor__3213E83F48B36DEB] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Message]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Message](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[senderId] [int] NULL,
	[receiverId] [int] NULL,
	[content] [nvarchar](max) NULL,
	[timestamp] [datetime] NULL,
	[ProductId] [int] NULL,
 CONSTRAINT [PK__Message__3213E83F7144CC1A] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderItem]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderItem](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[orderId] [int] NULL,
	[productId] [int] NULL,
	[quantity] [int] NULL,
	[unitPrice] [decimal](10, 2) NULL,
 CONSTRAINT [PK__OrderIte__3213E83F7EC2808B] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderTable]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderTable](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[buyerId] [int] NULL,
	[addressId] [int] NULL,
	[orderDate] [datetime] NULL,
	[totalPrice] [decimal](10, 2) NULL,
	[status] [nvarchar](20) NULL,
	[isCommented] [bit] NULL,
 CONSTRAINT [PK__OrderTab__3213E83F282EE92F] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Payment]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payment](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[orderId] [int] NULL,
	[userId] [int] NULL,
	[amount] [decimal](10, 2) NULL,
	[method] [nvarchar](50) NULL,
	[status] [nvarchar](20) NULL,
	[paidAt] [datetime] NULL,
 CONSTRAINT [PK__Payment__3213E83FC485D7F6] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Product]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Product](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[title] [nvarchar](255) NULL,
	[description] [nvarchar](max) NULL,
	[price] [decimal](10, 2) NULL,
	[images] [nvarchar](max) NULL,
	[categoryId] [int] NULL,
	[sellerId] [int] NULL,
	[isAuction] [bit] NULL,
	[auctionEndTime] [datetime] NULL,
 CONSTRAINT [PK__Product__3213E83FEFD48A5F] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReturnRequest]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReturnRequest](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[orderId] [int] NULL,
	[userId] [int] NULL,
	[reason] [nvarchar](max) NULL,
	[status] [nvarchar](20) NULL,
	[createdAt] [datetime] NULL,
 CONSTRAINT [PK__ReturnRe__3213E83FC9454B62] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Review]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Review](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[productId] [int] NULL,
	[reviewerId] [int] NULL,
	[rating] [int] NULL,
	[comment] [nvarchar](max) NULL,
	[createdAt] [datetime] NULL,
 CONSTRAINT [PK__Review__3213E83F0BF301EC] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SellerToBuyerReview]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SellerToBuyerReview](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SellerId] [int] NOT NULL,
	[SellerName] [nvarchar](100) NOT NULL,
	[BuyerId] [int] NOT NULL,
	[BuyerName] [nvarchar](100) NOT NULL,
	[Comment] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[OrderId] [int] NOT NULL,
	[ProductName] [nvarchar](255) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShippingInfo]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShippingInfo](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[orderId] [int] NULL,
	[carrier] [nvarchar](100) NULL,
	[trackingNumber] [nvarchar](100) NULL,
	[status] [nvarchar](50) NULL,
	[estimatedArrival] [datetime] NULL,
 CONSTRAINT [PK__Shipping__3213E83FF99F0D65] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Store]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Store](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[sellerId] [int] NULL,
	[storeName] [nvarchar](100) NULL,
	[description] [nvarchar](max) NULL,
	[bannerImageURL] [nvarchar](max) NULL,
 CONSTRAINT [PK__Store__3213E83FF102737C] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: 11/15/2025 8:41:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[username] [nvarchar](100) NULL,
	[email] [nvarchar](100) NULL,
	[password] [nvarchar](255) NULL,
	[role] [nvarchar](20) NULL,
	[avatarURL] [nvarchar](max) NULL,
 CONSTRAINT [PK__User__3213E83F0238F3F7] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250725100941_initials', N'8.0.10')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250725174944_update_remove_relationShip_OrderFeedback', N'8.0.10')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250725180030_update_add_relationShip_MessageProduct', N'8.0.10')
GO
SET IDENTITY_INSERT [dbo].[Address] ON 

INSERT [dbo].[Address] ([id], [userId], [fullName], [phone], [street], [city], [state], [country], [isDefault]) VALUES (9, 1, NULL, NULL, N'123 Main St', N'Hanoi', N'HN', N'Vietnam', 1)
INSERT [dbo].[Address] ([id], [userId], [fullName], [phone], [street], [city], [state], [country], [isDefault]) VALUES (10, 1, NULL, NULL, N'456 Side St', N'Hanoi', N'HN', N'Vietnam', 0)
INSERT [dbo].[Address] ([id], [userId], [fullName], [phone], [street], [city], [state], [country], [isDefault]) VALUES (11, 2, NULL, NULL, N'789 Lake Rd', N'HCMC', N'HCM', N'Vietnam', 1)
INSERT [dbo].[Address] ([id], [userId], [fullName], [phone], [street], [city], [state], [country], [isDefault]) VALUES (12, 5, NULL, NULL, N'321 Hill St', N'Danang', N'DN', N'Vietnam', 1)
INSERT [dbo].[Address] ([id], [userId], [fullName], [phone], [street], [city], [state], [country], [isDefault]) VALUES (13, 4, NULL, NULL, N'654 River St', N'Can Tho', N'CT', N'Vietnam', 1)
SET IDENTITY_INSERT [dbo].[Address] OFF
GO
SET IDENTITY_INSERT [dbo].[Category] ON 

INSERT [dbo].[Category] ([id], [name]) VALUES (1, N'Electronics')
INSERT [dbo].[Category] ([id], [name]) VALUES (2, N'Clothing')
INSERT [dbo].[Category] ([id], [name]) VALUES (3, N'Books')
INSERT [dbo].[Category] ([id], [name]) VALUES (4, N'Home')
INSERT [dbo].[Category] ([id], [name]) VALUES (5, N'Toys')
SET IDENTITY_INSERT [dbo].[Category] OFF
GO
SET IDENTITY_INSERT [dbo].[Coupon] ON 

INSERT [dbo].[Coupon] ([id], [code], [discountPercent], [startDate], [endDate], [maxUsage], [productId]) VALUES (1, N'SALE10', CAST(10.00 AS Decimal(5, 2)), CAST(N'2025-12-31T00:00:00.000' AS DateTime), CAST(N'2026-12-31T00:00:00.000' AS DateTime), 10, 1)
INSERT [dbo].[Coupon] ([id], [code], [discountPercent], [startDate], [endDate], [maxUsage], [productId]) VALUES (2, N'TSHIRT5', CAST(5.00 AS Decimal(5, 2)), CAST(N'2025-06-30T00:00:00.000' AS DateTime), CAST(N'2026-12-31T00:00:00.000' AS DateTime), 10, 2)
INSERT [dbo].[Coupon] ([id], [code], [discountPercent], [startDate], [endDate], [maxUsage], [productId]) VALUES (3, N'BOOK20', CAST(20.00 AS Decimal(5, 2)), CAST(N'2025-07-01T00:00:00.000' AS DateTime), CAST(N'2026-12-31T00:00:00.000' AS DateTime), 10, 3)
INSERT [dbo].[Coupon] ([id], [code], [discountPercent], [startDate], [endDate], [maxUsage], [productId]) VALUES (4, N'HOME15', CAST(15.00 AS Decimal(5, 2)), CAST(N'2025-08-15T00:00:00.000' AS DateTime), CAST(N'2026-12-31T00:00:00.000' AS DateTime), 10, 4)
INSERT [dbo].[Coupon] ([id], [code], [discountPercent], [startDate], [endDate], [maxUsage], [productId]) VALUES (5, N'TOY10', CAST(10.00 AS Decimal(5, 2)), CAST(N'2025-09-30T00:00:00.000' AS DateTime), CAST(N'2026-12-31T00:00:00.000' AS DateTime), 10, 5)
SET IDENTITY_INSERT [dbo].[Coupon] OFF
GO
SET IDENTITY_INSERT [dbo].[DetailFeedback] ON 

INSERT [dbo].[DetailFeedback] ([id], [DeliveryOnTime], [ExactSame], [Communication], [feedbackId]) VALUES (1, 3, 5, 4, 8)
INSERT [dbo].[DetailFeedback] ([id], [DeliveryOnTime], [ExactSame], [Communication], [feedbackId]) VALUES (2, 4, 3, 2, 9)
INSERT [dbo].[DetailFeedback] ([id], [DeliveryOnTime], [ExactSame], [Communication], [feedbackId]) VALUES (3, 5, 5, 4, 10)
INSERT [dbo].[DetailFeedback] ([id], [DeliveryOnTime], [ExactSame], [Communication], [feedbackId]) VALUES (4, 5, 2, 4, 11)
SET IDENTITY_INSERT [dbo].[DetailFeedback] OFF
GO
SET IDENTITY_INSERT [dbo].[Dispute] ON 

INSERT [dbo].[Dispute] ([id], [orderId], [raisedBy], [description], [status], [resolution], [submittedDate], [solvedDate], [comment]) VALUES (1, 2, 1, N'Yêu cầu giải quyết: refund
        Lý do chính: not-received
        Chi tiết lý do: Không có
        ---
        Nội dung từ người dùng:
        Hello my fen, kkkkkkkkkkkkkkkkkkkkkk', N'4', N'Hoàn tiền toàn bộ', CAST(N'2025-11-12T15:11:43.123' AS DateTime), CAST(N'2025-11-15T08:01:14.120' AS DateTime), N'comment')
INSERT [dbo].[Dispute] ([id], [orderId], [raisedBy], [description], [status], [resolution], [submittedDate], [solvedDate], [comment]) VALUES (2, 3, 1, N'Yêu cầu giải quyết: refund
        Lý do chính: not-received
        Chi tiết lý do: Không có
        ---
        Nội dung từ người dùng:
        sdsdadddddddddddddddddddddddddddddddddddddddddd', N'2', N'Chấp nhận trả hàng', CAST(N'2025-11-13T16:26:20.667' AS DateTime), CAST(N'2025-11-14T21:53:15.360' AS DateTime), N'ok bro')
INSERT [dbo].[Dispute] ([id], [orderId], [raisedBy], [description], [status], [resolution], [submittedDate], [solvedDate], [comment]) VALUES (3, 11, 1, N'Yêu cầu giải quyết: refund
        Lý do chính: not-received
        Chi tiết lý do: Không có
        ---
        Nội dung từ người dùng:
        djaslkdjsalkjdljaskdjlaskdjasldkasjldjsakdjasldjksaldjla', N'1', NULL, CAST(N'2025-11-14T14:49:43.727' AS DateTime), NULL, NULL)
INSERT [dbo].[Dispute] ([id], [orderId], [raisedBy], [description], [status], [resolution], [submittedDate], [solvedDate], [comment]) VALUES (4, 12, 1, N'Yêu cầu giải quyết: refund
        Lý do chính: not-received
        Chi tiết lý do: Không có
        ---
        Nội dung từ người dùng:
        dlfkldsf;ldkf;ldskf;dkslfldfdfsdfsfsdfdsfdsfdsfdsffds', N'2', N'Hoàn tiền một phần', CAST(N'2025-11-15T00:55:33.873' AS DateTime), CAST(N'2025-11-15T07:58:21.827' AS DateTime), N'comment')
SET IDENTITY_INSERT [dbo].[Dispute] OFF
GO
SET IDENTITY_INSERT [dbo].[Feedback] ON 

INSERT [dbo].[Feedback] ([id], [sellerId], [averageRating], [totalReviews], [positiveRate], [OrdersId], [comment]) VALUES (4, 2, CAST(4.00 AS Decimal(3, 2)), 1, CAST(1.00 AS Decimal(5, 2)), 2, N'Good buyer, communicate well, dedicated')
INSERT [dbo].[Feedback] ([id], [sellerId], [averageRating], [totalReviews], [positiveRate], [OrdersId], [comment]) VALUES (6, 1, CAST(3.00 AS Decimal(3, 2)), 1, CAST(1.00 AS Decimal(5, 2)), 4, N'Good buyer, communicate well, dedicated')
INSERT [dbo].[Feedback] ([id], [sellerId], [averageRating], [totalReviews], [positiveRate], [OrdersId], [comment]) VALUES (7, 5, CAST(5.00 AS Decimal(3, 2)), 1, CAST(1.00 AS Decimal(5, 2)), 5, N'Good buyer, communicate well, dedicated')
INSERT [dbo].[Feedback] ([id], [sellerId], [averageRating], [totalReviews], [positiveRate], [OrdersId], [comment]) VALUES (8, 5, CAST(4.00 AS Decimal(3, 2)), 1, CAST(1.00 AS Decimal(5, 2)), 3, N'Good buyer, communicate well, dedicated')
INSERT [dbo].[Feedback] ([id], [sellerId], [averageRating], [totalReviews], [positiveRate], [OrdersId], [comment]) VALUES (9, 5, CAST(4.00 AS Decimal(3, 2)), 1, CAST(-1.00 AS Decimal(5, 2)), 7, N'Not so good buyer')
INSERT [dbo].[Feedback] ([id], [sellerId], [averageRating], [totalReviews], [positiveRate], [OrdersId], [comment]) VALUES (10, 5, CAST(5.00 AS Decimal(3, 2)), 1, CAST(0.00 AS Decimal(5, 2)), 8, N'It is like what describe')
INSERT [dbo].[Feedback] ([id], [sellerId], [averageRating], [totalReviews], [positiveRate], [OrdersId], [comment]) VALUES (11, 2, CAST(3.00 AS Decimal(3, 2)), 1, CAST(1.00 AS Decimal(5, 2)), 12, N'ok')
SET IDENTITY_INSERT [dbo].[Feedback] OFF
GO
SET IDENTITY_INSERT [dbo].[Message] ON 

INSERT [dbo].[Message] ([id], [senderId], [receiverId], [content], [timestamp], [ProductId]) VALUES (32, 1, 5, N'hello', CAST(N'2025-11-15T01:03:39.493' AS DateTime), NULL)
INSERT [dbo].[Message] ([id], [senderId], [receiverId], [content], [timestamp], [ProductId]) VALUES (33, 5, 1, N'hi', CAST(N'2025-11-15T01:03:47.677' AS DateTime), NULL)
INSERT [dbo].[Message] ([id], [senderId], [receiverId], [content], [timestamp], [ProductId]) VALUES (34, 1, 5, N'hi cin ádf', CAST(N'2025-11-15T01:03:59.823' AS DateTime), NULL)
INSERT [dbo].[Message] ([id], [senderId], [receiverId], [content], [timestamp], [ProductId]) VALUES (35, 5, 1, N'chat', CAST(N'2025-11-15T01:04:08.067' AS DateTime), NULL)
SET IDENTITY_INSERT [dbo].[Message] OFF
GO
SET IDENTITY_INSERT [dbo].[OrderItem] ON 

INSERT [dbo].[OrderItem] ([id], [orderId], [productId], [quantity], [unitPrice]) VALUES (2, 2, 1, 1, CAST(1000.00 AS Decimal(10, 2)))
INSERT [dbo].[OrderItem] ([id], [orderId], [productId], [quantity], [unitPrice]) VALUES (3, 4, 2, 1, CAST(20.00 AS Decimal(10, 2)))
INSERT [dbo].[OrderItem] ([id], [orderId], [productId], [quantity], [unitPrice]) VALUES (4, 2, 3, 2, CAST(15.00 AS Decimal(10, 2)))
INSERT [dbo].[OrderItem] ([id], [orderId], [productId], [quantity], [unitPrice]) VALUES (5, 3, 4, 1, CAST(200.00 AS Decimal(10, 2)))
INSERT [dbo].[OrderItem] ([id], [orderId], [productId], [quantity], [unitPrice]) VALUES (6, 4, 1, 1, CAST(1000.00 AS Decimal(10, 2)))
INSERT [dbo].[OrderItem] ([id], [orderId], [productId], [quantity], [unitPrice]) VALUES (7, 5, 5, 1, CAST(50.00 AS Decimal(10, 2)))
INSERT [dbo].[OrderItem] ([id], [orderId], [productId], [quantity], [unitPrice]) VALUES (8, 7, 4, 2, CAST(30.00 AS Decimal(10, 2)))
INSERT [dbo].[OrderItem] ([id], [orderId], [productId], [quantity], [unitPrice]) VALUES (9, 8, 3, 4, CAST(40.00 AS Decimal(10, 2)))
INSERT [dbo].[OrderItem] ([id], [orderId], [productId], [quantity], [unitPrice]) VALUES (10, 9, 3, 1, CAST(15.00 AS Decimal(10, 2)))
INSERT [dbo].[OrderItem] ([id], [orderId], [productId], [quantity], [unitPrice]) VALUES (11, 10, 2, 1, CAST(20.00 AS Decimal(10, 2)))
INSERT [dbo].[OrderItem] ([id], [orderId], [productId], [quantity], [unitPrice]) VALUES (12, 11, 5, 1, CAST(50.00 AS Decimal(10, 2)))
INSERT [dbo].[OrderItem] ([id], [orderId], [productId], [quantity], [unitPrice]) VALUES (13, 12, 5, 1, CAST(50.00 AS Decimal(10, 2)))
SET IDENTITY_INSERT [dbo].[OrderItem] OFF
GO
SET IDENTITY_INSERT [dbo].[OrderTable] ON 

INSERT [dbo].[OrderTable] ([id], [buyerId], [addressId], [orderDate], [totalPrice], [status], [isCommented]) VALUES (2, 1, 9, CAST(N'2025-09-15T14:32:43.317' AS DateTime), CAST(1020.00 AS Decimal(10, 2)), N'Completed', NULL)
INSERT [dbo].[OrderTable] ([id], [buyerId], [addressId], [orderDate], [totalPrice], [status], [isCommented]) VALUES (3, 1, 10, CAST(N'2025-09-15T14:32:43.317' AS DateTime), CAST(35.00 AS Decimal(10, 2)), N'Completed', 1)
INSERT [dbo].[OrderTable] ([id], [buyerId], [addressId], [orderDate], [totalPrice], [status], [isCommented]) VALUES (4, 4, 11, CAST(N'2025-09-15T14:32:43.317' AS DateTime), CAST(200.00 AS Decimal(10, 2)), N'Completed', NULL)
INSERT [dbo].[OrderTable] ([id], [buyerId], [addressId], [orderDate], [totalPrice], [status], [isCommented]) VALUES (5, 4, 12, CAST(N'2025-09-15T14:32:43.317' AS DateTime), CAST(1000.00 AS Decimal(10, 2)), N'Completed', NULL)
INSERT [dbo].[OrderTable] ([id], [buyerId], [addressId], [orderDate], [totalPrice], [status], [isCommented]) VALUES (7, 1, 13, CAST(N'2025-09-15T14:32:43.317' AS DateTime), CAST(60.00 AS Decimal(10, 2)), N'Completed', NULL)
INSERT [dbo].[OrderTable] ([id], [buyerId], [addressId], [orderDate], [totalPrice], [status], [isCommented]) VALUES (8, 4, 12, CAST(N'2025-09-15T14:32:43.317' AS DateTime), CAST(30.00 AS Decimal(10, 2)), N'Completed', NULL)
INSERT [dbo].[OrderTable] ([id], [buyerId], [addressId], [orderDate], [totalPrice], [status], [isCommented]) VALUES (9, 1, NULL, CAST(N'2025-11-14T14:21:43.970' AS DateTime), CAST(15.00 AS Decimal(10, 2)), N'Completed', NULL)
INSERT [dbo].[OrderTable] ([id], [buyerId], [addressId], [orderDate], [totalPrice], [status], [isCommented]) VALUES (10, 1, NULL, CAST(N'2025-11-14T14:24:37.230' AS DateTime), CAST(20.00 AS Decimal(10, 2)), N'Completed', NULL)
INSERT [dbo].[OrderTable] ([id], [buyerId], [addressId], [orderDate], [totalPrice], [status], [isCommented]) VALUES (11, 1, NULL, CAST(N'2025-11-14T14:27:42.077' AS DateTime), CAST(50.00 AS Decimal(10, 2)), N'Completed', NULL)
INSERT [dbo].[OrderTable] ([id], [buyerId], [addressId], [orderDate], [totalPrice], [status], [isCommented]) VALUES (12, 1, NULL, CAST(N'2025-11-15T00:47:59.153' AS DateTime), CAST(50.00 AS Decimal(10, 2)), N'Completed', 1)
SET IDENTITY_INSERT [dbo].[OrderTable] OFF
GO
SET IDENTITY_INSERT [dbo].[Product] ON 

INSERT [dbo].[Product] ([id], [title], [description], [price], [images], [categoryId], [sellerId], [isAuction], [auctionEndTime]) VALUES (1, N'iPhone 14', N'iPhone 14', CAST(1000.00 AS Decimal(10, 2)), NULL, 1, 2, NULL, NULL)
INSERT [dbo].[Product] ([id], [title], [description], [price], [images], [categoryId], [sellerId], [isAuction], [auctionEndTime]) VALUES (2, N'Men T-Shirt', N'Men T-Shirt', CAST(20.00 AS Decimal(10, 2)), NULL, 2, 2, NULL, NULL)
INSERT [dbo].[Product] ([id], [title], [description], [price], [images], [categoryId], [sellerId], [isAuction], [auctionEndTime]) VALUES (3, N'Harry Potter', N'Harry Potter', CAST(15.00 AS Decimal(10, 2)), NULL, 3, 5, NULL, NULL)
INSERT [dbo].[Product] ([id], [title], [description], [price], [images], [categoryId], [sellerId], [isAuction], [auctionEndTime]) VALUES (4, N'Vacuum Cleaner', N'Vacuum Cleaner', CAST(200.00 AS Decimal(10, 2)), NULL, 4, 5, NULL, NULL)
INSERT [dbo].[Product] ([id], [title], [description], [price], [images], [categoryId], [sellerId], [isAuction], [auctionEndTime]) VALUES (5, N'Lego Set', N'Lego Set', CAST(50.00 AS Decimal(10, 2)), NULL, 5, 2, NULL, NULL)
SET IDENTITY_INSERT [dbo].[Product] OFF
GO
SET IDENTITY_INSERT [dbo].[ReturnRequest] ON 

INSERT [dbo].[ReturnRequest] ([id], [orderId], [userId], [reason], [status], [createdAt]) VALUES (2, 2, 1, N'Item defective', N'Pending', CAST(N'2025-09-15T14:38:38.987' AS DateTime))
INSERT [dbo].[ReturnRequest] ([id], [orderId], [userId], [reason], [status], [createdAt]) VALUES (3, 2, 1, N'Wrong size', N'Approved', CAST(N'2025-09-15T14:38:38.987' AS DateTime))
INSERT [dbo].[ReturnRequest] ([id], [orderId], [userId], [reason], [status], [createdAt]) VALUES (4, 3, 4, N'Changed mind', N'Rejected', CAST(N'2025-09-15T14:38:38.987' AS DateTime))
INSERT [dbo].[ReturnRequest] ([id], [orderId], [userId], [reason], [status], [createdAt]) VALUES (5, 4, 4, N'Product not as described', N'Pending', CAST(N'2025-09-15T14:38:38.987' AS DateTime))
INSERT [dbo].[ReturnRequest] ([id], [orderId], [userId], [reason], [status], [createdAt]) VALUES (6, 5, 1, N'Received late', N'Pending', CAST(N'2025-09-15T14:38:38.987' AS DateTime))
SET IDENTITY_INSERT [dbo].[ReturnRequest] OFF
GO
SET IDENTITY_INSERT [dbo].[Review] ON 

INSERT [dbo].[Review] ([id], [productId], [reviewerId], [rating], [comment], [createdAt]) VALUES (1, 1, 1, 5, N'Excellent phone!', CAST(N'2025-09-15T14:34:33.513' AS DateTime))
INSERT [dbo].[Review] ([id], [productId], [reviewerId], [rating], [comment], [createdAt]) VALUES (2, 2, 4, 4, N'Good quality T-Shirt', CAST(N'2025-09-15T14:34:33.513' AS DateTime))
INSERT [dbo].[Review] ([id], [productId], [reviewerId], [rating], [comment], [createdAt]) VALUES (3, 3, 1, 5, N'Great book for kids', CAST(N'2025-09-15T14:34:33.513' AS DateTime))
INSERT [dbo].[Review] ([id], [productId], [reviewerId], [rating], [comment], [createdAt]) VALUES (4, 4, 4, 3, N'Decent vacuum cleaner', CAST(N'2025-09-15T14:34:33.513' AS DateTime))
INSERT [dbo].[Review] ([id], [productId], [reviewerId], [rating], [comment], [createdAt]) VALUES (5, 5, 1, 5, N'My son loves it', CAST(N'2025-09-15T14:34:33.513' AS DateTime))
SET IDENTITY_INSERT [dbo].[Review] OFF
GO
SET IDENTITY_INSERT [dbo].[SellerToBuyerReview] ON 

INSERT [dbo].[SellerToBuyerReview] ([Id], [SellerId], [SellerName], [BuyerId], [BuyerName], [Comment], [CreatedAt], [OrderId], [ProductName]) VALUES (1, 5, N'user2@example.com', 1, N'user1', N'Giao dịch nhanh gọn, người mua tuyệt vời!', CAST(N'2025-11-14T22:27:30.0268145' AS DateTime2), 101, N'Sản phẩm A')
INSERT [dbo].[SellerToBuyerReview] ([Id], [SellerId], [SellerName], [BuyerId], [BuyerName], [Comment], [CreatedAt], [OrderId], [ProductName]) VALUES (2, 8, N'charlie', 1, N'user1', N'Người mua trả giá nhiều lần và thanh toán chậm.', CAST(N'2025-11-14T22:27:30.0268145' AS DateTime2), 102, N'Sản phẩm B')
INSERT [dbo].[SellerToBuyerReview] ([Id], [SellerId], [SellerName], [BuyerId], [BuyerName], [Comment], [CreatedAt], [OrderId], [ProductName]) VALUES (3, 10, N'eva', 1, N'user1', N'Giao dịch diễn ra bình thường.', CAST(N'2025-11-14T22:27:30.0268145' AS DateTime2), 103, N'Sản phẩm C')
INSERT [dbo].[SellerToBuyerReview] ([Id], [SellerId], [SellerName], [BuyerId], [BuyerName], [Comment], [CreatedAt], [OrderId], [ProductName]) VALUES (4, 5, N'user2@example.com', 1, N'user1', N'Great communication. A pleasure to do business with.', CAST(N'2025-11-14T17:18:38.2607645' AS DateTime2), 9, N'Harry Potter')
INSERT [dbo].[SellerToBuyerReview] ([Id], [SellerId], [SellerName], [BuyerId], [BuyerName], [Comment], [CreatedAt], [OrderId], [ProductName]) VALUES (5, 5, N'user2@example.com', 4, N'user1@example.com', N'Thank you for an easy, pleasant transaction. Excellent buyer. A+++++.', CAST(N'2025-11-15T01:36:43.9379682' AS DateTime2), 8, N'Harry Potter')
SET IDENTITY_INSERT [dbo].[SellerToBuyerReview] OFF
GO
SET IDENTITY_INSERT [dbo].[Store] ON 

INSERT [dbo].[Store] ([id], [sellerId], [storeName], [description], [bannerImageURL]) VALUES (2, 2, N'Charlie Tech', N'Electronics and gadgets', NULL)
INSERT [dbo].[Store] ([id], [sellerId], [storeName], [description], [bannerImageURL]) VALUES (3, 5, N'Eva Home', N'Books and Home appliances', NULL)
INSERT [dbo].[Store] ([id], [sellerId], [storeName], [description], [bannerImageURL]) VALUES (4, 2, N'Charlie Toys', N'Toys collection', NULL)
INSERT [dbo].[Store] ([id], [sellerId], [storeName], [description], [bannerImageURL]) VALUES (5, 5, N'Eva Fashion', N'Clothing store', NULL)
INSERT [dbo].[Store] ([id], [sellerId], [storeName], [description], [bannerImageURL]) VALUES (6, 1, N'Charlie Mix', N'All kinds of products', NULL)
SET IDENTITY_INSERT [dbo].[Store] OFF
GO
SET IDENTITY_INSERT [dbo].[User] ON 

INSERT [dbo].[User] ([id], [username], [email], [password], [role], [avatarURL]) VALUES (1, N'user1', N'nguyenvana@example.com', N'123456', N'buyer', N'')
INSERT [dbo].[User] ([id], [username], [email], [password], [role], [avatarURL]) VALUES (2, N'user2', N'tranvanb@example.com', N'654321', N'seller', N'')
INSERT [dbo].[User] ([id], [username], [email], [password], [role], [avatarURL]) VALUES (4, N'user1@example.com', N'user1@example.com', N'123', N'buyer', N'')
INSERT [dbo].[User] ([id], [username], [email], [password], [role], [avatarURL]) VALUES (5, N'user2@example.com', N'user2@example.com', N'123', N'seller', N'')
INSERT [dbo].[User] ([id], [username], [email], [password], [role], [avatarURL]) VALUES (6, N'alice', N'alice@example.com', N'123456', N'buyer', N'https://example.com/avatar/alice.png')
INSERT [dbo].[User] ([id], [username], [email], [password], [role], [avatarURL]) VALUES (7, N'bob', N'bob@example.com', N'123456', N'supporter', N'https://example.com/avatar/bob.png')
INSERT [dbo].[User] ([id], [username], [email], [password], [role], [avatarURL]) VALUES (8, N'charlie', N'charlie@example.com', N'123456', N'seller', N'https://example.com/avatar/charlie.png')
INSERT [dbo].[User] ([id], [username], [email], [password], [role], [avatarURL]) VALUES (9, N'david', N'david@example.com', N'123456', N'buyer', N'https://example.com/avatar/david.png')
INSERT [dbo].[User] ([id], [username], [email], [password], [role], [avatarURL]) VALUES (10, N'eva', N'eva@example.com', N'123456', N'seller', N'https://example.com/avatar/eva.png')
INSERT [dbo].[User] ([id], [username], [email], [password], [role], [avatarURL]) VALUES (11, N'admin', N'admin@gmail.com', N'123', N'admin', NULL)
SET IDENTITY_INSERT [dbo].[User] OFF
GO
/****** Object:  Index [IX_Address_userId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_Address_userId] ON [dbo].[Address]
(
	[userId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Bid_bidderId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_Bid_bidderId] ON [dbo].[Bid]
(
	[bidderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Bid_productId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_Bid_productId] ON [dbo].[Bid]
(
	[productId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Coupon_productId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_Coupon_productId] ON [dbo].[Coupon]
(
	[productId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Dispute_orderId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_Dispute_orderId] ON [dbo].[Dispute]
(
	[orderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Dispute_raisedBy]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_Dispute_raisedBy] ON [dbo].[Dispute]
(
	[raisedBy] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Feedback_OrdersId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_Feedback_OrdersId] ON [dbo].[Feedback]
(
	[OrdersId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Feedback_sellerId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_Feedback_sellerId] ON [dbo].[Feedback]
(
	[sellerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Inventory_productId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_Inventory_productId] ON [dbo].[Inventory]
(
	[productId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Message_ProductId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_Message_ProductId] ON [dbo].[Message]
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderItem_orderId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_OrderItem_orderId] ON [dbo].[OrderItem]
(
	[orderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderItem_productId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_OrderItem_productId] ON [dbo].[OrderItem]
(
	[productId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderTable_addressId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_OrderTable_addressId] ON [dbo].[OrderTable]
(
	[addressId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderTable_buyerId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_OrderTable_buyerId] ON [dbo].[OrderTable]
(
	[buyerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payment_orderId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_Payment_orderId] ON [dbo].[Payment]
(
	[orderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payment_userId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_Payment_userId] ON [dbo].[Payment]
(
	[userId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Product_categoryId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_Product_categoryId] ON [dbo].[Product]
(
	[categoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Product_sellerId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_Product_sellerId] ON [dbo].[Product]
(
	[sellerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ReturnRequest_orderId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_ReturnRequest_orderId] ON [dbo].[ReturnRequest]
(
	[orderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ReturnRequest_userId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_ReturnRequest_userId] ON [dbo].[ReturnRequest]
(
	[userId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Review_productId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_Review_productId] ON [dbo].[Review]
(
	[productId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Review_reviewerId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_Review_reviewerId] ON [dbo].[Review]
(
	[reviewerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ShippingInfo_orderId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_ShippingInfo_orderId] ON [dbo].[ShippingInfo]
(
	[orderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Store_sellerId]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE NONCLUSTERED INDEX [IX_Store_sellerId] ON [dbo].[Store]
(
	[sellerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__User__AB6E6164741FE703]    Script Date: 11/15/2025 8:41:55 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UQ__User__AB6E6164741FE703] ON [dbo].[User]
(
	[email] ASC
)
WHERE ([email] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[SellerToBuyerReview] ADD  DEFAULT (sysdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Address]  WITH CHECK ADD  CONSTRAINT [FK__Address__userId__3A81B327] FOREIGN KEY([userId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Address] CHECK CONSTRAINT [FK__Address__userId__3A81B327]
GO
ALTER TABLE [dbo].[Bid]  WITH CHECK ADD  CONSTRAINT [FK__Bid__bidderId__5629CD9C] FOREIGN KEY([bidderId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Bid] CHECK CONSTRAINT [FK__Bid__bidderId__5629CD9C]
GO
ALTER TABLE [dbo].[Bid]  WITH CHECK ADD  CONSTRAINT [FK__Bid__productId__5535A963] FOREIGN KEY([productId])
REFERENCES [dbo].[Product] ([id])
GO
ALTER TABLE [dbo].[Bid] CHECK CONSTRAINT [FK__Bid__productId__5535A963]
GO
ALTER TABLE [dbo].[Coupon]  WITH CHECK ADD  CONSTRAINT [FK__Coupon__productI__60A75C0F] FOREIGN KEY([productId])
REFERENCES [dbo].[Product] ([id])
GO
ALTER TABLE [dbo].[Coupon] CHECK CONSTRAINT [FK__Coupon__productI__60A75C0F]
GO
ALTER TABLE [dbo].[DetailFeedback]  WITH CHECK ADD  CONSTRAINT [FK_DetailFeedback_Feedback] FOREIGN KEY([feedbackId])
REFERENCES [dbo].[Feedback] ([id])
GO
ALTER TABLE [dbo].[DetailFeedback] CHECK CONSTRAINT [FK_DetailFeedback_Feedback]
GO
ALTER TABLE [dbo].[Dispute]  WITH CHECK ADD  CONSTRAINT [FK__Dispute__orderId__693CA210] FOREIGN KEY([orderId])
REFERENCES [dbo].[OrderTable] ([id])
GO
ALTER TABLE [dbo].[Dispute] CHECK CONSTRAINT [FK__Dispute__orderId__693CA210]
GO
ALTER TABLE [dbo].[Dispute]  WITH CHECK ADD  CONSTRAINT [FK__Dispute__raisedB__6A30C649] FOREIGN KEY([raisedBy])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Dispute] CHECK CONSTRAINT [FK__Dispute__raisedB__6A30C649]
GO
ALTER TABLE [dbo].[Feedback]  WITH CHECK ADD  CONSTRAINT [FK__Feedback__seller__66603565] FOREIGN KEY([sellerId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Feedback] CHECK CONSTRAINT [FK__Feedback__seller__66603565]
GO
ALTER TABLE [dbo].[Feedback]  WITH CHECK ADD  CONSTRAINT [FK_Feedback_OrderTable_OrdersId] FOREIGN KEY([OrdersId])
REFERENCES [dbo].[OrderTable] ([id])
GO
ALTER TABLE [dbo].[Feedback] CHECK CONSTRAINT [FK_Feedback_OrderTable_OrdersId]
GO
ALTER TABLE [dbo].[Inventory]  WITH CHECK ADD  CONSTRAINT [FK__Inventory__produ__6383C8BA] FOREIGN KEY([productId])
REFERENCES [dbo].[Product] ([id])
GO
ALTER TABLE [dbo].[Inventory] CHECK CONSTRAINT [FK__Inventory__produ__6383C8BA]
GO
ALTER TABLE [dbo].[Message]  WITH CHECK ADD  CONSTRAINT [FK_Message_Product_ProductId] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Product] ([id])
GO
ALTER TABLE [dbo].[Message] CHECK CONSTRAINT [FK_Message_Product_ProductId]
GO
ALTER TABLE [dbo].[OrderItem]  WITH CHECK ADD  CONSTRAINT [FK__OrderItem__order__46E78A0C] FOREIGN KEY([orderId])
REFERENCES [dbo].[OrderTable] ([id])
GO
ALTER TABLE [dbo].[OrderItem] CHECK CONSTRAINT [FK__OrderItem__order__46E78A0C]
GO
ALTER TABLE [dbo].[OrderItem]  WITH CHECK ADD  CONSTRAINT [FK__OrderItem__produ__47DBAE45] FOREIGN KEY([productId])
REFERENCES [dbo].[Product] ([id])
GO
ALTER TABLE [dbo].[OrderItem] CHECK CONSTRAINT [FK__OrderItem__produ__47DBAE45]
GO
ALTER TABLE [dbo].[OrderTable]  WITH CHECK ADD  CONSTRAINT [FK__OrderTabl__addre__440B1D61] FOREIGN KEY([addressId])
REFERENCES [dbo].[Address] ([id])
GO
ALTER TABLE [dbo].[OrderTable] CHECK CONSTRAINT [FK__OrderTabl__addre__440B1D61]
GO
ALTER TABLE [dbo].[OrderTable]  WITH CHECK ADD  CONSTRAINT [FK__OrderTabl__buyer__4316F928] FOREIGN KEY([buyerId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[OrderTable] CHECK CONSTRAINT [FK__OrderTabl__buyer__4316F928]
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD  CONSTRAINT [FK__Payment__orderId__4AB81AF0] FOREIGN KEY([orderId])
REFERENCES [dbo].[OrderTable] ([id])
GO
ALTER TABLE [dbo].[Payment] CHECK CONSTRAINT [FK__Payment__orderId__4AB81AF0]
GO
ALTER TABLE [dbo].[Payment]  WITH CHECK ADD  CONSTRAINT [FK__Payment__userId__4BAC3F29] FOREIGN KEY([userId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Payment] CHECK CONSTRAINT [FK__Payment__userId__4BAC3F29]
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD  CONSTRAINT [FK__Product__categor__3F466844] FOREIGN KEY([categoryId])
REFERENCES [dbo].[Category] ([id])
GO
ALTER TABLE [dbo].[Product] CHECK CONSTRAINT [FK__Product__categor__3F466844]
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD  CONSTRAINT [FK__Product__sellerI__403A8C7D] FOREIGN KEY([sellerId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Product] CHECK CONSTRAINT [FK__Product__sellerI__403A8C7D]
GO
ALTER TABLE [dbo].[ReturnRequest]  WITH CHECK ADD  CONSTRAINT [FK__ReturnReq__order__5165187F] FOREIGN KEY([orderId])
REFERENCES [dbo].[OrderTable] ([id])
GO
ALTER TABLE [dbo].[ReturnRequest] CHECK CONSTRAINT [FK__ReturnReq__order__5165187F]
GO
ALTER TABLE [dbo].[ReturnRequest]  WITH CHECK ADD  CONSTRAINT [FK__ReturnReq__userI__52593CB8] FOREIGN KEY([userId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[ReturnRequest] CHECK CONSTRAINT [FK__ReturnReq__userI__52593CB8]
GO
ALTER TABLE [dbo].[Review]  WITH CHECK ADD  CONSTRAINT [FK__Review__productI__59063A47] FOREIGN KEY([productId])
REFERENCES [dbo].[Product] ([id])
GO
ALTER TABLE [dbo].[Review] CHECK CONSTRAINT [FK__Review__productI__59063A47]
GO
ALTER TABLE [dbo].[Review]  WITH CHECK ADD  CONSTRAINT [FK__Review__reviewer__59FA5E80] FOREIGN KEY([reviewerId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Review] CHECK CONSTRAINT [FK__Review__reviewer__59FA5E80]
GO
ALTER TABLE [dbo].[ShippingInfo]  WITH CHECK ADD  CONSTRAINT [FK__ShippingI__order__4E88ABD4] FOREIGN KEY([orderId])
REFERENCES [dbo].[OrderTable] ([id])
GO
ALTER TABLE [dbo].[ShippingInfo] CHECK CONSTRAINT [FK__ShippingI__order__4E88ABD4]
GO
ALTER TABLE [dbo].[Store]  WITH CHECK ADD  CONSTRAINT [FK__Store__sellerId__6D0D32F4] FOREIGN KEY([sellerId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Store] CHECK CONSTRAINT [FK__Store__sellerId__6D0D32F4]
GO
USE [master]
GO
ALTER DATABASE [CloneEbayDB] SET  READ_WRITE 
GO
