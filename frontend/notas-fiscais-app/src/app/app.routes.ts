import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'produtos', pathMatch: 'full' },
  {
    path: 'produtos',
    loadComponent: () =>
      import('./features/produtos/produtos.component').then(m => m.ProdutosComponent)
  },
  {
    path: 'notas-fiscais',
    loadComponent: () =>
      import('./features/notas-fiscais/nota-list.component').then(m => m.NotaListComponent)
  },
  {
    path: 'notas-fiscais/:id',
    loadComponent: () =>
      import('./features/notas-fiscais/nota-detail.component').then(m => m.NotaDetailComponent)
  },
  { path: '**', redirectTo: 'produtos' }
];
