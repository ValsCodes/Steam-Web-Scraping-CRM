import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { ManualCheckPresetWrite } from '../../models';
import { ManualCheckService } from './manual-check.service';
import * as g from '../general-data';

describe('ManualCheckService global presets', () => {
  let service: ManualCheckService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(ManualCheckService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('updates the complete preset and URL selection through the admin route', () => {
    const input: ManualCheckPresetWrite = {
      gameId: 1, gameUrlIds: [7, 8], itemGroupId: null, name: 'Global', listingLimit: 10,
      cooldownMinutes: null, cooldownSeconds: null,
      criteria: [{ conditionOperatorId: null, nameContains: null, valueContains: 'Unusual' }],
    };
    service.updateGlobalPreset(3, input).subscribe((saved) => expect(saved.id).toBe(3));

    const request = http.expectOne(`${g.localHost}api/admin/manual-check-presets/3`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(input);
    request.flush({ ...input, id: 3 });
  });

  it('deletes through the admin route', () => {
    let completed = false;
    service.deleteGlobalPreset(3).subscribe(() => completed = true);

    const request = http.expectOne(`${g.localHost}api/admin/manual-check-presets/3`);
    expect(request.request.method).toBe('DELETE');
    request.flush(null);
    expect(completed).toBeTrue();
  });
});
