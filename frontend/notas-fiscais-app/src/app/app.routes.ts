import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'products', pathMatch: 'full' },
  {
    path: 'products',
    loadComponent: () =>
      import('./features/products/products.component').then(m => m.ProductsComponent)
  },
  {
    path: 'invoices',
    loadComponent: () =>
      import('./features/invoices/invoice-list.component').then(m => m.InvoiceListComponent)
  },
  {
    path: 'invoices/:id',
    loadComponent: () =>
      import('./features/invoices/invoice-detail.component').then(m => m.InvoiceDetailComponent)
  },
  {
    path: 'invoices/:id/print',
    loadComponent: () =>
      import('./features/invoices/invoice-print.component').then(m => m.InvoicePrintComponent)
  },
  {
    path: 'impressoes',
    loadComponent: () =>
      import('./features/print-log/print-log.component').then(m => m.PrintLogComponent)
  },
  { path: '**', redirectTo: 'products' }
];
