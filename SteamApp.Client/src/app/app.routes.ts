import { Routes } from '@angular/router';

import { WebScraperComponent } from "./pages/web-scraper/web-scraper.component";
import { ManualModeComponent } from "./pages/manual-mode/manual-mode.component";
import { ProductsBetaComponent } from "./pages/products/_products-beta/products-beta.component";
import { ProductsCatalogComponent } from './pages/products/products-catalog/products-catalog.component';
import { CreateProductFormComponent } from './pages/products/create-product-form/create-product-form.component';
import { EditProductFormComponent } from './pages/products/edit-product-form/edit-product-form.component';
import { ItemsCatalogComponent } from './pages/items/items-catalog/items-catalog.component';
import { CreateItemFormComponent } from './pages/items/create-item-form/create-item-form.component';
import { EditItemFormComponent } from './pages/items/edit-item-form/edit-item-form.component';
import { LoginComponent } from './pages/login/login.component';

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
    { path: 'login', component: LoginComponent, title: "Login" },
    { path: '', redirectTo: 'web-scraper', pathMatch: 'full' },
];
