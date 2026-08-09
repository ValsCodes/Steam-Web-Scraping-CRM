# Graph Report - SteamApp  (2026-08-08)

## Corpus Check
- 517 files · ~134,753 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 3366 nodes · 6480 edges · 237 communities (198 shown, 39 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 221 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- feedback-request.model.ts
- ManualModeV2
- TagService
- handleError
- ScrapingModeService
- .CreateAsync
- pages/index.ts
- WebScraperComponent
- SteamApp.Infrastructure.Context
- ApplicationDbContext
- FakeWishlistService
- devDependencies
- PixelsView
- ProductForm
- SteamApp.WebAPI.Services
- SteamApp.Domain.Enums
- SteamApp.Application.Mapper
- wish-lists-view.ts
- AuthController
- SecurityConfigurationTests
- Container (Dockerfile)
- EncryptionHashingService
- .CreateController
- SteamController
- RadioButtonsFilterComponent
- dependencies
- .CreateController
- AuthService
- WishListsView
- GameUrlsView
- SteamApp.Application.JsonObjects
- Подробно описание на проекта
- EmailService
- What You Must Do When Invoked
- Program
- .CreateMemoryCache
- enums/index.ts
- WatchListsView
- SteamServiceTests
- WatchItemDto
- SteamApp.Interfaces.Services
- app.component.ts
- WishlistCheckConsumer
- ProductsView
- WishlistJobIntegrationTests
- rabbitmq1
- SteamApp.Migrations
- ApplicationDbContextModelSnapshot.cs
- .GetAsync
- SteamApp.Application.Utilities
- GameUrlForm
- LoginComponent
- AdminUsersPage
- FakeEmailSmtpClient
- BackgroundWorkerService
- ProfilePage
- AdminUserEndpoints
- error-handler.ts
- TestDb
- SteamApp.Domain.Common
- TransientRetryPolicyService
- CancellationToken
- SecurityPipelineIntegrationTests
- CopyLinkComponent
- RecordingEmailSmtpClient
- IWishlistNotificationRecipientService
- SteamApp.WebAPI
- SteamApp.csproj
- options
- SteamApp.Interfaces
- IdentityRoleInitializer
- dependencies
- tsconfig.integration.json
- WatchListDto
- .GetHttpResposeAsync
- Required Review
- .CreateAnonymousClient
- SteamControllerIntegrationTests
- OneShotHttpServer
- SteamApp.Infrastructure
- SteamApp.Domain.csproj
- SteamApp.IntegrationTests
- IEmailSmtpClient
- AuthGuard
- ExtraPixelManager
- .CreateAuthenticatedClient
- TagsView
- SteamAppFactory
- SteamApp.Tests
- GamesView
- auth.service.ts
- GameUrlDto
- ProductDto
- .ResetDatabaseAsync
- DistributedCacheExtensions
- PageWindow
- SteamApp.Application.csproj
- development
- mock-api.ts
- RabbitMqOptions
- CountdownTimerComponent
- DropdownComponent
- SteamApiClient
- CapturingMessagePublisher
- xlsx
- SteamApp.Application.DTOs.Tag
- SteamService
- IdentityRoleInitializerTests
- production
- BaseApiClient
- FakeAuthenticationHandler
- .ParseSteamPrice
- .JsonResponsesUseCamelCase
- SteamApp.IntegrationTests.Support
- AuthApiClient
- steam-app-angular-client
- ComboBoxComponent
- NumberFilterComponent
- SteamApp.E2ETests
- SteamApp.Application.DTOs.GameUrl
- ScrapeRequestedConsumer
- WishListForm
- API Reference (Quick)
- .SteamScrapeEndpointsRequireAuthReturnMappedResultsAndUseCache
- angular.json
- architect
- TagFilterSelectComponent
- .ReadRequiredJsonAsync
- .UpdateProfile
- .Register
- ISteamService
- assets
- about-page.ts
- faq-page.ts
- Getting Started
- Steam Web Scraping CRM
- scripts
- SteamApp.Application.DTOs.GameUrlProduct
- SteamApp.Application.DTOs.GameUrlPixel
- .PagedCatalogEndpointsFilterSortAndClampForAUserJourney
- .ClientCredentialsTokenUnlocksProtectedApiSession
- .AddRabbitMqMessageBroker
- SteamApp.WebAPI.Migrations
- db_v2
- Migration
- GameUrlName
- EntitiesRework
- WacthListIsActive
- IsProductActive
- ProductRating
- ProductTags
- FixProductsTag
- GameInternalId_GameUrlPixelLocation
- CustomWatchlistUrl
- CustomWatchlistRework
- AddScrapingModeToGameUrl
- SeedScrapingModes
- BackfillGameUrlScrapingModes
- SyncModelChanges
- AddUserProfileFields
- AddUserOwnership
- IdentitySchemaInitializer
- ClaimsPrincipalExtensions.cs
- ItemNotFoundException.cs
- SecurityHeadersMiddlewareExtensions.cs
- .ReadJsonElementAsync
- Architecture Overview
- .ApiFlowsPersistRelationshipsThroughEfAndExposeProjectedNames
- .BackgroundWishlistWorkflowSendsEmailOnceAndUsesCacheOnSecondRun
- ITransientRetryPolicyService
- .MaterializeResultsIfNeeded
- SeoMetaService
- graphify reference: extra exports and benchmark
- Contributing
- Self-Hosted Mailserver
- Q: can we create a click once .exe or .bat file that would start the server and the client in Release mode and use the Local db connection instead of hosting the db
- Q: The launcher reports port 7443 already in use and it didn't open the browser automatically
- IMessagePublisher
- local-release
- CheckboxesFilterComponent
- TextFilterComponent
- components/index.ts
- home-page.ts
- .SeedAsync
- CacheKeys.cs
- .ProjectGameUrlDtos
- AddAutomatedScrapeHistory
- graphify reference: query, path, explain
- GameEndpoints
- GameUrlPixelsEndpoints
- PixelEndpoints
- ProductEndpoints
- ProductTagsEndpoints
- ScrapingModeEndpoints
- TagsEndpoints
- WatchListEndpoints
- WishListEndpoints
- SecurityPolicies.cs
- error-dialog.service.ts
- jest.integration.config.cjs
- environment.development.ts
- environment.production.ts
- pricing-page.ts
- copy-link.component.unit.spec.ts
- SteamApp.Application.DTOs
- .CreateService
- graphify reference: add a URL and watch a folder
- graphify reference: commit hook and native CLAUDE.md integration
- graphify reference: incremental update and cluster-only
- 7. Common troubleshooting
- SteamAppAngularClient
- ItemCreateResult.cs
- graphify reference: GitHub clone and cross-repo merge
- graphify reference: transcribe video and audio
- AGENTS.md
- @angular/forms
- extraction-spec.md
- env/README.md
- jasmine-core
- Q: did you allow the two the to communicate as I am getting Invalid user credentials. from the server
- karma
- karma-jasmine-html-reporter
- rxjs
- tailwindcss
- zone.js
- environment.local-release.ts
- .CheckWishlistItem
- jest-environment-jsdom

## God Nodes (most connected - your core abstractions)
1. `handleError()` - 101 edges
2. `SteamApp.Infrastructure.Context` - 55 edges
3. `SteamApp.Domain.Entities` - 55 edges
4. `AuthService` - 54 edges
5. `WebScraperComponent` - 42 edges
6. `GameService` - 42 edges
7. `SteamApp.WebAPI.Migrations` - 38 edges
8. `Подробно описание на проекта` - 35 edges
9. `SteamApp.Domain.Enums` - 33 edges
10. `SecurityConfigurationTests` - 33 edges

## Surprising Connections (you probably didn't know these)
- `WebScraperComponent` --references--> `StopwatchComponent`  [EXTRACTED]
  SteamApp.Client/src/app/pages/web-scraper/web-scraper.component.ts → SteamApp.Client/src/app/components/stopwatch.component.ts
- `ManualModeV2` --references--> `GameUrlProduct`  [EXTRACTED]
  SteamApp.Client/src/app/pages/manual-mode-v2/manual-mode-v2.ts → SteamApp.Client/src/app/models/game-url-product.model.ts
- `ManualModeV2` --references--> `GameUrl`  [EXTRACTED]
  SteamApp.Client/src/app/pages/manual-mode-v2/manual-mode-v2.ts → SteamApp.Client/src/app/models/game-url.model.ts
- `ManualModeV2` --references--> `Game`  [EXTRACTED]
  SteamApp.Client/src/app/pages/manual-mode-v2/manual-mode-v2.ts → SteamApp.Client/src/app/models/game.model.ts
- `ManualModeV2` --references--> `Tag`  [EXTRACTED]
  SteamApp.Client/src/app/pages/manual-mode-v2/manual-mode-v2.ts → SteamApp.Client/src/app/models/tag.model.ts

## Import Cycles
- None detected.

## Communities (237 total, 39 thin omitted)

### Community 0 - "feedback-request.model.ts"
Cohesion: 0.06
Nodes (27): CreateFeedbackRequest, FEEDBACK_REQUEST_HISTORY_ACTION_OPTIONS, FEEDBACK_REQUEST_STATUS_OPTIONS, FEEDBACK_REQUEST_TYPE_OPTIONS, FeedbackRequest, FeedbackRequestHistory, FeedbackRequestHistoryAction, feedbackRequestHistoryActionLabel() (+19 more)

### Community 1 - "ManualModeV2"
Cohesion: 0.05
Nodes (28): Directive, HostBinding, Status, formatMs(), ExternalLinkDirective, HostListener, Input, ExternalLinkHostComponent (+20 more)

### Community 2 - "TagService"
Cohesion: 0.12
Nodes (8): CreateTag, Tag, UpdateTag, UpdateTagStatus, TagForm, Component, TagService, Injectable

### Community 3 - "handleError"
Cohesion: 0.11
Nodes (11): getListingUrl(), Listing, WhishListResponse, ScrapeHistoryDetail, ScrapeHistoryRerunResponse, ScrapeJobAccepted, ScraperExecutionMode, ScraperExecutionModeItem (+3 more)

### Community 4 - "ScrapingModeService"
Cohesion: 0.22
Nodes (5): CreateScrapingMode, ScrapingMode, UpdateScrapingMode, ScrapingModeService, Injectable

### Community 5 - ".CreateAsync"
Cohesion: 0.10
Nodes (17): PageJson, Task, Test, AdminUserEndpointTests, HttpResponseMessage, Task, Test, TestCase (+9 more)

### Community 6 - "pages/index.ts"
Cohesion: 0.10
Nodes (16): CONSTANTS, ConfirmDialogComponent, ConfirmDialogData, Component, CreateGame, Game, UpdateGame, UpdateGameStatus (+8 more)

### Community 7 - "WebScraperComponent"
Cohesion: 0.07
Nodes (8): ScrapeHistory, ScrapeJobStatus, ScrapeHistoryDialogComponent, ScrapeHistoryJsonDialogComponent, Component, Component, ViewChild, WebScraperComponent

### Community 8 - "SteamApp.Infrastructure.Context"
Cohesion: 0.08
Nodes (30): SteamApp.WebAPI.Security, SteamApp.Domain.ValueObjects.Authentication, SteamApp.WebAPI.MinimalAPIs, SteamApp.Application.DTOs.Product, SteamApp.Tests.Security, SteamApp.IntegrationTests.Data, SteamApp.Tests.Controllers, SteamApp.Tests.TestSupport (+22 more)

### Community 9 - "ApplicationDbContext"
Cohesion: 0.07
Nodes (26): DbSet, EntityTypeBuilder, IdentityDbContext, IdentityRole, ModelBuilder, ApplicationDbContext, Task, ISteamRepository (+18 more)

### Community 10 - "FakeWishlistService"
Cohesion: 0.07
Nodes (27): ConcurrentDictionary, WhishListResponse, WishListCreateDto, WishListDto, CancellationToken, IEnumerable, IWebElement, Task (+19 more)

### Community 11 - "devDependencies"
Cohesion: 0.07
Nodes (27): @angular/cli, @angular/compiler-cli, @angular-devkit/build-angular, jest, jest-preset-angular, karma-chrome-launcher, karma-coverage, karma-jasmine (+19 more)

### Community 12 - "PixelsView"
Cohesion: 0.07
Nodes (12): CreatePixel, Pixel, PixelListItem, UpdatePixel, UpdatePixelStatus, PixelForm, Component, PixelsView (+4 more)

### Community 13 - "ProductForm"
Cohesion: 0.07
Nodes (12): CreateProduct, Product, UpdateProduct, UpdateProductStatus, CreateProductTag, ProductTag, ProductForm, Component (+4 more)

### Community 14 - "SteamApp.WebAPI.Services"
Cohesion: 0.11
Nodes (19): SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Consumers, SteamApp.WebAPI.Caching, SteamApp.WebAPI.MessageBrokers.Handlers.Wishlist, SteamApp.WebAPI.MessageBrokers.Abstractions, SteamApp.WebAPI.Services, SteamApp.WebAPI.Jobs, SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Publishing, SteamApp.WebAPI.MessageBrokers.Handlers.Scraping (+11 more)

### Community 15 - "SteamApp.Domain.Enums"
Cohesion: 0.07
Nodes (23): SteamApp.Application.DTOs.ScrapeHistory, SteamApp.Domain.Enums, SteamApp.Application.DTOs.FeedbackRequest, FeedbackRequestCreateDto, DateTime, FeedbackRequestDto, DateTime, FeedbackRequestHistoryDto (+15 more)

### Community 16 - "SteamApp.Application.Mapper"
Cohesion: 0.07
Nodes (21): SteamApp.Application.DTOs.WatchListItem, SteamApp.Application.DTOs.ScrapingMode, SteamApp.Application.DTOs.WatchList, SteamApp.Application.Mapper, Profile, ScrapingModeCreateDto, ScrapingModeDto, ScrapingModeUpdateDto (+13 more)

### Community 17 - "wish-lists-view.ts"
Cohesion: 0.12
Nodes (10): StatusDialogComponent, StatusDialogData, StatusDialogVariant, Component, CreateWishList, UpdateWishList, UpdateWishListStatus, WishList (+2 more)

### Community 18 - "AuthController"
Cohesion: 0.12
Nodes (11): Claim, ControllerBase, IdentityUser, ApplicationUser, DateTime, AuthResponse, TokenRequest, UserProfileResponse (+3 more)

### Community 19 - "SecurityConfigurationTests"
Cohesion: 0.16
Nodes (7): IConfiguration, IHostEnvironment, string, Task, Test, TestCase, SecurityConfigurationTests

### Community 20 - "Container (Dockerfile)"
Cohesion: 0.06
Nodes (37): commandName, environmentVariables, launchBrowser, launchUrl, publishAllPorts, useSSL, ASPNETCORE_ENVIRONMENT, ASPNETCORE_HTTP_PORTS (+29 more)

### Community 21 - "EncryptionHashingService"
Cohesion: 0.10
Nodes (10): ParsedHash, int, string, EncryptionHashingService, ParsedHash, string, EncryptionHashingOptions, IEncryptionHashingService (+2 more)

### Community 22 - ".CreateController"
Cohesion: 0.19
Nodes (9): LogLevel, IDistributedCache, ILogger, IMemoryCache, Mock, Task, Test, SteamControllerTests (+1 more)

### Community 23 - "SteamController"
Cohesion: 0.05
Nodes (46): HttpMessageHandler, HttpRequestMessage, DateTime, ScrapeHistoryDetailDto, DateTime, ScrapeHistorySetupDto, DateTime, ScrapeHistorySummaryDto (+38 more)

### Community 24 - "RadioButtonsFilterComponent"
Cohesion: 0.20
Nodes (5): RadioButtonsFilterComponent, RadioOption, Component, Input, Output

### Community 25 - "dependencies"
Cohesion: 0.07
Nodes (27): @angular/animations, @angular/cdk, @angular/common, @angular/compiler, @angular/core, @angular/material, @angular/platform-browser, @angular/platform-browser-dynamic (+19 more)

### Community 26 - ".CreateController"
Cohesion: 0.21
Nodes (10): SignInManager, IConfiguration, IHostEnvironment, IReadOnlyList, Mock, string, Task, Test (+2 more)

### Community 27 - "AuthService"
Cohesion: 0.11
Nodes (4): SessionExpiredPage, Component, AuthService, Injectable

### Community 28 - "WishListsView"
Cohesion: 0.13
Nodes (3): Component, ViewChild, WishListsView

### Community 29 - "GameUrlsView"
Cohesion: 0.09
Nodes (9): CreateGameUrl, GameUrl, UpdateGameUrl, UpdateGameUrlStatus, GameUrlsView, Component, ViewChild, GameUrlService (+1 more)

### Community 30 - "SteamApp.Application.JsonObjects"
Cohesion: 0.09
Nodes (19): SteamApp.Application.JsonObjects, IDictionary, Action, AppData, IList, Asset, AssetDescription, IList (+11 more)

### Community 31 - "Подробно описание на проекта"
Cohesion: 0.06
Nodes (35): 10) Нефункционални изисквания, 11) Какво трябва да включва „детайлната документация“ (препоръка), 12) Кратко резюме, 1) Какво представлява проектът, 2) Бизнес цел и проблем, който решава, 3.1 Каталог и конфигурация, 3.2 Релации (M2M), 3.3 Оперативно следене (+27 more)

### Community 32 - "EmailService"
Cohesion: 0.14
Nodes (14): IOptions, ILogger, EmailService, CancellationToken, ConcurrentQueue, IReadOnlyCollection, Task, CapturingEmailService (+6 more)

### Community 33 - "What You Must Do When Invoked"
Cohesion: 0.08
Nodes (24): For /graphify add and --watch, For /graphify query, For the commit hook and native CLAUDE.md integration, For --update and --cluster-only, /graphify, Honesty Rules, Interpreter guard for subcommands, Part A - Structural extraction for code files (+16 more)

### Community 34 - "Program"
Cohesion: 0.12
Nodes (15): AuthorizationPolicy, HttpContext, ClientDefinition, JwtSettings, CancellationToken, IConfiguration, IHostEnvironment, ILogger (+7 more)

### Community 35 - ".CreateMemoryCache"
Cohesion: 0.21
Nodes (9): MemoryCache, Task, SteamRepository, Task, Test, SteamRepositoryTests, Task, Test (+1 more)

### Community 36 - "enums/index.ts"
Cohesion: 0.12
Nodes (22): activityFilterIds, ActivityFilters, activityFiltersCollection, activityFiltersMap, Class, classesCollection, classesMap, classFiltersCollection (+14 more)

### Community 37 - "WatchListsView"
Cohesion: 0.08
Nodes (11): CreateWatchList, UpdateWatchList, UpdateWatchListStatus, WatchList, Component, WatchListForm, Component, ViewChild (+3 more)

### Community 38 - "SteamServiceTests"
Cohesion: 0.23
Nodes (4): CancelAfter, Task, Test, SteamServiceTests

### Community 39 - "WatchItemDto"
Cohesion: 0.20
Nodes (9): IEnumerable, ScrapeHistoryRerunResponseDto, WatchItemDto, IEnumerable, IReadOnlyCollection, Task, FakeSteamService, IReadOnlyList (+1 more)

### Community 40 - "SteamApp.Interfaces.Services"
Cohesion: 0.12
Nodes (10): SteamApp.Infrastructure.Services, SteamApp.IntegrationTests.External, SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.DependencyInjection, SteamApp.Application.DTOs.WatchItem, SteamApp.Interfaces.Repositories, SteamApp.Interfaces.Services, SteamApp.Tests.Services, SteamApp.WebAPI (+2 more)

### Community 41 - "app.component.ts"
Cohesion: 0.10
Nodes (14): AppComponent, Component, appConfig, aboutPageStructuredData, faqPageStructuredData, routes, SiteFooter, Component (+6 more)

### Community 42 - "WishlistCheckConsumer"
Cohesion: 0.17
Nodes (9): BackgroundService, CancellationToken, Task, WishlistCheckMessageHandler, WishlistCheckRequested, CancellationToken, JsonSerializerOptions, Task (+1 more)

### Community 43 - "ProductsView"
Cohesion: 0.14
Nodes (3): ProductsView, Component, ViewChild

### Community 44 - "WishlistJobIntegrationTests"
Cohesion: 0.17
Nodes (11): IDistributedCache, JsonSerializerOptions, MemoryDistributedCache, string, Task, Test, WishlistJobIntegrationTests, CancellationToken (+3 more)

### Community 45 - "rabbitmq1"
Cohesion: 0.09
Nodes (22): dependencies, mssql1, rabbitmq1, redis1, connectionId, dynamicId, secretStore, type (+14 more)

### Community 46 - "SteamApp.Migrations"
Cohesion: 0.11
Nodes (9): SteamApp.Migrations, MigrationBuilder, ModelBuilder, AddFeedbackRequests, MigrationBuilder, ModelBuilder, AddFeedbackRequestHistory, MigrationBuilder (+1 more)

### Community 47 - "ApplicationDbContextModelSnapshot.cs"
Cohesion: 0.40
Nodes (3): ModelSnapshot, ModelBuilder, ApplicationDbContextModelSnapshot

### Community 48 - ".GetAsync"
Cohesion: 0.22
Nodes (9): CancellationToken, IEnumerable, Task, WishlistRepository, CancellationToken, IEnumerable, Task, IWishlistRepository (+1 more)

### Community 49 - "SteamApp.Application.Utilities"
Cohesion: 0.12
Nodes (9): SteamApp.Application.Utilities, string, FeedbackRequestReference, UrlUtilities, WebApplication, FeedbackRequestEndpoints, IEnumerable, WebApplication (+1 more)

### Community 51 - "LoginComponent"
Cohesion: 0.15
Nodes (4): AuthMode, LoginComponent, PasswordRequirement, Component

### Community 52 - "AdminUsersPage"
Cohesion: 0.19
Nodes (6): AdminUsersPage, Component, AdminUserEffectiveRole, AdminUserService, AdminUserSummary, Injectable

### Community 53 - "FakeEmailSmtpClient"
Cohesion: 0.26
Nodes (7): CancellationToken, Exception, MimeMessage, SecureSocketOptions, Task, ValueTask, FakeEmailSmtpClient

### Community 54 - "BackgroundWorkerService"
Cohesion: 0.36
Nodes (6): PeriodicTimer, CancellationToken, SemaphoreSlim, string, Task, BackgroundWorkerService

### Community 55 - "ProfilePage"
Cohesion: 0.18
Nodes (4): ProfilePage, Component, UpdateUserProfileRequest, UserProfile

### Community 56 - "AdminUserEndpoints"
Cohesion: 0.20
Nodes (9): IResult, CancellationToken, IdentityResult, IEnumerable, Task, UserManager, WebApplication, AdminUserEndpoints (+1 more)

### Community 57 - "error-handler.ts"
Cohesion: 0.07
Nodes (12): CreateGameUrlPixel, GameUrlPixel, CreateGameUrlProduct, GameUrlProduct, UpdateAdminUserRoleRequest, ErrorDialogBridge, getErrorMessage(), GameUrlPixelService (+4 more)

### Community 58 - "TestDb"
Cohesion: 0.19
Nodes (8): DbContextOptions, IDbContextFactory, IDisposable, InMemoryDatabaseRoot, string, TestDatabase, TestDb, TestDbContextFactory

### Community 59 - "SteamApp.Domain.Common"
Cohesion: 0.13
Nodes (12): Color, SteamApp.Domain.ValueObjects, SteamApp.Domain.Common, int, string, Constants, Dictionary, StaticCollections (+4 more)

### Community 60 - "TransientRetryPolicyService"
Cohesion: 0.14
Nodes (12): HttpStatusCode, CancellationToken, Exception, Func, Task, TimeSpan, TransientRetryPolicyService, string (+4 more)

### Community 61 - "CancellationToken"
Cohesion: 0.21
Nodes (7): SmtpClient, CancellationToken, MimeMessage, SecureSocketOptions, Task, ValueTask, MailKitEmailSmtpClient

### Community 62 - "SecurityPipelineIntegrationTests"
Cohesion: 0.33
Nodes (3): Task, Test, SecurityPipelineIntegrationTests

### Community 63 - "CopyLinkComponent"
Cohesion: 0.27
Nodes (3): CopyLinkComponent, Component, Input

### Community 64 - "RecordingEmailSmtpClient"
Cohesion: 0.21
Nodes (8): CancellationToken, MimeMessage, SecureSocketOptions, Task, Test, ValueTask, EmailServiceIntegrationTests, RecordingEmailSmtpClient

### Community 65 - "IWishlistNotificationRecipientService"
Cohesion: 0.15
Nodes (12): CancellationToken, IReadOnlyList, StubRecipientService, CancellationToken, IReadOnlyList, Task, IWishlistNotificationRecipientService, WishlistNotificationRecipient (+4 more)

### Community 66 - "SteamApp.WebAPI"
Cohesion: 0.13
Nodes (15): Microsoft.Azure.StackExchangeRedis (3.2.1), Microsoft.Extensions.Caching.StackExchangeRedis (9.0.2), RabbitMQ.Client (7.1.2), SteamApp.WebAPI, net9.0, HtmlAgilityPack (1.12.4), Microsoft.AspNetCore.Authentication.JwtBearer (9.0.15), Microsoft.AspNetCore.Mvc.NewtonsoftJson (9.0.15) (+7 more)

### Community 67 - "SteamApp.csproj"
Cohesion: 0.13
Nodes (14): Selenium.WebDriver (4.43.0), net9.0, DotNetSeleniumExtras.WaitHelpers (3.11.0), HtmlAgilityPack (1.12.4), Microsoft.AspNetCore.Authentication.JwtBearer (9.0.15), Microsoft.AspNetCore.Mvc.NewtonsoftJson (9.0.15), Microsoft.EntityFrameworkCore.Design (9.0.15), Microsoft.EntityFrameworkCore.SqlServer (9.0.15) (+6 more)

### Community 68 - "options"
Cohesion: 0.17
Nodes (15): @angular/material/prebuilt-themes/azure-blue.css, node_modules/@angular/material/prebuilt-themes/indigo-pink.css, src/scss/main.scss, zone.js, zone.js/testing, options, browser, index (+7 more)

### Community 69 - "SteamApp.Interfaces"
Cohesion: 0.18
Nodes (7): SteamApp.Interfaces, SteamApp.WebAPI.Jobs.Base, CancellationToken, Task, IJobService, TimeSpan, WorkerOptions

### Community 70 - "IdentityRoleInitializer"
Cohesion: 0.25
Nodes (7): HashSet, CancellationToken, Exception, IdentityResult, string, Task, IdentityRoleInitializer

### Community 71 - "dependencies"
Cohesion: 0.14
Nodes (13): dependencies, mssql1, rabbitmq1, redis1, connectionId, dynamicId, type, connectionId (+5 more)

### Community 72 - "tsconfig.integration.json"
Cohesion: 0.15
Nodes (12): jest, node, setup-jest.ts, src/**/*.d.ts, src/**/*.integration.test.ts, ./tsconfig.json, compilerOptions, outDir (+4 more)

### Community 73 - "WatchListDto"
Cohesion: 0.23
Nodes (8): DateOnly, WatchListCreateDto, DateOnly, WatchListDto, CancellationToken, List, Task, WatchListManager

### Community 74 - ".GetHttpResposeAsync"
Cohesion: 0.40
Nodes (3): CancellationToken, Task, HttpUtilities

### Community 75 - "Required Review"
Cohesion: 0.12
Nodes (16): 1. Architecture Alignment, 2. Required Change Logic, 3. Error Handling, 4. Maintainability, SOLID, Resource Ownership, and Lifetime Safety, 5. Asynchronous Execution, Cancellation, and Concurrency, 6. Tests and Verification Evidence, Blocking, Context Environment (+8 more)

### Community 76 - ".CreateAnonymousClient"
Cohesion: 0.17
Nodes (9): SteamApp.IntegrationTests.Startup, IReadOnlyDictionary, Task, Test, StartupConfigurationE2ETests, Task, Test, StartupIntegrationTests (+1 more)

### Community 77 - "SteamControllerIntegrationTests"
Cohesion: 0.38
Nodes (4): SteamApp.IntegrationTests.Controllers, Task, Test, SteamControllerIntegrationTests

### Community 78 - "OneShotHttpServer"
Cohesion: 0.13
Nodes (12): IAsyncDisposable, IConnection, Task, ValueTask, OneShotHttpServer, CancellationToken, SemaphoreSlim, Task (+4 more)

### Community 79 - "SteamApp.Infrastructure"
Cohesion: 0.17
Nodes (12): Konscious.Security.Cryptography.Argon2 (1.3.1), MailKit (4.17.0), Microsoft.AspNetCore.Identity.EntityFrameworkCore (9.0.15), Microsoft.AspNetCore.JsonPatch (9.0.15), Microsoft.Extensions.Options (9.0.15), System.Drawing.Common (9.0.15), SteamApp.Infrastructure, net9.0 (+4 more)

### Community 80 - "SteamApp.Domain.csproj"
Cohesion: 0.18
Nodes (8): MathNet.Numerics (5.0.0), Microsoft.EntityFrameworkCore (9.0.15), SteamApp.Interfaces, net9.0, Microsoft.NET.Sdk, net9.0, Newtonsoft.Json (13.0.4), Microsoft.NET.Sdk

### Community 81 - "SteamApp.IntegrationTests"
Cohesion: 0.17
Nodes (12): Microsoft.AspNetCore.Mvc.Testing (9.0.15), Microsoft.EntityFrameworkCore.Sqlite (9.0.15), SteamApp.IntegrationTests, net9.0, coverlet.collector (10.0.0), Microsoft.EntityFrameworkCore.InMemory (9.0.15), Microsoft.NET.Test.Sdk (18.5.1), Moq (4.20.72) (+4 more)

### Community 82 - "IEmailSmtpClient"
Cohesion: 0.24
Nodes (6): Queue, IEmailSmtpClient, IEmailSmtpClientFactory, MailKitEmailSmtpClientFactory, RecordingEmailSmtpClientFactory, FakeEmailSmtpClientFactory

### Community 84 - "ExtraPixelManager"
Cohesion: 0.29
Nodes (6): PixelCreateDto, PixelDto, CancellationToken, List, Task, ExtraPixelManager

### Community 85 - ".CreateAuthenticatedClient"
Cohesion: 0.26
Nodes (7): Task, Test, EfRepositoryIntegrationTests, DateTime, string, IntegrationJwt, DateTime

### Community 86 - "TagsView"
Cohesion: 0.17
Nodes (3): TagsView, Component, ViewChild

### Community 87 - "SteamAppFactory"
Cohesion: 0.24
Nodes (5): IWebHostBuilder, SqliteConnection, Dictionary, SteamAppFactory, WebApplicationFactory

### Community 88 - "SteamApp.Tests"
Cohesion: 0.18
Nodes (11): Microsoft.AspNetCore.TestHost (9.0.15), SteamApp.Tests, net9.0, coverlet.collector (10.0.0), Microsoft.EntityFrameworkCore.InMemory (9.0.15), Microsoft.NET.Test.Sdk (18.5.1), Moq (4.20.72), NUnit (4.6.0) (+3 more)

### Community 89 - "GamesView"
Cohesion: 0.15
Nodes (3): GamesView, Component, ViewChild

### Community 90 - "auth.service.ts"
Cohesion: 0.15
Nodes (6): AuthInterceptor, Injectable, ChangePasswordRequest, DeleteUserRequest, TokenResponse, createJwt()

### Community 91 - "GameUrlDto"
Cohesion: 0.29
Nodes (6): GameUrlCreateDto, GameUrlDto, CancellationToken, List, Task, GameUrlManager

### Community 92 - "ProductDto"
Cohesion: 0.29
Nodes (6): ProductCreateDto, ProductDto, CancellationToken, List, Task, ProductManager

### Community 93 - ".ResetDatabaseAsync"
Cohesion: 0.37
Nodes (5): Task, Test, TestCase, MinimalApiCrudIntegrationTests, Task

### Community 94 - "DistributedCacheExtensions"
Cohesion: 0.36
Nodes (6): CancellationToken, IDistributedCache, JsonSerializerOptions, Task, TimeSpan, DistributedCacheExtensions

### Community 95 - "PageWindow"
Cohesion: 0.24
Nodes (6): IReadOnlyCollection, PagedResponse, IQueryable, IReadOnlyCollection, PageWindow, PaginationExtensions

### Community 96 - "SteamApp.Application.csproj"
Cohesion: 0.20
Nodes (8): AutoMapper (15.1.3), Microsoft.Extensions.Configuration.Abstractions (9.0.15), net9.0, Newtonsoft.Json (13.0.4), Microsoft.NET.Sdk, net9.0, System.IdentityModel.Tokens.Jwt (8.18.0), Microsoft.NET.Sdk

### Community 97 - "development"
Cohesion: 0.20
Nodes (10): build, builder, configurations, defaultConfiguration, development, buildTarget, extractLicenses, fileReplacements (+2 more)

### Community 98 - "mock-api.ts"
Cohesion: 0.36
Nodes (6): corsHeaders, createJwt(), fulfillJson(), MockApiState, mockSteamApi(), signIn()

### Community 99 - "RabbitMqOptions"
Cohesion: 0.15
Nodes (10): CancellationToken, Task, TimeSpan, WishlistCheckJob, CancellationToken, JsonSerializerOptions, Task, WishlistNotificationConsumer (+2 more)

### Community 100 - "CountdownTimerComponent"
Cohesion: 0.16
Nodes (6): CountdownTimerComponent, Component, Input, SiteHeaderComponent, Component, CurrentUser

### Community 101 - "DropdownComponent"
Cohesion: 0.24
Nodes (4): DropdownComponent, Component, HostListener, Input

### Community 102 - "SteamApiClient"
Cohesion: 0.26
Nodes (8): BaseDto, GameCreateDto, GameDto, CancellationToken, List, Task, GameManager, SteamApiClient

### Community 103 - "CapturingMessagePublisher"
Cohesion: 0.24
Nodes (6): CancellationToken, ConcurrentQueue, IReadOnlyCollection, Task, CapturingMessagePublisher, PublishedMessage

### Community 105 - "SteamApp.Application.DTOs.Tag"
Cohesion: 0.22
Nodes (5): SteamApp.Application.DTOs.Tag, TagCreateDto, TagDto, TagUpdateDto, TagUpdateStatusDto

### Community 106 - "SteamService"
Cohesion: 0.27
Nodes (4): JsonUtilities, IEnumerable, Task, SteamService

### Community 107 - "IdentityRoleInitializerTests"
Cohesion: 0.42
Nodes (5): ServiceProvider, IConfiguration, Task, Test, IdentityRoleInitializerTests

### Community 108 - "production"
Cohesion: 0.22
Nodes (9): serve, production, budgets, buildTarget, fileReplacements, outputHashing, builder, configurations (+1 more)

### Community 109 - "BaseApiClient"
Cohesion: 0.39
Nodes (5): CancellationToken, DateTime, string, Task, BaseApiClient

### Community 110 - "FakeAuthenticationHandler"
Cohesion: 0.25
Nodes (6): AuthenticateResult, AuthenticationHandler, AuthenticationSchemeOptions, string, Task, FakeAuthenticationHandler

### Community 111 - ".ParseSteamPrice"
Cohesion: 0.29
Nodes (4): IWebElement, IWebElement, Mock, TestCase

### Community 112 - ".JsonResponsesUseCamelCase"
Cohesion: 0.39
Nodes (4): SteamApp.IntegrationTests.Contracts, Task, Test, ApiContractIntegrationTests

### Community 113 - "SteamApp.IntegrationTests.Support"
Cohesion: 0.21
Nodes (5): SteamApp.E2ETests.Security, SteamApp.IntegrationTests.Security, SteamApp.E2ETests.MinimalApis, SteamApp.E2ETests.Support, SteamApp.IntegrationTests.Support

### Community 114 - "AuthApiClient"
Cohesion: 0.25
Nodes (5): SteamApp.WebApiClient, CancellationToken, Task, AuthApiClient, TokenResponse

### Community 115 - "steam-app-angular-client"
Cohesion: 0.25
Nodes (8): steam-app-angular-client, style, @schematics/angular:component, prefix, projectType, root, schematics, sourceRoot

### Community 116 - "ComboBoxComponent"
Cohesion: 0.25
Nodes (4): ComboBoxComponent, KeyOf, Component, Input

### Community 117 - "NumberFilterComponent"
Cohesion: 0.25
Nodes (4): NumberFilterComponent, Component, Input, Output

### Community 118 - "SteamApp.E2ETests"
Cohesion: 0.25
Nodes (8): SteamApp.E2ETests, net9.0, coverlet.collector (10.0.0), Microsoft.NET.Test.Sdk (18.5.1), NUnit (4.6.0), NUnit3TestAdapter (6.2.0), NUnit.Analyzers (4.13.0), Microsoft.NET.Sdk

### Community 119 - "SteamApp.Application.DTOs.GameUrl"
Cohesion: 0.10
Nodes (10): SteamApp.WebApiClient.Managers, SteamApp.Application.DTOs.GameUrl, SteamApp.Application.DTOs.Pixel, SteamApp.Application.DTOs.Game, GameUpdateDto, GameUpdateStatusDto, GameUrlUpdateDto, GameUrlUpdateStatusDto (+2 more)

### Community 120 - "ScrapeRequestedConsumer"
Cohesion: 0.29
Nodes (5): ScrapeRequested, CancellationToken, JsonSerializerOptions, Task, ScrapeRequestedConsumer

### Community 122 - "API Reference (Quick)"
Cohesion: 0.15
Nodes (13): API Reference (Quick), Auth, Game URL Pixels (M2M), Game URL Products (M2M), Game URLs, Games, Pixels, Product Tags (M2M) (+5 more)

### Community 123 - ".SteamScrapeEndpointsRequireAuthReturnMappedResultsAndUseCache"
Cohesion: 0.38
Nodes (4): SteamApp.E2ETests.Controllers, Task, Test, SteamControllerE2ETests

### Community 124 - "angular.json"
Cohesion: 0.29
Nodes (6): cli, analytics, newProjectRoot, projects, $schema, version

### Community 125 - "architect"
Cohesion: 0.29
Nodes (7): extract-i18n, test, builder, options, buildTarget, architect, builder

### Community 126 - "TagFilterSelectComponent"
Cohesion: 0.29
Nodes (4): NamedTag, TagFilterSelectComponent, Component, Output

### Community 127 - ".ReadRequiredJsonAsync"
Cohesion: 0.33
Nodes (5): HttpClient, HttpResponseMessage, JsonElement, Task, E2EClientExtensions

### Community 128 - ".UpdateProfile"
Cohesion: 0.17
Nodes (11): Authorize, HttpDelete, HttpPut, ModelStateDictionary, ChangePasswordRequest, DeleteUserRequest, UpdateUserProfileRequest, HttpGet (+3 more)

### Community 129 - ".Register"
Cohesion: 0.22
Nodes (4): AllowAnonymous, LoginRequest, RegisterRequest, HttpPost

### Community 130 - "ISteamService"
Cohesion: 0.60
Nodes (3): IEnumerable, Task, ISteamService

### Community 131 - "assets"
Cohesion: 0.33
Nodes (6): src/assets, src/favicon.ico, src/manifest.webmanifest, src/robots.txt, src/sitemap.xml, assets

### Community 132 - "about-page.ts"
Cohesion: 0.40
Nodes (4): AboutAudience, AboutHighlight, AboutPage, Component

### Community 133 - "faq-page.ts"
Cohesion: 0.40
Nodes (4): FaqGroup, FaqItem, FaqPage, Component

### Community 134 - "Getting Started"
Cohesion: 0.15
Nodes (13): 1. Clone repository, 2. Configure backend secrets/config, 3. Backend startup, 4. Database migrations (if needed), 5. Frontend startup, 6. Authentication flow, 8. Useful commands, Backend (+5 more)

### Community 135 - "Steam Web Scraping CRM"
Cohesion: 0.15
Nodes (13): 1) Clone, 2) Start backend, 3) Start frontend, Backend, Core features, Current repository layout, Documentation index, Frontend (+5 more)

### Community 136 - "scripts"
Cohesion: 0.15
Nodes (12): name, private, scripts, build, ng, start, test, test:e2e (+4 more)

### Community 137 - "SteamApp.Application.DTOs.GameUrlProduct"
Cohesion: 0.40
Nodes (3): SteamApp.Application.DTOs.GameUrlProduct, GameUrlProductCreateDto, GameUrlProductDto

### Community 138 - "SteamApp.Application.DTOs.GameUrlPixel"
Cohesion: 0.22
Nodes (5): SteamApp.Application.DTOs.GameUrlPixel, GameUrlPixelCreateDto, GameUrlPixelDto, Test, ControllerSecurityMetadataTests

### Community 139 - ".PagedCatalogEndpointsFilterSortAndClampForAUserJourney"
Cohesion: 0.60
Nodes (3): Task, Test, CatalogManagementE2ETests

### Community 140 - ".ClientCredentialsTokenUnlocksProtectedApiSession"
Cohesion: 0.60
Nodes (3): Task, Test, SecurityE2ETests

### Community 141 - ".AddRabbitMqMessageBroker"
Cohesion: 0.60
Nodes (3): IConfiguration, IServiceCollection, RabbitMqServiceCollectionExtensions

### Community 142 - "SteamApp.WebAPI.Migrations"
Cohesion: 0.28
Nodes (4): SteamApp.WebAPI.Migrations, MigrationBuilder, ModelBuilder, db12

### Community 143 - "db_v2"
Cohesion: 0.29
Nodes (3): MigrationBuilder, ModelBuilder, db_v2

### Community 144 - "Migration"
Cohesion: 0.25
Nodes (4): Migration, MigrationBuilder, ModelBuilder, db_v21

### Community 145 - "GameUrlName"
Cohesion: 0.29
Nodes (3): MigrationBuilder, ModelBuilder, GameUrlName

### Community 146 - "EntitiesRework"
Cohesion: 0.29
Nodes (3): MigrationBuilder, ModelBuilder, EntitiesRework

### Community 147 - "WacthListIsActive"
Cohesion: 0.29
Nodes (3): MigrationBuilder, ModelBuilder, WacthListIsActive

### Community 148 - "IsProductActive"
Cohesion: 0.29
Nodes (3): MigrationBuilder, ModelBuilder, IsProductActive

### Community 149 - "ProductRating"
Cohesion: 0.29
Nodes (3): MigrationBuilder, ModelBuilder, ProductRating

### Community 150 - "ProductTags"
Cohesion: 0.29
Nodes (3): MigrationBuilder, ModelBuilder, ProductTags

### Community 151 - "FixProductsTag"
Cohesion: 0.29
Nodes (3): MigrationBuilder, ModelBuilder, FixProductsTag

### Community 152 - "GameInternalId_GameUrlPixelLocation"
Cohesion: 0.29
Nodes (3): MigrationBuilder, ModelBuilder, GameInternalId_GameUrlPixelLocation

### Community 153 - "CustomWatchlistUrl"
Cohesion: 0.29
Nodes (3): MigrationBuilder, ModelBuilder, CustomWatchlistUrl

### Community 154 - "CustomWatchlistRework"
Cohesion: 0.29
Nodes (3): MigrationBuilder, ModelBuilder, CustomWatchlistRework

### Community 155 - "AddScrapingModeToGameUrl"
Cohesion: 0.29
Nodes (3): MigrationBuilder, ModelBuilder, AddScrapingModeToGameUrl

### Community 156 - "SeedScrapingModes"
Cohesion: 0.29
Nodes (3): MigrationBuilder, ModelBuilder, SeedScrapingModes

### Community 158 - "SyncModelChanges"
Cohesion: 0.29
Nodes (3): MigrationBuilder, ModelBuilder, SyncModelChanges

### Community 159 - "AddUserProfileFields"
Cohesion: 0.29
Nodes (3): MigrationBuilder, ModelBuilder, AddUserProfileFields

### Community 160 - "AddUserOwnership"
Cohesion: 0.29
Nodes (3): MigrationBuilder, ModelBuilder, AddUserOwnership

### Community 161 - "IdentitySchemaInitializer"
Cohesion: 0.40
Nodes (4): CancellationToken, string, Task, IdentitySchemaInitializer

### Community 163 - "ItemNotFoundException.cs"
Cohesion: 0.50
Nodes (3): SteamApp.WebAPI.Exceptions, Exception, ItemNotFoundException

### Community 165 - ".ReadJsonElementAsync"
Cohesion: 0.33
Nodes (4): HttpResponseMessage, JsonElement, Task, JsonTestExtensions

### Community 166 - "Architecture Overview"
Cohesion: 0.20
Nodes (10): API composition, Architecture Overview, Backend layers, Background processing, Configuration model, Frontend organization, High-level system, Important many-to-many relationships (+2 more)

### Community 167 - ".ApiFlowsPersistRelationshipsThroughEfAndExposeProjectedNames"
Cohesion: 0.50
Nodes (3): Task, Test, RepositoryE2ETests

### Community 168 - ".BackgroundWishlistWorkflowSendsEmailOnceAndUsesCacheOnSecondRun"
Cohesion: 0.50
Nodes (3): Task, Test, WishlistServiceE2ETests

### Community 169 - "ITransientRetryPolicyService"
Cohesion: 0.22
Nodes (5): CancellationToken, Exception, Func, Task, ITransientRetryPolicyService

### Community 170 - ".MaterializeResultsIfNeeded"
Cohesion: 0.33
Nodes (4): IEnumerable, IReadOnlyList, IReadOnlyList, Task

### Community 172 - "graphify reference: extra exports and benchmark"
Cohesion: 0.22
Nodes (8): graphify reference: extra exports and benchmark, Step 6b - Wiki (only if --wiki flag), Step 7 - Neo4j export (only if --neo4j or --neo4j-push flag), Step 7a - FalkorDB export (only if --falkordb or --falkordb-push flag), Step 7b - SVG export (only if --svg flag), Step 7c - GraphML export (only if --graphml flag), Step 7d - MCP server (only if --mcp flag), Step 8 - Token reduction benchmark (only if total_words > 5000)

### Community 173 - "Contributing"
Cohesion: 0.22
Nodes (8): Backend, Branching and PRs, Coding guidelines, Commit message examples, Contributing, Frontend, Local checks, Reporting issues

### Community 174 - "Self-Hosted Mailserver"
Cohesion: 0.22
Nodes (9): Backend Configuration, Certificates, Configure, DNS, Local Smoke-Test Domain, Prerequisites, Self-Hosted Mailserver, Smoke Checks (+1 more)

### Community 175 - "Q: can we create a click once .exe or .bat file that would start the server and the client in Release mode and use the Local db connection instead of hosting the db"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: can we create a click once .exe or .bat file that would start the server and the client in Release mode and use the Local db connection instead of hosting the db, Source Nodes

### Community 176 - "Q: The launcher reports port 7443 already in use and it didn't open the browser automatically"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: The launcher reports port 7443 already in use and it didn't open the browser automatically, Source Nodes

### Community 177 - "IMessagePublisher"
Cohesion: 0.22
Nodes (6): CancellationToken, Task, IMessagePublisher, CancellationToken, Task, RabbitMqPublisher

### Community 178 - "local-release"
Cohesion: 0.25
Nodes (8): local-release, budgets, buildTarget, extractLicenses, fileReplacements, optimization, outputHashing, sourceMap

### Community 179 - "CheckboxesFilterComponent"
Cohesion: 0.25
Nodes (4): CheckboxesFilterComponent, Component, Input, Output

### Community 180 - "TextFilterComponent"
Cohesion: 0.25
Nodes (4): TextFilterComponent, Component, Input, Output

### Community 182 - "home-page.ts"
Cohesion: 0.40
Nodes (4): HomePage, HomeSectionCard, Component, Workflow

### Community 183 - ".SeedAsync"
Cohesion: 0.48
Nodes (4): IServiceProvider, string, Task, IntegrationSeed

### Community 185 - ".ProjectGameUrlDtos"
Cohesion: 0.38
Nodes (4): IQueryable, Task, WebApplication, GameUrlEndpoints

### Community 186 - "AddAutomatedScrapeHistory"
Cohesion: 0.29
Nodes (3): MigrationBuilder, ModelBuilder, AddAutomatedScrapeHistory

### Community 187 - "graphify reference: query, path, explain"
Cohesion: 0.33
Nodes (5): For /graphify explain, For /graphify path, graphify reference: query, path, explain, Step 0 — Constrained query expansion (REQUIRED before traversal), Step 1 — Traversal

### Community 198 - "error-dialog.service.ts"
Cohesion: 0.40
Nodes (3): ErrorDialogComponent, ErrorDialogData, Component

### Community 208 - "pricing-page.ts"
Cohesion: 0.40
Nodes (4): PricingAccent, PricingPage, PricingPlan, Component

### Community 211 - "SteamApp.Application.DTOs"
Cohesion: 0.29
Nodes (3): SteamApp.Application.DTOs, BaseUpdateDto, ProductTagCreateDto

### Community 213 - "graphify reference: add a URL and watch a folder"
Cohesion: 0.50
Nodes (3): For /graphify add, For --watch, graphify reference: add a URL and watch a folder

### Community 214 - "graphify reference: commit hook and native CLAUDE.md integration"
Cohesion: 0.50
Nodes (3): For git commit hook, For native CLAUDE.md integration, graphify reference: commit hook and native CLAUDE.md integration

### Community 215 - "graphify reference: incremental update and cluster-only"
Cohesion: 0.50
Nodes (3): For --cluster-only, For --update (incremental re-extraction), graphify reference: incremental update and cluster-only

### Community 216 - "7. Common troubleshooting"
Cohesion: 0.50
Nodes (4): 7. Common troubleshooting, Angular production build fails due Google Fonts inlining, API throws “Missing required configuration”, CORS/auth issues

### Community 217 - "SteamAppAngularClient"
Cohesion: 0.50
Nodes (3): Build, Development server, SteamAppAngularClient

### Community 218 - "ItemCreateResult.cs"
Cohesion: 0.40
Nodes (4): SteamApp.Infrastructure, SteamApp.Models.OperationResults, BaseOperationResult, ItemCreateResult

### Community 226 - "Q: did you allow the two the to communicate as I am getting Invalid user credentials. from the server"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: did you allow the two the to communicate as I am getting Invalid user credentials. from the server, Source Nodes

### Community 235 - ".CheckWishlistItem"
Cohesion: 0.50
Nodes (3): CancellationToken, Task, SteamManager

## Knowledge Gaps
- **479 isolated node(s):** `$schema`, `version`, `newProjectRoot`, `projectType`, `style` (+474 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **39 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Work-memory lessons

**Preferred sources** — corroborated by past sessions; start here.
- `local-release` (2× useful, score=1.99954733)
- `Program.cs` (2× useful, score=1.999351516)

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SteamApp.Infrastructure.Context` connect `SteamApp.Infrastructure.Context` to `SteamApp.WebAPI.Migrations`, `SteamApp.WebAPI.Services`, `Migration`, `db_v2`, `GameUrlName`, `EntitiesRework`, `WacthListIsActive`, `IsProductActive`, `ProductRating`, `ProductTags`, `FixProductsTag`, `CustomWatchlistUrl`, `CustomWatchlistRework`, `AddScrapingModeToGameUrl`, `SeedScrapingModes`, `GameInternalId_GameUrlPixelLocation`, `SyncModelChanges`, `BackfillGameUrlScrapingModes`, `AddUserProfileFields`, `AddUserOwnership`, `SteamApp.Interfaces.Services`, `SteamApp.Migrations`, `ApplicationDbContextModelSnapshot.cs`, `AddAutomatedScrapeHistory`, `SteamApp.Domain.Enums`?**
  _High betweenness centrality (0.073) - this node is a cross-community bridge._
- **Why does `SteamApp.Interfaces.Services` connect `SteamApp.Interfaces.Services` to `EmailService`, `ITransientRetryPolicyService`, `SteamApp.WebAPI.Services`, `SteamApp.IntegrationTests.Support`, `IEmailSmtpClient`, `EncryptionHashingService`, `TransientRetryPolicyService`?**
  _High betweenness centrality (0.031) - this node is a cross-community bridge._
- **Why does `SteamAppFactory` connect `SteamAppFactory` to `EmailService`, `Program`, `WatchItemDto`, `SteamApp.Interfaces.Services`, `CapturingMessagePublisher`, `FakeWishlistService`, `.CreateAnonymousClient`, `.CreateAuthenticatedClient`, `.ResetDatabaseAsync`?**
  _High betweenness centrality (0.025) - this node is a cross-community bridge._
- **Are the 84 inferred relationships involving `handleError()` (e.g. with `.getUsers()` and `.updateRole()`) actually correct?**
  _`handleError()` has 84 INFERRED edges - model-reasoned connections that need verification._
- **What connects `$schema`, `version`, `newProjectRoot` to the rest of the system?**
  _479 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `feedback-request.model.ts` be split into smaller, more focused modules?**
  _Cohesion score 0.059018367961457395 - nodes in this community are weakly interconnected._
- **Should `ManualModeV2` be split into smaller, more focused modules?**
  _Cohesion score 0.05157894736842105 - nodes in this community are weakly interconnected._