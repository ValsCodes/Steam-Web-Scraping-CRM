/*
Insert one exact-match manual-check preset for the 33 S-tier TF2 hat effects.

Source: TF2 Unusual Effects.xlsx, "Hat Unusual Effects", S-tier rows marked
Consider = Yes.

The script only creates the preset and its criteria. It does not create or
modify the existing Team Fortress 2 game, Unusual Hats URL, item group, or
products.

Reruns are safe when the existing preset definition matches exactly. The
script stops and rolls back rather than changing a same-name preset with
different settings or criteria.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @UserId nvarchar(450) = N'REPLACE_WITH_USER_ID';
DECLARE @GameName nvarchar(300) = N'Team Fortress 2';
DECLARE @GameUrlName nvarchar(300) = N'Unusual Hats';
DECLARE @PresetName nvarchar(100) = N'Unusual Hats - S Tier Effects';
DECLARE @ListingLimit int = 10;
DECLARE @NowUtc datetime2(7) = SYSUTCDATETIME();

DECLARE @GameId bigint;
DECLARE @ItemGroupId bigint;
DECLARE @OrOperatorId bigint;
DECLARE @PresetId bigint;
DECLARE @HadExistingCriteria bit;
DECLARE @InsertedPresets int = 0;
DECLARE @InsertedCriteria int = 0;

DECLARE @Effects table
(
    sort_order int NOT NULL PRIMARY KEY,
    effect_name nvarchar(200) NOT NULL UNIQUE
);

INSERT INTO @Effects (sort_order, effect_name)
VALUES
    (0, N'Green Energy'),
    (1, N'Purple Energy'),
    (2, N'Burning Flames'),
    (3, N'Scorching Flames'),
    (4, N'Sunbeams'),
    (5, N'Cloud 9'),
    (6, N'Miami Nights'),
    (7, N'Disco Beat Down'),
    (8, N'Arcana'),
    (9, N'Spellbound'),
    (10, N'Nebula'),
    (11, N'Death by Disco'),
    (12, N'Tesla Coil'),
    (13, N'Ring of Fire'),
    (14, N'White Lightning'),
    (15, N'Fifth Dimension'),
    (16, N'Vicious Vortex'),
    (17, N'Iridescence'),
    (18, N'Cosmic Constellations'),
    (19, N'Celestial Starburst'),
    (20, N'Radiant Legacy'),
    (21, N'North Star'),
    (22, N'Rainbow Reverie'),
    (23, N'Warp Drive'),
    (24, N'Spectral Fire'),
    (25, N'Galactic Flame'),
    (26, N'Crystal Crown'),
    (27, N'Chromatic Blaze'),
    (28, N'Frostfire'),
    (29, N'Spectrum Inferno'),
    (30, N'Umbral Lights'),
    (31, N'Polar Prism'),
    (32, N'Stellar Orbit');

/* ============================ VALIDATION ============================ */

IF @UserId = N'REPLACE_WITH_USER_ID' OR NULLIF(LTRIM(RTRIM(@UserId)), N'') IS NULL
    THROW 50000, 'Set @UserId before running this script.', 1;

IF @ListingLimit < 1
    THROW 50001, 'The listing limit must be positive.', 1;

IF (SELECT COUNT(*) FROM @Effects) <> 33
   OR (SELECT MIN(sort_order) FROM @Effects) <> 0
   OR (SELECT MAX(sort_order) FROM @Effects) <> 32
    THROW 50002, 'Expected exactly 33 contiguous zero-based S-tier effects.', 1;

IF EXISTS
(
    SELECT 1
    FROM @Effects
    WHERE NULLIF(LTRIM(RTRIM(effect_name)), N'') IS NULL
       OR LEN(CONCAT(N'Unusual Effect: ', effect_name)) > 200
)
    THROW 50003, 'Every S-tier effect must have a name that fits the criterion limit.', 1;

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
        THROW 50021, 'Expected exactly one active Unusual Hats URL for the selected game and @UserId.', 1;

    SELECT @ItemGroupId = item_group_id
    FROM dbo.game_url
    WHERE game_id = @GameId
      AND user_id = @UserId
      AND name = @GameUrlName
      AND is_active = CAST(1 AS bit);

    IF @ItemGroupId IS NULL
        THROW 50022, 'The active Unusual Hats URL must reference an item group.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.item_group
        WHERE id = @ItemGroupId
          AND game_id = @GameId
          AND user_id = @UserId
    )
        THROW 50023, 'The Unusual Hats item group does not belong to the selected game and @UserId.', 1;

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
        THROW 50025, 'Duplicate existing presets were found for the S-tier preset name.', 1;

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
        THROW 50026, 'The S-tier preset could not be resolved uniquely.', 1;

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
        THROW 50027, 'The existing S-tier preset has different settings. Rename it or update it through the application.', 1;

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
                   effect.sort_order,
                   CASE WHEN effect.sort_order = 0 THEN NULL ELSE N'OR' END,
                   0,
                   0,
                   CAST(NULL AS nvarchar(200)),
                   CONCAT(N'Unusual Effect: ', effect.effect_name)
               FROM @Effects AS effect
           )
           OR EXISTS
           (
               SELECT
                   effect.sort_order,
                   CASE WHEN effect.sort_order = 0 THEN NULL ELSE N'OR' END,
                   0,
                   0,
                   CAST(NULL AS nvarchar(200)),
                   CONCAT(N'Unusual Effect: ', effect.effect_name)
               FROM @Effects AS effect

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
        THROW 50028, 'The existing S-tier preset has different criteria. Rename it or update it through the application.', 1;

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
        CASE WHEN effect.sort_order = 0 THEN NULL ELSE @OrOperatorId END,
        effect.sort_order,
        0,
        0,
        NULL,
        CONCAT(N'Unusual Effect: ', effect.effect_name)
    FROM @Effects AS effect
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
