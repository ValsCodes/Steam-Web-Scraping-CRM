/*
    One-time ownership assignment script.

    Replace PUT_USER_ID_HERE with the target AspNetUsers.Id value, then run this
    after the AddUserOwnership migration has been applied.

    This updates every row in every currently user-owned domain table:
    game, game_url, product, pixel, tag, watch_list, wish_list.
*/

DECLARE @TargetUserId nvarchar(450) = N'PUT_USER_ID_HERE';

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF NULLIF(LTRIM(RTRIM(@TargetUserId)), N'') IS NULL
BEGIN
    THROW 50000, 'Set @TargetUserId before running this script.', 1;
END;

IF NOT EXISTS (
    SELECT 1
    FROM [dbo].[AspNetUsers]
    WHERE [Id] = @TargetUserId
)
BEGIN
    THROW 50001, 'The specified @TargetUserId does not exist in AspNetUsers.', 1;
END;

DECLARE @Results table
(
    [TableName] sysname NOT NULL,
    [RowsUpdated] int NOT NULL
);

BEGIN TRY
    BEGIN TRANSACTION;

    UPDATE [dbo].[game]
    SET [user_id] = @TargetUserId;
    INSERT INTO @Results VALUES (N'game', @@ROWCOUNT);

    UPDATE [dbo].[game_url]
    SET [user_id] = @TargetUserId;
    INSERT INTO @Results VALUES (N'game_url', @@ROWCOUNT);

    UPDATE [dbo].[product]
    SET [user_id] = @TargetUserId;
    INSERT INTO @Results VALUES (N'product', @@ROWCOUNT);

    UPDATE [dbo].[pixel]
    SET [user_id] = @TargetUserId;
    INSERT INTO @Results VALUES (N'pixel', @@ROWCOUNT);

    UPDATE [dbo].[tag]
    SET [user_id] = @TargetUserId;
    INSERT INTO @Results VALUES (N'tag', @@ROWCOUNT);

    UPDATE [dbo].[watch_list]
    SET [user_id] = @TargetUserId;
    INSERT INTO @Results VALUES (N'watch_list', @@ROWCOUNT);

    UPDATE [dbo].[wish_list]
    SET [user_id] = @TargetUserId;
    INSERT INTO @Results VALUES (N'wish_list', @@ROWCOUNT);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;

SELECT [TableName], [RowsUpdated]
FROM @Results
ORDER BY [TableName];
