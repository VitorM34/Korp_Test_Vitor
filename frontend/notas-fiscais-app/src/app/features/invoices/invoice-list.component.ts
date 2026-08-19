import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { EMPTY, catchError, filter, switchMap, tap } from 'rxjs';
import { InvoiceService } from '../../core/services/invoice.service';
import { Invoice } from '../../core/models/invoice.model';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';
import { InvoiceStatusPipe } from '../../shared/pipes/invoice-status.pipe';

export type InvoiceFilter = 'all' | 'Open' | 'Closed';

@Component({
  selector: 'app-invoice-list',
  standalone: true,
  imports: [CommonModule, RouterModule, MatButtonModule, MatSnackBarModule, MatProgressSpinnerModule, MatIconModule, MatDialogModule, InvoiceStatusPipe],
  templateUrl: './invoice-list.component.html',
  styleUrls: ['./invoice-list.component.scss']
})
export class InvoiceListComponent implements OnInit {
  private readonly invoiceService = inject(InvoiceService);
  private readonly router         = inject(Router);
  private readonly snackBar       = inject(MatSnackBar);
  private readonly dialog         = inject(MatDialog);

  readonly invoices  = signal<Invoice[]>([]);
  readonly loading   = signal(false);
  readonly creating  = signal(false);
  readonly deleting  = signal<string | null>(null);
  readonly filterBy  = signal<InvoiceFilter>('all');
  readonly search    = signal('');

  readonly openInvoicesCount   = computed(() => this.invoices().filter(n => n.status === 'Open').length);
  readonly closedInvoicesCount = computed(() => this.invoices().filter(n => n.status === 'Closed').length);

  readonly filteredInvoices = computed(() => {
    const filterBy = this.filterBy();
    const term = this.search().trim().toLowerCase();

    return this.invoices().filter(invoice => {
      if (filterBy !== 'all' && invoice.status !== filterBy) return false;
      if (!term) return true;

      const byNumber = String(invoice.number).includes(term);
      const byProduct = invoice.items.some(item =>
        item.productDescription.toLowerCase().includes(term)
      );
      return byNumber || byProduct;
    });
  });

  readonly emptyList = computed(() => this.invoices().length === 0);
  readonly noSearchResults = computed(() =>
    !this.emptyList() && this.filteredInvoices().length === 0
  );

  ngOnInit(): void {
    this.loadInvoices();
  }

  applyFilter(filterBy: InvoiceFilter): void {
    this.filterBy.set(filterBy);
  }

  onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  clearFilters(): void {
    this.filterBy.set('all');
    this.search.set('');
  }

  loadInvoices(): void {
    this.loading.set(true);
    this.invoiceService.list()
      .pipe(
        catchError(() => {
          this.snackBar.open('Erro ao carregar notas fiscais.', 'Fechar', { duration: 4000 });
          this.loading.set(false);
          return EMPTY;
        })
      )
      .subscribe(list => {
        this.invoices.set(list);
        this.loading.set(false);
      });
  }

  createInvoice(): void {
    this.creating.set(true);
    this.invoiceService.create()
      .pipe(catchError(() => { this.creating.set(false); return EMPTY; }))
      .subscribe(invoice => {
        this.creating.set(false);
        this.router.navigate(['/invoices', invoice.id]);
      });
  }

  deleteInvoice(invoice: Invoice, event: Event): void {
    event.stopPropagation();

    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Excluir Nota Fiscal',
        message: `Excluir a Nota Nº ${invoice.number} permanentemente?`,
        confirmLabel: 'Excluir',
        confirmColor: 'warn',
        icon: 'delete_forever',
        iconColor: '#b71c1c'
      },
      width: '380px'
    });

    ref.afterClosed()
      .pipe(
        filter(ok => !!ok),
        tap(() => this.deleting.set(invoice.id)),
        switchMap(() => this.invoiceService.delete(invoice.id).pipe(
          catchError(() => { this.deleting.set(null); return EMPTY; })
        ))
      )
      .subscribe(() => {
        this.invoices.update(list => list.filter(n => n.id !== invoice.id));
        this.deleting.set(null);
        this.snackBar.open(`Nota Nº ${invoice.number} excluída.`, 'OK', { duration: 3000 });
      });
  }

  viewInvoice(id: string, event: Event): void {
    event.stopPropagation();
    window.open(`/invoices/${id}/print`, '_blank');
  }

  openDetail(id: string): void {
    this.router.navigate(['/invoices', id]);
  }
}
