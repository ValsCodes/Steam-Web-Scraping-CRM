import { ChangeDetectorRef } from '@angular/core';
import { fakeAsync, tick } from '@angular/core/testing';
import { of } from 'rxjs';

import {
  ExternalLinkDisclosureService,
  GameUrlProductService,
  ManualCheckService,
} from '../../services';
import { GameUrlProduct, ManualCheckRunDetail, ScrapingModeEnum } from '../../models';
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
      ['existsByGameUrl'],
    );
    gameUrlProductService.existsByGameUrl.and.returnValue(of([]));
    manualCheckService = jasmine.createSpyObj<ManualCheckService>('ManualCheckService', [
      'createRun',
      'getRun',
      'cancelRun',
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
      [
        'steam://openurl/https://steamcommunity.com/market/search?q=2',
        '/manual-mode-v2',
      ],
    ]);
    expect(component.currentIndex).toBe(1);
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
      [
        'steam://openurl/https://steamcommunity.com/market/listings/440/Alpha%20Item',
        '/manual-mode-v2',
      ],
    ]);
    expect(component.currentIndex).toBe(1);
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
  }));

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
      startAutomatedRun(gameUrlId: number, presetId: number): void;
    }).startAutomatedRun(2, 3);
    tick(0);

    expect(gameUrlProductService.existsByGameUrl).toHaveBeenCalledOnceWith(2);
    expect(component.products.map((product) => product.productName)).toEqual(['Loaded Item']);
  }));

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
      correlationId: 'test',
      setup: {
        presetId: 3,
        presetName: 'Sheens',
        gameId: 1,
        gameName: 'TF2',
        gameUrlId: 2,
        gameUrlName: 'Market',
        matchMode: 'Any',
        criteria: [{ nameContains: null, valueContains: 'Mean Green' }],
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
