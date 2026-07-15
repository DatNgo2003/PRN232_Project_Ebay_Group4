/*
    Account and address for testing order/payment email notifications.

    Run with:
      sqlcmd -S .\SQLEXPRESS -U sa -P "123" -d CloneEbayDB -i .\Seed_EmailTest.sql
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

    DECLARE @Username nvarchar(100) = N'email_test_buyer';
    DECLARE @Email nvarchar(100) = N'ngominhson2@gmail.com';
    DECLARE @Password nvarchar(255) = N'EmailTest123!';
    DECLARE @UserId int;
    DECLARE @AddressId int;

    SELECT @UserId = id
    FROM [User]
    WHERE username = @Username OR email = @Email;

    IF @UserId IS NULL
    BEGIN
        INSERT INTO [User] (username, email, password, role, avatarURL)
        VALUES (@Username, @Email, @Password, N'buyer', NULL);

        SET @UserId = CONVERT(int, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE [User]
        SET username = @Username,
            email = @Email,
            password = @Password,
            role = N'buyer'
        WHERE id = @UserId;
    END;

    SELECT @AddressId = id
    FROM Address
    WHERE userId = @UserId
      AND street = N'Email Test Street 456';

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
            @UserId,
            N'Email Test Buyer',
            N'0912345678',
            N'Email Test Street 456',
            N'Hanoi',
            N'Ha Noi',
            N'Vietnam',
            1
        );

        SET @AddressId = CONVERT(int, SCOPE_IDENTITY());
    END;

    UPDATE Address
    SET isDefault = CASE WHEN id = @AddressId THEN 1 ELSE 0 END
    WHERE userId = @UserId;

    COMMIT TRANSACTION;

    SELECT
        N'Email test account seeded' AS Result,
        @Username AS Username,
        @Email AS Email,
        @Password AS Password,
        @UserId AS UserId,
        @AddressId AS AddressId,
        6 AS ProductId;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
