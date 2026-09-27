/*
Reusable SQL Server seed template for one existing SteamApp game.

Edit only the INPUT section. The script inserts missing item groups, game URLs,
products, URL/product links, presets, and ordered preset criteria.

Preset expression rules used by the application:
- A preset must contain 1-25 criteria.
- Criterion 0 has no operator. Every later criterion has one.
- Valid operators: AND, OR, AND NOT, OR NOT, XOR, NAND, NOR.
- name_contains and value_contains are combined with AND inside one criterion
  and must match the same Steam asset description.
- open_group_count and close_group_count encode parentheses. For example:
    (A OR B) AND C
  is represented as:
    0: operator NULL, open 1, close 0, criterion A
    1: operator OR,   open 0, close 1, criterion B
    2: operator AND,  open 0, close 0, criterion C

Reruns are safe when existing preset definitions match the input exactly.
The script stops rather than changing an existing preset with different settings
or criteria.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

/* ============================== INPUT ============================== */

DECLARE @UserId nvarchar(450) = N'REPLACE_WITH_USER_ID';
DECLARE @GameId bigint = 0; -- Existing game.id owned by @UserId.
DECLARE @NowUtc datetime2(7) = SYSUTCDATETIME();

DECLARE @ItemGroups table
(
    item_group_key nvarchar(100) NOT NULL PRIMARY KEY,
    name nvarchar(255) NOT NULL UNIQUE
);

INSERT INTO @ItemGroups (item_group_key, name)
VALUES
    (N'default_group', N'REPLACE_WITH_ITEM_GROUP_NAME');

DECLARE @GameUrls table
(
    game_url_key nvarchar(100) NOT NULL PRIMARY KEY,
    name nvarchar(300) NOT NULL UNIQUE,
    partial_url nvarchar(max) NULL,
    start_page int NULL,
    end_page int NULL,
    pixel_image_height int NULL,
    pixel_image_width int NULL,
    pixel_x int NULL,
    pixel_y int NULL,
    scraping_mode_id bigint NULL,
    is_active bit NOT NULL,
    item_group_key nvarchar(100) NULL
);

INSERT INTO @GameUrls
(
    game_url_key,
    name,
    partial_url,
    start_page,
    end_page,
    pixel_image_height,
    pixel_image_width,
    pixel_x,
    pixel_y,
    scraping_mode_id,
    is_active,
    item_group_key
)
VALUES
    (
        N'primary_market',
        N'REPLACE_WITH_GAME_URL_NAME',
        N'https://steamcommunity.com/market/listings/REPLACE_WITH_STEAM_APP_ID/',
        NULL,
        NULL,
        NULL,
        NULL,
        NULL,
        NULL,
        NULL,
        CAST(1 AS bit),
        N'default_group'
    );

DECLARE @Products table
(
    product_key nvarchar(100) NOT NULL PRIMARY KEY,
    name nvarchar(300) NOT NULL UNIQUE,
    is_active bit NOT NULL,
    rating int NULL
);

INSERT INTO @Products (product_key, name, is_active, rating)
VALUES
    (N'product_1', N'REPLACE_WITH_PRODUCT_NAME', CAST(1 AS bit), NULL);

DECLARE @GameUrlProducts table
(
    game_url_key nvarchar(100) NOT NULL,
    product_key nvarchar(100) NOT NULL,
    current_stock int NOT NULL,
    PRIMARY KEY (game_url_key, product_key)
);

INSERT INTO @GameUrlProducts (game_url_key, product_key, current_stock)
VALUES
    (N'primary_market', N'product_1', 0);

DECLARE @Presets table
(
    preset_key nvarchar(100) NOT NULL PRIMARY KEY,
    name nvarchar(100) NOT NULL UNIQUE,
    item_group_key nvarchar(100) NULL,
    listing_limit int NOT NULL,
    cooldown_minutes int NULL,
    cooldown_seconds int NULL
);

INSERT INTO @Presets
(
    preset_key,
    name,
    item_group_key,
    listing_limit,
    cooldown_minutes,
    cooldown_seconds
)
VALUES
    (
        N'attribute_combination_1',
        N'REPLACE_WITH_PRESET_NAME',
        N'default_group',
        10,
        NULL,
        NULL
    );

DECLARE @PresetCriteria table
(
    preset_key nvarchar(100) NOT NULL,
    sort_order int NOT NULL,
    operator_name nvarchar(20) NULL,
    open_group_count int NOT NULL,
    close_group_count int NOT NULL,
    name_contains nvarchar(200) NULL,
    value_contains nvarchar(200) NULL,
    PRIMARY KEY (preset_key, sort_order)
);

-- Example expression: (Attribute A OR Attribute B) AND Required Attribute.
-- Replace the three value_contains placeholders or replace these rows entirely.
INSERT INTO @PresetCriteria
(
    preset_key,
    sort_order,
    operator_name,
    open_group_count,
    close_group_count,
    name_contains,
    value_contains
)
VALUES
    (N'attribute_combination_1', 0, NULL, 1, 0, N'attribute', N'REPLACE_WITH_ATTRIBUTE_A'),
    (N'attribute_combination_1', 1, N'OR', 0, 1, N'attribute', N'REPLACE_WITH_ATTRIBUTE_B'),
    (N'attribute_combination_1', 2, N'AND', 0, 0, N'attribute', N'REPLACE_WITH_REQUIRED_ATTRIBUTE');

/* ============================ VALIDATION ============================ */

IF @UserId = N'REPLACE_WITH_USER_ID' OR NULLIF(LTRIM(RTRIM(@UserId)), N'') IS NULL
    THROW 50000, 'Set @UserId before running this script.', 1;

IF @GameId <= 0
    THROW 50001, 'Set @GameId to an existing game owned by @UserId.', 1;

IF EXISTS
(
    SELECT 1
    FROM @ItemGroups
    WHERE name LIKE N'REPLACE[_]WITH[_]%'
)
OR EXISTS
(
    SELECT 1
    FROM @GameUrls
    WHERE name LIKE N'REPLACE[_]WITH[_]%'
       OR partial_url LIKE N'%REPLACE_WITH_%'
)
OR EXISTS
(
    SELECT 1
    FROM @Products
    WHERE name LIKE N'REPLACE[_]WITH[_]%'
)
OR EXISTS
(
    SELECT 1
    FROM @Presets
    WHERE name LIKE N'REPLACE[_]WITH[_]%'
)
OR EXISTS
(
    SELECT 1
    FROM @PresetCriteria
    WHERE name_contains LIKE N'REPLACE[_]WITH[_]%'
       OR value_contains LIKE N'REPLACE[_]WITH[_]%'
)
    THROW 50002, 'Replace every placeholder in the INPUT section before running this script.', 1;

IF EXISTS
(
    SELECT 1
    FROM @GameUrls AS input
    WHERE input.item_group_key IS NOT NULL
      AND NOT EXISTS
      (
          SELECT 1
          FROM @ItemGroups AS item_group
          WHERE item_group.item_group_key = input.item_group_key
      )
)
    THROW 50003, 'A game URL references an item_group_key that is not declared in @ItemGroups.', 1;

IF EXISTS
(
    SELECT 1
    FROM @Presets AS input
    WHERE input.item_group_key IS NOT NULL
      AND NOT EXISTS
      (
          SELECT 1
          FROM @ItemGroups AS item_group
          WHERE item_group.item_group_key = input.item_group_key
      )
)
    THROW 50004, 'A preset references an item_group_key that is not declared in @ItemGroups.', 1;

IF EXISTS
(
    SELECT 1
    FROM @GameUrlProducts AS input
    WHERE NOT EXISTS
          (
              SELECT 1
              FROM @GameUrls AS game_url
              WHERE game_url.game_url_key = input.game_url_key
          )
       OR NOT EXISTS
          (
              SELECT 1
              FROM @Products AS product
              WHERE product.product_key = input.product_key
          )
)
    THROW 50005, 'A URL/product link references an undeclared key.', 1;

IF EXISTS
(
    SELECT 1
    FROM @PresetCriteria AS criterion
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM @Presets AS preset
        WHERE preset.preset_key = criterion.preset_key
    )
)
    THROW 50006, 'A criterion references a preset_key that is not declared in @Presets.', 1;

IF EXISTS
(
    SELECT 1
    FROM @Presets AS preset
    OUTER APPLY
    (
        SELECT COUNT(*) AS criterion_count
        FROM @PresetCriteria AS criterion
        WHERE criterion.preset_key = preset.preset_key
    ) AS counts
    WHERE counts.criterion_count NOT BETWEEN 1 AND 25
)
    THROW 50007, 'Every preset must contain between 1 and 25 criteria.', 1;

IF EXISTS
(
    SELECT 1
    FROM @Presets
    WHERE listing_limit < 1
       OR (cooldown_minutes IS NULL AND cooldown_seconds IS NOT NULL)
       OR (cooldown_minutes IS NOT NULL AND cooldown_seconds IS NULL)
       OR cooldown_minutes NOT BETWEEN 0 AND 59
       OR cooldown_seconds NOT BETWEEN 0 AND 59
)
    THROW 50008, 'Listing limits must be positive; cooldown minutes and seconds must both be empty or both be 0-59.', 1;

IF EXISTS
(
    SELECT 1
    FROM @PresetCriteria
    WHERE open_group_count NOT BETWEEN 0 AND 25
       OR close_group_count NOT BETWEEN 0 AND 25
       OR (NULLIF(LTRIM(RTRIM(name_contains)), N'') IS NULL
           AND NULLIF(LTRIM(RTRIM(value_contains)), N'') IS NULL)
)
    THROW 50009, 'Each criterion needs a name or value term, and group counts must be 0-25.', 1;

IF EXISTS
(
    SELECT 1
    FROM @PresetCriteria
    WHERE (sort_order = 0 AND operator_name IS NOT NULL)
       OR (sort_order > 0 AND operator_name IS NULL)
)
    THROW 50010, 'Criterion 0 must have no operator; every later criterion must have one.', 1;

IF EXISTS
(
    SELECT 1
    FROM @PresetCriteria
    GROUP BY preset_key
    HAVING MIN(sort_order) <> 0
       OR MAX(sort_order) <> COUNT(*) - 1
)
    THROW 50011, 'Each preset must use contiguous zero-based criterion sort_order values.', 1;

IF EXISTS
(
    SELECT 1
    FROM
    (
        SELECT
            criterion.preset_key,
            criterion.sort_order,
            SUM(criterion.open_group_count - criterion.close_group_count) OVER
            (
                PARTITION BY criterion.preset_key
                ORDER BY criterion.sort_order
                ROWS UNBOUNDED PRECEDING
            ) AS running_depth,
            SUM(criterion.open_group_count) OVER
            (
                PARTITION BY criterion.preset_key
            ) AS total_groups,
            SUM(criterion.open_group_count - criterion.close_group_count) OVER
            (
                PARTITION BY criterion.preset_key
            ) AS final_depth
        FROM @PresetCriteria AS criterion
    ) AS group_balance
    WHERE running_depth NOT BETWEEN 0 AND 25
       OR total_groups > 25
       OR final_depth <> 0
)
    THROW 50012, 'Preset criterion parentheses are unbalanced or exceed 25 groups/levels.', 1;

/* ============================== INSERT ============================== */

DECLARE @ItemGroupMap table
(
    item_group_key nvarchar(100) NOT NULL PRIMARY KEY,
    item_group_id bigint NOT NULL UNIQUE
);

DECLARE @GameUrlMap table
(
    game_url_key nvarchar(100) NOT NULL PRIMARY KEY,
    game_url_id bigint NOT NULL UNIQUE
);

DECLARE @ProductMap table
(
    product_key nvarchar(100) NOT NULL PRIMARY KEY,
    product_id bigint NOT NULL UNIQUE
);

DECLARE @PresetMap table
(
    preset_key nvarchar(100) NOT NULL PRIMARY KEY,
    preset_id bigint NOT NULL UNIQUE,
    had_existing_criteria bit NOT NULL
);

BEGIN TRY
    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.game WITH (UPDLOCK, HOLDLOCK)
        WHERE id = @GameId
          AND user_id = @UserId
    )
        THROW 50020, 'The selected game was not found for @UserId.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @PresetCriteria AS criterion
        WHERE criterion.operator_name IS NOT NULL
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.manual_check_condition_operator AS condition_operator
              WHERE condition_operator.name = criterion.operator_name
          )
    )
        THROW 50021, 'A criterion uses an unsupported operator name.', 1;

    INSERT INTO dbo.item_group (game_id, name, user_id)
    SELECT @GameId, input.name, @UserId
    FROM @ItemGroups AS input
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.item_group AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.game_id = @GameId
          AND existing.user_id = @UserId
          AND existing.name = input.name
    );

    INSERT INTO @ItemGroupMap (item_group_key, item_group_id)
    SELECT input.item_group_key, existing.id
    FROM @ItemGroups AS input
    JOIN dbo.item_group AS existing
      ON existing.game_id = @GameId
     AND existing.user_id = @UserId
     AND existing.name = input.name;

    IF (SELECT COUNT(*) FROM @ItemGroupMap) <> (SELECT COUNT(*) FROM @ItemGroups)
        THROW 50022, 'One or more item groups could not be resolved uniquely.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @GameUrls AS input
        WHERE
        (
            SELECT COUNT(*)
            FROM dbo.game_url AS existing WITH (UPDLOCK, HOLDLOCK)
            WHERE existing.game_id = @GameId
              AND existing.user_id = @UserId
              AND existing.name = input.name
        ) > 1
    )
        THROW 50023, 'Duplicate existing game_url rows were found for an input name.', 1;

    INSERT INTO dbo.game_url
    (
        game_id,
        partial_url,
        start_page,
        end_page,
        name,
        pixel_image_height,
        pixel_image_width,
        pixel_x,
        pixel_y,
        scraping_mode_id,
        is_active,
        user_id,
        item_group_id
    )
    SELECT
        @GameId,
        input.partial_url,
        input.start_page,
        input.end_page,
        input.name,
        input.pixel_image_height,
        input.pixel_image_width,
        input.pixel_x,
        input.pixel_y,
        input.scraping_mode_id,
        input.is_active,
        @UserId,
        item_group.item_group_id
    FROM @GameUrls AS input
    LEFT JOIN @ItemGroupMap AS item_group
      ON item_group.item_group_key = input.item_group_key
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.game_url AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.game_id = @GameId
          AND existing.user_id = @UserId
          AND existing.name = input.name
    );

    INSERT INTO @GameUrlMap (game_url_key, game_url_id)
    SELECT input.game_url_key, existing.id
    FROM @GameUrls AS input
    JOIN dbo.game_url AS existing
      ON existing.game_id = @GameId
     AND existing.user_id = @UserId
     AND existing.name = input.name;

    IF (SELECT COUNT(*) FROM @GameUrlMap) <> (SELECT COUNT(*) FROM @GameUrls)
        THROW 50024, 'One or more game URLs could not be resolved uniquely.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @GameUrls AS input
        JOIN @GameUrlMap AS game_url_map
          ON game_url_map.game_url_key = input.game_url_key
        JOIN dbo.game_url AS existing
          ON existing.id = game_url_map.game_url_id
        LEFT JOIN @ItemGroupMap AS item_group
          ON item_group.item_group_key = input.item_group_key
        WHERE (existing.partial_url <> input.partial_url
               OR (existing.partial_url IS NULL AND input.partial_url IS NOT NULL)
               OR (existing.partial_url IS NOT NULL AND input.partial_url IS NULL))
           OR ISNULL(existing.start_page, -2147483648) <> ISNULL(input.start_page, -2147483648)
           OR ISNULL(existing.end_page, -2147483648) <> ISNULL(input.end_page, -2147483648)
           OR ISNULL(existing.pixel_image_height, -2147483648) <> ISNULL(input.pixel_image_height, -2147483648)
           OR ISNULL(existing.pixel_image_width, -2147483648) <> ISNULL(input.pixel_image_width, -2147483648)
           OR ISNULL(existing.pixel_x, -2147483648) <> ISNULL(input.pixel_x, -2147483648)
           OR ISNULL(existing.pixel_y, -2147483648) <> ISNULL(input.pixel_y, -2147483648)
           OR ISNULL(existing.scraping_mode_id, -1) <> ISNULL(input.scraping_mode_id, -1)
           OR existing.is_active <> input.is_active
           OR ISNULL(existing.item_group_id, -1) <> ISNULL(item_group.item_group_id, -1)
    )
        THROW 50030, 'An existing game URL has different settings. Rename it or update it through the application.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @Products AS input
        WHERE
        (
            SELECT COUNT(*)
            FROM dbo.product AS existing WITH (UPDLOCK, HOLDLOCK)
            WHERE existing.game_id = @GameId
              AND existing.user_id = @UserId
              AND existing.name = input.name
        ) > 1
    )
        THROW 50025, 'Duplicate existing product rows were found for an input name.', 1;

    INSERT INTO dbo.product (game_id, name, is_active, rating, user_id)
    SELECT @GameId, input.name, input.is_active, input.rating, @UserId
    FROM @Products AS input
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.product AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.game_id = @GameId
          AND existing.user_id = @UserId
          AND existing.name = input.name
    );

    INSERT INTO @ProductMap (product_key, product_id)
    SELECT input.product_key, existing.id
    FROM @Products AS input
    JOIN dbo.product AS existing
      ON existing.game_id = @GameId
     AND existing.user_id = @UserId
     AND existing.name = input.name;

    IF (SELECT COUNT(*) FROM @ProductMap) <> (SELECT COUNT(*) FROM @Products)
        THROW 50026, 'One or more products could not be resolved uniquely.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @Products AS input
        JOIN @ProductMap AS product_map
          ON product_map.product_key = input.product_key
        JOIN dbo.product AS existing
          ON existing.id = product_map.product_id
        WHERE existing.is_active <> input.is_active
           OR ISNULL(existing.rating, -2147483648) <> ISNULL(input.rating, -2147483648)
    )
        THROW 50031, 'An existing product has different settings. Rename it or update it through the application.', 1;

    INSERT INTO dbo.game_url_products (game_url_id, product_id, current_stock)
    SELECT game_url.game_url_id, product.product_id, input.current_stock
    FROM @GameUrlProducts AS input
    JOIN @GameUrlMap AS game_url
      ON game_url.game_url_key = input.game_url_key
    JOIN @ProductMap AS product
      ON product.product_key = input.product_key
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.game_url_products AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.game_url_id = game_url.game_url_id
          AND existing.product_id = product.product_id
    );

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
        item_group.item_group_id,
        input.name,
        input.listing_limit,
        input.cooldown_minutes,
        input.cooldown_seconds,
        @NowUtc,
        @NowUtc
    FROM @Presets AS input
    LEFT JOIN @ItemGroupMap AS item_group
      ON item_group.item_group_key = input.item_group_key
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.manual_check_preset AS existing WITH (UPDLOCK, HOLDLOCK)
        WHERE existing.game_id = @GameId
          AND existing.name = input.name
    );

    INSERT INTO @PresetMap (preset_key, preset_id, had_existing_criteria)
    SELECT
        input.preset_key,
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
    FROM @Presets AS input
    JOIN dbo.manual_check_preset AS existing
      ON existing.game_id = @GameId
     AND existing.name = input.name;

    IF (SELECT COUNT(*) FROM @PresetMap) <> (SELECT COUNT(*) FROM @Presets)
        THROW 50027, 'One or more presets could not be resolved uniquely.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @Presets AS input
        JOIN @PresetMap AS preset_map
          ON preset_map.preset_key = input.preset_key
        JOIN dbo.manual_check_preset AS existing
          ON existing.id = preset_map.preset_id
        LEFT JOIN @ItemGroupMap AS item_group
          ON item_group.item_group_key = input.item_group_key
        WHERE existing.listing_limit <> input.listing_limit
           OR ISNULL(existing.cooldown_minutes, -1) <> ISNULL(input.cooldown_minutes, -1)
           OR ISNULL(existing.cooldown_seconds, -1) <> ISNULL(input.cooldown_seconds, -1)
           OR ISNULL(existing.item_group_id, -1) <> ISNULL(item_group.item_group_id, -1)
    )
        THROW 50028, 'An existing preset has different settings. Rename it or update it through the application.', 1;

    IF EXISTS
    (
        SELECT
            preset_map.preset_key,
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
        WHERE preset_map.had_existing_criteria = 1

        EXCEPT

        SELECT
            input.preset_key,
            input.sort_order,
            input.operator_name,
            input.open_group_count,
            input.close_group_count,
            NULLIF(LTRIM(RTRIM(input.name_contains)), N''),
            NULLIF(LTRIM(RTRIM(input.value_contains)), N'')
        FROM @PresetCriteria AS input
        JOIN @PresetMap AS preset_map
          ON preset_map.preset_key = input.preset_key
        WHERE preset_map.had_existing_criteria = 1
    )
    OR EXISTS
    (
        SELECT
            input.preset_key,
            input.sort_order,
            input.operator_name,
            input.open_group_count,
            input.close_group_count,
            NULLIF(LTRIM(RTRIM(input.name_contains)), N''),
            NULLIF(LTRIM(RTRIM(input.value_contains)), N'')
        FROM @PresetCriteria AS input
        JOIN @PresetMap AS preset_map
          ON preset_map.preset_key = input.preset_key
        WHERE preset_map.had_existing_criteria = 1

        EXCEPT

        SELECT
            preset_map.preset_key,
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
        WHERE preset_map.had_existing_criteria = 1
    )
        THROW 50029, 'An existing preset has different criteria. Rename it or update it through the application.', 1;

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
        condition_operator.id,
        input.sort_order,
        input.open_group_count,
        input.close_group_count,
        NULLIF(LTRIM(RTRIM(input.name_contains)), N''),
        NULLIF(LTRIM(RTRIM(input.value_contains)), N'')
    FROM @PresetCriteria AS input
    JOIN @PresetMap AS preset_map
      ON preset_map.preset_key = input.preset_key
    LEFT JOIN dbo.manual_check_condition_operator AS condition_operator
      ON condition_operator.name = input.operator_name
    WHERE preset_map.had_existing_criteria = 0;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

/* ============================== RESULT ============================== */

SELECT
    g.id AS game_id,
    g.name AS game_name,
    gu.id AS game_url_id,
    gu.name AS game_url_name,
    p.id AS product_id,
    p.name AS product_name,
    gup.current_stock
FROM dbo.game AS g
JOIN dbo.game_url AS gu
  ON gu.game_id = g.id
JOIN dbo.game_url_products AS gup
  ON gup.game_url_id = gu.id
JOIN dbo.product AS p
  ON p.id = gup.product_id
JOIN @GameUrlMap AS game_url_map
  ON game_url_map.game_url_id = gu.id
JOIN @ProductMap AS product_map
  ON product_map.product_id = p.id
WHERE g.id = @GameId
ORDER BY gu.name, p.name;

SELECT
    g.id AS game_id,
    g.name AS game_name,
    preset.id AS preset_id,
    preset.name AS preset_name,
    item_group.id AS item_group_id,
    item_group.name AS item_group_name,
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
JOIN dbo.game AS g
  ON g.id = preset.game_id
JOIN @PresetMap AS preset_map
  ON preset_map.preset_id = preset.id
LEFT JOIN dbo.item_group AS item_group
  ON item_group.id = preset.item_group_id
JOIN dbo.manual_check_criterion AS criterion
  ON criterion.manual_check_preset_id = preset.id
LEFT JOIN dbo.manual_check_condition_operator AS condition_operator
  ON condition_operator.id = criterion.condition_operator_id
ORDER BY preset.name, criterion.sort_order;
