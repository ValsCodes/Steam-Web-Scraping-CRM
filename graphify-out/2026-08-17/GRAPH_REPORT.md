# Graph Report - SteamApp  (2026-08-17)

## Corpus Check
- 566 files · ~161,248 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 3902 nodes · 7747 edges · 258 communities (198 shown, 60 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 370 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `ffe3744e`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- feedback-request.model.ts
- web-scraper.component.ts
- ManualModeV2
- handleError
- ScrapeHistoryDataService
- .CreateAsync
- pages/index.ts
- WebScraperComponent
- SteamApp.Infrastructure.Context
- ApplicationDbContext
- FakeWishlistService
- devDependencies
- PixelService
- ProductForm
- SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Options
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
- WatchListService
- SteamServiceTests
- WatchItemDto
- SteamApp.Interfaces.Services
- app.component.ts
- RabbitMqOptions
- ProductsView
- WishlistJobIntegrationTests
- rabbitmq1
- SteamApp.Migrations
- ApplicationDbContextModelSnapshot.cs
- .GetAsync
- FeedbackRequestEndpoints
- GameUrlForm
- LoginComponent
- AdminUsersPage
- FakeEmailSmtpClient
- BackgroundWorkerService
- ProfilePage
- AdminUserEndpoints
- GameUrlProductService
- TestDb
- SteamApp.Domain.Common
- TransientRetryPolicyService
- CancellationToken
- .CreateAnonymousClient
- CopyLinkComponent
- RecordingEmailSmtpClient
- IWishlistNotificationRecipientService
- SteamApp.WebAPI
- SteamApp.csproj
- options
- WishlistCheckJob
- IdentityRoleInitializer
- dependencies
- tsconfig.integration.json
- WatchListDto
- .GetHttpResposeAsync
- Required Review
- StartupIntegrationTests
- .ResetDatabaseAsync
- RabbitMqConnection
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
- MinimalApiCrudIntegrationTests
- DistributedCacheExtensions
- PageWindow
- SteamApp.Application.csproj
- development
- mock-api.ts
- WishlistNotificationConsumer
- CountdownTimerComponent
- DropdownComponent
- GameDto
- CapturingMessagePublisher
- xlsx
- SteamApp.Application.DTOs.Tag
- SteamService
- IdentityRoleInitializerTests
- production
- BaseApiClient
- FakeAuthenticationHandler
- PixelsView
- .JsonResponsesUseCamelCase
- SteamApp.IntegrationTests.Support
- SteamApiClient
- steam-app-angular-client
- ComboBoxComponent
- NumberFilterComponent
- SteamApp.E2ETests
- MinimalApiCrudIntegrationTests.cs
- ScrapeRequestedConsumer
- WishListForm
- API Reference (Quick)
- .SteamScrapeEndpointsRequireAuthReturnMappedResultsAndUseCache
- angular.json
- architect
- TagFilterSelectComponent
- .ReadRequiredJsonAsync
- UpdateUserProfileRequest.cs
- LoginRequest.cs
- WatchListsView
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
- db12
- SteamApp.WebAPI.Migrations
- db_v21
- Migration
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
- AutomatedScrapeHistory
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
- ScrapeEndpointDefinitions
- home-page.ts
- .SeedAsync
- CacheKeys.cs
- SteamControllerTests.cs
- HttpMessageHandlerStub
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
- ScrapeHistorySummaryDto
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
- AddScrapeHistoryJobStatus
- AddManualCheckListingLimit
- environment.local-release.ts
- OneShotHttpServer
- .CreateJob
- AddFeedbackRequests
- .SwaggerIsExposedInDevelopmentAndHiddenBehindProductionApiSecurity
- SteamApp.Application.DTOs.ScrapingMode
- GameUrlProductsEndpoints
- .CreateTokenResponse
- Q: I still get Invalid user credentials. and the client doesn't open upon clicking the bat
- .attachTableControls
- JsonUtilities
- IntegrationJwt
- ISteamRepository
- ClientDefinition
- ControllerSecurityMetadataTests
- .ExecuteAsync
- ChangePasswordRequest.cs
- DeleteUserRequest.cs
- 20260515171509_AddUserOwnership.Designer.cs
- 20260627144144_AddFeedbackRequestHistory.Designer.cs
- ScrapingMode
- ScrapingModeEndpoints
- WatchListEndpoints
- WishListEndpoints

## God Nodes (most connected - your core abstractions)
1. `handleError()` - 111 edges
2. `SteamApp.Domain.Entities` - 60 edges
3. `SteamApp.Infrastructure.Context` - 58 edges
4. `AuthService` - 54 edges
5. `ManualModeV2` - 49 edges
6. `SteamApp.Domain.Enums` - 46 edges
7. `GameService` - 44 edges
8. `WebScraperComponent` - 42 edges
9. `SteamApp.WebAPI.Migrations` - 38 edges
10. `ApplicationDbContext` - 35 edges

## Surprising Connections (you probably didn't know these)
- `mockSteamApi()` --indirect_call--> `route()`  [INFERRED]
  SteamApp.Client/e2e/support/mock-api.ts → SteamApp.Client/src/app/services/auth/auth.guard.unit.spec.ts
- `createComponent()` --indirect_call--> `ExternalLinkDisclosurePage`  [INFERRED]
  SteamApp.Client/src/app/pages/external-link-disclosure/external-link-disclosure-page.unit.spec.ts → SteamApp.Client/src/app/pages/external-link-disclosure/external-link-disclosure-page.ts
- `createComponent()` --indirect_call--> `FeedbackRequestForm`  [INFERRED]
  SteamApp.Client/src/app/pages/feedback/feedback-request-form/feedback-request-form.unit.spec.ts → SteamApp.Client/src/app/pages/feedback/feedback-request-form/feedback-request-form.ts
- `setup()` --indirect_call--> `GameService`  [INFERRED]
  SteamApp.Client/src/app/pages/wish-list/wish-list-form/wish-list-form.unit.spec.ts → SteamApp.Client/src/app/services/game/game.service.ts
- `setup()` --indirect_call--> `GameService`  [INFERRED]
  SteamApp.Client/src/app/pages/wish-list/wish-lists-view/wish-lists-view.unit.spec.ts → SteamApp.Client/src/app/services/game/game.service.ts

## Import Cycles
- None detected.

## Communities (258 total, 60 thin omitted)

### Community 0 - "feedback-request.model.ts"
Cohesion: 0.06
Nodes (26): CreateFeedbackRequest, FEEDBACK_REQUEST_HISTORY_ACTION_OPTIONS, FEEDBACK_REQUEST_STATUS_OPTIONS, FEEDBACK_REQUEST_TYPE_OPTIONS, FeedbackRequest, FeedbackRequestHistory, FeedbackRequestHistoryAction, feedbackRequestHistoryActionLabel() (+18 more)

### Community 1 - "web-scraper.component.ts"
Cohesion: 0.06
Nodes (28): Directive, HostBinding, Status, CONSTANTS, formatMs(), ExternalLinkDirective, HostListener, Input (+20 more)

### Community 2 - "ManualModeV2"
Cohesion: 0.17
Nodes (3): ManualCheckCriterion, ManualCheckSetupDialogComponent, Component

### Community 3 - "handleError"
Cohesion: 0.05
Nodes (13): CreateTag, Tag, UpdateTag, UpdateTagStatus, ProductForm, Component, TagForm, Component (+5 more)

### Community 4 - "ScrapeHistoryDataService"
Cohesion: 0.20
Nodes (10): CancellationToken, Task, TimeSpan, ScrapeRequestedMessageHandler, CancellationToken, IReadOnlyList, Task, IScrapeHistoryDataService (+2 more)

### Community 5 - ".CreateAsync"
Cohesion: 0.10
Nodes (17): PageJson, Task, Test, AdminUserEndpointTests, HttpResponseMessage, Task, Test, TestCase (+9 more)

### Community 6 - "pages/index.ts"
Cohesion: 0.06
Nodes (21): ConfirmDialogComponent, ConfirmDialogData, Component, CreateGame, Game, UpdateGame, UpdateGameStatus, GameForm (+13 more)

### Community 8 - "SteamApp.Infrastructure.Context"
Cohesion: 0.08
Nodes (15): GameUrl, CreateGameUrlProduct, GameUrlProduct, CreateScrapingMode, ScrapingMode, ScrapingModeEnum, UpdateScrapingMode, ScraperExecutionMode (+7 more)

### Community 9 - "ApplicationDbContext"
Cohesion: 0.11
Nodes (4): ScrapeJobStatus, Component, ViewChild, WebScraperComponent

### Community 10 - "FakeWishlistService"
Cohesion: 0.27
Nodes (5): IWebElement, Mock, Task, Test, WishlistServiceTests

### Community 11 - "devDependencies"
Cohesion: 0.04
Nodes (47): @angular/cli, @angular/compiler-cli, @angular-devkit/build-angular, jasmine-core, jest, jest-environment-jsdom, jest-preset-angular, karma (+39 more)

### Community 12 - "PixelService"
Cohesion: 0.08
Nodes (12): CreatePixel, Pixel, PixelListItem, UpdatePixel, UpdatePixelStatus, PixelForm, Component, PixelsView (+4 more)

### Community 13 - "ProductForm"
Cohesion: 0.29
Nodes (4): SteamApp.Interfaces, SteamApp.WebAPI.Jobs.Base, TimeSpan, WorkerOptions

### Community 14 - "SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Options"
Cohesion: 0.11
Nodes (19): SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Consumers, SteamApp.WebAPI.Caching, SteamApp.WebAPI.MessageBrokers.Handlers.Wishlist, SteamApp.WebAPI.MessageBrokers.Abstractions, SteamApp.WebAPI.Jobs, SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Publishing, SteamApp.WebAPI.MessageBrokers.Handlers.Scraping, SteamApp.E2ETests.Services (+11 more)

### Community 15 - "SteamApp.Domain.Enums"
Cohesion: 0.08
Nodes (27): SteamApp.WebAPI.MinimalAPIs, SteamApp.Application.DTOs.Product, SteamApp.Application.Utilities, SteamApp.Tests.TestSupport, SteamApp.E2ETests.Repositories, SteamApp.WebAPI.Contracts.Pagination, SteamApp.Tests.Repositories, SteamApp.Application.Caching (+19 more)

### Community 16 - "SteamApp.Application.Mapper"
Cohesion: 0.04
Nodes (29): SteamApp.Application.DTOs.WatchListItem, SteamApp.WebApiClient.Managers, SteamApp.Application.DTOs.Tag, SteamApp.Application.DTOs.GameUrl, SteamApp.Application.DTOs.Game, SteamApp.Application.DTOs.WatchList, SteamApp.Application.Mapper, Profile (+21 more)

### Community 17 - "wish-lists-view.ts"
Cohesion: 0.05
Nodes (17): StatusDialogComponent, StatusDialogData, StatusDialogVariant, Component, CreateWishList, UpdateWishList, UpdateWishListStatus, WishList (+9 more)

### Community 18 - "AuthController"
Cohesion: 0.09
Nodes (21): Angular client, Architecture alignment, Async, workers, brokers, and caching, Blocking, Blocking Findings, Data access, DI, and resources, Decision, Errors, logging, and external boundaries (+13 more)

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
Cohesion: 0.18
Nodes (10): LogLevel, MemoryCache, IDistributedCache, ILogger, IMemoryCache, Mock, Task, Test (+2 more)

### Community 23 - "SteamController"
Cohesion: 0.14
Nodes (15): CancellationToken, Exception, HttpGet, HttpPost, IActionResult, IReadOnlyList, Task, SteamController (+7 more)

### Community 24 - "RadioButtonsFilterComponent"
Cohesion: 0.09
Nodes (21): Architecture and placement, Async, workers, brokers, and client state, Behavioral Contract, Blocking findings, Blocking Findings, Code Refactoring Validation Report, Data, DI, and resource ownership, Decision (+13 more)

### Community 25 - "dependencies"
Cohesion: 0.06
Nodes (33): @angular/animations, @angular/cdk, @angular/common, @angular/compiler, @angular/core, @angular/forms, @angular/material, @angular/platform-browser (+25 more)

### Community 26 - ".CreateController"
Cohesion: 0.21
Nodes (10): SignInManager, IConfiguration, IHostEnvironment, IReadOnlyList, Mock, string, Task, Test (+2 more)

### Community 27 - "AuthService"
Cohesion: 0.11
Nodes (4): SessionExpiredPage, Component, AuthService, Injectable

### Community 28 - "WishListsView"
Cohesion: 0.05
Nodes (40): DistributedCacheEntryOptions, IDictionary, JObject, RetryConditionHeaderValue, ManualCheckProductInputDto, Action, AppData, IList (+32 more)

### Community 29 - "GameUrlsView"
Cohesion: 0.12
Nodes (16): API and browser controls, Authentication and authorization, External URLs and scraping, Findings, Findings, Mandatory rules, Merge gate, RabbitMQ, workers, and Redis (+8 more)

### Community 30 - "SteamApp.Application.JsonObjects"
Cohesion: 0.06
Nodes (19): SteamApp.WebAPI.Services, SteamApp.Application.DTOs.ScrapeHistory, SteamApp.Domain.Enums, SteamApp.Application.DTOs.WatchItem, SteamApp.Application.JsonObjects, SteamApp.Application.DTOs.ManualCheck, IList, Asset (+11 more)

### Community 31 - "Подробно описание на проекта"
Cohesion: 0.06
Nodes (35): 10) Нефункционални изисквания, 11) Какво трябва да включва „детайлната документация“ (препоръка), 12) Кратко резюме, 1) Какво представлява проектът, 2) Бизнес цел и проблем, който решава, 3.1 Каталог и конфигурация, 3.2 Релации (M2M), 3.3 Оперативно следене (+27 more)

### Community 32 - "EmailService"
Cohesion: 0.13
Nodes (14): IOptions, ILogger, EmailService, CancellationToken, ConcurrentQueue, IReadOnlyCollection, Task, CapturingEmailService (+6 more)

### Community 33 - "What You Must Do When Invoked"
Cohesion: 0.08
Nodes (24): For /graphify add and --watch, For /graphify query, For the commit hook and native CLAUDE.md integration, For --update and --cluster-only, /graphify, Honesty Rules, Interpreter guard for subcommands, Part A - Structural extraction for code files (+16 more)

### Community 34 - "Program"
Cohesion: 0.13
Nodes (14): AuthorizationPolicy, HttpContext, JwtSettings, CancellationToken, IConfiguration, IHostEnvironment, ILogger, int (+6 more)

### Community 35 - ".CreateMemoryCache"
Cohesion: 0.22
Nodes (9): DbContextOptions, InMemoryDatabaseRoot, Task, SteamRepository, Task, Test, SteamRepositoryTests, string (+1 more)

### Community 36 - "enums/index.ts"
Cohesion: 0.12
Nodes (22): activityFilterIds, ActivityFilters, activityFiltersCollection, activityFiltersMap, Class, classesCollection, classesMap, classFiltersCollection (+14 more)

### Community 37 - "WatchListService"
Cohesion: 0.08
Nodes (11): CreateWatchList, UpdateWatchList, UpdateWatchListStatus, WatchList, Component, WatchListForm, Component, ViewChild (+3 more)

### Community 38 - "SteamServiceTests"
Cohesion: 0.11
Nodes (13): CancelAfter, IAsyncDisposable, IWebElement, Mock, Task, Test, TestCase, SteamServiceTests (+5 more)

### Community 39 - "WatchItemDto"
Cohesion: 0.06
Nodes (29): IEnumerable, ScrapeHistoryRerunResponseDto, WatchItemDto, AssetDescription, List, ListingDetails, Result, SearchData (+21 more)

### Community 40 - "SteamApp.Interfaces.Services"
Cohesion: 0.18
Nodes (6): SteamApp.Infrastructure.Services, SteamApp.IntegrationTests.External, SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.DependencyInjection, SteamApp.Interfaces.Repositories, SteamApp.Interfaces.Services, SteamApp.Tests.Services

### Community 41 - "app.component.ts"
Cohesion: 0.16
Nodes (8): ErrorDialogComponent, ErrorDialogData, Component, SiteFooter, Component, ErrorDialogBridge, ErrorDialogService, Injectable

### Community 42 - "RabbitMqOptions"
Cohesion: 0.07
Nodes (27): WhishListResponse, WishListCreateDto, WishListDto, WishlistRepository, CancellationToken, IEnumerable, IWebElement, Task (+19 more)

### Community 44 - "WishlistJobIntegrationTests"
Cohesion: 0.25
Nodes (7): IDistributedCache, JsonSerializerOptions, MemoryDistributedCache, string, Task, Test, WishlistJobIntegrationTests

### Community 45 - "rabbitmq1"
Cohesion: 0.09
Nodes (22): dependencies, mssql1, rabbitmq1, redis1, connectionId, dynamicId, secretStore, type (+14 more)

### Community 46 - "SteamApp.Migrations"
Cohesion: 0.08
Nodes (13): SteamApp.Migrations, ModelBuilder, AddAutomatedScrapeHistory, ModelBuilder, AddFeedbackRequests, MigrationBuilder, ModelBuilder, AddFeedbackRequestHistory (+5 more)

### Community 47 - "ApplicationDbContextModelSnapshot.cs"
Cohesion: 0.08
Nodes (14): SteamApp.WebAPI.Migrations, ModelSnapshot, ModelBuilder, GameUrlName, ModelBuilder, EntitiesRework, ModelBuilder, ProductTags (+6 more)

### Community 48 - ".GetAsync"
Cohesion: 0.08
Nodes (16): CreateGameUrl, UpdateGameUrl, UpdateGameUrlStatus, CreateProduct, Product, UpdateProduct, UpdateProductStatus, CreateProductTag (+8 more)

### Community 49 - "FeedbackRequestEndpoints"
Cohesion: 0.33
Nodes (4): HttpResponseMessage, JsonElement, Task, JsonTestExtensions

### Community 50 - "GameUrlForm"
Cohesion: 0.08
Nodes (11): Listing, WhishListResponse, ScrapeHistory, ScrapeHistoryDetail, ScrapeHistoryRerunResponse, ScrapeJobAccepted, ScrapeHistoryDialogComponent, ScrapeHistoryJsonDialogComponent (+3 more)

### Community 51 - "LoginComponent"
Cohesion: 0.15
Nodes (4): AuthMode, LoginComponent, PasswordRequirement, Component

### Community 52 - "AdminUsersPage"
Cohesion: 0.19
Nodes (6): AdminUsersPage, Component, AdminUserEffectiveRole, AdminUserService, AdminUserSummary, Injectable

### Community 53 - "FakeEmailSmtpClient"
Cohesion: 0.19
Nodes (9): CancellationToken, Exception, MimeMessage, SecureSocketOptions, Task, Test, ValueTask, EmailServiceTests (+1 more)

### Community 54 - "BackgroundWorkerService"
Cohesion: 0.36
Nodes (6): PeriodicTimer, CancellationToken, SemaphoreSlim, string, Task, BackgroundWorkerService

### Community 55 - "ProfilePage"
Cohesion: 0.18
Nodes (4): ProfilePage, Component, UpdateUserProfileRequest, UserProfile

### Community 56 - "AdminUserEndpoints"
Cohesion: 0.20
Nodes (9): IResult, CancellationToken, IdentityResult, IEnumerable, Task, UserManager, WebApplication, AdminUserEndpoints (+1 more)

### Community 57 - "GameUrlProductService"
Cohesion: 0.13
Nodes (3): GameUrlsView, Component, ViewChild

### Community 58 - "TestDb"
Cohesion: 0.06
Nodes (32): DbSet, EntityTypeBuilder, IDbContextFactory, IdentityDbContext, IdentityRole, IDisposable, ModelBuilder, ApplicationDbContext (+24 more)

### Community 59 - "SteamApp.Domain.Common"
Cohesion: 0.13
Nodes (12): Color, SteamApp.Domain.ValueObjects, SteamApp.Domain.Common, int, string, Constants, Dictionary, StaticCollections (+4 more)

### Community 60 - "TransientRetryPolicyService"
Cohesion: 0.22
Nodes (9): CancellationToken, Exception, Func, HttpStatusCode, Task, TimeSpan, TransientRetryPolicyService, string (+1 more)

### Community 61 - "CancellationToken"
Cohesion: 0.21
Nodes (7): SmtpClient, CancellationToken, MimeMessage, SecureSocketOptions, Task, ValueTask, MailKitEmailSmtpClient

### Community 62 - ".CreateAnonymousClient"
Cohesion: 0.35
Nodes (3): Task, Test, SecurityPipelineIntegrationTests

### Community 63 - "CopyLinkComponent"
Cohesion: 0.17
Nodes (5): CopyLinkComponent, Component, Input, CopyLinkHostComponent, Component

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

### Community 69 - "WishlistCheckJob"
Cohesion: 0.20
Nodes (8): IConnection, CancellationToken, SemaphoreSlim, Task, ValueTask, RabbitMqConnection, CancellationToken, Task

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
Cohesion: 0.07
Nodes (6): CreateGameUrlPixel, GameUrlPixel, GameUrlForm, Component, GameUrlPixelService, Injectable

### Community 75 - "Required Review"
Cohesion: 0.12
Nodes (16): 1. Architecture Alignment, 2. Required Change Logic, 3. Error Handling, 4. Maintainability, SOLID, Resource Ownership, and Lifetime Safety, 5. Asynchronous Execution, Cancellation, and Concurrency, 6. Tests and Verification Evidence, Blocking, Context Environment (+8 more)

### Community 76 - "StartupIntegrationTests"
Cohesion: 0.24
Nodes (5): SteamApp.IntegrationTests.Startup, IReadOnlyDictionary, Task, Test, StartupIntegrationTests

### Community 77 - ".ResetDatabaseAsync"
Cohesion: 0.35
Nodes (5): SteamApp.IntegrationTests.Controllers, Task, Test, SteamControllerIntegrationTests, Task

### Community 78 - "RabbitMqConnection"
Cohesion: 0.18
Nodes (9): ManualCheckProductTrace, ManualCheckRunDetail, ManualCheckRunSummary, HistoryView, ManualCheckHistoryDialogComponent, ManualCheckHistoryDialogData, Component, ManualCheckSteamResultDialogComponent (+1 more)

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

### Community 83 - "AuthGuard"
Cohesion: 0.24
Nodes (3): AuthGuard, Injectable, route()

### Community 84 - "ExtraPixelManager"
Cohesion: 0.29
Nodes (6): PixelCreateDto, PixelDto, CancellationToken, List, Task, ExtraPixelManager

### Community 85 - ".CreateAuthenticatedClient"
Cohesion: 0.36
Nodes (5): Task, Test, EfRepositoryIntegrationTests, DateTime, HttpClient

### Community 86 - "TagsView"
Cohesion: 0.25
Nodes (7): CancellationToken, IEnumerable, Task, WishList, Task, Test, WishlistRepositoryTests

### Community 87 - "SteamAppFactory"
Cohesion: 0.24
Nodes (5): IWebHostBuilder, SqliteConnection, Dictionary, SteamAppFactory, WebApplicationFactory

### Community 88 - "SteamApp.Tests"
Cohesion: 0.18
Nodes (11): Microsoft.AspNetCore.TestHost (9.0.15), SteamApp.Tests, net9.0, coverlet.collector (10.0.0), Microsoft.EntityFrameworkCore.InMemory (9.0.15), Microsoft.NET.Test.Sdk (18.5.1), Moq (4.20.72), NUnit (4.6.0) (+3 more)

### Community 89 - "GamesView"
Cohesion: 0.14
Nodes (16): DateTime, ScrapeHistoryDetailDto, DateTime, ScrapeHistorySetupDto, DateTime, ScrapeHistorySummaryDto, ScrapeJobAcceptedDto, DateTime (+8 more)

### Community 91 - "GameUrlDto"
Cohesion: 0.29
Nodes (6): GameUrlCreateDto, GameUrlDto, CancellationToken, List, Task, GameUrlManager

### Community 92 - "ProductDto"
Cohesion: 0.26
Nodes (7): ProductCreateDto, ProductDto, CancellationToken, List, Task, ProductManager, SteamApiClient

### Community 93 - "MinimalApiCrudIntegrationTests"
Cohesion: 0.38
Nodes (4): Task, Test, TestCase, MinimalApiCrudIntegrationTests

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

### Community 99 - "WishlistNotificationConsumer"
Cohesion: 0.18
Nodes (8): CancellationToken, Task, WishlistNotificationMessageHandler, WishlistNotificationRequested, CancellationToken, JsonSerializerOptions, Task, WishlistNotificationConsumer

### Community 100 - "CountdownTimerComponent"
Cohesion: 0.07
Nodes (15): CheckboxesFilterComponent, Component, Input, Output, RadioButtonsFilterComponent, RadioOption, Component, Input (+7 more)

### Community 101 - "DropdownComponent"
Cohesion: 0.10
Nodes (11): DropdownComponent, Component, HostListener, Input, SiteHeaderComponent, Component, ChangePasswordRequest, CurrentUser (+3 more)

### Community 102 - "GameDto"
Cohesion: 0.33
Nodes (6): GameCreateDto, GameDto, CancellationToken, List, Task, GameManager

### Community 103 - "CapturingMessagePublisher"
Cohesion: 0.24
Nodes (6): CancellationToken, ConcurrentQueue, IReadOnlyCollection, Task, CapturingMessagePublisher, PublishedMessage

### Community 104 - "xlsx"
Cohesion: 0.20
Nodes (9): ManualCheckAssetMatch, ManualCheckDescriptionMatch, ManualCheckProductError, ManualCheckProductInput, ManualCheckProductResult, ManualCheckProgress, ManualCheckRunResults, ManualCheckRunStatus (+1 more)

### Community 105 - "SteamApp.Application.DTOs.Tag"
Cohesion: 0.22
Nodes (5): CancellationToken, Exception, Func, Task, ITransientRetryPolicyService

### Community 106 - "SteamService"
Cohesion: 0.05
Nodes (52): SteamApp.WebAPI.Exceptions, EnableRateLimiting, Exception, DateTime, List, ManualCheckAssetMatchDto, ManualCheckCriterionDto, ManualCheckDescriptionMatchDto (+44 more)

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

### Community 112 - ".JsonResponsesUseCamelCase"
Cohesion: 0.39
Nodes (4): SteamApp.IntegrationTests.Contracts, Task, Test, ApiContractIntegrationTests

### Community 113 - "SteamApp.IntegrationTests.Support"
Cohesion: 0.13
Nodes (10): AppComponent, Component, appConfig, aboutPageStructuredData, faqPageStructuredData, routes, LoadingInterceptor, Injectable (+2 more)

### Community 114 - "SteamApiClient"
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

### Community 119 - "MinimalApiCrudIntegrationTests.cs"
Cohesion: 0.22
Nodes (7): CancellationToken, Task, IJobService, CancellationToken, Task, TimeSpan, WishlistCheckJob

### Community 120 - "ScrapeRequestedConsumer"
Cohesion: 0.25
Nodes (6): BackgroundService, WishlistCheckRequested, CancellationToken, JsonSerializerOptions, Task, WishlistCheckConsumer

### Community 121 - "WishListForm"
Cohesion: 0.33
Nodes (3): CountdownTimerComponent, Component, Input

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
Cohesion: 0.29
Nodes (5): ScrapeRequested, CancellationToken, JsonSerializerOptions, Task, ScrapeRequestedConsumer

### Community 128 - "UpdateUserProfileRequest.cs"
Cohesion: 0.25
Nodes (7): Angular tests, Commands, Completion report, Coverage, .NET tests, Objective, SteamApp Testing Directive

### Community 129 - "LoginRequest.cs"
Cohesion: 0.13
Nodes (8): AllowAnonymous, Claim, DateTime, AuthResponse, ClientDefinition, TokenRequest, HttpPost, IEnumerable

### Community 130 - "WatchListsView"
Cohesion: 0.25
Nodes (4): SteamApp.Application.DTOs, BaseDto, BaseUpdateDto, ProductTagCreateDto

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
Cohesion: 0.46
Nodes (4): CancellationToken, Exception, Task, ManualCheckWorker

### Community 137 - "SteamApp.Application.DTOs.GameUrlProduct"
Cohesion: 0.40
Nodes (3): SteamApp.Application.DTOs.GameUrlProduct, GameUrlProductCreateDto, GameUrlProductDto

### Community 138 - "SteamApp.Application.DTOs.GameUrlPixel"
Cohesion: 0.41
Nodes (4): Mock, Task, Test, ManualChecksControllerTests

### Community 139 - ".PagedCatalogEndpointsFilterSortAndClampForAUserJourney"
Cohesion: 0.60
Nodes (3): Task, Test, CatalogManagementE2ETests

### Community 140 - ".ClientCredentialsTokenUnlocksProtectedApiSession"
Cohesion: 0.60
Nodes (3): Task, Test, SecurityE2ETests

### Community 141 - ".AddRabbitMqMessageBroker"
Cohesion: 0.60
Nodes (3): IConfiguration, IServiceCollection, RabbitMqServiceCollectionExtensions

### Community 142 - "db12"
Cohesion: 0.15
Nodes (7): Migration, MigrationBuilder, db12, MigrationBuilder, IsProductActive, MigrationBuilder, AddManualChecks

### Community 148 - "IsProductActive"
Cohesion: 0.48
Nodes (4): IServiceProvider, string, Task, IntegrationSeed

### Community 157 - "AutomatedScrapeHistory"
Cohesion: 0.29
Nodes (5): CancellationToken, Task, CancellationToken, Task, WishlistCheckMessageHandler

### Community 162 - "ClaimsPrincipalExtensions.cs"
Cohesion: 0.50
Nodes (3): CancellationToken, Task, SteamManager

### Community 163 - "ItemNotFoundException.cs"
Cohesion: 0.50
Nodes (3): Task, Test, TransientRetryPolicyServiceTests

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
Cohesion: 0.29
Nodes (6): Bind the review, Locate affected paths, Output, Resolve directives, Review and validate, SteamApp PR Review

### Community 170 - ".MaterializeResultsIfNeeded"
Cohesion: 0.08
Nodes (20): SteamApp.Application.DTOs.FeedbackRequest, FeedbackRequestCreateDto, DateTime, FeedbackRequestDto, DateTime, FeedbackRequestHistoryDto, FeedbackRequestUpdateDto, FeedbackRequestUpdateStatusDto (+12 more)

### Community 171 - "SeoMetaService"
Cohesion: 0.29
Nodes (6): Bind and scope, Inspect, Output, Resolve the directive, SteamApp Security Review, Validate

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
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: make it not reuse it should restart the currently active one, Source Nodes

### Community 178 - "local-release"
Cohesion: 0.25
Nodes (8): local-release, budgets, buildTarget, extractLicenses, fileReplacements, optimization, outputHashing, sourceMap

### Community 179 - "CheckboxesFilterComponent"
Cohesion: 0.29
Nodes (4): SteamApp.Application.DTOs.ScrapingMode, ScrapingModeCreateDto, ScrapingModeDto, ScrapingModeUpdateDto

### Community 183 - ".SeedAsync"
Cohesion: 0.33
Nodes (4): SteamApp.E2ETests.Security, Task, Test, StartupConfigurationE2ETests

### Community 185 - "SteamControllerTests.cs"
Cohesion: 0.07
Nodes (21): ClaimsPrincipal, SteamApp.WebAPI.Security, SteamApp.Domain.ValueObjects.Authentication, SteamApp.Tests.Security, SteamApp.IntegrationTests.Data, SteamApp.Tests.Controllers, SteamApp.IntegrationTests.Security, SteamApp.E2ETests.MinimalApis (+13 more)

### Community 186 - "HttpMessageHandlerStub"
Cohesion: 0.25
Nodes (7): HttpMessageHandler, HttpRequestMessage, CancellationToken, Func, HttpResponseMessage, Task, HttpMessageHandlerStub

### Community 187 - "graphify reference: query, path, explain"
Cohesion: 0.33
Nodes (5): For /graphify explain, For /graphify path, graphify reference: query, path, explain, Step 0 — Constrained query expansion (REQUIRED before traversal), Step 1 — Traversal

### Community 188 - "GameEndpoints"
Cohesion: 0.33
Nodes (5): Choose the smallest inspection, Fallback, Procedure, SteamApp Codebase Discovery, SteamApp orientation

### Community 189 - "GameUrlPixelsEndpoints"
Cohesion: 0.16
Nodes (9): ManualCheckMatchMode, ManualCheckPreset, ManualCheckPresetWrite, ManualCheckRunAccepted, ManualCheckRunRequest, ManualCheckSetupDialogData, ManualCheckSetupDialogResult, ManualCheckService (+1 more)

### Community 190 - "PixelEndpoints"
Cohesion: 0.29
Nodes (4): UrlUtilities, IEnumerable, WebApplication, GameUrlProductsEndpoints

### Community 191 - "ProductEndpoints"
Cohesion: 0.40
Nodes (4): SteamApp.Infrastructure, SteamApp.Models.OperationResults, BaseOperationResult, ItemCreateResult

### Community 192 - "ProductTagsEndpoints"
Cohesion: 0.15
Nodes (11): Channel, IAsyncEnumerable, Task, Test, ManualCheckQueueTests, CancellationToken, ConcurrentDictionary, ValueTask (+3 more)

### Community 193 - "ScrapingModeEndpoints"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Inside Automated Check presets remain on Loading presets, failed product checks have no readable trace in Automated Check History, preset creation may return 429, and ManualChecksController should not use HandleAsync., Source Nodes

### Community 194 - "TagsEndpoints"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Fix the frontend problems from the attached Angular diagnostics., Source Nodes

### Community 195 - "WatchListEndpoints"
Cohesion: 0.50
Nodes (3): DateTime, string, IntegrationJwt

### Community 196 - "WishListEndpoints"
Cohesion: 0.33
Nodes (5): Establish behavior first, Resolve directives, Review, SteamApp Refactoring PR Review, Validation and output

### Community 198 - "error-dialog.service.ts"
Cohesion: 0.24
Nodes (4): SeoMetaService, SeoRouteData, Injectable, environment

### Community 211 - "SteamApp.Application.DTOs"
Cohesion: 0.40
Nodes (4): Boundaries, SteamApp Graphify, Updates, Use the existing graph

### Community 212 - ".CreateService"
Cohesion: 0.40
Nodes (4): Output, Procedure, Resolve the directive, SteamApp Unit Tests

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
Nodes (3): SteamApp.Application.DTOs.GameUrlPixel, GameUrlPixelCreateDto, GameUrlPixelDto

### Community 221 - "AGENTS.md"
Cohesion: 0.15
Nodes (12): Async, workers, brokers, and caching, Code conventions, Command execution, Completion, Data access and dependency injection, Discovery and generated graphs, Project map, Repository workflows (+4 more)

### Community 222 - "@angular/forms"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Create project-specific AGENTS.md, workflow directives, and skills from the supplied generic directives, including security., Source Nodes

### Community 226 - "Q: did you allow the two the to communicate as I am getting Invalid user credentials. from the server"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: did you allow the two the to communicate as I am getting Invalid user credentials. from the server, Source Nodes

### Community 227 - "karma"
Cohesion: 0.40
Nodes (4): PricingAccent, PricingPage, PricingPlan, Component

### Community 236 - ".CreateJob"
Cohesion: 0.29
Nodes (5): HttpClient, HttpResponseMessage, JsonElement, Task, E2EClientExtensions

### Community 238 - ".SwaggerIsExposedInDevelopmentAndHiddenBehindProductionApiSecurity"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Profile page only renders fetched data after a manual click, Source Nodes

### Community 239 - "SteamApp.Application.DTOs.ScrapingMode"
Cohesion: 0.33
Nodes (4): CancellationToken, string, Task, IdentitySchemaInitializer

### Community 240 - "GameUrlProductsEndpoints"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: Rework the Batch Panel in manual-mode-v2.html with previous and next batch buttons, defaults of 1, and no Run Batch increment, Source Nodes

### Community 241 - ".CreateTokenResponse"
Cohesion: 0.10
Nodes (18): Authorize, ControllerBase, IdentityUser, ModelStateDictionary, ApplicationUser, ChangePasswordRequest, DeleteUserRequest, RegisterRequest (+10 more)

### Community 242 - "Q: I still get Invalid user credentials. and the client doesn't open upon clicking the bat"
Cohesion: 0.40
Nodes (4): Answer, Outcome, Q: I still get Invalid user credentials. and the client doesn't open upon clicking the bat, Source Nodes

## Knowledge Gaps
- **598 isolated node(s):** `$schema`, `version`, `newProjectRoot`, `projectType`, `style` (+593 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **60 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Work-memory lessons

**Preferred sources** — corroborated by past sessions; start here.
- `local-release` (4× useful, score=3.3324181)
- `browser` (3× useful, score=2.499427608)
- `Program.cs` (3× useful, score=2.499082423)
- `ManualCheckHistoryDialogComponent` (2× useful, score=1.940820252) _(code changed — re-verify)_
- `ManualModeV2` (2× useful, score=1.898496016) _(code changed — re-verify)_
- `AuthController` (2× useful, score=1.666163046)
- `.Login()` (2× useful, score=1.666163046)

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SteamApp.Infrastructure.Context` connect `SteamApp.Domain.Enums` to `SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Options`, `Migration`, `SteamApp.Application.JsonObjects`, `SteamApp.Interfaces.Services`, `SteamApp.Migrations`, `ApplicationDbContextModelSnapshot.cs`, `home-page.ts`, `SteamControllerTests.cs`, `SecurityPolicies.cs`, `ScrapeHistorySummaryDto`, `AddScrapeHistoryJobStatus`, `OneShotHttpServer`, `AddFeedbackRequests`, `SteamApp.Application.DTOs.ScrapingMode`, `.attachTableControls`, `IntegrationJwt`, `ISteamRepository`, `ControllerSecurityMetadataTests`, `DeleteUserRequest.cs`, `20260515171509_AddUserOwnership.Designer.cs`, `20260627144144_AddFeedbackRequestHistory.Designer.cs`, `ScrapingMode`?**
  _High betweenness centrality (0.066) - this node is a cross-community bridge._
- **Why does `SteamApp.Interfaces.Services` connect `SteamApp.Interfaces.Services` to `EmailService`, `WatchItemDto`, `SteamApp.Application.DTOs.Tag`, `SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Options`, `IEmailSmtpClient`, `EncryptionHashingService`, `SteamControllerTests.cs`, `SteamApp.Application.JsonObjects`?**
  _High betweenness centrality (0.029) - this node is a cross-community bridge._
- **Why does `SteamApp.Domain.Entities` connect `SteamApp.Domain.Enums` to `SteamApp.Interfaces.Services`, `SteamService`, `.MaterializeResultsIfNeeded`, `SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Options`, `SteamApp.Application.Mapper`, `TagsView`, `SteamControllerTests.cs`, `TestDb`, `SteamApp.Application.JsonObjects`?**
  _High betweenness centrality (0.029) - this node is a cross-community bridge._
- **Are the 93 inferred relationships involving `handleError()` (e.g. with `.getUsers()` and `.updateRole()`) actually correct?**
  _`handleError()` has 93 INFERRED edges - model-reasoned connections that need verification._
- **What connects `$schema`, `version`, `newProjectRoot` to the rest of the system?**
  _598 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `feedback-request.model.ts` be split into smaller, more focused modules?**
  _Cohesion score 0.0636030636030636 - nodes in this community are weakly interconnected._
- **Should `web-scraper.component.ts` be split into smaller, more focused modules?**
  _Cohesion score 0.06487434248977206 - nodes in this community are weakly interconnected._