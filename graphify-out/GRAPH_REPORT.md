# Graph Report - .  (2026-07-21)

## Corpus Check
- cluster-only mode — file stats not available

## Summary
- 3161 nodes · 6350 edges · 208 communities (146 shown, 62 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 308 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `66bee445`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Community 0
- Community 1
- Community 2
- Community 3
- Community 4
- Community 5
- Community 6
- Community 7
- Community 8
- Community 9
- Community 10
- Community 11
- Community 12
- Community 13
- Community 14
- Community 15
- Community 16
- Community 17
- Community 18
- Community 19
- Community 20
- Community 21
- Community 22
- Community 23
- Community 24
- Community 25
- Community 26
- Community 27
- Community 28
- Community 29
- Community 30
- Community 31
- Community 32
- Community 33
- Community 34
- Community 35
- Community 36
- Community 37
- Community 38
- Community 39
- Community 40
- Community 41
- Community 42
- Community 43
- Community 44
- Community 45
- Community 46
- Community 47
- Community 48
- Community 49
- Community 50
- Community 51
- Community 52
- Community 53
- Community 54
- Community 55
- Community 56
- Community 57
- Community 58
- Community 59
- Community 60
- Community 61
- Community 62
- Community 63
- Community 64
- Community 65
- Community 66
- Community 67
- Community 68
- Community 69
- Community 70
- Community 71
- Community 72
- Community 73
- Community 74
- Community 75
- Community 76
- Community 77
- Community 78
- Community 79
- Community 80
- Community 81
- Community 82
- Community 83
- Community 84
- Community 85
- Community 86
- Community 87
- Community 88
- Community 89
- Community 90
- Community 91
- Community 92
- Community 93
- Community 94
- Community 95
- Community 96
- Community 97
- Community 98
- Community 99
- Community 100
- Community 101
- Community 102
- Community 103
- Community 104
- Community 105
- Community 106
- Community 107
- Community 108
- Community 109
- Community 110
- Community 111
- Community 112
- Community 113
- Community 114
- Community 115
- Community 116
- Community 117
- Community 118
- Community 119
- Community 120
- Community 121
- Community 122
- Community 123
- Community 124
- Community 125
- Community 126
- Community 127
- Community 128
- Community 129
- Community 130
- Community 131
- Community 132
- Community 133
- Community 134
- Community 135
- Community 136
- Community 137
- Community 138
- Community 139
- Community 140
- Community 141
- Community 142
- Community 143
- Community 144
- Community 145
- Community 146
- Community 147
- Community 148
- Community 149
- Community 150
- Community 151
- Community 152
- Community 153
- Community 154
- Community 155
- Community 156
- Community 157
- Community 158
- Community 159
- Community 160
- Community 161
- Community 162
- Community 163
- Community 164
- Community 165
- Community 166
- Community 167
- Community 168
- Community 169
- Community 170
- Community 171
- Community 172
- Community 173
- Community 174
- Community 175
- Community 176
- Community 177
- Community 178
- Community 179
- Community 180
- Community 181
- Community 182
- Community 183
- Community 184
- Community 185
- Community 186
- Community 187
- Community 188
- Community 189
- Community 190
- Community 191
- Community 192
- Community 193
- Community 194
- Community 195
- Community 196
- Community 197
- Community 198
- Community 199
- Community 200
- Community 201

## God Nodes (most connected - your core abstractions)
1. `handleError()` - 101 edges
2. `SteamApp.Infrastructure.Context` - 55 edges
3. `SteamApp.Domain.Entities` - 55 edges
4. `AuthService` - 54 edges
5. `GameService` - 44 edges
6. `WebScraperComponent` - 42 edges
7. `SteamApp.WebAPI.Migrations` - 38 edges
8. `SteamApp.Domain.Enums` - 33 edges
9. `SecurityConfigurationTests` - 33 edges
10. `ManualModeV2` - 32 edges

## Surprising Connections (you probably didn't know these)
- `mockSteamApi()` --indirect_call--> `route()`  [INFERRED]
  SteamApp.Client/e2e/support/mock-api.ts → SteamApp.Client/src/app/services/auth/auth.guard.unit.spec.ts
- `createComponent()` --indirect_call--> `ExternalLinkDisclosurePage`  [INFERRED]
  SteamApp.Client/src/app/pages/external-link-disclosure/external-link-disclosure-page.unit.spec.ts → SteamApp.Client/src/app/pages/external-link-disclosure/external-link-disclosure-page.ts
- `createComponent()` --indirect_call--> `FeedbackRequestForm`  [INFERRED]
  SteamApp.Client/src/app/pages/feedback/feedback-request-form/feedback-request-form.unit.spec.ts → SteamApp.Client/src/app/pages/feedback/feedback-request-form/feedback-request-form.ts
- `setup()` --indirect_call--> `WishListService`  [INFERRED]
  SteamApp.Client/src/app/pages/wish-list/wish-list-form/wish-list-form.unit.spec.ts → SteamApp.Client/src/app/services/wish-list/wish-list.service.ts
- `setup()` --indirect_call--> `WishListsView`  [INFERRED]
  SteamApp.Client/src/app/pages/wish-list/wish-lists-view/wish-lists-view.unit.spec.ts → SteamApp.Client/src/app/pages/wish-list/wish-lists-view/wish-lists-view.ts

## Import Cycles
- None detected.

## Communities (208 total, 62 thin omitted)

### Community 0 - "Community 0"
Cohesion: 0.06
Nodes (28): CreateFeedbackRequest, FEEDBACK_REQUEST_HISTORY_ACTION_OPTIONS, FEEDBACK_REQUEST_STATUS_OPTIONS, FEEDBACK_REQUEST_TYPE_OPTIONS, FeedbackRequest, FeedbackRequestHistory, FeedbackRequestHistoryAction, feedbackRequestHistoryActionLabel() (+20 more)

### Community 1 - "Community 1"
Cohesion: 0.05
Nodes (24): Status, formatMs(), HostListener, ExternalLinkHostComponent, Component, makeEnumHelpers(), encode(), externalUrlWarning() (+16 more)

### Community 2 - "Community 2"
Cohesion: 0.05
Nodes (25): ConfirmDialogComponent, ConfirmDialogData, Component, CreateTag, Tag, UpdateTag, UpdateTagStatus, HomePage (+17 more)

### Community 3 - "Community 3"
Cohesion: 0.06
Nodes (34): ConcurrentDictionary, WhishListResponse, WishListCreateDto, WishListDto, CancellationToken, IEnumerable, Task, WishlistRepository (+26 more)

### Community 4 - "Community 4"
Cohesion: 0.08
Nodes (24): Directive, HostBinding, CONSTANTS, ExternalLinkDirective, Input, getListingUrl(), CreateGameUrl, GameUrl (+16 more)

### Community 5 - "Community 5"
Cohesion: 0.10
Nodes (17): PageJson, Task, Test, AdminUserEndpointTests, HttpResponseMessage, Task, Test, TestCase (+9 more)

### Community 6 - "Community 6"
Cohesion: 0.07
Nodes (14): CreateGame, Game, UpdateGame, UpdateGameStatus, GameForm, Component, GamesView, Component (+6 more)

### Community 7 - "Community 7"
Cohesion: 0.07
Nodes (8): ScrapeHistory, ScrapeJobStatus, ScrapeHistoryDialogComponent, ScrapeHistoryJsonDialogComponent, Component, Component, ViewChild, WebScraperComponent

### Community 8 - "Community 8"
Cohesion: 0.10
Nodes (20): SteamApp.WebAPI.Security, SteamApp.Domain.ValueObjects.Authentication, SteamApp.WebAPI.MinimalAPIs, SteamApp.Tests.Security, SteamApp.WebAPI.Contracts.Pagination, SteamApp.Infrastructure.Context, SteamApp.IntegrationTests.Security, SteamApp.WebAPI (+12 more)

### Community 9 - "Community 9"
Cohesion: 0.06
Nodes (32): DbSet, EntityTypeBuilder, IdentityDbContext, IdentityRole, ModelBuilder, ApplicationDbContext, IServiceProvider, string (+24 more)

### Community 10 - "Community 10"
Cohesion: 0.07
Nodes (24): IEnumerable, ScrapeHistoryRerunResponseDto, WatchItemDto, CancellationToken, Task, HttpUtilities, JsonUtilities, IEnumerable (+16 more)

### Community 11 - "Community 11"
Cohesion: 0.04
Nodes (47): @angular/cli, @angular/compiler-cli, @angular-devkit/build-angular, jasmine-core, jest, jest-environment-jsdom, jest-preset-angular, karma (+39 more)

### Community 12 - "Community 12"
Cohesion: 0.07
Nodes (12): CreatePixel, Pixel, PixelListItem, UpdatePixel, UpdatePixelStatus, PixelForm, Component, PixelsView (+4 more)

### Community 13 - "Community 13"
Cohesion: 0.07
Nodes (12): CreateProduct, Product, UpdateProduct, UpdateProductStatus, CreateProductTag, ProductTag, ProductForm, Component (+4 more)

### Community 14 - "Community 14"
Cohesion: 0.15
Nodes (15): SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Consumers, SteamApp.WebAPI.Caching, SteamApp.WebAPI.MessageBrokers.Handlers.Wishlist, SteamApp.WebAPI.MessageBrokers.Abstractions, SteamApp.WebAPI.Jobs, SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Publishing, SteamApp.WebAPI.MessageBrokers.Handlers.Scraping, SteamApp.E2ETests.Services (+7 more)

### Community 15 - "Community 15"
Cohesion: 0.07
Nodes (23): SteamApp.Application.DTOs.ScrapeHistory, SteamApp.Domain.Enums, SteamApp.Application.DTOs.FeedbackRequest, FeedbackRequestCreateDto, DateTime, FeedbackRequestDto, DateTime, FeedbackRequestHistoryDto (+15 more)

### Community 16 - "Community 16"
Cohesion: 0.05
Nodes (33): SteamApp.Application.DTOs.Product, SteamApp.IntegrationTests.Data, SteamApp.WebApiClient.Managers, SteamApp.Tests.TestSupport, SteamApp.Tests.Repositories, SteamApp.Application.Caching, SteamApp.Application.DTOs.GameUrl, SteamApp.Domain.Entities (+25 more)

### Community 17 - "Community 17"
Cohesion: 0.10
Nodes (11): ScrapeJobAccepted, CreateWishList, UpdateWishList, UpdateWishListStatus, WishList, setup(), handleError(), SteamService (+3 more)

### Community 18 - "Community 18"
Cohesion: 0.10
Nodes (20): Authorize, Claim, ControllerBase, HttpDelete, HttpPut, IdentityUser, ModelStateDictionary, ApplicationUser (+12 more)

### Community 19 - "Community 19"
Cohesion: 0.16
Nodes (7): IConfiguration, IHostEnvironment, string, Task, Test, TestCase, SecurityConfigurationTests

### Community 20 - "Community 20"
Cohesion: 0.06
Nodes (37): commandName, environmentVariables, launchBrowser, launchUrl, publishAllPorts, useSSL, ASPNETCORE_ENVIRONMENT, ASPNETCORE_HTTP_PORTS (+29 more)

### Community 21 - "Community 21"
Cohesion: 0.10
Nodes (10): ParsedHash, int, string, EncryptionHashingService, ParsedHash, string, EncryptionHashingOptions, IEncryptionHashingService (+2 more)

### Community 22 - "Community 22"
Cohesion: 0.19
Nodes (9): LogLevel, IDistributedCache, ILogger, IMemoryCache, Mock, Task, Test, SteamControllerTests (+1 more)

### Community 23 - "Community 23"
Cohesion: 0.17
Nodes (12): CancellationToken, Exception, HttpGet, HttpPost, IActionResult, IReadOnlyList, Task, SteamController (+4 more)

### Community 24 - "Community 24"
Cohesion: 0.07
Nodes (15): CheckboxesFilterComponent, Component, Input, Output, RadioButtonsFilterComponent, RadioOption, Component, Input (+7 more)

### Community 25 - "Community 25"
Cohesion: 0.06
Nodes (33): @angular/animations, @angular/cdk, @angular/common, @angular/compiler, @angular/core, @angular/forms, @angular/material, @angular/platform-browser (+25 more)

### Community 26 - "Community 26"
Cohesion: 0.21
Nodes (10): SignInManager, IConfiguration, IHostEnvironment, IReadOnlyList, Mock, string, Task, Test (+2 more)

### Community 28 - "Community 28"
Cohesion: 0.09
Nodes (7): StatusDialogComponent, StatusDialogData, StatusDialogVariant, Component, Component, ViewChild, WishListsView

### Community 29 - "Community 29"
Cohesion: 0.05
Nodes (11): CreateGameUrlPixel, GameUrlPixel, CreateGameUrlProduct, GameUrlProduct, GameUrlForm, Component, UpdateAdminUserRoleRequest, GameUrlPixelService (+3 more)

### Community 30 - "Community 30"
Cohesion: 0.09
Nodes (19): SteamApp.Application.JsonObjects, IDictionary, Action, AppData, IList, Asset, AssetDescription, IList (+11 more)

### Community 31 - "Community 31"
Cohesion: 0.25
Nodes (5): SteamApp.Application.DTOs.WatchListItem, SteamApp.Application.DTOs.WatchList, DateOnly, WatchListUpdateDto, WatchListUpdateStatusDto

### Community 32 - "Community 32"
Cohesion: 0.09
Nodes (19): IOptions, ILogger, EmailService, CancellationToken, ConcurrentQueue, IReadOnlyCollection, Task, CapturingEmailService (+11 more)

### Community 33 - "Community 33"
Cohesion: 0.14
Nodes (16): DateTime, ScrapeHistoryDetailDto, DateTime, ScrapeHistorySetupDto, DateTime, ScrapeHistorySummaryDto, ScrapeJobAcceptedDto, DateTime (+8 more)

### Community 34 - "Community 34"
Cohesion: 0.13
Nodes (14): AuthorizationPolicy, HttpContext, JwtSettings, CancellationToken, IConfiguration, IHostEnvironment, ILogger, int (+6 more)

### Community 35 - "Community 35"
Cohesion: 0.16
Nodes (13): DbContextOptions, InMemoryDatabaseRoot, MemoryCache, Task, SteamRepository, Task, Test, SteamRepositoryTests (+5 more)

### Community 36 - "Community 36"
Cohesion: 0.12
Nodes (22): activityFilterIds, ActivityFilters, activityFiltersCollection, activityFiltersMap, Class, classesCollection, classesMap, classFiltersCollection (+14 more)

### Community 37 - "Community 37"
Cohesion: 0.14
Nodes (8): CreateWatchList, UpdateWatchList, UpdateWatchListStatus, WatchList, Component, WatchListForm, Injectable, WatchListService

### Community 39 - "Community 39"
Cohesion: 0.11
Nodes (12): CancelAfter, IWebElement, Mock, Task, Test, TestCase, SteamServiceTests, Task (+4 more)

### Community 40 - "Community 40"
Cohesion: 0.09
Nodes (15): SteamApp.Infrastructure.Services, SteamApp.WebAPI.Services, SteamApp.Tests.Controllers, SteamApp.IntegrationTests.External, SteamApp.Application.DTOs.WishListItem, SteamApp.Application.DTOs.WatchItem, SteamApp.Interfaces.Repositories, SteamApp.Application.Services (+7 more)

### Community 41 - "Community 41"
Cohesion: 0.14
Nodes (6): LoadingInterceptor, Injectable, LoadingStateService, Injectable, SeoMetaService, Injectable

### Community 42 - "Community 42"
Cohesion: 0.13
Nodes (3): GameUrlsView, Component, ViewChild

### Community 44 - "Community 44"
Cohesion: 0.25
Nodes (7): IDistributedCache, JsonSerializerOptions, MemoryDistributedCache, string, Task, Test, WishlistJobIntegrationTests

### Community 45 - "Community 45"
Cohesion: 0.09
Nodes (22): dependencies, mssql1, rabbitmq1, redis1, connectionId, dynamicId, secretStore, type (+14 more)

### Community 46 - "Community 46"
Cohesion: 0.12
Nodes (10): SteamApp.Migrations, Migration, MigrationBuilder, AddAutomatedScrapeHistory, MigrationBuilder, AddFeedbackRequests, MigrationBuilder, AddFeedbackRequestHistory (+2 more)

### Community 47 - "Community 47"
Cohesion: 0.09
Nodes (12): SteamApp.WebAPI.Migrations, ModelSnapshot, ModelBuilder, EntitiesRework, ModelBuilder, CustomWatchlistUrl, ModelBuilder, CustomWatchlistRework (+4 more)

### Community 48 - "Community 48"
Cohesion: 0.19
Nodes (10): CancellationToken, Task, TimeSpan, ScrapeRequestedMessageHandler, CancellationToken, IReadOnlyList, Task, IScrapeHistoryDataService (+2 more)

### Community 49 - "Community 49"
Cohesion: 0.12
Nodes (9): SteamApp.Application.Utilities, string, FeedbackRequestReference, UrlUtilities, WebApplication, FeedbackRequestEndpoints, IEnumerable, WebApplication (+1 more)

### Community 50 - "Community 50"
Cohesion: 0.14
Nodes (9): SiteFooter, Component, SiteHeaderComponent, Component, ChangePasswordRequest, CurrentUser, DeleteUserRequest, TokenResponse (+1 more)

### Community 51 - "Community 51"
Cohesion: 0.15
Nodes (4): AuthMode, LoginComponent, PasswordRequirement, Component

### Community 52 - "Community 52"
Cohesion: 0.19
Nodes (6): AdminUsersPage, Component, AdminUserEffectiveRole, AdminUserService, AdminUserSummary, Injectable

### Community 53 - "Community 53"
Cohesion: 0.19
Nodes (9): CancellationToken, Exception, MimeMessage, SecureSocketOptions, Task, Test, ValueTask, EmailServiceTests (+1 more)

### Community 54 - "Community 54"
Cohesion: 0.12
Nodes (13): SteamApp.Interfaces, SteamApp.WebAPI.Jobs.Base, PeriodicTimer, CancellationToken, Task, IJobService, CancellationToken, SemaphoreSlim (+5 more)

### Community 55 - "Community 55"
Cohesion: 0.18
Nodes (4): ProfilePage, Component, UpdateUserProfileRequest, UserProfile

### Community 56 - "Community 56"
Cohesion: 0.20
Nodes (9): IResult, CancellationToken, IdentityResult, IEnumerable, Task, UserManager, WebApplication, AdminUserEndpoints (+1 more)

### Community 57 - "Community 57"
Cohesion: 0.18
Nodes (7): ErrorDialogComponent, ErrorDialogData, Component, ErrorDialogBridge, ErrorDialogService, Injectable, getErrorMessage()

### Community 58 - "Community 58"
Cohesion: 0.16
Nodes (3): Component, ViewChild, WatchListsView

### Community 59 - "Community 59"
Cohesion: 0.13
Nodes (12): Color, SteamApp.Domain.ValueObjects, SteamApp.Domain.Common, int, string, Constants, Dictionary, StaticCollections (+4 more)

### Community 60 - "Community 60"
Cohesion: 0.20
Nodes (9): HttpStatusCode, CancellationToken, Exception, Func, Task, TimeSpan, TransientRetryPolicyService, string (+1 more)

### Community 61 - "Community 61"
Cohesion: 0.21
Nodes (7): SmtpClient, CancellationToken, MimeMessage, SecureSocketOptions, Task, ValueTask, MailKitEmailSmtpClient

### Community 62 - "Community 62"
Cohesion: 0.35
Nodes (3): Task, Test, SecurityPipelineIntegrationTests

### Community 63 - "Community 63"
Cohesion: 0.17
Nodes (5): CopyLinkComponent, Component, Input, CopyLinkHostComponent, Component

### Community 64 - "Community 64"
Cohesion: 0.21
Nodes (8): CancellationToken, MimeMessage, SecureSocketOptions, Task, Test, ValueTask, EmailServiceIntegrationTests, RecordingEmailSmtpClient

### Community 65 - "Community 65"
Cohesion: 0.15
Nodes (12): CancellationToken, IReadOnlyList, StubRecipientService, CancellationToken, IReadOnlyList, Task, IWishlistNotificationRecipientService, WishlistNotificationRecipient (+4 more)

### Community 66 - "Community 66"
Cohesion: 0.13
Nodes (15): Microsoft.Azure.StackExchangeRedis (3.2.1), Microsoft.Extensions.Caching.StackExchangeRedis (9.0.2), RabbitMQ.Client (7.1.2), SteamApp.WebAPI, net9.0, HtmlAgilityPack (1.12.4), Microsoft.AspNetCore.Authentication.JwtBearer (9.0.15), Microsoft.AspNetCore.Mvc.NewtonsoftJson (9.0.15) (+7 more)

### Community 67 - "Community 67"
Cohesion: 0.13
Nodes (14): Selenium.WebDriver (4.43.0), net9.0, DotNetSeleniumExtras.WaitHelpers (3.11.0), HtmlAgilityPack (1.12.4), Microsoft.AspNetCore.Authentication.JwtBearer (9.0.15), Microsoft.AspNetCore.Mvc.NewtonsoftJson (9.0.15), Microsoft.EntityFrameworkCore.Design (9.0.15), Microsoft.EntityFrameworkCore.SqlServer (9.0.15) (+6 more)

### Community 68 - "Community 68"
Cohesion: 0.17
Nodes (15): @angular/material/prebuilt-themes/azure-blue.css, node_modules/@angular/material/prebuilt-themes/indigo-pink.css, src/scss/main.scss, zone.js, zone.js/testing, options, browser, index (+7 more)

### Community 69 - "Community 69"
Cohesion: 0.14
Nodes (11): CancellationToken, Task, TimeSpan, WishlistCheckJob, CancellationToken, Task, CancellationToken, Task (+3 more)

### Community 70 - "Community 70"
Cohesion: 0.25
Nodes (7): HashSet, CancellationToken, Exception, IdentityResult, string, Task, IdentityRoleInitializer

### Community 71 - "Community 71"
Cohesion: 0.14
Nodes (13): dependencies, mssql1, rabbitmq1, redis1, connectionId, dynamicId, type, connectionId (+5 more)

### Community 72 - "Community 72"
Cohesion: 0.15
Nodes (12): jest, node, setup-jest.ts, src/**/*.d.ts, src/**/*.integration.test.ts, ./tsconfig.json, compilerOptions, outDir (+4 more)

### Community 73 - "Community 73"
Cohesion: 0.23
Nodes (8): DateOnly, WatchListCreateDto, DateOnly, WatchListDto, CancellationToken, List, Task, WatchListManager

### Community 74 - "Community 74"
Cohesion: 0.27
Nodes (5): IWebElement, Mock, Task, Test, WishlistServiceTests

### Community 75 - "Community 75"
Cohesion: 0.18
Nodes (5): AllowAnonymous, ClientDefinition, LoginRequest, TokenRequest, HttpPost

### Community 76 - "Community 76"
Cohesion: 0.24
Nodes (5): SteamApp.IntegrationTests.Startup, IReadOnlyDictionary, Task, Test, StartupIntegrationTests

### Community 77 - "Community 77"
Cohesion: 0.35
Nodes (5): SteamApp.IntegrationTests.Controllers, Task, Test, SteamControllerIntegrationTests, Task

### Community 78 - "Community 78"
Cohesion: 0.18
Nodes (9): IAsyncDisposable, IConnection, CancellationToken, SemaphoreSlim, Task, ValueTask, RabbitMqConnection, CancellationToken (+1 more)

### Community 79 - "Community 79"
Cohesion: 0.17
Nodes (12): Konscious.Security.Cryptography.Argon2 (1.3.1), MailKit (4.17.0), Microsoft.AspNetCore.Identity.EntityFrameworkCore (9.0.15), Microsoft.AspNetCore.JsonPatch (9.0.15), Microsoft.Extensions.Options (9.0.15), System.Drawing.Common (9.0.15), SteamApp.Infrastructure, net9.0 (+4 more)

### Community 80 - "Community 80"
Cohesion: 0.18
Nodes (9): MathNet.Numerics (5.0.0), Microsoft.EntityFrameworkCore (9.0.15), SteamApp.Interfaces, net9.0, Microsoft.NET.Sdk, SteamApp.Domain, net9.0, Newtonsoft.Json (13.0.4) (+1 more)

### Community 81 - "Community 81"
Cohesion: 0.17
Nodes (12): Microsoft.AspNetCore.Mvc.Testing (9.0.15), Microsoft.EntityFrameworkCore.Sqlite (9.0.15), SteamApp.IntegrationTests, net9.0, coverlet.collector (10.0.0), Microsoft.EntityFrameworkCore.InMemory (9.0.15), Microsoft.NET.Test.Sdk (18.5.1), Moq (4.20.72) (+4 more)

### Community 82 - "Community 82"
Cohesion: 0.24
Nodes (6): Queue, IEmailSmtpClient, IEmailSmtpClientFactory, MailKitEmailSmtpClientFactory, RecordingEmailSmtpClientFactory, FakeEmailSmtpClientFactory

### Community 83 - "Community 83"
Cohesion: 0.24
Nodes (3): AuthGuard, Injectable, route()

### Community 84 - "Community 84"
Cohesion: 0.26
Nodes (7): PixelCreateDto, PixelDto, CancellationToken, List, Task, ExtraPixelManager, SteamApiClient

### Community 85 - "Community 85"
Cohesion: 0.36
Nodes (5): Task, Test, EfRepositoryIntegrationTests, DateTime, HttpClient

### Community 86 - "Community 86"
Cohesion: 0.18
Nodes (8): CancellationToken, Task, WishlistNotificationMessageHandler, WishlistNotificationRequested, CancellationToken, JsonSerializerOptions, Task, WishlistNotificationConsumer

### Community 87 - "Community 87"
Cohesion: 0.24
Nodes (5): IWebHostBuilder, SqliteConnection, Dictionary, SteamAppFactory, WebApplicationFactory

### Community 88 - "Community 88"
Cohesion: 0.18
Nodes (11): Microsoft.AspNetCore.TestHost (9.0.15), SteamApp.Tests, net9.0, coverlet.collector (10.0.0), Microsoft.EntityFrameworkCore.InMemory (9.0.15), Microsoft.NET.Test.Sdk (18.5.1), Moq (4.20.72), NUnit (4.6.0) (+3 more)

### Community 91 - "Community 91"
Cohesion: 0.29
Nodes (6): GameUrlCreateDto, GameUrlDto, CancellationToken, List, Task, GameUrlManager

### Community 92 - "Community 92"
Cohesion: 0.29
Nodes (6): ProductCreateDto, ProductDto, CancellationToken, List, Task, ProductManager

### Community 93 - "Community 93"
Cohesion: 0.38
Nodes (4): Task, Test, TestCase, MinimalApiCrudIntegrationTests

### Community 94 - "Community 94"
Cohesion: 0.36
Nodes (6): CancellationToken, IDistributedCache, JsonSerializerOptions, Task, TimeSpan, DistributedCacheExtensions

### Community 95 - "Community 95"
Cohesion: 0.24
Nodes (6): IReadOnlyCollection, PagedResponse, IQueryable, IReadOnlyCollection, PageWindow, PaginationExtensions

### Community 96 - "Community 96"
Cohesion: 0.20
Nodes (9): AutoMapper (15.1.3), Microsoft.Extensions.Configuration.Abstractions (9.0.15), SteamApp.Application, net9.0, Newtonsoft.Json (13.0.4), Microsoft.NET.Sdk, net9.0, System.IdentityModel.Tokens.Jwt (8.18.0) (+1 more)

### Community 97 - "Community 97"
Cohesion: 0.20
Nodes (10): serve, development, buildTarget, extractLicenses, fileReplacements, optimization, sourceMap, builder (+2 more)

### Community 98 - "Community 98"
Cohesion: 0.36
Nodes (6): corsHeaders, createJwt(), fulfillJson(), MockApiState, mockSteamApi(), signIn()

### Community 99 - "Community 99"
Cohesion: 0.29
Nodes (6): AppComponent, Component, appConfig, aboutPageStructuredData, faqPageStructuredData, routes

### Community 100 - "Community 100"
Cohesion: 0.33
Nodes (3): CountdownTimerComponent, Component, Input

### Community 101 - "Community 101"
Cohesion: 0.24
Nodes (4): DropdownComponent, Component, HostListener, Input

### Community 102 - "Community 102"
Cohesion: 0.33
Nodes (6): GameCreateDto, GameDto, CancellationToken, List, Task, GameManager

### Community 103 - "Community 103"
Cohesion: 0.24
Nodes (6): CancellationToken, ConcurrentQueue, IReadOnlyCollection, Task, CapturingMessagePublisher, PublishedMessage

### Community 104 - "Community 104"
Cohesion: 0.25
Nodes (6): BackgroundService, ScrapeRequested, CancellationToken, JsonSerializerOptions, Task, ScrapeRequestedConsumer

### Community 105 - "Community 105"
Cohesion: 0.22
Nodes (5): SteamApp.Application.DTOs.Tag, TagCreateDto, TagDto, TagUpdateDto, TagUpdateStatusDto

### Community 106 - "Community 106"
Cohesion: 0.25
Nodes (7): HttpMessageHandler, HttpRequestMessage, CancellationToken, Func, HttpResponseMessage, Task, HttpMessageHandlerStub

### Community 107 - "Community 107"
Cohesion: 0.42
Nodes (5): ServiceProvider, IConfiguration, Task, Test, IdentityRoleInitializerTests

### Community 108 - "Community 108"
Cohesion: 0.22
Nodes (9): build, builder, configurations, defaultConfiguration, production, budgets, buildTarget, fileReplacements (+1 more)

### Community 109 - "Community 109"
Cohesion: 0.39
Nodes (5): CancellationToken, DateTime, string, Task, BaseApiClient

### Community 110 - "Community 110"
Cohesion: 0.25
Nodes (6): AuthenticateResult, AuthenticationHandler, AuthenticationSchemeOptions, string, Task, FakeAuthenticationHandler

### Community 111 - "Community 111"
Cohesion: 0.25
Nodes (4): SteamApp.Application.DTOs, BaseDto, BaseUpdateDto, ProductTagCreateDto

### Community 112 - "Community 112"
Cohesion: 0.39
Nodes (4): SteamApp.IntegrationTests.Contracts, Task, Test, ApiContractIntegrationTests

### Community 113 - "Community 113"
Cohesion: 0.24
Nodes (4): SteamApp.E2ETests.Repositories, SteamApp.E2ETests.MinimalApis, SteamApp.E2ETests.Support, SteamApp.IntegrationTests.Support

### Community 114 - "Community 114"
Cohesion: 0.25
Nodes (5): SteamApp.WebApiClient, CancellationToken, Task, AuthApiClient, TokenResponse

### Community 115 - "Community 115"
Cohesion: 0.25
Nodes (8): steam-app-angular-client, style, @schematics/angular:component, prefix, projectType, root, schematics, sourceRoot

### Community 116 - "Community 116"
Cohesion: 0.25
Nodes (4): ComboBoxComponent, KeyOf, Component, Input

### Community 117 - "Community 117"
Cohesion: 0.25
Nodes (4): NumberFilterComponent, Component, Input, Output

### Community 118 - "Community 118"
Cohesion: 0.25
Nodes (8): SteamApp.E2ETests, net9.0, coverlet.collector (10.0.0), Microsoft.NET.Test.Sdk (18.5.1), NUnit (4.6.0), NUnit3TestAdapter (6.2.0), NUnit.Analyzers (4.13.0), Microsoft.NET.Sdk

### Community 119 - "Community 119"
Cohesion: 0.50
Nodes (3): Task, Test, TransientRetryPolicyServiceTests

### Community 120 - "Community 120"
Cohesion: 0.29
Nodes (5): WishlistCheckRequested, CancellationToken, JsonSerializerOptions, Task, WishlistCheckConsumer

### Community 121 - "Community 121"
Cohesion: 0.33
Nodes (4): SteamApp.E2ETests.Security, Task, Test, StartupConfigurationE2ETests

### Community 122 - "Community 122"
Cohesion: 0.29
Nodes (4): SteamApp.Application.DTOs.ScrapingMode, ScrapingModeCreateDto, ScrapingModeDto, ScrapingModeUpdateDto

### Community 123 - "Community 123"
Cohesion: 0.38
Nodes (4): SteamApp.E2ETests.Controllers, Task, Test, SteamControllerE2ETests

### Community 124 - "Community 124"
Cohesion: 0.29
Nodes (6): cli, analytics, newProjectRoot, projects, $schema, version

### Community 125 - "Community 125"
Cohesion: 0.29
Nodes (7): extract-i18n, test, builder, options, buildTarget, architect, builder

### Community 126 - "Community 126"
Cohesion: 0.29
Nodes (4): NamedTag, TagFilterSelectComponent, Component, Output

### Community 127 - "Community 127"
Cohesion: 0.33
Nodes (5): HttpClient, HttpResponseMessage, JsonElement, Task, E2EClientExtensions

### Community 129 - "Community 129"
Cohesion: 0.40
Nodes (4): SteamApp.Infrastructure, SteamApp.Models.OperationResults, BaseOperationResult, ItemCreateResult

### Community 130 - "Community 130"
Cohesion: 0.33
Nodes (4): IDbContextFactory, IDisposable, TestDatabase, TestDbContextFactory

### Community 131 - "Community 131"
Cohesion: 0.33
Nodes (6): src/assets, src/favicon.ico, src/manifest.webmanifest, src/robots.txt, src/sitemap.xml, assets

### Community 132 - "Community 132"
Cohesion: 0.40
Nodes (4): AboutAudience, AboutHighlight, AboutPage, Component

### Community 133 - "Community 133"
Cohesion: 0.40
Nodes (4): FaqGroup, FaqItem, FaqPage, Component

### Community 135 - "Community 135"
Cohesion: 0.33
Nodes (4): HttpResponseMessage, JsonElement, Task, JsonTestExtensions

### Community 136 - "Community 136"
Cohesion: 0.40
Nodes (3): IMemoryCache, string, ScrapeEndpointDefinitions

### Community 137 - "Community 137"
Cohesion: 0.40
Nodes (3): SteamApp.Application.DTOs.GameUrlProduct, GameUrlProductCreateDto, GameUrlProductDto

### Community 138 - "Community 138"
Cohesion: 0.40
Nodes (3): SteamApp.Application.DTOs.GameUrlPixel, GameUrlPixelCreateDto, GameUrlPixelDto

### Community 139 - "Community 139"
Cohesion: 0.60
Nodes (3): Task, Test, CatalogManagementE2ETests

### Community 140 - "Community 140"
Cohesion: 0.60
Nodes (3): Task, Test, SecurityE2ETests

### Community 141 - "Community 141"
Cohesion: 0.60
Nodes (3): IConfiguration, IServiceCollection, RabbitMqServiceCollectionExtensions

### Community 161 - "Community 161"
Cohesion: 0.40
Nodes (4): CancellationToken, string, Task, IdentitySchemaInitializer

### Community 163 - "Community 163"
Cohesion: 0.50
Nodes (3): SteamApp.WebAPI.Exceptions, Exception, ItemNotFoundException

### Community 167 - "Community 167"
Cohesion: 0.50
Nodes (3): Task, Test, RepositoryE2ETests

### Community 168 - "Community 168"
Cohesion: 0.50
Nodes (3): Task, Test, WishlistServiceE2ETests

### Community 169 - "Community 169"
Cohesion: 0.50
Nodes (3): DateTime, string, IntegrationJwt

## Knowledge Gaps
- **313 isolated node(s):** `$schema`, `version`, `newProjectRoot`, `projectType`, `style` (+308 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **62 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SteamApp.Infrastructure.Context` connect `Community 8` to `Community 15`, `Community 16`, `Community 157`, `Community 40`, `Community 171`, `Community 172`, `Community 173`, `Community 174`, `Community 47`, `Community 175`, `Community 176`, `Community 177`, `Community 178`, `Community 179`, `Community 180`, `Community 181`, `Community 182`, `Community 184`, `Community 183`, `Community 185`, `Community 186`, `Community 187`, `Community 46`, `Community 113`?**
  _High betweenness centrality (0.081) - this node is a cross-community bridge._
- **Why does `SteamApp.Interfaces.Services` connect `Community 40` to `Community 32`, `Community 8`, `Community 14`, `Community 113`, `Community 82`, `Community 21`, `Community 60`?**
  _High betweenness centrality (0.042) - this node is a cross-community bridge._
- **Why does `SteamApp.WebAPI.Migrations` connect `Community 47` to `Community 142`, `Community 143`, `Community 144`, `Community 145`, `Community 146`, `Community 147`, `Community 148`, `Community 149`, `Community 150`, `Community 151`, `Community 152`, `Community 153`, `Community 154`, `Community 155`, `Community 156`, `Community 157`, `Community 158`, `Community 159`, `Community 160`, `Community 171`, `Community 172`, `Community 173`, `Community 174`, `Community 175`, `Community 176`, `Community 177`, `Community 178`, `Community 179`, `Community 180`, `Community 181`, `Community 182`, `Community 183`, `Community 184`?**
  _High betweenness centrality (0.033) - this node is a cross-community bridge._
- **Are the 84 inferred relationships involving `handleError()` (e.g. with `.getUsers()` and `.updateRole()`) actually correct?**
  _`handleError()` has 84 INFERRED edges - model-reasoned connections that need verification._
- **Are the 2 inferred relationships involving `GameService` (e.g. with `setup()` and `setup()`) actually correct?**
  _`GameService` has 2 INFERRED edges - model-reasoned connections that need verification._
- **What connects `$schema`, `version`, `newProjectRoot` to the rest of the system?**
  _313 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.059319482083709726 - nodes in this community are weakly interconnected._