import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { EMPTY, catchError } from 'rxjs';
import { InvoiceService } from '../../core/services/invoice.service';
import { PrintLogEntry } from '../../core/models/invoice.model';

@Component({
  selector: 'app-print-log',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatSnackBarModule, MatProgressSpinnerModule, MatIconModule],
  templateUrl: './print-log.component.html',
  styleUrls: ['./print-log.component.scss']
})
export class PrintLogComponent implements OnInit {
  private readonly invoiceService = inject(InvoiceService);
  private readonly router         = inject(Router);
  private readonly snackBar       = inject(MatSnackBar);

  readonly entries = signal<PrintLogEntry[]>([]);
  readonly loading = signal(false);
  readonly search  = signal('');

  readonly totalCount = computed(() => this.entries().length);
  readonly todayCount = computed(() => {
    const today = new Date().toDateString();
    return this.entries().filter(e => new Date(e.printedDate).toDateString() === today).length;
  });
  readonly distinctInvoicesCount = computed(() =>
    new Set(this.entries().map(e => e.invoiceId)).size
  );

  readonly filteredEntries = computed(() => {
    const term = this.search().trim().toLowerCase();
    if (!term) return this.entries();
    return this.entries().filter(e => String(e.invoiceNumber).includes(term));
  });

  readonly emptyList = computed(() => this.entries().length === 0);
  readonly noSearchResults = computed(() =>
    !this.emptyList() && this.filteredEntries().length === 0
  );

  ngOnInit(): void {
    this.loadEntries();
  }

  onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  clearFilters(): void {
    this.search.set('');
  }

  loadEntries(): void {
    this.loading.set(true);
    this.invoiceService.getPrintLog()
      .pipe(
        catchError(() => {
          this.snackBar.open('Erro ao carregar histórico de impressões.', 'Fechar', { duration: 4000 });
          this.loading.set(false);
          return EMPTY;
        })
      )
      .subscribe(list => {
        this.entries.set(list);
        this.loading.set(false);
      });
  }

  openInvoice(invoiceId: string): void {
    this.router.navigate(['/invoices', invoiceId]);
  }

  viewInvoice(invoiceId: string, event: Event): void {
    event.stopPropagation();
    window.open(`/invoices/${invoiceId}/print`, '_blank');
  }
}
