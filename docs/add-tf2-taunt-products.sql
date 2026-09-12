SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @UserId nvarchar(450) = N'REPLACE_WITH_USER_ID';
DECLARE @GameId bigint;
DECLARE @TagId bigint;
DECLARE @InsertedProducts int = 0;
DECLARE @InsertedProductTags int = 0;

IF @UserId = N'REPLACE_WITH_USER_ID' OR NULLIF(LTRIM(RTRIM(@UserId)), N'') IS NULL
    THROW 50000, 'Set @UserId before running this script.', 1;

IF (SELECT COUNT(*)
    FROM dbo.game
    WHERE name = N'Team Fortress 2'
      AND user_id = @UserId) <> 1
    THROW 50001, 'Expected exactly one Team Fortress 2 game for @UserId.', 1;

SELECT @GameId = id
FROM dbo.game
WHERE name = N'Team Fortress 2'
  AND user_id = @UserId;

IF (SELECT COUNT(*)
    FROM dbo.tag
    WHERE game_id = @GameId
      AND name = N'Taunt'
      AND user_id = @UserId) <> 1
    THROW 50002, 'Expected exactly one Taunt tag for the Team Fortress 2 game and @UserId.', 1;

SELECT @TagId = id
FROM dbo.tag
WHERE game_id = @GameId
  AND name = N'Taunt'
  AND user_id = @UserId;

DECLARE @ProductNames table
(
    name nvarchar(255) NOT NULL PRIMARY KEY
);

INSERT INTO @ProductNames (name)
VALUES
    (N'Bad Pipes'),
    (N'Bare Knuckle Beatdown'),
    (N'Battin'' a Thousand'),
    (N'Bear Hug'),
    (N'Borrowed Bones'),
    (N'Bucking Bronco'),
    (N'Buffoon''s Bivouac'),
    (N'Burstchester'),
    (N'Buy a Life'),
    (N'Can It!'),
    (N'Chairholder'),
    (N'Cheers!'),
    (N'Commending Clap'),
    (N'Conga'),
    (N'Cremators Condolences'),
    (N'Crushing Defeat'),
    (N'Curtain Call'),
    (N'Dead Mann''s Drink'),
    (N'Deep Fried Desire'),
    (N'Didgeridrongo'),
    (N'Disco Fever'),
    (N'Doctor''s Defibrillators'),
    (N'Drunk Mann''s Cannon'),
    (N'Faux-calization'),
    (N'Flippin'' Awesome'),
    (N'Flying Colors'),
    (N'Fore-Head Slice'),
    (N'Foul Play'),
    (N'Fresh Brewed Victory'),
    (N'Friendly Fire'),
    (N'Healthcare Hog'),
    (N'Heartbreaker'),
    (N'I See You'),
    (N'Kazotsky Kick'),
    (N'Killer Joke'),
    (N'Luxury Lounge'),
    (N'Mannrobics'),
    (N'Most Wanted'),
    (N'Mourning Mercs'),
    (N'Neck Snap'),
    (N'Oblooterated'),
    (N'Panzer Pants'),
    (N'Party Trick'),
    (N'Peace Out'),
    (N'Peace!'),
    (N'Pool Party'),
    (N'Rancho Relaxo'),
    (N'Results Are In'),
    (N'Ring King'),
    (N'Roar O''War'),
    (N'Roasty Toasty'),
    (N'Rock, Paper, Scissors'),
    (N'Rocket Jockey'),
    (N'Runner''s Rhythm'),
    (N'Russian Rubdown'),
    (N'Scorcher''s Solo'),
    (N'Scotsmann''s Stagger'),
    (N'Second Rate Sorcery'),
    (N'Shanty Shipmate'),
    (N'Shooter''s Stakeout'),
    (N'Skullcracker'),
    (N'Soldier''s Requiem'),
    (N'Spent Well Spirits'),
    (N'Spin-to-Win'),
    (N'Square Dance'),
    (N'Star-Spangled Strategy'),
    (N'Straight Shooter Tutor'),
    (N'Surgeon''s Squeezebox'),
    (N'Tailored Terminal'),
    (N'Teufort Tango'),
    (N'Texan Trickshot'),
    (N'Texas Truckin'''),
    (N'Texas Twirl ''Em'),
    (N'The Balloonibouncer'),
    (N'The Boiling Point'),
    (N'The Boston Boarder'),
    (N'The Boston Breakdance'),
    (N'The Box Trot'),
    (N'The Bunnyhopper'),
    (N'The Carlton'),
    (N'The Circuit Breaker'),
    (N'The Critical Fail'),
    (N'The Crypt Creeper'),
    (N'The Director''s Vision'),
    (N'The Drunken Sailor'),
    (N'The Dueling Banjo'),
    (N'The Final Score'),
    (N'The Fist Bump'),
    (N'The Fubar Fanfare'),
    (N'The Head Doctor'),
    (N'The Headcase'),
    (N'The High Five!'),
    (N'The Homerunner''s Hobby'),
    (N'The Hot Wheeler'),
    (N'The Jumping Jack'),
    (N'The Killer Signature'),
    (N'The Killer Solo'),
    (N'The Mannbulance!'),
    (N'The Meet the Medic'),
    (N'The Pooped Deck'),
    (N'The Profane Puppeteer'),
    (N'The Proletariat Posedown'),
    (N'The Punchline'),
    (N'The Road Rager'),
    (N'The Russian Arms Race'),
    (N'The Scaredy-Cat!'),
    (N'The Schadenfreude'),
    (N'The Scooty Scoot'),
    (N'The Shred Alert'),
    (N'The Skating Scorcher'),
    (N'The Soviet Strongarm'),
    (N'The Table Tantrum'),
    (N'The Trackman''s Touchdown'),
    (N'The Travel Agent'),
    (N'The Victory Lap'),
    (N'Time Out Therapy'),
    (N'Unleashed Rage'),
    (N'Yeti Punch'),
    (N'Yeti Smash'),
    (N'Zoomin'' Broom');

BEGIN TRY
    BEGIN TRANSACTION;

    INSERT INTO dbo.product
    (
        game_id,
        name,
        rating,
        is_active,
        user_id
    )
    SELECT
        @GameId,
        names.name,
        NULL,
        CAST(1 AS bit),
        @UserId
    FROM @ProductNames AS names
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.product AS product WITH (UPDLOCK, HOLDLOCK)
        WHERE product.game_id = @GameId
          AND product.user_id = @UserId
          AND product.name = names.name
    );

    SET @InsertedProducts = @@ROWCOUNT;

    INSERT INTO dbo.product_tags
    (
        product_id,
        tag_id
    )
    SELECT
        product.id,
        @TagId
    FROM dbo.product AS product
    INNER JOIN @ProductNames AS names
        ON names.name = product.name
    WHERE product.game_id = @GameId
      AND product.user_id = @UserId
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.product_tags AS productTag WITH (UPDLOCK, HOLDLOCK)
          WHERE productTag.product_id = product.id
            AND productTag.tag_id = @TagId
      );

    SET @InsertedProductTags = @@ROWCOUNT;

    COMMIT TRANSACTION;

    SELECT
        @GameId AS game_id,
        @TagId AS tag_id,
        @InsertedProducts AS inserted_products,
        @InsertedProductTags AS inserted_product_tags;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
