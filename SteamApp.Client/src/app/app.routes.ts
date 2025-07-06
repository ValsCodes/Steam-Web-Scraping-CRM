import { Routes } from '@angular/router';

import { WebScraperComponent } from "./components/web-scraper/web-scraper.component";
import { ManualModeComponent } from "./components/manual-mode/manual-mode.component";
import { ProductsBetaComponent } from "./components/products/_products-beta/products-beta.component";
import { ProductsCatalogComponent } from './components/products/products-catalog/products-catalog.component';
import { CreateProductFormComponent } from './components/products/create-product-form/create-product-form.component';
import { EditProductFormComponent } from './components/products/edit-product-form/edit-product-form.component';
import { ItemsCatalogComponent } from './components/items/items-catalog/items-catalog.component';
import { CreateItemFormComponent } from './components/items/create-item-form/create-item-form.component';
import { EditItemFormComponent } from './components/items/edit-item-form/edit-item-form.component';

export const routes: Routes = [
    { path: 'web-scraper', component: WebScraperComponent, title: "Web Scraper" },
    { path: 'manual-mode', component: ManualModeComponent, title: "Manual Mode" },
    { path: 'products-catalog', component: ProductsCatalogComponent, title: "Products Catalog" },
    { path: 'create-product', component: CreateProductFormComponent, title: "Create Product" },
    { path: 'edit-product/:id', component: EditProductFormComponent, title: "Edit Product" },
    { path: 'products-beta', component: ProductsBetaComponent, title: "Products Beta" },
    { path: 'items-catalog', component: ItemsCatalogComponent, title: "Items Catalog" },
    { path: 'create-item', component: CreateItemFormComponent, title: "Create Item" },
    { path: 'edit-item/:id', component: EditItemFormComponent, title: "Edit Item" },
    { path: '', redirectTo: 'web-scraper', pathMatch: 'full' },
];
