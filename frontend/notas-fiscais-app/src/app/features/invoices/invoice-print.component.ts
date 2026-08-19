import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, DecimalPipe, UpperCasePipe, DatePipe, SlicePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { InvoiceService } from '../../core/services/invoice.service';
import { Invoice } from '../../core/models/invoice.model';

@Component({
  selector: 'app-invoice-print',
  standalone: true,
  imports: [CommonModule, DecimalPipe, UpperCasePipe, DatePipe, SlicePipe],
  templateUrl: './invoice-print.component.html',
  styleUrls: ['./invoice-print.component.scss']
})
export class InvoicePrintComponent implements OnInit {
  private readonly route          = inject(ActivatedRoute);
  private readonly router         = inject(Router);
  private readonly invoiceService = inject(InvoiceService);

  invoice: Invoice | null = null;
  loading = true;
  error = '';

  readonly accessKey = '3526 0261 5858 6519 9560 5500 7000 0540 4312 0250 2038';
  readonly protocol   = '126250011924458';
  readonly currentDate = new Date();

  // Simula barras do código de barras
  readonly barcodeChunks: number[] = Array.from({ length: 60 }, (_, i) =>
    [1, 1, 2, 1, 3, 1, 1, 2, 3, 1, 2, 1][i % 12]
  );

  get totalItems(): number {
    return this.invoice?.items.reduce((acc, i) => acc + i.quantity, 0) ?? 0;
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.invoiceService.getById(id).subscribe({
      next: (invoice) => { this.invoice = invoice; this.loading = false; },
      error: () => { this.error = 'Não foi possível carregar a nota fiscal.'; this.loading = false; }
    });
  }

  print(): void {
    window.print();

    // Só registra reimpressão se a nota já estiver emitida (evita logar a "Prévia" de uma nota ainda aberta,
    // que usa esta mesma tela). A emissão em si já é registrada automaticamente pelo backend.
    if (this.invoice?.status === 'Closed') {
      this.invoiceService.logReprint(this.invoice.id).subscribe({ error: () => {} });
    }
  }
  back(): void  { this.router.navigate(['/invoices', this.route.snapshot.paramMap.get('id')]); }
}
