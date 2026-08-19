import { Injectable, signal } from '@angular/core';
import { Breadcrumb } from '../models/breadcrumb.model';

@Injectable({ providedIn: 'root' })
export class PageContextService {
  private readonly crumbsState = signal<Breadcrumb[]>([]);

  readonly crumbs = this.crumbsState.asReadonly();

  set(crumbs: Breadcrumb[]): void {
    this.crumbsState.set(crumbs);
  }

  clear(): void {
    this.crumbsState.set([]);
  }
}
