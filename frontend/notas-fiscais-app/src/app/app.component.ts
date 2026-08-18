import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, MatIconModule],
  template: `
    <mat-toolbar color="primary" class="app-toolbar">
      <mat-icon class="toolbar-icon">description</mat-icon>
      <span class="toolbar-title">Sistema de Notas Fiscais</span>
      <span class="spacer"></span>
      <a mat-button routerLink="/produtos" routerLinkActive="active-link">
        <mat-icon>inventory_2</mat-icon> Produtos
      </a>
      <a mat-button routerLink="/notas-fiscais" routerLinkActive="active-link">
        <mat-icon>receipt_long</mat-icon> Notas Fiscais
      </a>
    </mat-toolbar>

    <main class="main-content">
      <router-outlet />
    </main>
  `,
  styles: [`
    .app-toolbar {
      position: sticky;
      top: 0;
      z-index: 100;
    }
    .toolbar-icon { margin-right: 8px; }
    .toolbar-title { font-size: 1.1rem; font-weight: 500; }
    .spacer { flex: 1 1 auto; }
    .main-content { min-height: calc(100vh - 64px); background: #f5f7fa; }
    .active-link { background: rgba(255,255,255,0.15) !important; border-radius: 4px; }
    a mat-icon { margin-right: 4px; vertical-align: middle; }
  `]
})
export class AppComponent {}
