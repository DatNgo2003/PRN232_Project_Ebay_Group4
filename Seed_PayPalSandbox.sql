/*
    PayPal Sandbox test data for CloneEbayDB

    Run with:
      sqlcmd -S .\SQLEXPRESS -U sa -P "123" -d CloneEbayDB -i .\Seed_PayPalSandbox.sql

    This script is idempotent. It does not insert fake orders or payments;
    the application must create a Pending PayPal order and capture it through
    the PayPal Sandbox API.
*/

USE [CloneEbayDB];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @BuyerUsername nvarchar(100) = N'paypal_sandbox_buyer';
    DECLARE @BuyerEmail nvarchar(100) = N'paypal.buyer@example.test';
    DECLARE @BuyerPassword nvarchar(255) = N'PayPal123!';
    DECLARE @BuyerId int;

    DECLARE @SellerUsername nvarchar(100) = N'paypal_sandbox_seller';
    DECLARE @SellerEmail nvarchar(100) = N'paypal.seller@example.test';
    DECLARE @SellerPassword nvarchar(255) = N'PayPal123!';
    DECLARE @SellerId int;

    DECLARE @CategoryId int;
    DECLARE @AddressId int;
    DECLARE @ProductId int;
    DECLARE @CouponId int;

    /* AuthService currently compares the stored password directly. */
    SELECT @BuyerId = id
    FROM [User]
    WHERE username = @BuyerUsername;

    IF @BuyerId IS NULL
    BEGIN
        INSERT INTO [User] (username, email, password, role, avatarURL)
        VALUES (@BuyerUsername, @BuyerEmail, @BuyerPassword, N'buyer', NULL);

        SET @BuyerId = CONVERT(int, SCOPE_IDENTITY());
    END;

    SELECT @SellerId = id
    FROM [User]
    WHERE username = @SellerUsername;

    IF @SellerId IS NULL
    BEGIN
        INSERT INTO [User] (username, email, password, role, avatarURL)
        VALUES (@SellerUsername, @SellerEmail, @SellerPassword, N'seller', NULL);

        SET @SellerId = CONVERT(int, SCOPE_IDENTITY());
    END;

    SELECT @CategoryId = id
    FROM Category
    WHERE name = N'PayPal Sandbox Test';

    IF @CategoryId IS NULL
    BEGIN
        INSERT INTO Category (name)
        VALUES (N'PayPal Sandbox Test');

        SET @CategoryId = CONVERT(int, SCOPE_IDENTITY());
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM Store
        WHERE sellerId = @SellerId
    )
    BEGIN
        INSERT INTO Store (sellerId, storeName, description, bannerImageURL)
        VALUES
        (
            @SellerId,
            N'PayPal Sandbox Test Store',
            N'Store used only for PayPal Sandbox checkout testing.',
            NULL
        );
    END;

    SELECT @AddressId = id
    FROM Address
    WHERE userId = @BuyerId
      AND street = N'PayPal Test Street 123';

    IF @AddressId IS NULL
    BEGIN
        INSERT INTO Address
        (
            userId,
            fullName,
            phone,
            street,
            city,
            state,
            country,
            isDefault
        )
        VALUES
        (
            @BuyerId,
            N'PayPal Sandbox Buyer',
            N'0900000000',
            N'PayPal Test Street 123',
            N'Hanoi',
            N'Ha Noi',
            N'Vietnam',
            1
        );

        SET @AddressId = CONVERT(int, SCOPE_IDENTITY());
    END;

    UPDATE Address
    SET isDefault = CASE WHEN id = @AddressId THEN 1 ELSE 0 END
    WHERE userId = @BuyerId;

    SELECT @ProductId = id
    FROM Product
    WHERE title = N'PayPal Sandbox Test Product';

    IF @ProductId IS NULL
    BEGIN
        INSERT INTO Product
        (
            title,
            description,
            price,
            images,
            categoryId,
            sellerId,
            isAuction,
            auctionEndTime
        )
        VALUES
        (
            N'PayPal Sandbox Test Product',
            N'Low-value product for testing PayPal Sandbox checkout in USD.',
            9.99,
            N'https://placehold.co/600x600/png?text=PayPal+Sandbox',
            @CategoryId,
            @SellerId,
            0,
            NULL
        );

        SET @ProductId = CONVERT(int, SCOPE_IDENTITY());
    END;
    ELSE
    BEGIN
        UPDATE Product
        SET price = 9.99,
            categoryId = @CategoryId,
            sellerId = @SellerId,
            isAuction = 0,
            description = N'Low-value product for testing PayPal Sandbox checkout in USD.',
            images = N'https://placehold.co/600x600/png?text=PayPal+Sandbox'
        WHERE id = @ProductId;
    END;

    IF EXISTS (SELECT 1 FROM Inventory WHERE productId = @ProductId)
    BEGIN
        UPDATE Inventory
        SET quantity = CASE WHEN ISNULL(quantity, 0) < 100 THEN 100 ELSE quantity END,
            lastUpdated = GETDATE()
        WHERE productId = @ProductId;
    END
    ELSE
    BEGIN
        INSERT INTO Inventory (productId, quantity, lastUpdated)
        VALUES (@ProductId, 100, GETDATE());
    END;

    SELECT @CouponId = id
    FROM Coupon
    WHERE code = N'PAYPAL10';

    IF @CouponId IS NULL
    BEGIN
        INSERT INTO Coupon
        (
            code,
            discountPercent,
            startDate,
            endDate,
            maxUsage,
            productId,
            usedCount
        )
        VALUES
        (
            N'PAYPAL10',
            10.00,
            DATEADD(day, -1, GETDATE()),
            DATEADD(day, 30, GETDATE()),
            100,
            @ProductId,
            0
        );

        SET @CouponId = CONVERT(int, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE Coupon
        SET discountPercent = 10.00,
            startDate = DATEADD(day, -1, GETDATE()),
            endDate = DATEADD(day, 30, GETDATE()),
            maxUsage = 100,
            productId = @ProductId
        WHERE id = @CouponId;
    END;

    COMMIT TRANSACTION;

    SELECT
        N'PayPal Sandbox seed completed' AS Result,
        @BuyerUsername AS BuyerUsername,
        @BuyerPassword AS BuyerPassword,
        @BuyerId AS BuyerId,
        @AddressId AS AddressId,
        @ProductId AS ProductId,
        N'PAYPAL10' AS OptionalCouponCode,
        @SellerUsername AS SellerUsername,
        @SellerId AS SellerId;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
