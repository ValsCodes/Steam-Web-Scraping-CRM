import { ChangeDetectorRef } from '@angular/core';
import { fakeAsync, tick } from '@angular/core/testing';
import { of } from 'rxjs';
import * as XLSX from 'xlsx';

import {
  ExternalLinkDisclosureService,
  GameUrlProductService,
  ManualCheckService,
} from '../../services';
import { GameUrlProduct, ManualCheckRunDetail, ScrapingModeEnum } from '../../models';
import { ManualCheckSetupDialogComponent } from './manual-check-setup-dialog.component';
import { ManualCheckSteamResultDialogComponent } from './manual-check-steam-result-dialog.component';
import { ManualCheckTraceDialogComponent } from './manual-check-trace-dialog.component';
import { AdvancedStockDialogComponent } from './advanced-stock-dialog.component';
import { ManualModeV2 } from './manual-mode-v2';

describe('ManualModeV2 external link disclosure', () => {
  let component: ManualModeV2;
  let disclosure: jasmine.SpyObj<ExternalLinkDisclosureService>;
  let gameUrlProductService: jasmine.SpyObj<GameUrlProductService>;
  let manualCheckService: jasmine.SpyObj<ManualCheckService>;
  let dialog: { open: jasmine.Spy };
  let cdr: jasmine.SpyObj<ChangeDetectorRef>;

  beforeEach(() => {
    const emptyService = {
      getAll: () => of([]),
      existsByGameUrl: () => of([]),
    };

    cdr = jasmine.createSpyObj<ChangeDetectorRef>('ChangeDetectorRef', [
      'markForCheck',
      'detectChanges',
    ]);
    disclosure = jasmine.createSpyObj<ExternalLinkDisclosureService>(
      'ExternalLinkDisclosureService',
      ['openTrustedUrl'],
    );
    gameUrlProductService = jasmine.createSpyObj<GameUrlProductService>(
      'GameUrlProductService',
      ['existsByGameUrl', 'incrementCurrentStock', 'decrementCurrentStock'],
    );
    gameUrlProductService.existsByGameUrl.and.returnValue(of([]));
    manualCheckService = jasmine.createSpyObj<ManualCheckService>('ManualCheckService', [
      'createRun',
      'getRun',
      'cancelRun',
      'pauseRun',
      'continueRun',
    ]);
    dialog = { open: jasmine.createSpy('open') };

    component = new ManualModeV2(
      emptyService as never,
      emptyService as never,
      gameUrlProductService,
      emptyService as never,
      emptyService as never,
      manualCheckService,
      dialog as never,
      cdr,
      disclosure,
    );
  });

  it('routes open-all through disclosure and stops when acceptance is needed', () => {
    component.productsFiltered = [
      { fullUrl: 'https://backpack.tf/stats' },
      { fullUrl: 'https://steamcommunity.com/market/' },
    ] as never;
    disclosure.openTrustedUrl.and.returnValue('needs-disclosure');

    component.openAllButtonClicked();

    expect(disclosure.openTrustedUrl).toHaveBeenCalledOnceWith(
      'https://backpack.tf/stats',
      '/manual-mode-v2',
    );
  });

  it('opens batch URLs through the disclosure service without advancing the current item', () => {
    component.selectedGameUrl = {
      scrapingModeId: ScrapingModeEnum.Batch,
      partialUrl: 'https://steamcommunity.com/market/search?q={0}',
    } as never;
    component.currentIndex = 1;
    component.batchSize = 2;
    disclosure.openTrustedUrl.and.returnValue('opened');

    component.startBatchButtonClicked();

    expect(disclosure.openTrustedUrl.calls.allArgs()).toEqual([
      ['https://steamcommunity.com/market/search?q=1', '/manual-mode-v2'],
      ['https://steamcommunity.com/market/search?q=2', '/manual-mode-v2'],
    ]);
    expect(component.currentIndex).toBe(1);
    expect(cdr.markForCheck).toHaveBeenCalled();
  });

  it('opens batch URLs through Steam when Steam mode is checked', () => {
    component.selectedGameUrl = {
      scrapingModeId: ScrapingModeEnum.Batch,
      partialUrl: 'https://steamcommunity.com/market/search?q={0}',
    } as never;
    component.currentIndex = 1;
    component.batchSize = 2;
    component.openInSteamMode = true;
    disclosure.openTrustedUrl.and.returnValue('opened');

    component.startBatchButtonClicked();

    expect(disclosure.openTrustedUrl.calls.allArgs()).toEqual([
      [
        'steam://openurl/https://steamcommunity.com/market/search?q=1',
        '/manual-mode-v2',
      ],
    ]);
    expect(component.currentIndex).toBe(1);
    expect(component.batchSize).toBe(1);
  });

  it('opens manual product batch URLs through Steam when Steam mode is checked', () => {
    component.selectedGameUrl = {
      scrapingModeId: ScrapingModeEnum.ManualBatch,
    } as never;
    component.productsFiltered = [
      { fullUrl: 'https://backpack.tf/stats/Unique/Hat/Tradable/Craftable' },
      { fullUrl: 'https://steamcommunity.com/market/listings/440/Alpha%20Item' },
    ] as never;
    component.currentIndex = 1;
    component.batchSize = 2;
    component.openInSteamMode = true;
    disclosure.openTrustedUrl.and.returnValue('opened');

    component.startBatchButtonClicked();

    expect(disclosure.openTrustedUrl.calls.allArgs()).toEqual([
      [
        'steam://openurl/https://backpack.tf/stats/Unique/Hat/Tradable/Craftable',
        '/manual-mode-v2',
      ],
    ]);
    expect(component.currentIndex).toBe(1);
    expect(component.batchSize).toBe(1);
  });

  it('forces a batch size of one until Steam mode is disabled', () => {
    component.batchSize = 4;

    component.openInSteamMode = true;

    expect(component.openInSteamMode).toBeTrue();
    expect(component.batchSize).toBe(1);

    component.openInSteamMode = false;
    component.batchSize = 3;

    expect(component.openInSteamMode).toBeFalse();
    expect(component.batchSize).toBe(3);
  });

  it('runs and selects the previous batch', () => {
    component.selectedGameUrl = {
      scrapingModeId: ScrapingModeEnum.Batch,
      partialUrl: 'https://steamcommunity.com/market/search?q={0}',
    } as never;
    component.currentIndex = 3;
    component.batchSize = 2;
    disclosure.openTrustedUrl.and.returnValue('opened');

    component.runPreviousBatchButtonClicked();

    expect(disclosure.openTrustedUrl.calls.allArgs()).toEqual([
      ['https://steamcommunity.com/market/search?q=1', '/manual-mode-v2'],
      ['https://steamcommunity.com/market/search?q=2', '/manual-mode-v2'],
    ]);
    expect(component.currentIndex).toBe(1);
  });

  it('runs and selects the next batch', () => {
    component.selectedGameUrl = {
      scrapingModeId: ScrapingModeEnum.Batch,
      partialUrl: 'https://steamcommunity.com/market/search?q={0}',
    } as never;
    component.currentIndex = 1;
    component.batchSize = 2;
    disclosure.openTrustedUrl.and.returnValue('opened');

    component.runNextBatchButtonClicked();

    expect(disclosure.openTrustedUrl.calls.allArgs()).toEqual([
      ['https://steamcommunity.com/market/search?q=3', '/manual-mode-v2'],
      ['https://steamcommunity.com/market/search?q=4', '/manual-mode-v2'],
    ]);
    expect(component.currentIndex).toBe(3);
  });

  it('resets batch settings to one', () => {
    component.currentIndex = 9;
    component.batchSize = 4;

    component.clearBatchButtonClicked();

    expect(component.currentIndex).toBe(1);
    expect(component.batchSize).toBe(1);
  });

  it('uses Steam URLs for product cards when Steam mode is checked', () => {
    const productUrl = 'https://steamcommunity.com/market/listings/440/Alpha%20Item';

    expect(component.getProductOpenUrl(productUrl)).toBe(productUrl);

    component.openInSteamMode = true;

    expect(component.getProductOpenUrl(productUrl)).toBe(
      'steam://openurl/https://steamcommunity.com/market/listings/440/Alpha%20Item',
    );
  });

  it('only enables automated checks for active Manual Batch sources', () => {
    expect(component.isAutomatedCheckEligible({
      isActive: true,
      scrapingModeId: ScrapingModeEnum.ManualBatch,
    } as never)).toBeTrue();
    expect(component.isAutomatedCheckEligible({
      isActive: false,
      scrapingModeId: ScrapingModeEnum.ManualBatch,
    } as never)).toBeFalse();
    expect(component.isAutomatedCheckEligible({
      isActive: true,
      scrapingModeId: ScrapingModeEnum.Batch,
    } as never)).toBeFalse();
  });

  it('applies partial automated results to the existing product grid', fakeAsync(() => {
    dialog.open.and.returnValue({ afterClosed: () => of(77) });
    manualCheckService.getRun.and.returnValue(of(runDetail('CompletedWithErrors', 1, 1)));
    component.products = [
      {
        productId: 5,
        productName: 'Matched Item',
        isActive: true,
        tags: ['primary'],
        rating: 7,
      },
      {
        productId: 6,
        productName: 'Unmatched Item',
        isActive: true,
        tags: [],
        rating: 3,
      },
    ] as never;

    component.automatedCheckHistoryButtonClicked();
    tick(0);

    expect(component.products.map((x) => x.productName)).toEqual(['Matched Item', 'Unmatched Item']);
    expect(component.productsFiltered).toEqual(component.products);
    expect(component.getAutomatedMatch(5)?.matchedAssets.length).toBe(1);
    expect(component.automatedCheckWarning).toContain('1 product page(s) failed');
    expect(component.hasAutomatedCheckResult).toBeTrue();
    expect(component.formatDuration(component.automatedRun?.durationMilliseconds)).toBe('1.2 s');
  }));

  it('opens a terminal automated result directly in the result dialog', () => {
    const completed = runDetail('CompletedWithErrors', 1, 1);
    component.automatedRun = completed;

    component.openAutomatedResult();

    expect(dialog.open).toHaveBeenCalledWith(
      ManualCheckTraceDialogComponent,
      jasmine.objectContaining({
        data: { detail: completed, initialView: 'errors' },
      }),
    );
  });

  it('does not open the result dialog while the run is still active', () => {
    component.automatedRun = runDetail('Running', 0, 0);
    component.automatedRunActive = true;

    component.openAutomatedResult();

    expect(component.canViewAutomatedResult).toBeFalse();
    expect(dialog.open).not.toHaveBeenCalled();
  });

  it('opens a canceled result with no product failures on the checks view', () => {
    const canceled = runDetail('Canceled', 1, 0);
    component.automatedRun = canceled;

    component.openAutomatedResult();

    expect(dialog.open).toHaveBeenCalledWith(
      ManualCheckTraceDialogComponent,
      jasmine.objectContaining({
        data: { detail: canceled, initialView: 'results' },
      }),
    );
  });

  it('filters the product list to partial matches from a canceled run', () => {
    const canceled = runDetail('Canceled', 1, 0);
    manualCheckService.cancelRun.and.returnValue(of(canceled));
    component.products = [
      {
        productId: 5,
        productName: 'Matched Item',
        fullUrl: 'https://steamcommunity.com/market/listings/440/Matched%20Item',
        isActive: true,
        tags: ['primary'],
        rating: 7,
      },
      {
        productId: 6,
        productName: 'Other Item',
        fullUrl: 'https://steamcommunity.com/market/listings/440/Other%20Item',
        isActive: true,
        tags: [],
        rating: 3,
      },
    ] as never;
    component.automatedRunId = 77;
    component.automatedRunActive = true;

    component.cancelAutomatedRun();
    component.setAutomatedMatchesOnly(true);

    expect(component.canViewAutomatedResult).toBeTrue();
    expect(component.productsFiltered.map((product) => product.productId)).toEqual([5]);
  });

  it('composes matched-only filtering with the existing rating filter', () => {
    const completed = runDetail('Succeeded', 1, 0);
    manualCheckService.cancelRun.and.returnValue(of(completed));
    component.products = [
      { productId: 5, productName: 'Matched Item', isActive: true, rating: 7 },
      { productId: 6, productName: 'Other Item', isActive: true, rating: 10 },
    ] as never;
    component.automatedRunId = 77;
    component.automatedRunActive = true;
    component.cancelAutomatedRun();
    component.searchByRatingFilterControl.setValue(8);

    component.setAutomatedMatchesOnly(true);

    expect(component.productsFiltered).toEqual([]);
  });

  it('uses the matched-only filtered list for Open All and clears the filter', () => {
    const completed = runDetail('Succeeded', 1, 0);
    manualCheckService.cancelRun.and.returnValue(of(completed));
    component.products = [
      {
        productId: 5,
        productName: 'Matched Item',
        fullUrl: 'https://steamcommunity.com/market/listings/440/Matched%20Item',
        isActive: true,
      },
      {
        productId: 6,
        productName: 'Other Item',
        fullUrl: 'https://steamcommunity.com/market/listings/440/Other%20Item',
        isActive: true,
      },
    ] as never;
    component.automatedRunId = 77;
    component.automatedRunActive = true;
    disclosure.openTrustedUrl.and.returnValue('opened');
    component.cancelAutomatedRun();
    component.setAutomatedMatchesOnly(true);

    component.openAllButtonClicked();

    expect(disclosure.openTrustedUrl).toHaveBeenCalledOnceWith(
      'https://steamcommunity.com/market/listings/440/Matched%20Item',
      '/manual-mode-v2',
    );

    component.clearFiltersButtonClicked();

    expect(component.showAutomatedMatchesOnly).toBeFalse();
    expect(component.productsFiltered).toEqual(component.products);
  });

  it('exports only the matched products when the automated filter is enabled', () => {
    const completed = runDetail('Succeeded', 1, 0);
    manualCheckService.cancelRun.and.returnValue(of(completed));
    component.products = [
      { productId: 5, productName: 'Matched Item', isActive: true },
      { productId: 6, productName: 'Other Item', isActive: true },
    ] as never;
    component.automatedRunId = 77;
    component.automatedRunActive = true;
    component.cancelAutomatedRun();
    component.setAutomatedMatchesOnly(true);
    const worksheet = {} as XLSX.WorkSheet;
    const workbook = {} as XLSX.WorkBook;
    const toSheet = spyOn(XLSX.utils, 'json_to_sheet').and.returnValue(worksheet);
    spyOn(XLSX.utils, 'book_new').and.returnValue(workbook);
    spyOn(XLSX.utils, 'book_append_sheet').and.throwError('Stop before browser download.');

    expect(() => component.exportButtonClicked()).toThrowError('Stop before browser download.');

    expect(toSheet).toHaveBeenCalledOnceWith([component.products[0]]);
  });

  it('keeps the loaded product grid after a successful zero-match run', fakeAsync(() => {
    dialog.open.and.returnValue({ afterClosed: () => of(78) });
    manualCheckService.getRun.and.returnValue(of(runDetail('Succeeded', 0, 0)));
    component.products = [{ productId: 8, productName: 'Loaded Item', isActive: true }] as never;

    component.automatedCheckHistoryButtonClicked();
    tick(0);

    expect(component.products.map((product) => product.productName)).toEqual(['Loaded Item']);
    expect(component.hasAutomatedCheckResult).toBeTrue();
    expect(component.automatedCheckError).toBe('');
  }));

  it('cancels an active automated run and keeps the product grid', () => {
    const canceled = runDetail('Canceled', 0, 0);
    canceled.errorText = 'Canceled by the user before any products were checked.';
    manualCheckService.cancelRun.and.returnValue(of(canceled));
    component.products = [{ productId: 8, productName: 'Loaded Item', isActive: true }] as never;
    component.automatedRunId = 77;
    component.automatedRunActive = true;

    component.cancelAutomatedRun();

    expect(manualCheckService.cancelRun).toHaveBeenCalledOnceWith(77);
    expect(component.automatedRun?.status).toBe('Canceled');
    expect(component.products.map((product) => product.productName)).toEqual(['Loaded Item']);
    expect(component.automatedCheckWarning).toContain('Canceled by the user');
  });

  it('loads the product grid automatically when an automated run starts', fakeAsync(() => {
    const completed = runDetail('Succeeded', 0, 0);
    component.selectedGameUrl = { id: 2 } as never;
    const loadedProduct = {
      productId: 8,
      productName: 'Loaded Item',
      isActive: true,
    } as GameUrlProduct;
    gameUrlProductService.existsByGameUrl.and.returnValue(of([loadedProduct]));
    manualCheckService.createRun.and.returnValue(of({
      runId: 77,
      run: completed,
    }));
    manualCheckService.getRun.and.returnValue(of(completed));

    (component as unknown as {
      startAutomatedRun(
        gameUrlId: number,
        presetId: number,
        bypassCache: boolean,
        productIds: number[] | null,
      ): void;
    }).startAutomatedRun(2, 3, false, null);
    tick(0);

    expect(gameUrlProductService.existsByGameUrl).toHaveBeenCalledOnceWith(2);
    expect(manualCheckService.createRun).toHaveBeenCalledOnceWith({
      gameUrlId: 2,
      presetId: 3,
      bypassCache: false,
      productIds: null,
    });
    expect(component.products.map((product) => product.productName)).toEqual(['Loaded Item']);
  }));

  it('passes the manual refresh choice from the setup panel to the run request', fakeAsync(() => {
    const completed = runDetail('Succeeded', 0, 0);
    component.gameIdControl.setValue(1);
    component.selectedGameUrl = {
      id: 2,
      name: 'Market',
      isActive: true,
      scrapingModeId: ScrapingModeEnum.ManualBatch,
    } as never;
    dialog.open.and.returnValue({
      afterClosed: () => of({ presetId: 3, bypassCache: true }),
    });
    manualCheckService.createRun.and.returnValue(of({ runId: 77, run: completed }));
    manualCheckService.getRun.and.returnValue(of(completed));

    component.automatedCheckButtonClicked();
    tick(0);

    expect(dialog.open).toHaveBeenCalledOnceWith(
      ManualCheckSetupDialogComponent,
      jasmine.objectContaining({
        width: 'min(76rem, 96vw)',
        maxWidth: '96vw',
        maxHeight: '92vh',
        panelClass: 'manual-check-setup-dialog-panel',
        disableClose: true,
      }),
    );
    expect(manualCheckService.createRun).toHaveBeenCalledOnceWith({
      gameUrlId: 2,
      presetId: 3,
      bypassCache: true,
      productIds: null,
    });
  }));

  it('runs an automated check for selected products in grid order', fakeAsync(() => {
    const completed = runDetail('Succeeded', 0, 0);
    component.gameIdControl.setValue(1);
    component.selectedGameUrl = {
      id: 2,
      name: 'Market',
      isActive: true,
      scrapingModeId: ScrapingModeEnum.ManualBatch,
    } as never;
    component.products = [
      { productId: 5, productName: 'First', isActive: true },
      { productId: 6, productName: 'Second', isActive: true },
    ] as never;
    component.productsFiltered = [...component.products];
    component.setProductSelected(6, true);
    component.setProductSelected(5, true);
    dialog.open.and.returnValue({
      afterClosed: () => of({ presetId: 3, bypassCache: false }),
    });
    manualCheckService.createRun.and.returnValue(of({ runId: 77, run: completed }));
    manualCheckService.getRun.and.returnValue(of(completed));

    component.automatedCheckSelectedProducts();
    tick(0);

    expect(manualCheckService.createRun).toHaveBeenCalledOnceWith({
      gameUrlId: 2,
      presetId: 3,
      bypassCache: false,
      productIds: [5, 6],
    });
  }));

  it('runs an automated check for one product from its card action', fakeAsync(() => {
    const completed = runDetail('Succeeded', 0, 0);
    const product = { productId: 6, productName: 'Second', isActive: true } as GameUrlProduct;
    component.gameIdControl.setValue(1);
    component.selectedGameUrl = {
      id: 2,
      name: 'Market',
      isActive: true,
      scrapingModeId: ScrapingModeEnum.ManualBatch,
    } as never;
    component.products = [product];
    dialog.open.and.returnValue({
      afterClosed: () => of({ presetId: 3, bypassCache: true }),
    });
    manualCheckService.createRun.and.returnValue(of({ runId: 77, run: completed }));
    manualCheckService.getRun.and.returnValue(of(completed));

    component.automatedCheckProduct(product);
    tick(0);

    expect(manualCheckService.createRun).toHaveBeenCalledOnceWith({
      gameUrlId: 2,
      presetId: 3,
      bypassCache: true,
      productIds: [6],
    });
  }));

  it('filters stock presets and exact values mutually exclusively', () => {
    component.ngOnInit();
    component.products = [
      { productId: 1, productName: 'Negative', currentStock: -1 },
      { productId: 2, productName: 'Zero', currentStock: 0 },
      { productId: 3, productName: 'Positive', currentStock: 2 },
    ] as GameUrlProduct[];
    component.stockStateFilterControl.setValue('negative');
    expect(component.productsFiltered.map((product) => product.productId)).toEqual([1]);
    expect(component.searchByStockFilterControl.value).toBeNull();

    component.searchByStockFilterControl.setValue(0);
    expect(component.stockStateFilterControl.value).toBe('all');
    expect(component.productsFiltered.map((product) => product.productId)).toEqual([2]);
  });

  it('sorts equal stock values by name and product ID', () => {
    component.ngOnInit();
    component.products = [
      { productId: 3, productName: 'Zulu', currentStock: 5 },
      { productId: 2, productName: 'Alpha', currentStock: -1 },
      { productId: 1, productName: 'Alpha', currentStock: -1 },
    ] as GameUrlProduct[];
    component.stockSortControl.setValue('ascending');

    expect(component.productsFiltered.map((product) => product.productId)).toEqual([1, 2, 3]);
  });

  it('updates one card from the authoritative increment response', () => {
    const product = {
      productId: 6,
      gameUrlId: 2,
      productName: 'Second',
      currentStock: 4,
    } as GameUrlProduct;
    component.products = [product];
    component.productsFiltered = [product];
    gameUrlProductService.incrementCurrentStock.and.returnValue(of({ currentStock: 5 }));

    component.incrementCurrentStock(product);

    expect(gameUrlProductService.incrementCurrentStock).toHaveBeenCalledOnceWith(6, 2);
    expect(product.currentStock).toBe(5);
    expect(component.isStockUpdating(6)).toBeFalse();
  });

  it('opens Advanced Stock and applies the returned final value', () => {
    const product = {
      productId: 6,
      gameUrlId: 2,
      productName: 'Second',
      currentStock: 4,
    } as GameUrlProduct;
    component.products = [product];
    component.productsFiltered = [product];
    dialog.open.and.returnValue({ afterClosed: () => of(-10) });

    component.openAdvancedStock(product);

    expect(dialog.open).toHaveBeenCalledOnceWith(
      AdvancedStockDialogComponent,
      jasmine.objectContaining({
        data: {
          productId: 6,
          gameUrlId: 2,
          productName: 'Second',
          currentStock: 4,
        },
      }),
    );
    expect(product.currentStock).toBe(-10);
  });

  it('preserves selection across filters and selects only currently shown products', () => {
    component.products = [
      { productId: 5, productName: 'First', isActive: true },
      { productId: 6, productName: 'Second', isActive: true },
      { productId: 7, productName: 'Third', isActive: true },
    ] as never;
    component.productsFiltered = [component.products[0], component.products[1]];

    component.setFilteredProductsSelected(true);
    component.productsFiltered = [component.products[2]];

    expect(component.selectedProductCount).toBe(2);
    expect(component.isProductSelected(5)).toBeTrue();
    expect(component.isProductSelected(6)).toBeTrue();
    expect(component.isProductSelected(7)).toBeFalse();
  });

  it('applies matched, no-match, failed, and pending outcomes from a running poll', fakeAsync(() => {
    const running = runDetail('Running', 1, 1);
    running.totalProducts = 4;
    running.checkedProducts = 3;
    running.setup.products = [5, 6, 7, 8].map((productId) => ({
      productId,
      productName: `Product ${productId}`,
      gameUrlId: 2,
      gameUrlName: 'Market',
      fullUrl: `https://steamcommunity.com/market/listings/440/Product%20${productId}`,
      tags: [],
      rating: null,
    }));
    running.results.productTraces = [
      { productId: 5, productName: 'Matched', fullUrl: '', matchEvaluated: true, matched: true, matchedAssetCount: 1, steamApiResultJson: null, durationMilliseconds: 1 },
      { productId: 6, productName: 'Failed', fullUrl: '', matchEvaluated: false, matched: false, matchedAssetCount: 0, steamApiResultJson: null, durationMilliseconds: 1 },
      { productId: 7, productName: 'No match', fullUrl: '', matchEvaluated: true, matched: false, matchedAssetCount: 0, steamApiResultJson: null, durationMilliseconds: 1 },
    ];
    dialog.open.and.returnValue({ afterClosed: () => of(77) });
    manualCheckService.getRun.and.returnValue(of(running));

    component.automatedCheckHistoryButtonClicked();
    tick(0);

    expect(component.getAutomatedProductOutcome(5)).toBe('Matched');
    expect(component.getAutomatedProductOutcome(6)).toBe('Failed');
    expect(component.getAutomatedProductOutcome(7)).toBe('No match');
    expect(component.getAutomatedProductOutcome(8)).toBe('Pending');
    expect(component.automatedRunActive).toBeTrue();
    component.ngOnDestroy();
  }));

  it('opens a product Steam API result only when the response is available', fakeAsync(() => {
    const running = runDetail('Running', 0, 0);
    const availableTrace = {
      productId: 5,
      productName: 'Available',
      fullUrl: 'https://steamcommunity.com/market/listings/440/Available',
      matchEvaluated: true,
      matched: false,
      matchedAssetCount: 0,
      steamApiResultJson: '{"success":true}',
      durationMilliseconds: 1,
    };
    running.setup.products = [5, 6].map((productId) => ({
      productId,
      productName: `Product ${productId}`,
      gameUrlId: 2,
      gameUrlName: 'Market',
      fullUrl: `https://steamcommunity.com/market/listings/440/Product%20${productId}`,
      tags: [],
      rating: null,
    }));
    running.results.productTraces = [
      availableTrace,
      {
        ...availableTrace,
        productId: 6,
        productName: 'Unavailable',
        steamApiResultJson: null,
      },
    ];
    dialog.open.and.returnValue({ afterClosed: () => of(77) });
    manualCheckService.getRun.and.returnValue(of(running));

    component.automatedCheckHistoryButtonClicked();
    tick(0);
    dialog.open.calls.reset();

    expect(component.getAutomatedSteamApiResult(5)).toBe(availableTrace);
    expect(component.getAutomatedSteamApiResult(6)).toBeNull();

    component.openAutomatedSteamApiResult(5);
    component.openAutomatedSteamApiResult(6);

    expect(dialog.open).toHaveBeenCalledOnceWith(
      ManualCheckSteamResultDialogComponent,
      jasmine.objectContaining({ data: availableTrace }),
    );
    component.ngOnDestroy();
  }));

  it('pauses and continues the same automated run', () => {
    const running = runDetail('Running', 0, 0);
    const pausing = { ...running, status: 'PauseRequested' as const };
    const paused = { ...running, status: 'Paused' as const };
    component.automatedRun = running;
    component.automatedRunId = 77;
    component.automatedRunActive = true;
    manualCheckService.pauseRun.and.returnValue(of(pausing));

    component.pauseAutomatedRun();

    expect(manualCheckService.pauseRun).toHaveBeenCalledOnceWith(77);
    expect(component.automatedRun?.status).toBe('PauseRequested');

    component.automatedRun = paused;
    manualCheckService.continueRun.and.returnValue(of({
      runId: 77,
      run: { ...paused, status: 'Queued' },
    }));

    component.continueAutomatedRun();

    expect(manualCheckService.continueRun).toHaveBeenCalledOnceWith(77);
    expect(component.automatedRun?.status).toBe('Queued');
  });

  it('includes the resolved product URL in warning tooltips', () => {
    const productUrl = 'https://steamcommunity.com/market/listings/440/Alpha%20Item';
    component.openInSteamMode = true;

    expect(component.getProductOpenTooltip(productUrl)).toBe(
      'Warning: this destination is not verified by SteamApp. Review it carefully before opening. Destination: steam://openurl/https://steamcommunity.com/market/listings/440/Alpha%20Item',
    );
  });

  function runDetail(
    status: ManualCheckRunDetail['status'],
    matches: number,
    failures: number,
  ): ManualCheckRunDetail {
    const now = new Date().toISOString();
    return {
      id: 77,
      presetId: 3,
      presetName: 'Sheens',
      gameId: 1,
      gameName: 'TF2',
      gameUrlId: 2,
      gameUrlName: 'Market',
      totalProducts: matches + failures,
      checkedProducts: matches + failures,
      matchedProducts: matches,
      failedProducts: failures,
      progress: {
        totalProducts: matches + failures,
        checkedProducts: matches + failures,
        matchedProducts: matches,
        failedProducts: failures,
      },
      status,
      date: now,
      startedAtUtc: now,
      completedAtUtc: now,
      durationMilliseconds: 1234,
      correlationId: 'test',
      setup: {
        presetId: 3,
        presetName: 'Sheens',
        gameId: 1,
        gameName: 'TF2',
        gameUrlId: 2,
        gameUrlName: 'Market',
        listingLimit: 10,
        cooldownMinutes: null,
        cooldownSeconds: null,
        bypassCache: false,
        requestedProductIds: null,
        criteria: [{ conditionOperatorId: null, nameContains: null, valueContains: 'Mean Green' }],
        products: [],
        requestedAtUtc: now,
      },
      results: {
        matches: matches ? [{
          productId: 5,
          productName: 'Matched Item',
          gameUrlId: 2,
          gameUrlName: 'Market',
          fullUrl: 'https://steamcommunity.com/market/listings/440/Matched%20Item',
          tags: ['primary'],
          rating: 7,
          matchedAssets: [{
            appId: '440',
            contextId: '2',
            assetId: 'asset',
            classId: 'class',
            instanceId: 'instance',
            marketName: 'Matched Item',
            iconUrl: '',
            descriptions: [{
              name: 'attribute',
              value: 'Sheen: Mean Green',
              color: '7ea9d1',
              matchedCriterionIndexes: [0],
            }],
          }],
        }] : [],
        productTraces: [],
        errors: failures ? [{
          productId: 6,
          productName: 'Failed Item',
          fullUrl: 'https://steamcommunity.com/market/listings/440/Failed%20Item',
          error: 'timeout',
        }] : [],
      },
      errorText: status === 'Failed'
        ? 'failed'
        : status === 'Canceled'
          ? 'Canceled by the user.'
          : null,
    };
  }
});
