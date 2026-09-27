/*
Read-only SQL Server inspection script for SteamApp catalog and manual-check presets.

Set either filter to NULL to inspect all accessible rows. The script returns
separate result sets to avoid multiplying every product by every preset criterion.
*/

SET NOCOUNT ON;

DECLARE @GameId bigint = NULL;
DECLARE @UserId nvarchar(450) = NULL;

DECLARE @RelevantTables table
(
    schema_name sysname NOT NULL,
    table_name sysname NOT NULL,
    PRIMARY KEY (schema_name, table_name)
);

INSERT INTO @RelevantTables (schema_name, table_name)
VALUES
    (N'dbo', N'game'),
    (N'dbo', N'game_url'),
    (N'dbo', N'game_url_products'),
    (N'dbo', N'product'),
    (N'dbo', N'item_group'),
    (N'dbo', N'manual_check_preset'),
    (N'dbo', N'manual_check_criterion'),
    (N'dbo', N'manual_check_condition_operator');

/* 1. Columns, types, nullability, identity, and defaults. */
SELECT
    schema_name = schema_info.name,
    table_name = table_info.name,
    ordinal_position = column_info.column_id,
    column_name = column_info.name,
    data_type = type_info.name,
    character_maximum_length =
        CASE
            WHEN type_info.name IN (N'nchar', N'nvarchar') AND column_info.max_length > 0
                THEN column_info.max_length / 2
            WHEN type_info.name IN (N'char', N'varchar', N'binary', N'varbinary')
                THEN column_info.max_length
            WHEN column_info.max_length = -1
                THEN -1
            ELSE NULL
        END,
    column_info.precision,
    column_info.scale,
    column_info.is_nullable,
    column_info.is_identity,
    default_definition = default_constraint.definition
FROM sys.tables AS table_info
JOIN sys.schemas AS schema_info
  ON schema_info.schema_id = table_info.schema_id
JOIN @RelevantTables AS requested
  ON requested.schema_name = schema_info.name
 AND requested.table_name = table_info.name
JOIN sys.columns AS column_info
  ON column_info.object_id = table_info.object_id
JOIN sys.types AS type_info
  ON type_info.user_type_id = column_info.user_type_id
LEFT JOIN sys.default_constraints AS default_constraint
  ON default_constraint.object_id = column_info.default_object_id
ORDER BY schema_info.name, table_info.name, column_info.column_id;

/* 2. Primary keys, unique constraints/indexes, and their ordered columns. */
SELECT
    schema_name = schema_info.name,
    table_name = table_info.name,
    key_name = index_info.name,
    key_type =
        CASE
            WHEN index_info.is_primary_key = 1 THEN N'PRIMARY KEY'
            WHEN index_info.is_unique_constraint = 1 THEN N'UNIQUE CONSTRAINT'
            WHEN index_info.is_unique = 1 THEN N'UNIQUE INDEX'
        END,
    key_ordinal = index_column.key_ordinal,
    column_name = column_info.name
FROM sys.tables AS table_info
JOIN sys.schemas AS schema_info
  ON schema_info.schema_id = table_info.schema_id
JOIN @RelevantTables AS requested
  ON requested.schema_name = schema_info.name
 AND requested.table_name = table_info.name
JOIN sys.indexes AS index_info
  ON index_info.object_id = table_info.object_id
 AND index_info.is_unique = 1
JOIN sys.index_columns AS index_column
  ON index_column.object_id = index_info.object_id
 AND index_column.index_id = index_info.index_id
 AND index_column.key_ordinal > 0
JOIN sys.columns AS column_info
  ON column_info.object_id = index_column.object_id
 AND column_info.column_id = index_column.column_id
ORDER BY schema_info.name, table_info.name, index_info.name, index_column.key_ordinal;

/* 3. Check constraints, including preset grouping limits. */
SELECT
    schema_name = schema_info.name,
    table_name = table_info.name,
    constraint_name = check_constraint.name,
    check_constraint.definition,
    check_constraint.is_disabled,
    check_constraint.is_not_trusted
FROM sys.check_constraints AS check_constraint
JOIN sys.tables AS table_info
  ON table_info.object_id = check_constraint.parent_object_id
JOIN sys.schemas AS schema_info
  ON schema_info.schema_id = table_info.schema_id
JOIN @RelevantTables AS requested
  ON requested.schema_name = schema_info.name
 AND requested.table_name = table_info.name
ORDER BY schema_info.name, table_info.name, check_constraint.name;

/* 4. Foreign-key relationships and delete behavior. */
SELECT
    foreign_key_name = foreign_key.name,
    child_schema = child_schema.name,
    child_table = child_table.name,
    child_column = child_column.name,
    parent_schema = parent_schema.name,
    parent_table = parent_table.name,
    parent_column = parent_column.name,
    foreign_key.delete_referential_action_desc
FROM sys.foreign_keys AS foreign_key
JOIN sys.foreign_key_columns AS foreign_key_column
  ON foreign_key_column.constraint_object_id = foreign_key.object_id
JOIN sys.tables AS child_table
  ON child_table.object_id = foreign_key.parent_object_id
JOIN sys.schemas AS child_schema
  ON child_schema.schema_id = child_table.schema_id
JOIN sys.columns AS child_column
  ON child_column.object_id = child_table.object_id
 AND child_column.column_id = foreign_key_column.parent_column_id
JOIN sys.tables AS parent_table
  ON parent_table.object_id = foreign_key.referenced_object_id
JOIN sys.schemas AS parent_schema
  ON parent_schema.schema_id = parent_table.schema_id
JOIN sys.columns AS parent_column
  ON parent_column.object_id = parent_table.object_id
 AND parent_column.column_id = foreign_key_column.referenced_column_id
WHERE EXISTS
(
    SELECT 1
    FROM @RelevantTables AS requested
    WHERE requested.schema_name = child_schema.name
      AND requested.table_name = child_table.name
)
ORDER BY child_schema.name, child_table.name, foreign_key.name, foreign_key_column.constraint_column_id;

/* 5. Supported operators and their stable IDs. */
SELECT
    condition_operator_id = condition_operator.id,
    operator_name = condition_operator.name
FROM dbo.manual_check_condition_operator AS condition_operator
ORDER BY condition_operator.id;

/* 6. Explicit game URL/product mapping. */
SELECT
    game_id = game.id,
    game_name = game.name,
    game_is_active = game.is_active,
    game_user_id = game.user_id,
    game_url_id = game_url.id,
    game_url_name = game_url.name,
    game_url_partial_url = game_url.partial_url,
    game_url_start_page = game_url.start_page,
    game_url_end_page = game_url.end_page,
    game_url_scraping_mode_id = game_url.scraping_mode_id,
    game_url_is_active = game_url.is_active,
    game_url_item_group_id = game_url.item_group_id,
    game_url_item_group_name = game_url_group.name,
    product_id = product.id,
    product_name = product.name,
    product_is_active = product.is_active,
    product_rating = product.rating,
    current_stock = game_url_product.current_stock
FROM dbo.game AS game
JOIN dbo.game_url AS game_url
  ON game_url.game_id = game.id
JOIN dbo.game_url_products AS game_url_product
  ON game_url_product.game_url_id = game_url.id
JOIN dbo.product AS product
  ON product.id = game_url_product.product_id
LEFT JOIN dbo.item_group AS game_url_group
  ON game_url_group.id = game_url.item_group_id
WHERE (@GameId IS NULL OR game.id = @GameId)
  AND (@UserId IS NULL OR game.user_id = @UserId)
ORDER BY game.name, game_url.name, product.name;

/* 7. Raw preset map: one ordered row per criterion. */
;WITH PresetCriteria AS
(
    SELECT
        game_id = game.id,
        game_name = game.name,
        game_user_id = game.user_id,
        preset_id = preset.id,
        preset_name = preset.name,
        preset_item_group_id = preset.item_group_id,
        preset_item_group_name = item_group.name,
        preset.listing_limit,
        preset.cooldown_minutes,
        preset.cooldown_seconds,
        preset.created_at_utc,
        preset.updated_at_utc,
        criterion_id = criterion.id,
        criterion.sort_order,
        condition_operator_id = criterion.condition_operator_id,
        operator_name = condition_operator.name,
        criterion.open_group_count,
        criterion.close_group_count,
        criterion.name_contains,
        criterion.value_contains,
        criterion_text =
            CASE WHEN criterion.id IS NULL THEN NULL ELSE CONCAT
            (
                N'[',
                CASE
                    WHEN criterion.name_contains IS NOT NULL
                        THEN CONCAT(N'name contains "', REPLACE(criterion.name_contains, N'"', N'""'), N'"')
                    ELSE N''
                END,
                CASE
                    WHEN criterion.name_contains IS NOT NULL AND criterion.value_contains IS NOT NULL
                        THEN N' AND '
                    ELSE N''
                END,
                CASE
                    WHEN criterion.value_contains IS NOT NULL
                        THEN CONCAT(N'value contains "', REPLACE(criterion.value_contains, N'"', N'""'), N'"')
                    ELSE N''
                END,
                N']'
            ) END,
        expression_token =
            CASE WHEN criterion.id IS NULL THEN NULL ELSE CONCAT
            (
                CASE
                    WHEN criterion.sort_order = 0 THEN N''
                    ELSE CONCAT(COALESCE(condition_operator.name, N'<MISSING OPERATOR>'), N' ')
                END,
                REPLICATE(N'(', criterion.open_group_count),
                N'[',
                CASE
                    WHEN criterion.name_contains IS NOT NULL
                        THEN CONCAT(N'name contains "', REPLACE(criterion.name_contains, N'"', N'""'), N'"')
                    ELSE N''
                END,
                CASE
                    WHEN criterion.name_contains IS NOT NULL AND criterion.value_contains IS NOT NULL
                        THEN N' AND '
                    ELSE N''
                END,
                CASE
                    WHEN criterion.value_contains IS NOT NULL
                        THEN CONCAT(N'value contains "', REPLACE(criterion.value_contains, N'"', N'""'), N'"')
                    ELSE N''
                END,
                N']',
                REPLICATE(N')', criterion.close_group_count)
            ) END
    FROM dbo.manual_check_preset AS preset
    JOIN dbo.game AS game
      ON game.id = preset.game_id
    LEFT JOIN dbo.item_group AS item_group
      ON item_group.id = preset.item_group_id
    LEFT JOIN dbo.manual_check_criterion AS criterion
      ON criterion.manual_check_preset_id = preset.id
    LEFT JOIN dbo.manual_check_condition_operator AS condition_operator
      ON condition_operator.id = criterion.condition_operator_id
    WHERE (@GameId IS NULL OR game.id = @GameId)
      AND (@UserId IS NULL OR game.user_id = @UserId)
)
SELECT
    game_id,
    game_name,
    game_user_id,
    preset_id,
    preset_name,
    preset_item_group_id,
    preset_item_group_name,
    listing_limit,
    cooldown_minutes,
    cooldown_seconds,
    created_at_utc,
    updated_at_utc,
    criterion_id,
    sort_order,
    condition_operator_id,
    operator_name,
    open_group_count,
    close_group_count,
    name_contains,
    value_contains,
    criterion_text,
    expression_token
FROM PresetCriteria
ORDER BY game_name, preset_name, sort_order;

/* 8. One readable expression per preset. */
;WITH ExpressionTokens AS
(
    SELECT
        game_id = game.id,
        game_name = game.name,
        preset_id = preset.id,
        preset_name = preset.name,
        item_group_id = preset.item_group_id,
        item_group_name = item_group.name,
        preset.listing_limit,
        preset.cooldown_minutes,
        preset.cooldown_seconds,
        criterion.sort_order,
        expression_token =
            CASE WHEN criterion.id IS NULL THEN NULL ELSE CAST
            (
                CONCAT
                (
                    CASE
                        WHEN criterion.sort_order = 0 THEN N''
                        ELSE CONCAT(COALESCE(condition_operator.name, N'<MISSING OPERATOR>'), N' ')
                    END,
                    REPLICATE(N'(', criterion.open_group_count),
                    N'[',
                    CASE
                        WHEN criterion.name_contains IS NOT NULL
                            THEN CONCAT(N'name contains "', REPLACE(criterion.name_contains, N'"', N'""'), N'"')
                        ELSE N''
                    END,
                    CASE
                        WHEN criterion.name_contains IS NOT NULL AND criterion.value_contains IS NOT NULL
                            THEN N' AND '
                        ELSE N''
                    END,
                    CASE
                        WHEN criterion.value_contains IS NOT NULL
                            THEN CONCAT(N'value contains "', REPLACE(criterion.value_contains, N'"', N'""'), N'"')
                        ELSE N''
                    END,
                    N']',
                    REPLICATE(N')', criterion.close_group_count)
                )
                AS nvarchar(max)
            )
            END
    FROM dbo.manual_check_preset AS preset
    JOIN dbo.game AS game
      ON game.id = preset.game_id
    LEFT JOIN dbo.item_group AS item_group
      ON item_group.id = preset.item_group_id
    LEFT JOIN dbo.manual_check_criterion AS criterion
      ON criterion.manual_check_preset_id = preset.id
    LEFT JOIN dbo.manual_check_condition_operator AS condition_operator
      ON condition_operator.id = criterion.condition_operator_id
    WHERE (@GameId IS NULL OR game.id = @GameId)
      AND (@UserId IS NULL OR game.user_id = @UserId)
)
SELECT
    game_id,
    game_name,
    preset_id,
    preset_name,
    item_group_id,
    item_group_name,
    listing_limit,
    cooldown_minutes,
    cooldown_seconds,
    criterion_count = COUNT(sort_order),
    criteria_expression = STRING_AGG(expression_token, N' ')
        WITHIN GROUP (ORDER BY sort_order)
FROM ExpressionTokens
GROUP BY
    game_id,
    game_name,
    preset_id,
    preset_name,
    item_group_id,
    item_group_name,
    listing_limit,
    cooldown_minutes,
    cooldown_seconds
ORDER BY game_name, preset_name;

/* 9. Integrity audit. A healthy database returns no rows. */
;WITH OrderedCriteria AS
(
    SELECT
        preset_id = preset.id,
        preset_name = preset.name,
        game_id = game.id,
        game_name = game.name,
        criterion.sort_order,
        expected_sort_order = ROW_NUMBER() OVER
        (
            PARTITION BY preset.id
            ORDER BY criterion.sort_order
        ) - 1,
        criterion.condition_operator_id,
        criterion.open_group_count,
        criterion.close_group_count,
        criterion.name_contains,
        criterion.value_contains,
        running_depth = SUM(criterion.open_group_count - criterion.close_group_count) OVER
        (
            PARTITION BY preset.id
            ORDER BY criterion.sort_order
            ROWS UNBOUNDED PRECEDING
        ),
        final_depth = SUM(criterion.open_group_count - criterion.close_group_count) OVER
        (
            PARTITION BY preset.id
        ),
        total_groups = SUM(criterion.open_group_count) OVER
        (
            PARTITION BY preset.id
        ),
        criterion_count = COUNT(criterion.id) OVER
        (
            PARTITION BY preset.id
        )
    FROM dbo.manual_check_preset AS preset
    JOIN dbo.game AS game
      ON game.id = preset.game_id
    LEFT JOIN dbo.manual_check_criterion AS criterion
      ON criterion.manual_check_preset_id = preset.id
    WHERE (@GameId IS NULL OR game.id = @GameId)
      AND (@UserId IS NULL OR game.user_id = @UserId)
)
SELECT DISTINCT
    game_id,
    game_name,
    preset_id,
    preset_name,
    issue =
        CASE
            WHEN criterion_count = 0 THEN N'Preset has no criteria'
            WHEN criterion_count > 25 THEN N'Preset has more than 25 criteria'
            WHEN sort_order <> expected_sort_order THEN N'Criterion sort_order is not contiguous and zero-based'
            WHEN sort_order = 0 AND condition_operator_id IS NOT NULL THEN N'First criterion has an operator'
            WHEN sort_order > 0 AND condition_operator_id IS NULL THEN N'Later criterion has no operator'
            WHEN name_contains IS NULL AND value_contains IS NULL THEN N'Criterion has no search term'
            WHEN open_group_count NOT BETWEEN 0 AND 25 THEN N'Invalid open_group_count'
            WHEN close_group_count NOT BETWEEN 0 AND 25 THEN N'Invalid close_group_count'
            WHEN running_depth < 0 THEN N'Criterion closes a group that was not opened'
            WHEN running_depth > 25 THEN N'Criterion nesting exceeds 25 levels'
            WHEN total_groups > 25 THEN N'Preset contains more than 25 explicit groups'
            WHEN final_depth <> 0 THEN N'Preset contains unclosed groups'
        END
FROM OrderedCriteria
WHERE criterion_count = 0
   OR criterion_count > 25
   OR sort_order <> expected_sort_order
   OR (sort_order = 0 AND condition_operator_id IS NOT NULL)
   OR (sort_order > 0 AND condition_operator_id IS NULL)
   OR (name_contains IS NULL AND value_contains IS NULL)
   OR open_group_count NOT BETWEEN 0 AND 25
   OR close_group_count NOT BETWEEN 0 AND 25
   OR running_depth NOT BETWEEN 0 AND 25
   OR total_groups > 25
   OR final_depth <> 0
ORDER BY game_name, preset_name, issue;
