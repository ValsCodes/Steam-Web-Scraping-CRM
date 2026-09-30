/*
Insert one exact-match manual-check preset for the four S-tier TF2 hat paints.

Source: TF2 Hat Margins.xlsx, "Hats", green rows marked Buy = Yes.

Run with SQLCMD variable UserId set to the owning SteamApp user ID. The script
only creates the preset and its criteria. It does not create or modify the
existing Team Fortress 2 game, Hats URL, item group, or products.

Reruns are safe when the existing preset definition matches exactly. The
script stops and rolls back rather than changing a same-name preset with
different settings or criteria.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @UserId nvarchar(450) = N'$(UserId)';
DECLARE @GameName nvarchar(300) = N'Team Fortress 2';
DECLARE @GameUrlName nvarchar(300) = N'Hats';
DECLARE @PresetName nvarchar(100) = N'Hats - S Tier Paints';
DECLARE @ListingLimit int = 10;
DECLARE @NowUtc datetime2(7) = SYSUTCDATETIME();

DECLARE @GameId bigint;
DECLARE @ItemGroupId bigint;
DECLARE @OrOperatorId bigint;
DECLARE @PresetId bigint;
DECLARE @HadExistingCriteria bit;
DECLARE @InsertedPresets int = 0;
DECLARE @InsertedCriteria int = 0;

DECLARE @Paints table
(
    sort_order int NOT NULL PRIMARY KEY,
    paint_name nvarchar(200) NOT NULL UNIQUE
);

INSERT INTO @Paints (sort_order, paint_name)
VALUES
    (0, N'A Distinctive Lack of Hue'),
    (1, N'Pink as Hell'),
    (2, N'The Bitter Taste of Defeat and Lime'),
    (3, N'An Extraordinary Abundance of Tinge');

/* ============================ VALIDATION ============================ */

IF NULLIF(LTRIM(RTRIM(@UserId)), N'') IS NULL OR TRY_CONVERT(uniqueidentifier, @UserId) IS NULL
    THROW 50000, 'Set the SQLCMD UserId variable before running this script.', 1;

IF @ListingLimit < 1
    THROW 50001, 'The listing limit must be positive.', 1;

IF (SELECT COUNT(*) FROM @Paints) <> 4
   OR (SELECT MIN(sort_order) FROM @Paints) <> 0
   OR (SELECT MAX(sort_order) FROM @Paints) <> 3
    THROW 50002, 'Expected exactly four contiguous zero-based S-tier paints.', 1;

IF EXISTS
(
    SELECT 1
    FROM @Paints
    WHERE NULLIF(LTRIM(RTRIM(paint_name)), N'') IS NULL
       OR LEN(CONCAT(N'Paint Color: ', paint_name)) > 200
)
    THROW 50003, 'Every S-tier paint must have a name that fits the criterion limit.', 1;

/* ============================== INSERT ============================== */

BEGIN TRY
    BEGIN TRANSACTION;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.game WITH (UPDLOCK, HOLDLOCK)
        WHERE name = @GameName
          AND user_id = @UserId
    ) <> 1
        THROW 50020, 'Expected exactly one Team Fortress 2 game for @UserId.', 1;

    SELECT @GameId = id
    FROM dbo.game
    WHERE name = @GameName
      AND user_id = @UserId;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.game_url WITH (UPDLOCK, HOLDLOCK)
        WHERE game_id = @GameId
          AND user_id = @UserId
          AND name = @GameUrlName
          AND is_active = CAST(1 AS bit)
    ) <> 1
        THROW 50021, 'Expected exactly one active Hats URL for the selected game and @UserId.', 1;

    SELECT @ItemGroupId = item_group_id
    FROM dbo.game_url
    WHERE game_id = @GameId
      AND user_id = @UserId
      AND name = @GameUrlName
      AND is_active = CAST(1 AS bit);

    IF @ItemGroupId IS NULL
        THROW 50022, 'The active Hats URL must reference an item group.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.item_group
        WHERE id = @ItemGroupId
          AND game_id = @GameId
          AND user_id = @UserId
    )
        THROW 50023, 'The Hats item group does not belong to the selected game and @UserId.', 1;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.manual_check_condition_operator
        WHERE name = N'OR'
    ) <> 1
        THROW 50024, 'Expected exactly one supported OR condition operator.', 1;

    SELECT @OrOperatorId = id
    FROM dbo.manual_check_condition_operator
    WHERE name = N'OR';

    IF
    (
        SELECT COUNT(*)
        FROM dbo.manual_check_preset WITH (UPDLOCK, HOLDLOCK)
        WHERE game_id = @GameId
          AND name = @PresetName
    ) > 1
        THROW 50025, 'Duplicate existing presets were found for the S-tier paint preset name.', 1;

    INSERT INTO dbo.manual_check_preset
    (
        game_id,
        item_group_id,
        name,
        listing_limit,
        cooldown_minutes,
        cooldown_seconds,
        created_at_utc,
        updated_at_utc
    )
    SELECT
        @GameId,
        @ItemGroupId,
        @PresetName,
        @ListingLimit,
        NULL,
        NULL,
        @NowUtc,
        @NowUtc
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.manual_check_preset AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.game_id = @GameId
          AND existing.name = @PresetName
    );

    SET @InsertedPresets = @@ROWCOUNT;

    SELECT
        @PresetId = id,
        @HadExistingCriteria = CAST
        (
            CASE WHEN EXISTS
            (
                SELECT 1
                FROM dbo.manual_check_criterion AS criterion
                WHERE criterion.manual_check_preset_id = preset.id
            ) THEN 1 ELSE 0 END
            AS bit
        )
    FROM dbo.manual_check_preset AS preset
    WHERE preset.game_id = @GameId
      AND preset.name = @PresetName;

    IF @PresetId IS NULL
        THROW 50026, 'The S-tier paint preset could not be resolved uniquely.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.manual_check_preset
        WHERE id = @PresetId
          AND
          (
              ISNULL(item_group_id, -1) <> @ItemGroupId
              OR listing_limit <> @ListingLimit
              OR cooldown_minutes IS NOT NULL
              OR cooldown_seconds IS NOT NULL
          )
    )
        THROW 50027, 'The existing S-tier paint preset has different settings.', 1;

    IF @HadExistingCriteria = CAST(1 AS bit)
       AND
       (
           EXISTS
           (
               SELECT
                   existing.sort_order,
                   condition_operator.name,
                   existing.open_group_count,
                   existing.close_group_count,
                   existing.name_contains,
                   existing.value_contains
               FROM dbo.manual_check_criterion AS existing
               LEFT JOIN dbo.manual_check_condition_operator AS condition_operator
                 ON condition_operator.id = existing.condition_operator_id
               WHERE existing.manual_check_preset_id = @PresetId

               EXCEPT

               SELECT
                   paint.sort_order,
                   CASE WHEN paint.sort_order = 0 THEN NULL ELSE N'OR' END,
                   0,
                   0,
                   CAST(NULL AS nvarchar(200)),
                   CONCAT(N'Paint Color: ', paint.paint_name)
               FROM @Paints AS paint
           )
           OR EXISTS
           (
               SELECT
                   paint.sort_order,
                   CASE WHEN paint.sort_order = 0 THEN NULL ELSE N'OR' END,
                   0,
                   0,
                   CAST(NULL AS nvarchar(200)),
                   CONCAT(N'Paint Color: ', paint.paint_name)
               FROM @Paints AS paint

               EXCEPT

               SELECT
                   existing.sort_order,
                   condition_operator.name,
                   existing.open_group_count,
                   existing.close_group_count,
                   existing.name_contains,
                   existing.value_contains
               FROM dbo.manual_check_criterion AS existing
               LEFT JOIN dbo.manual_check_condition_operator AS condition_operator
                 ON condition_operator.id = existing.condition_operator_id
               WHERE existing.manual_check_preset_id = @PresetId
           )
       )
        THROW 50028, 'The existing S-tier paint preset has different criteria.', 1;

    INSERT INTO dbo.manual_check_criterion
    (
        manual_check_preset_id,
        condition_operator_id,
        sort_order,
        open_group_count,
        close_group_count,
        name_contains,
        value_contains
    )
    SELECT
        @PresetId,
        CASE WHEN paint.sort_order = 0 THEN NULL ELSE @OrOperatorId END,
        paint.sort_order,
        0,
        0,
        NULL,
        CONCAT(N'Paint Color: ', paint.paint_name)
    FROM @Paints AS paint
    WHERE @HadExistingCriteria = CAST(0 AS bit);

    SET @InsertedCriteria = @@ROWCOUNT;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

/* ============================== RESULT ============================== */

SELECT
    @GameId AS game_id,
    @GameName AS game_name,
    @GameUrlName AS game_url_name,
    @ItemGroupId AS item_group_id,
    @PresetId AS preset_id,
    @PresetName AS preset_name,
    @InsertedPresets AS inserted_presets,
    @InsertedCriteria AS inserted_criteria;

SELECT
    preset.id AS preset_id,
    preset.name AS preset_name,
    preset.item_group_id,
    preset.listing_limit,
    preset.cooldown_minutes,
    preset.cooldown_seconds,
    criterion.sort_order,
    condition_operator.name AS operator_name,
    criterion.open_group_count,
    criterion.close_group_count,
    criterion.name_contains,
    criterion.value_contains
FROM dbo.manual_check_preset AS preset
JOIN dbo.manual_check_criterion AS criterion
  ON criterion.manual_check_preset_id = preset.id
LEFT JOIN dbo.manual_check_condition_operator AS condition_operator
  ON condition_operator.id = criterion.condition_operator_id
WHERE preset.id = @PresetId
ORDER BY criterion.sort_order;
