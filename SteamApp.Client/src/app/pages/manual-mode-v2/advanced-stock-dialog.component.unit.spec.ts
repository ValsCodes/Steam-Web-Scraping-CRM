import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { of } from 'rxjs';

import { GameUrlProductService } from '../../services';
import {
  AdvancedStockDialogComponent,
  AdvancedStockDialogData,
} from './advanced-stock-dialog.component';

describe('AdvancedStockDialogComponent', () => {
  let fixture: ComponentFixture<AdvancedStockDialogComponent>;
  let component: AdvancedStockDialogComponent;
  let service: jasmine.SpyObj<GameUrlProductService>;
  let dialogRef: jasmine.SpyObj<MatDialogRef<AdvancedStockDialogComponent, number>>;

  beforeEach(async () => {
    service = jasmine.createSpyObj<GameUrlProductService>('GameUrlProductService', [
      'assignCurrentStock',
      'incrementCurrentStock',
      'decrementCurrentStock',
      'getCurrentStockHistory',
    ]);
    service.getCurrentStockHistory.and.returnValue(of({
      items: [], pageNumber: 1, pageSize: 25, totalCount: 0, totalPages: 0,
    }));
    dialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);

    await TestBed.configureTestingModule({
      imports: [AdvancedStockDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: {
          productId: 4,
          gameUrlId: 7,
          productName: 'Rocket Launcher',
          currentStock: 3,
        } satisfies AdvancedStockDialogData },
        { provide: MatDialogRef, useValue: dialogRef },
        { provide: GameUrlProductService, useValue: service },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AdvancedStockDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads the first history page on open', () => {
    expect(service.getCurrentStockHistory).toHaveBeenCalledOnceWith(4, 7, 1, 25);
  });

  it('assigns a signed integer and refreshes history', () => {
    service.assignCurrentStock.and.returnValue(of({ currentStock: -8 }));
    component.stockControl.setValue(-8);

    component.assign();

    expect(service.assignCurrentStock).toHaveBeenCalledOnceWith(4, 7, -8);
    expect(component.currentStock).toBe(-8);
    expect(service.getCurrentStockHistory).toHaveBeenCalledTimes(2);
  });

  it('rejects a fractional assignment', () => {
    component.stockControl.setValue(1.5);

    component.assign();

    expect(service.assignCurrentStock).not.toHaveBeenCalled();
  });

  it('increments and returns the final value when closed', () => {
    service.incrementCurrentStock.and.returnValue(of({ currentStock: 4 }));

    component.increment();
    component.close();

    expect(component.currentStock).toBe(4);
    expect(dialogRef.close).toHaveBeenCalledOnceWith(4);
  });
});
