/*
Insert five exact-match manual-check presets for desirable TF2 taunt effects.

Source: TF2 Unusual Effects.xlsx, "Taunt Unusual Effects", rows marked
Consider = Yes. The source order is preserved and split into batches of at
most 25 criteria, which is the application limit.

The script only creates presets and criteria. It does not create or modify the
existing Team Fortress 2 game, Unusual Taunts URL, item group, or products.

Reruns are safe when existing preset definitions match exactly. The script
stops and rolls back rather than changing a same-name preset with different
settings or criteria.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @UserId nvarchar(450) = N'REPLACE_WITH_USER_ID';
DECLARE @GameName nvarchar(300) = N'Team Fortress 2';
DECLARE @GameUrlName nvarchar(300) = N'Unusual Taunts';
DECLARE @ListingLimit int = 10;
DECLARE @NowUtc datetime2(7) = SYSUTCDATETIME();

DECLARE @GameId bigint;
DECLARE @ItemGroupId bigint;
DECLARE @OrOperatorId bigint;
DECLARE @InsertedPresets int = 0;
DECLARE @InsertedCriteria int = 0;

DECLARE @PresetDefinitions table
(
    batch_number int NOT NULL PRIMARY KEY,
    preset_name nvarchar(100) NOT NULL UNIQUE
);

INSERT INTO @PresetDefinitions (batch_number, preset_name)
VALUES
    (1, N'Unusual Taunts - S/A Effects (1 of 5)'),
    (2, N'Unusual Taunts - A Effects (2 of 5)'),
    (3, N'Unusual Taunts - A/B Effects (3 of 5)'),
    (4, N'Unusual Taunts - B Effects (4 of 5)'),
    (5, N'Unusual Taunts - B Effects (5 of 5)');

DECLARE @Effects table
(
    effect_rank int NOT NULL PRIMARY KEY,
    tier nchar(1) NOT NULL,
    effect_name nvarchar(200) NOT NULL UNIQUE
);

INSERT INTO @Effects (effect_rank, tier, effect_name)
VALUES
    (1, N'S', N'Holy Grail'),
    (2, N'S', N'Screaming Tiger'),
    (3, N'S', N'Mega Strike'),
    (4, N'S', N'Arcane Assistance'),
    (5, N'S', N'Apotheosis'),
    (6, N'S', N'Ascension'),
    (7, N'S', N'Aurora Borealis'),
    (8, N'S', N'Aurora Australis'),
    (9, N'S', N'Aurora Polaris'),
    (10, N'S', N'Charged Arcane'),
    (11, N'S', N'Thunderous Rage'),
    (12, N'S', N'Godlike'),
    (13, N'S', N'Electrum'),
    (14, N'S', N'Iconic Outline'),
    (15, N'S', N'Galactic Cloud'),
    (16, N'S', N'Northern Nights'),
    (17, N'S', N'Showtime Spotlights'),
    (18, N'A', N'Fountain of Delight'),
    (19, N'A', N'Skill Gotten Gains'),
    (20, N'A', N'Midnight Whirlwind'),
    (21, N'A', N'Silver Cyclone'),
    (22, N'A', N'Hellish Inferno'),
    (23, N'A', N'Spectral Swirl'),
    (24, N'A', N'Infernal Flames'),
    (25, N'A', N'Roaring Rockets'),
    (26, N'A', N'Eerie Lightning'),
    (27, N'A', N'Terrifying Thunder'),
    (28, N'A', N'Arctic Aurora'),
    (29, N'A', N'Astral Presence'),
    (30, N'A', N'Spellbound Aspect'),
    (31, N'A', N'Reindoonicorn Rancher'),
    (32, N'A', N'Shimmering Lights'),
    (33, N'A', N'Spectral Shackles'),
    (34, N'A', N'Cursed Confinement'),
    (35, N'A', N'Thundering Spirit'),
    (36, N'A', N'Galvanic Defiance'),
    (37, N'A', N'Golden Glimmer'),
    (38, N'A', N'Sublime Snowstorm'),
    (39, N'A', N'Prismatic Haze'),
    (40, N'A', N'Death Grip'),
    (41, N'A', N'Boundless Blizzard'),
    (42, N'A', N'Solar Scorched'),
    (43, N'A', N'Blooming Beacon'),
    (44, N'A', N'Beaming Beacon'),
    (45, N'A', N'Blazing Beacon'),
    (46, N'A', N'Deep-Sea Devourer'),
    (47, N'A', N'Eldritch Horror'),
    (48, N'A', N'Potion Explosion'),
    (49, N'A', N'Galactic Connection'),
    (50, N'A', N'Eldritch Rift'),
    (51, N'A', N'Dragonflies'' Embrace'),
    (52, N'A', N'Dragonflies'' Nature'),
    (53, N'A', N'Dragonflies'' Lucent'),
    (54, N'A', N'Electrocution'),
    (55, N'A', N'Fiesta Royale'),
    (56, N'A', N'Elemental'),
    (57, N'A', N'Bountiful Riches'),
    (58, N'A', N'Sakura Blessings'),
    (59, N'A', N'Mystic Fusion'),
    (60, N'A', N'Award Winning'),
    (61, N'A', N'Operatic Triumph'),
    (62, N'A', N'Seamine'),
    (63, N'A', N'Subtle Silhouette'),
    (64, N'A', N'Shark Attack'),
    (65, N'A', N'Fossil Fueled'),
    (66, N'A', N'Snow Dome'),
    (67, N'A', N'Scorching Sensation'),
    (68, N'A', N'Burning Sensation'),
    (69, N'A', N'Galactic Dust'),
    (70, N'A', N'Aurora Aura'),
    (71, N'A', N'Wheat Field'),
    (72, N'A', N'Luminous Drift'),
    (73, N'A', N'Luminous Shift'),
    (74, N'A', N'Deep Dive'),
    (75, N'B', N'Spooky Night'),
    (76, N'B', N'Ominous Night'),
    (77, N'B', N'Bewitched'),
    (78, N'B', N'Accursed'),
    (79, N'B', N'Enchanted'),
    (80, N'B', N'Nether Void'),
    (81, N'B', N'Wintery Wisp'),
    (82, N'B', N'Spectral Escort'),
    (83, N'B', N'Emerald Allurement'),
    (84, N'B', N'Pyrophoric Personality'),
    (85, N'B', N'Delightful Star'),
    (86, N'B', N'Frosted Star'),
    (87, N'B', N'Cavalier de Carte'),
    (88, N'B', N'Amethyst Winds'),
    (89, N'B', N'Golden Gusts'),
    (90, N'B', N'Smissmas Swirls'),
    (91, N'B', N'Glamorous Dazzle'),
    (92, N'B', N'Linguistic Deviation'),
    (93, N'B', N'Aurelian Seal'),
    (94, N'B', N'Runic Imprisonment'),
    (95, N'B', N'Rising Ritual'),
    (96, N'B', N'Convulsive Fiery'),
    (97, N'B', N'Midnight Sparklers'),
    (98, N'B', N'Deepsea Rave'),
    (99, N'B', N'Floppin'' Frenzy'),
    (100, N'B', N'Pastel Trance'),
    (101, N'B', N'Wildflower Meadows'),
    (102, N'B', N'Dead Man''s Party'),
    (103, N'B', N'Haunted Cremation'),
    (104, N'B', N'Dark Twilight'),
    (105, N'B', N'Permafrost Essence'),
    (106, N'B', N'Distress Signal'),
    (107, N'B', N'Carioca''s Call'),
    (108, N'B', N'Grand Jubilee'),
    (109, N'B', N'Teamwork Valorance'),
    (110, N'B', N'Legacy Logo'),
    (111, N'B', N'Power Pressure'),
    (112, N'B', N'Magnifying Momentum'),
    (113, N'B', N'Charging Catalyst'),
    (114, N'B', N'Amplifying Aura'),
    (115, N'B', N'Lavender Sensation'),
    (116, N'B', N'Verdant Phenomenon'),
    (117, N'B', N'Tangled Lights'),
    (118, N'B', N'Ocean Reef'),
    (119, N'B', N'Lost Signal'),
    (120, N'B', N'Summer Wave'),
    (121, N'B', N'Split Malice'),
    (122, N'B', N'Void Crawlers'),
    (123, N'B', N'Morbidly Beast');

DECLARE @PresetCriteria table
(
    batch_number int NOT NULL,
    sort_order int NOT NULL,
    operator_name nvarchar(20) NULL,
    open_group_count int NOT NULL,
    close_group_count int NOT NULL,
    name_contains nvarchar(200) NULL,
    value_contains nvarchar(200) NOT NULL,
    PRIMARY KEY (batch_number, sort_order)
);

INSERT INTO @PresetCriteria
(
    batch_number,
    sort_order,
    operator_name,
    open_group_count,
    close_group_count,
    name_contains,
    value_contains
)
SELECT
    batch_number = ((effect.effect_rank - 1) / 25) + 1,
    sort_order = (effect.effect_rank - 1) % 25,
    operator_name = CASE WHEN (effect.effect_rank - 1) % 25 = 0 THEN NULL ELSE N'OR' END,
    open_group_count = 0,
    close_group_count = 0,
    name_contains = NULL,
    value_contains = CONCAT(N'Unusual Effect: ', effect.effect_name)
FROM @Effects AS effect;

/* ============================ VALIDATION ============================ */

IF @UserId = N'REPLACE_WITH_USER_ID' OR NULLIF(LTRIM(RTRIM(@UserId)), N'') IS NULL
    THROW 50000, 'Set @UserId before running this script.', 1;

IF @ListingLimit < 1
    THROW 50001, 'The listing limit must be positive.', 1;

IF (SELECT COUNT(*) FROM @PresetDefinitions) <> 5
   OR (SELECT MIN(batch_number) FROM @PresetDefinitions) <> 1
   OR (SELECT MAX(batch_number) FROM @PresetDefinitions) <> 5
    THROW 50002, 'Expected exactly five preset definitions.', 1;

IF (SELECT COUNT(*) FROM @Effects) <> 123
   OR (SELECT MIN(effect_rank) FROM @Effects) <> 1
   OR (SELECT MAX(effect_rank) FROM @Effects) <> 123
    THROW 50003, 'Expected 123 contiguous one-based effect ranks.', 1;

IF EXISTS
(
    SELECT 1
    FROM @Effects
    WHERE tier NOT IN (N'S', N'A', N'B')
       OR NULLIF(LTRIM(RTRIM(effect_name)), N'') IS NULL
       OR LEN(CONCAT(N'Unusual Effect: ', effect_name)) > 200
)
    THROW 50004, 'Effects must be named S, A, or B entries that fit the criterion limit.', 1;

IF (SELECT COUNT(*) FROM @Effects WHERE tier = N'S' AND effect_rank BETWEEN 1 AND 17) <> 17
   OR (SELECT COUNT(*) FROM @Effects WHERE tier = N'A' AND effect_rank BETWEEN 18 AND 74) <> 57
   OR (SELECT COUNT(*) FROM @Effects WHERE tier = N'B' AND effect_rank BETWEEN 75 AND 123) <> 49
    THROW 50005, 'Effect ranks no longer match the expected S, A, and B tier ranges.', 1;

IF EXISTS
(
    SELECT definition.batch_number
    FROM @PresetDefinitions AS definition
    LEFT JOIN @PresetCriteria AS criterion
      ON criterion.batch_number = definition.batch_number
    GROUP BY definition.batch_number
    HAVING COUNT(criterion.sort_order) NOT BETWEEN 1 AND 25
)
OR EXISTS
(
    SELECT 1
    FROM @PresetCriteria AS criterion
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM @PresetDefinitions AS definition
        WHERE definition.batch_number = criterion.batch_number
    )
)
    THROW 50006, 'Every effect must map to a declared preset containing 1-25 criteria.', 1;

IF EXISTS
(
    SELECT 1
    FROM @PresetCriteria
    WHERE (sort_order = 0 AND operator_name IS NOT NULL)
       OR (sort_order > 0 AND (operator_name IS NULL OR operator_name <> N'OR'))
       OR open_group_count <> 0
       OR close_group_count <> 0
       OR name_contains IS NOT NULL
)
    THROW 50007, 'Generated criteria must be an ungrouped OR expression.', 1;

/* ============================== INSERT ============================== */

DECLARE @PresetMap table
(
    batch_number int NOT NULL PRIMARY KEY,
    preset_id bigint NOT NULL UNIQUE,
    had_existing_criteria bit NOT NULL
);

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
        THROW 50021, 'Expected exactly one active Unusual Taunts URL for the selected game and @UserId.', 1;

    SELECT @ItemGroupId = item_group_id
    FROM dbo.game_url
    WHERE game_id = @GameId
      AND user_id = @UserId
      AND name = @GameUrlName
      AND is_active = CAST(1 AS bit);

    IF @ItemGroupId IS NULL
        THROW 50022, 'The active Unusual Taunts URL must reference an item group.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.item_group
        WHERE id = @ItemGroupId
          AND game_id = @GameId
          AND user_id = @UserId
    )
        THROW 50023, 'The Unusual Taunts item group does not belong to the selected game and @UserId.', 1;

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

    IF EXISTS
    (
        SELECT 1
        FROM @PresetDefinitions AS definition
        WHERE
        (
            SELECT COUNT(*)
            FROM dbo.manual_check_preset AS existing WITH (UPDLOCK, HOLDLOCK)
            WHERE existing.game_id = @GameId
              AND existing.name = definition.preset_name
        ) > 1
    )
        THROW 50025, 'Duplicate existing presets were found for an input name.', 1;

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
        definition.preset_name,
        @ListingLimit,
        NULL,
        NULL,
        @NowUtc,
        @NowUtc
    FROM @PresetDefinitions AS definition
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.manual_check_preset AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.game_id = @GameId
          AND existing.name = definition.preset_name
    );

    SET @InsertedPresets = @@ROWCOUNT;

    INSERT INTO @PresetMap (batch_number, preset_id, had_existing_criteria)
    SELECT
        definition.batch_number,
        existing.id,
        CAST
        (
            CASE WHEN EXISTS
            (
                SELECT 1
                FROM dbo.manual_check_criterion AS criterion
                WHERE criterion.manual_check_preset_id = existing.id
            ) THEN 1 ELSE 0 END
            AS bit
        )
    FROM @PresetDefinitions AS definition
    JOIN dbo.manual_check_preset AS existing
      ON existing.game_id = @GameId
     AND existing.name = definition.preset_name;

    IF (SELECT COUNT(*) FROM @PresetMap) <> (SELECT COUNT(*) FROM @PresetDefinitions)
        THROW 50026, 'One or more presets could not be resolved uniquely.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @PresetDefinitions AS definition
        JOIN @PresetMap AS preset_map
          ON preset_map.batch_number = definition.batch_number
        JOIN dbo.manual_check_preset AS existing
          ON existing.id = preset_map.preset_id
        WHERE ISNULL(existing.item_group_id, -1) <> @ItemGroupId
           OR existing.listing_limit <> @ListingLimit
           OR existing.cooldown_minutes IS NOT NULL
           OR existing.cooldown_seconds IS NOT NULL
    )
        THROW 50027, 'An existing preset has different settings. Rename it or update it through the application.', 1;

    IF EXISTS
    (
        SELECT
            preset_map.batch_number,
            existing.sort_order,
            condition_operator.name,
            existing.open_group_count,
            existing.close_group_count,
            existing.name_contains,
            existing.value_contains
        FROM @PresetMap AS preset_map
        JOIN dbo.manual_check_criterion AS existing
          ON existing.manual_check_preset_id = preset_map.preset_id
        LEFT JOIN dbo.manual_check_condition_operator AS condition_operator
          ON condition_operator.id = existing.condition_operator_id
        WHERE preset_map.had_existing_criteria = CAST(1 AS bit)

        EXCEPT

        SELECT
            input.batch_number,
            input.sort_order,
            input.operator_name,
            input.open_group_count,
            input.close_group_count,
            input.name_contains,
            input.value_contains
        FROM @PresetCriteria AS input
        JOIN @PresetMap AS preset_map
          ON preset_map.batch_number = input.batch_number
        WHERE preset_map.had_existing_criteria = CAST(1 AS bit)
    )
    OR EXISTS
    (
        SELECT
            input.batch_number,
            input.sort_order,
            input.operator_name,
            input.open_group_count,
            input.close_group_count,
            input.name_contains,
            input.value_contains
        FROM @PresetCriteria AS input
        JOIN @PresetMap AS preset_map
          ON preset_map.batch_number = input.batch_number
        WHERE preset_map.had_existing_criteria = CAST(1 AS bit)

        EXCEPT

        SELECT
            preset_map.batch_number,
            existing.sort_order,
            condition_operator.name,
            existing.open_group_count,
            existing.close_group_count,
            existing.name_contains,
            existing.value_contains
        FROM @PresetMap AS preset_map
        JOIN dbo.manual_check_criterion AS existing
          ON existing.manual_check_preset_id = preset_map.preset_id
        LEFT JOIN dbo.manual_check_condition_operator AS condition_operator
          ON condition_operator.id = existing.condition_operator_id
        WHERE preset_map.had_existing_criteria = CAST(1 AS bit)
    )
        THROW 50028, 'An existing preset has different criteria. Rename it or update it through the application.', 1;

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
        preset_map.preset_id,
        CASE WHEN input.operator_name IS NULL THEN NULL ELSE @OrOperatorId END,
        input.sort_order,
        input.open_group_count,
        input.close_group_count,
        input.name_contains,
        input.value_contains
    FROM @PresetCriteria AS input
    JOIN @PresetMap AS preset_map
      ON preset_map.batch_number = input.batch_number
    WHERE preset_map.had_existing_criteria = CAST(0 AS bit);

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
FROM @PresetMap AS preset_map
JOIN dbo.manual_check_preset AS preset
  ON preset.id = preset_map.preset_id
JOIN dbo.manual_check_criterion AS criterion
  ON criterion.manual_check_preset_id = preset.id
LEFT JOIN dbo.manual_check_condition_operator AS condition_operator
  ON condition_operator.id = criterion.condition_operator_id
ORDER BY preset_map.batch_number, criterion.sort_order;
