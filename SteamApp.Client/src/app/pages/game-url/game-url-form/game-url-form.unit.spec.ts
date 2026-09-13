import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';

import { GameUrlForm } from './game-url-form';
import { GameUrlProduct, ItemGroup, Product, ProductTagDetail } from '../../../models';
import {
  GameService,
  GameUrlPixelService,
  GameUrlProductService,
  GameUrlService,
  ItemGroupService,
  PixelService,
  ProductService,
  ScrapingModeService,
} from '../../../services';

describe('GameUrlForm item groups', () => {
  async function setup(id?: string, products: Product[] = [], initialRelations: GameUrlProduct[] = []) {
    const gameUrlService = jasmine.createSpyObj<GameUrlService>('GameUrlService', ['create', 'getById', 'update']);
    const itemGroupService = jasmine.createSpyObj<ItemGroupService>('ItemGroupService', ['create', 'getByGame']);
    const router = jasmine.createSpyObj<Router>('Router', ['navigate']);
    const lookupService = { getAll: () => of([]) };
    const relationService = { existsByGameUrl: () => of([]) };
    const productRelationService = jasmine.createSpyObj<GameUrlProductService>(
      'GameUrlProductService', ['existsByGameUrl', 'bulkSync', 'create', 'delete']);
    productRelationService.existsByGameUrl.and.returnValue(of(initialRelations));
    productRelationService.bulkSync.and.returnValue(of(void 0));

    itemGroupService.getByGame.and.returnValue(of([{ id: 10, gameId: 1, name: 'Category' }]));
    itemGroupService.create.and.returnValue(of({ id: 11, gameId: 1, name: 'Priority' }));
    gameUrlService.getById.and.returnValue(of({
      id: Number(id ?? 1),
      name: 'Market',
      gameId: 1,
      gameName: 'Portal',
      itemGroupId: 10,
      itemGroupName: 'Category',
      scrapingModeId: 1,
      isActive: true,
    }));
    gameUrlService.create.and.returnValue(of({
      id: 20,
      name: 'Market',
      gameId: 1,
      gameName: 'Portal',
      itemGroupId: null,
      itemGroupName: null,
      isActive: true,
    }));
    gameUrlService.update.and.returnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [GameUrlForm],
      providers: [
        { provide: GameUrlService, useValue: gameUrlService },
        { provide: ItemGroupService, useValue: itemGroupService },
        { provide: GameService, useValue: lookupService },
        { provide: ProductService, useValue: { getAll: () => of(products) } },
        { provide: PixelService, useValue: lookupService },
        { provide: ScrapingModeService, useValue: lookupService },
        { provide: GameUrlProductService, useValue: productRelationService },
        { provide: GameUrlPixelService, useValue: relationService },
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap(id ? { id } : {}) } } },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(GameUrlForm);
    const component = fixture.componentInstance;
    fixture.detectChanges();
    return { component, fixture, gameUrlService, itemGroupService, router, productRelationService };
  }

  it('saves 700 selected products in one bulk call instead of individual relation requests', async () => {
    const { component, productRelationService, router } = await setup();
    component.form.controls.gameId.setValue(1);
    const productIds = Array.from({ length: 700 }, (_, index) => index + 1);
    component.selectedProductIds.set(productIds);
    component.onSubmit();

    expect(productRelationService.bulkSync).toHaveBeenCalledOnceWith(20, productIds);
    expect(productRelationService.create).not.toHaveBeenCalled();
    expect(productRelationService.delete).not.toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/game-urls']);
  });

  it('keeps the selection after a bulk failure and retries against the already-created URL', async () => {
    const { component, fixture, gameUrlService, productRelationService, router } = await setup();
    component.form.controls.gameId.setValue(1);
    component.selectedProductIds.set([1, 2]);
    productRelationService.bulkSync.and.returnValue(throwError(() => new Error('Batch failed')));
    component.onSubmit();
    fixture.detectChanges();

    expect(router.navigate).not.toHaveBeenCalled();
    expect(component.isSubmitting).toBeFalse();
    expect(component.selectedProductIds()).toEqual([1, 2]);
    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain('all-or-nothing');
    productRelationService.bulkSync.and.returnValue(of(void 0));
    component.onSubmit();

    expect(gameUrlService.create).toHaveBeenCalledTimes(1);
    expect(gameUrlService.update).toHaveBeenCalledWith(20, jasmine.anything());
    expect(productRelationService.bulkSync).toHaveBeenCalledTimes(2);
    expect(component.submitError).toBe('');
  });

  it('bulk-syncs an edit selection including deselections and unchanged hidden products', async () => {
    const { component, productRelationService } = await setup('7', [], [
      { productId: 1, productName: 'One', gameUrlId: 7, gameUrlName: 'Market', fullUrl: '', tags: [], isActive: true, rating: null, currentStock: 4 },
      { productId: 2, productName: 'Two', gameUrlId: 7, gameUrlName: 'Market', fullUrl: '', tags: [], isActive: true, rating: null, currentStock: 0 },
    ]);
    component.selectedProductIds.set([2, 3]);
    component.productNameFilterControl.setValue('hidden');
    component.onSubmit();
    expect(productRelationService.bulkSync).toHaveBeenCalledOnceWith(7, [2, 3]);
    expect(productRelationService.delete).not.toHaveBeenCalled();
  });

  function tag(id: number, name: string, itemGroupId: number | null = null, isActive = true): ProductTagDetail {
    return { id, name, isActive, itemGroupId, itemGroupName: itemGroupId === null ? null : 'Category' };
  }

  function product(id: number, name: string | null, tags: ProductTagDetail[] = [], gameId = 1, isActive = true): Product {
    return { id, name, gameId, gameName: 'Portal', isActive, tags: tags.map(tag => tag.name!), tagDetails: tags };
  }

  it('filters names and tags case-insensitively with AND matching and removable chips', async () => {
    const primary = tag(1, 'Primary', 10);
    const rare = tag(2, 'Rare');
    const { component, fixture } = await setup(undefined, [
      product(1, 'Rocket Launcher', [primary, rare]),
      product(2, 'Rocket', [primary]),
      product(3, null),
    ]);
    component.form.controls.gameId.setValue(1);
    component.productNameFilterControl.setValue('ROCKET');
    expect(component.filteredProducts.map(product => product.id)).toEqual([1, 2]);
    component.productTagSelectControl.setValue(primary);
    component.productTagSelectControl.setValue(rare);
    fixture.detectChanges();

    expect(component.filteredProducts.map(product => product.id)).toEqual([1]);
    expect(component.productTagFilters()).toEqual(['primary', 'rare']);
    expect(component.productTagSelectControl.value).toBeNull();
    expect(component.productTagSelectControl.disabled).toBeTrue();
    expect(fixture.nativeElement.querySelectorAll('.product-filter-chip').length).toBe(2);

    component.removeProductTagFilter('rare');
    expect(component.filteredProducts.map(product => product.id)).toEqual([1, 2]);
    expect(component.productTagSelectControl.enabled).toBeTrue();
    component.clearProductFilters();
    expect(component.filteredProducts.map(product => product.id)).toEqual([1, 2, 3]);
  });

  it('groups and deduplicates only active tags on selectable products for the chosen game', async () => {
    const primary = tag(1, 'Primary', 10);
    const { component, fixture } = await setup(undefined, [
      product(1, 'One', [primary, tag(2, 'Archived', null, false)]),
      product(2, 'Two', [primary, tag(3, 'Rare')]),
      product(3, 'Other game', [tag(4, 'Other')], 2),
      product(4, 'Inactive product', [tag(5, 'Hidden')], 1, false),
    ]);
    expect(component.productTagSelectControl.disabled).toBeTrue();
    component.form.controls.gameId.setValue(1);
    fixture.detectChanges();
    expect(component.productTagGroups.map(group => group.name)).toEqual(['Category', 'Ungrouped']);
    expect(component.productTagGroups.flatMap(group => group.items.map(tag => tag.id))).toEqual([1, 3]);
    expect(Array.from(fixture.nativeElement.querySelectorAll('optgroup')).map(group => (group as HTMLOptGroupElement).label))
      .toEqual(['Category', 'Ungrouped']);
    component.productNameFilterControl.setValue('One');
    component.productTagSelectControl.setValue(primary);
    component.form.controls.gameId.setValue(2);
    expect(component.productNameFilterControl.value).toBe('');
    expect(component.productTagFilters()).toEqual([]);
    expect(component.productTagGroups[0].items[0].id).toBe(4);
    component.form.controls.gameId.setValue(null);
    expect(component.productTagSelectControl.disabled).toBeTrue();
  });

  it('renders the first tag in braces and every tag in its hover text without reordering', async () => {
    const { component, fixture } = await setup(undefined, [
      product(1, 'Rocket', [tag(1, 'Primary'), tag(2, 'Archived', null, false)]),
      product(2, 'Untagged'),
    ]);
    component.form.controls.gameId.setValue(1);
    fixture.detectChanges();
    const preview: HTMLElement = fixture.nativeElement.querySelector('.product-first-tag');
    expect(preview.textContent).toBe('{Primary}');
    expect(preview.title).toBe('Primary, Archived');
    expect(fixture.nativeElement.querySelectorAll('.product-first-tag').length).toBe(1);
    component.productNameFilterControl.setValue('missing');
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.relation-empty').textContent).toContain('No products match');
    component.form.controls.gameId.setValue(null);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.relation-empty').textContent).toContain('No products available');
  });


  it('previews forward and reverse ranges without selecting, then applies the previewed range', async () => {
    const { component } = await setup(undefined, [3, 1, 4, 2].map(id => product(id, 'Product ' + id)));
    component.form.controls.gameId.setValue(1);
    component.toggleProduct(1, true);
    component.onProductHover(2, new MouseEvent('mouseenter', { shiftKey: true }));
    expect([...component.productPreviewIds]).toEqual([1, 4, 2]);
    expect(component.selectedProductIds()).toEqual([1]);
    component.onProductHover(3, new MouseEvent('mouseenter', { shiftKey: true }));
    expect([...component.productPreviewIds]).toEqual([3, 1]);
    component.toggleProduct(3, true, true);
    expect(component.productPreviewIds.size).toBe(0);
    expect(new Set(component.selectedProductIds())).toEqual(new Set([1, 3]));
    component.onProductHover(2, new MouseEvent('mouseenter', { shiftKey: true }));
    const pending = [...component.productPreviewIds];
    component.toggleProduct(2, false, true);
    expect(pending.every(id => !new Set(component.selectedProductIds()).has(id))).toBeTrue();
  });

  it('updates a stationary hover on Shift changes and clears previews on exit, blur and filters', async () => {
    const { component } = await setup(undefined, [3, 1, 4, 2].map(id => product(id, 'Product ' + id)));
    component.form.controls.gameId.setValue(1);
    component.toggleProduct(3, true);
    component.onProductHover(2, new MouseEvent('mouseenter'));
    expect(component.productPreviewIds.size).toBe(0);
    component.onProductPreviewKey(new KeyboardEvent('keydown', { key: 'Shift', shiftKey: true }));
    expect(component.productPreviewIds.size).toBe(4);
    component.onProductPreviewKey(new KeyboardEvent('keyup', { key: 'Shift' }));
    expect(component.productPreviewIds.size).toBe(0);
    component.onProductPreviewKey(new KeyboardEvent('keydown', { key: 'Shift', shiftKey: true }));
    expect(component.productPreviewIds.size).toBe(4);
    component.clearProductPreview();
    expect(component.productPreviewIds.size).toBe(0);
    component.onProductHover(2, new MouseEvent('mouseenter', { shiftKey: true }));
    component.onProductPreviewBlur();
    expect(component.productPreviewIds.size).toBe(0);
    component.onProductHover(2, new MouseEvent('mouseenter', { shiftKey: true }));
    component.productNameFilterControl.setValue('Product');
    expect(component.productPreviewIds.size).toBe(0);
  });

  it('renders Shift-hover preview classes only on visible cards and clears them on grid exit', async () => {
    const { component, fixture } = await setup(undefined, [product(3, 'Match'), product(99, 'Hidden'), product(1, 'Match'), product(2, 'Match')]);
    component.form.controls.gameId.setValue(1);
    component.productNameFilterControl.setValue('Match');
    component.toggleProduct(3, true);
    fixture.detectChanges();
    const endpoint: HTMLInputElement = fixture.nativeElement.querySelector('#game-url-product-2');
    endpoint.parentElement!.dispatchEvent(new MouseEvent('mouseenter'));
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Shift', shiftKey: true }));
    fixture.detectChanges();
    expect([...component.productPreviewIds]).toEqual([3, 1, 2]);
    expect(fixture.nativeElement.querySelectorAll('.product-range-preview').length).toBe(3);
    endpoint.closest('.relation-grid')!.dispatchEvent(new MouseEvent('mouseleave'));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.product-range-preview').length).toBe(0);
  });

  it('does not preview without a visible anchor and clears the range on Select All', async () => {
    const { component } = await setup(undefined, [3, 1, 4, 2].map(id => product(id, 'Product ' + id)));
    component.form.controls.gameId.setValue(1);
    component.onProductHover(2, new MouseEvent('mouseenter', { shiftKey: true }));
    expect(component.productPreviewIds.size).toBe(0);
    component.toggleProduct(99, true);
    component.onProductHover(2, new MouseEvent('mouseenter', { shiftKey: true }));
    expect(component.productPreviewIds.size).toBe(0);
    component.toggleProduct(3, true);
    component.onProductHover(2, new MouseEvent('mouseenter', { shiftKey: true }));
    component.toggleAllProducts(true);
    expect(component.productPreviewIds.size).toBe(0);
  });


 it('selects and deselects inclusive Shift ranges in either direction while preserving outside selections', async () => {
    const { component } = await setup(undefined, [1, 2, 3, 4, 5].map(id => product(id, 'Product ' + id)));
    component.form.controls.gameId.setValue(1);
    component.selectedProductIds.set([5]);
    component.toggleProduct(2, true);
    component.toggleProduct(4, true, true);
    expect(component.selectedProductIds()).toEqual([5, 2, 3, 4]);
    component.toggleProduct(1, true, true);
    expect(component.selectedProductIds()).toContain(1);
    component.toggleProduct(3, false, true);
    expect(component.selectedProductIds()).toEqual([5, 4, 1]);
    component.toggleProduct(4, false);
    component.toggleProduct(1, false, true);
    expect(component.selectedProductIds()).toEqual([5]);
  });

  it('uses visible display order for ranges and select-all without changing hidden selections', async () => {
    const { component } = await setup(undefined, [product(3, 'Match'), product(2, 'Hidden'), product(1, 'Match'), product(4, 'Match')]);
    component.form.controls.gameId.setValue(1);
    component.selectedProductIds.set([2]);
    component.productNameFilterControl.setValue('match');
    component.toggleProduct(3, true);
    component.toggleProduct(4, true, true);
    expect(component.selectedProductIds()).toEqual([2, 3, 1, 4]);
    expect(component.areAllFilteredProductsSelected).toBeTrue();
    component.toggleAllProducts(false);
    expect(component.selectedProductIds()).toEqual([2]);
    component.toggleProduct(4, true, true);
    expect(component.selectedProductIds()).toEqual([2, 4]);
    expect(component.areSomeFilteredProductsSelected).toBeTrue();
    component.toggleAllProducts(true);
    component.clearProductFilters();
    expect(component.selectedProductIds()).toEqual([2, 4, 3, 1]);
  });

  it('resets the range anchor on filter changes and falls back when an anchor disappears', async () => {
    const { component } = await setup(undefined, [1, 2, 3].map(id => product(id, 'Product ' + id)));
    component.form.controls.gameId.setValue(1);
    component.toggleProduct(1, true);
    component.productNameFilterControl.setValue('Product');
    component.toggleProduct(3, true, true);
    expect(component.selectedProductIds()).toEqual([1, 3]);
    component.products.set([product(1, 'Product 1'), product(2, 'Product 2')]);
    component.toggleProduct(2, true, true);
    expect(component.selectedProductIds()).toEqual([1, 3, 2]);
    component.form.controls.gameId.setValue(2);
    expect(component.selectedProductIds()).toEqual([]);
  });

  it('handles checkbox and card clicks once and applies the endpoint state to Shift ranges', async () => {
    const { component, fixture } = await setup(undefined, [1, 2, 3].map(id => product(id, 'Product ' + id)));
    component.form.controls.gameId.setValue(1);
    fixture.detectChanges();
    const first: HTMLInputElement = fixture.nativeElement.querySelector('#game-url-product-1');
    first.click();
    fixture.detectChanges();
    expect(first.checked).toBeTrue();
    const endpoint: HTMLInputElement = fixture.nativeElement.querySelector('#game-url-product-3');
    endpoint.parentElement!.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true, shiftKey: true }));
    fixture.detectChanges();
    expect(component.selectedProductIds()).toEqual([1, 2, 3]);
    endpoint.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true, shiftKey: true }));
    fixture.detectChanges();
    expect(component.selectedProductIds()).toEqual([]);
    expect(endpoint.checked).toBeFalse();
  });

  it('loads groups for the selected game and clears them when no game is selected', async () => {
    const { component, itemGroupService } = await setup();
    expect(component.newItemGroupName.disabled).toBeTrue();
    component.form.controls.gameId.setValue(1);
    component.form.controls.itemGroupId.setValue(10);

    expect(itemGroupService.getByGame).toHaveBeenCalledWith(1);
    expect(component.itemGroups().map(group => group.name)).toEqual(['Category']);
    expect(component.newItemGroupName.enabled).toBeTrue();

    component.form.controls.gameId.setValue(null);
    expect(component.form.controls.itemGroupId.value).toBeNull();
    expect(component.itemGroups()).toEqual([]);
    expect(component.newItemGroupName.disabled).toBeTrue();
  });

  it('ignores a stale group lookup after switching games', async () => {
    const { component, itemGroupService } = await setup();
    const oldGroups = new Subject<ItemGroup[]>();
    itemGroupService.getByGame.and.returnValues(oldGroups, of([{ id: 12, gameId: 2, name: 'Other game' }]));
    component.form.controls.gameId.setValue(1);
    component.form.controls.gameId.setValue(2);
    oldGroups.next([{ id: 10, gameId: 1, name: 'Category' }]);

    expect(component.itemGroups().map(group => group.id)).toEqual([12]);
    expect(component.form.controls.itemGroupId.value).toBeNull();
  });

  it('creates a trimmed group inline and selects it in the form', async () => {
    const { component, fixture, itemGroupService } = await setup();
    component.form.controls.gameId.setValue(1);
    component.newItemGroupName.setValue(' Priority ');
    component.createItemGroup();
    fixture.detectChanges();

    expect(itemGroupService.create).toHaveBeenCalledWith({ gameId: 1, name: 'Priority' });
    expect(component.form.controls.itemGroupId.value).toBe(11);
    expect(component.itemGroups().map(group => group.name)).toEqual(['Category', 'Priority']);
    expect(component.newItemGroupName.value).toBe('');
    expect(component.isCreatingItemGroup).toBeFalse();
    const select: HTMLSelectElement = fixture.nativeElement.querySelector('#game-url-item-group-id');
    expect(Array.from(select.options).map(option => option.textContent?.trim())).toEqual(['No group', 'Category', 'Priority']);
    expect(select.selectedOptions[0].textContent?.trim()).toBe('Priority');
  });

  it('rejects blank and over-length group names without sending a request', async () => {
    const { component, itemGroupService } = await setup();
    component.form.controls.gameId.setValue(1);
    for (const name of ['   ', 'x'.repeat(256)]) {
      component.newItemGroupName.setValue(name);
      component.createItemGroup();
    }

    expect(itemGroupService.create).not.toHaveBeenCalled();
  });

  it('waits for the group lookup before allowing inline creation', async () => {
    const { component, itemGroupService } = await setup();
    const lookup = new Subject<ItemGroup[]>();
    itemGroupService.getByGame.and.returnValue(lookup);
    component.form.controls.gameId.setValue(1);
    component.newItemGroupName.setValue('Priority');
    component.createItemGroup();
    expect(component.isLoadingItemGroups).toBeTrue();
    expect(itemGroupService.create).not.toHaveBeenCalled();

    lookup.next([]);
    lookup.complete();
    component.createItemGroup();
    expect(component.form.controls.itemGroupId.value).toBe(11);
  });

  it('recovers from a failed lookup and a failed group creation', async () => {
    const { component, itemGroupService } = await setup();
    itemGroupService.getByGame.and.returnValues(throwError(() => new Error('Lookup failed')), of([]));
    component.form.controls.gameId.setValue(1);
    expect(component.isLoadingItemGroups).toBeFalse();
    expect(component.itemGroups()).toEqual([]);
    component.form.controls.gameId.setValue(2);
    expect(itemGroupService.getByGame).toHaveBeenCalledWith(2);

    itemGroupService.create.and.returnValue(throwError(() => new Error('Creation failed')));
    component.newItemGroupName.setValue('Priority');
    component.createItemGroup();
    expect(component.isCreatingItemGroup).toBeFalse();
    expect(component.newItemGroupName.enabled).toBeTrue();
    expect(component.form.controls.itemGroupId.value).toBeNull();
  });

  it('loads the assigned group in edit mode and sends null to clear it', async () => {
    const { component, itemGroupService, gameUrlService, router } = await setup('7');
    expect(component.isEditMode).toBeTrue();
    expect(component.form.controls.gameId.disabled).toBeTrue();
    expect(itemGroupService.getByGame).toHaveBeenCalledWith(1);
    expect(component.form.controls.itemGroupId.value).toBe(10);

    component.form.controls.itemGroupId.setValue(null);
    component.onSubmit();
    expect(gameUrlService.update).toHaveBeenCalledWith(7, jasmine.objectContaining({ itemGroupId: null }));
    expect(gameUrlService.create).not.toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/game-urls']);
  });

  for (const itemGroupId of [null, 10]) {
    it(`submits the optional group ${itemGroupId} in the create payload`, async () => {
      const { component, gameUrlService, router } = await setup();
      component.form.patchValue({ gameId: 1, name: 'Market', itemGroupId });
      component.onSubmit();

      expect(gameUrlService.create).toHaveBeenCalledWith(jasmine.objectContaining({ gameId: 1, itemGroupId }));
      expect(router.navigate).toHaveBeenCalledWith(['/game-urls']);
    });
  }

  it('blocks submission while creating and does not assign a late result to another game', async () => {
    const { component, itemGroupService, gameUrlService } = await setup();
    const created = new Subject<ItemGroup>();
    itemGroupService.create.and.returnValue(created);
    component.form.controls.gameId.setValue(1);
    component.newItemGroupName.setValue('Priority');
    component.createItemGroup();
    component.onSubmit();
    expect(gameUrlService.create).not.toHaveBeenCalled();

    component.form.controls.gameId.setValue(2);
    created.next({ id: 11, gameId: 1, name: 'Priority' });
    created.complete();
    expect(component.form.controls.itemGroupId.value).toBeNull();
    expect(component.isCreatingItemGroup).toBeFalse();
    expect(component.newItemGroupName.enabled).toBeTrue();
  });
});
