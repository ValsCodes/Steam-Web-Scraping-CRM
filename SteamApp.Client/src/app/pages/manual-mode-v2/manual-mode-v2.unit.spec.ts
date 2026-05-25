import { ChangeDetectorRef } from '@angular/core';
import { of } from 'rxjs';

import { ExternalLinkDisclosureService } from '../../services';
import { ScrapingModeEnum } from '../../models';
import { ManualModeV2 } from './manual-mode-v2';

describe('ManualModeV2 external link disclosure', () => {
  let component: ManualModeV2;
  let disclosure: jasmine.SpyObj<ExternalLinkDisclosureService>;
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

    component = new ManualModeV2(
      emptyService as never,
      emptyService as never,
      emptyService as never,
      emptyService as never,
      emptyService as never,
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

  it('opens batch URLs through the disclosure service and advances the batch index', () => {
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
    expect(component.currentIndex).toBe(3);
    expect(cdr.markForCheck).toHaveBeenCalled();
  });
});
