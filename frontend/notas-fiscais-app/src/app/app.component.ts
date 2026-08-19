import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatRippleModule } from '@angular/material/core';
import { CommonModule } from '@angular/common';
import { filter, map, startWith } from 'rxjs';
import { PageContextService } from './core/services/page-context.service';
import { Breadcrumb } from './core/models/breadcrumb.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatRippleModule, CommonModule],
  template: `
    <div class="shell" [class.shell--print]="isPrintView()">
      <!-- Sidebar -->
      <aside class="sidebar">
        <div class="brand">
          <div class="brand-icon">
            <span class="material-icons">receipt</span>
          </div>
          <div class="brand-text">
            <span class="brand-name">NotaFiscal</span>
            <span class="brand-sub">Sistema de Emissão</span>
          </div>
        </div>

        <nav class="nav">
          <a class="nav-item" routerLink="/products" routerLinkActive="nav-item--active" matRipple>
            <span class="material-icons">store</span>
            <span>Produtos</span>
          </a>
          <a class="nav-item" routerLink="/invoices" routerLinkActive="nav-item--active" matRipple>
            <span class="material-icons">description</span>
            <span>Notas Fiscais</span>
          </a>
          <a class="nav-item" routerLink="/impressoes" routerLinkActive="nav-item--active" matRipple>
            <span class="material-icons">print</span>
            <span>Impressões</span>
          </a>
        </nav>

        <div class="sidebar-footer">
          <span class="material-icons">verified_user</span>
          <span>Korp Sistemas</span>
        </div>
      </aside>

      <!-- Main area -->
      <div class="main">
        <header class="topbar">
          <nav class="breadcrumb" aria-label="breadcrumb">
            @for (crumb of crumbs(); track crumb.label; let last = $last) {
              @if (crumb.path && !last) {
                <a [routerLink]="crumb.path">{{ crumb.label }}</a>
                <span class="bc-sep">/</span>
              } @else {
                <span [class.bc-current]="last">{{ crumb.label }}</span>
                @if (!last) { <span class="bc-sep">/</span> }
              }
            }
          </nav>
          <div class="topbar-user">
            <div class="user-meta">
              <span class="user-name">Vitor</span>
              <span class="user-role">Operador</span>
            </div>
            <div class="avatar" title="Vitor">V</div>
          </div>
        </header>
        <main class="content">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; height: 100vh; }

    .shell {
      display: flex;
      height: 100vh;
      overflow: hidden;
    }

    /* ── Sidebar ── */
    .sidebar {
      width: 240px;
      min-width: 240px;
      background: linear-gradient(175deg, #1a237e 0%, #283593 60%, #1565c0 100%);
      display: flex;
      flex-direction: column;
      padding: 0;
      box-shadow: 4px 0 20px rgba(0,0,0,0.25);
      z-index: 10;
      transition: width 0.2s, min-width 0.2s;
    }

    .brand {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 28px 20px 24px;
      border-bottom: 1px solid rgba(255,255,255,0.1);
      overflow: hidden;
    }
    .brand-icon {
      width: 42px; height: 42px;
      min-width: 42px;
      background: rgba(255,255,255,0.15);
      border-radius: 12px;
      display: flex; align-items: center; justify-content: center;
      mat-icon { color: #fff; font-size: 22px; }
    }
    .brand-text { display: flex; flex-direction: column; overflow: hidden; }
    .brand-name { color: #fff; font-weight: 700; font-size: 1rem; letter-spacing: 0.3px; white-space: nowrap; }
    .brand-sub  { color: rgba(255,255,255,0.55); font-size: 0.72rem; margin-top: 1px; white-space: nowrap; }

    .nav {
      flex: 1;
      padding: 16px 12px;
      display: flex;
      flex-direction: column;
      gap: 4px;
      overflow: hidden;
    }
    .nav-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 11px 14px;
      border-radius: 10px;
      color: rgba(255,255,255,0.72);
      text-decoration: none;
      font-size: 0.9rem;
      font-weight: 500;
      transition: background 0.2s, color 0.2s;
      cursor: pointer;
      white-space: nowrap;
      overflow: hidden;
      mat-icon { font-size: 20px; min-width: 20px; }
      &:hover { background: rgba(255,255,255,0.1); color: #fff; }
    }
    .nav-item--active {
      background: rgba(255,255,255,0.18) !important;
      color: #fff !important;
      box-shadow: inset 3px 0 0 #64b5f6;
    }

    .sidebar-footer {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 18px 20px;
      border-top: 1px solid rgba(255,255,255,0.1);
      color: rgba(255,255,255,0.4);
      font-size: 0.78rem;
      overflow: hidden;
      white-space: nowrap;
      mat-icon { font-size: 16px; min-width: 16px; }
    }

    /* ── Sidebar responsiva ── */
    @media (max-width: 1100px) {
      .sidebar { width: 200px; min-width: 200px; }
      .brand { padding: 22px 16px 18px; }
      .nav { padding: 12px 8px; }
    }

    @media (max-width: 860px) {
      .sidebar { width: 64px; min-width: 64px; }
      .brand { padding: 18px 11px; gap: 0; }
      .brand-text { display: none; }
      .nav { padding: 12px 8px; }
      .nav-item { padding: 12px; gap: 0; justify-content: center; }
      .nav-item span { display: none; }
      .sidebar-footer span { display: none; }
      .sidebar-footer { padding: 16px; justify-content: center; }
      .user-meta { display: none; }
    }

    /* ── Main ── */
    .main {
      flex: 1;
      min-width: 0;
      display: flex;
      flex-direction: column;
      overflow: hidden;
      background: #f0f2f5;
    }

    .topbar {
      height: 60px;
      background: #fff;
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 16px;
      padding: 0 28px;
      border-bottom: 1px solid #e8eaf0;
      box-shadow: 0 1px 4px rgba(0,0,0,0.06);
      flex-shrink: 0;
      position: relative;
      z-index: 20;
    }

    .breadcrumb {
      display: flex;
      align-items: center;
      gap: 8px;
      min-width: 0;
      font-size: 0.88rem;
      flex-wrap: wrap;

      a {
        color: #5c6bc0;
        text-decoration: none;
        font-weight: 500;
        &:hover { text-decoration: underline; }
      }
    }
    .bc-sep { color: #cfd8dc; }
    .bc-current { color: #37474f; font-weight: 600; }

    .topbar-user {
      display: flex;
      align-items: center;
      gap: 10px;
      flex-shrink: 0;
    }
    .user-meta {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      line-height: 1.15;
    }
    .user-name { font-size: 0.85rem; font-weight: 700; color: #1a237e; }
    .user-role { font-size: 0.72rem; color: #90a4ae; }

    .avatar {
      width: 36px; height: 36px;
      border-radius: 50%;
      background: linear-gradient(135deg, #1565c0, #42a5f5);
      color: #fff;
      display: flex; align-items: center; justify-content: center;
      font-weight: 700; font-size: 0.9rem;
      cursor: pointer;
    }

    .content {
      flex: 1;
      overflow: auto;
      padding: 24px;
      min-width: 0;
      position: relative;
      z-index: 0;
      isolation: isolate;
    }

    /* Tela da DANFE: só a nota, sem menu/topbar por cima */
    .shell--print .sidebar,
    .shell--print .topbar {
      display: none;
    }
    .shell--print .content {
      padding: 0;
      overflow: hidden;
      display: flex;
      flex-direction: column;
    }
    .shell--print .content > *:not(router-outlet) {
      flex: 1;
      min-height: 0;
      height: 100%;
    }
  `]
})
export class AppComponent {
  private readonly router = inject(Router);
  private readonly pageContext = inject(PageContextService);

  private readonly url = toSignal(
    this.router.events.pipe(
      filter((e): e is NavigationEnd => e instanceof NavigationEnd),
      map(e => e.urlAfterRedirects),
      startWith(this.router.url)
    ),
    { initialValue: this.router.url }
  );

  readonly crumbs = computed<Breadcrumb[]>(() => {
    const custom = this.pageContext.crumbs();
    if (custom.length > 0) return custom;
    return this.crumbsFromUrl(this.url());
  });

  isPrintView(): boolean {
    return this.url().includes('/print');
  }

  private crumbsFromUrl(url: string): Breadcrumb[] {
    const path = url.split('?')[0];
    if (path.startsWith('/products')) {
      return [{ label: 'Produtos' }];
    }
    if (path === '/invoices') {
      return [{ label: 'Notas Fiscais' }];
    }
    if (path.startsWith('/invoices/')) {
      return [
        { label: 'Notas Fiscais', path: '/invoices' },
        { label: 'Detalhe' }
      ];
    }
    if (path === '/impressoes') {
      return [{ label: 'Impressões' }];
    }
    return [{ label: 'Início' }];
  }
}
