import { Component, OnInit, signal, computed, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { EMPTY, catchError, tap, switchMap, filter } from 'rxjs';
import { InvoiceService } from '../../core/services/invoice.service';
import { ProductService } from '../../core/services/product.service';
import { Invoice, InvoiceItem } from '../../core/models/invoice.model';
import { Product } from '../../core/models/product.model';
import { ConfirmDialogComponent } from '../../shared/confirm-dialog/confirm-dialog.component';
import { PageContextService } from '../../core/services/page-context.service';
import { InvoiceStatusPipe } from '../../shared/pipes/invoice-status.pipe';

@Component({
  selector: 'app-invoice-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatDividerModule,
    MatDialogModule,
    InvoiceStatusPipe
  ],
  templateUrl: './invoice-detail.component.html',
  styleUrls: ['./invoice-detail.component.scss']
})
export class InvoiceDetailComponent implements OnInit {
  private readonly route          = inject(ActivatedRoute);
  private readonly router         = inject(Router);
  private readonly invoiceService = inject(InvoiceService);
  private readonly productService = inject(ProductService);
  private readonly fb             = inject(FormBuilder);
  private readonly snackBar       = inject(MatSnackBar);
  private readonly dialog         = inject(MatDialog);
  private readonly pageContext    = inject(PageContextService);
  private readonly destroyRef     = inject(DestroyRef);

  readonly invoice        = signal<Invoice | null>(null);
  readonly products       = signal<Product[]>([]);
  readonly loading        = signal(true);
  readonly adding         = signal(false);
  readonly printing       = signal(false);
  readonly removing       = signal<string | null>(null);  // itemId sendo removido
  readonly deleting       = signal(false);
  readonly editingItemId  = signal<string | null>(null);
  readonly printError     = signal('');

  // Controla a revelação progressiva do "Resumo da emissão" (Criada → Emitida → Estoque debitado).
  // Começa em 3 (tudo visível) porque ao abrir uma nota já fechada não há nada para animar;
  // só é zerado e escalonado de novo quando a emissão acontece nesta sessão (ver print()).
  readonly emissionStep = signal(3);

  readonly totalUnits = computed(() =>
    this.invoice()?.items.reduce((acc, i) => acc + i.quantity, 0) ?? 0
  );

  readonly itemForm = this.fb.group({
    productId: ['', Validators.required],
    quantity: [1, [Validators.required, Validators.min(1)]]
  });

  get selectedProduct(): Product | undefined {
    const id = this.itemForm.get('productId')?.value;
    return this.products().find(p => p.id === id);
  }

  ngOnInit(): void {
    this.destroyRef.onDestroy(() => this.pageContext.clear());
    const id = this.route.snapshot.paramMap.get('id')!;
    this.loadData(id);
  }

  loadData(id: string): void {
    this.loading.set(true);
    this.invoiceService.getById(id)
      .pipe(
        tap(invoice => {
          this.invoice.set(invoice);
          this.loading.set(false);
          this.pageContext.set([
            { label: 'Notas Fiscais', path: '/invoices' },
            { label: `#${invoice.number}` }
          ]);
        }),
        switchMap(() => this.productService.list()),
        catchError(() => {
          this.snackBar.open('Erro ao carregar dados.', 'Fechar', { duration: 4000 });
          this.loading.set(false);
          return EMPTY;
        })
      )
      .subscribe(list => this.products.set(list));
  }

  addItem(): void {
    if (this.itemForm.invalid || !this.invoice()) return;
    const product = this.selectedProduct;
    if (!product) return;

    this.adding.set(true);
    const request = {
      productId: product.id,
      productDescription: product.description,
      quantity: this.itemForm.get('quantity')!.value ?? 1
    };

    this.invoiceService.addItem(this.invoice()!.id, request)
      .pipe(catchError(() => { this.adding.set(false); return EMPTY; }))
      .subscribe(updatedInvoice => {
        this.invoice.set(updatedInvoice);
        this.itemForm.patchValue({ quantity: 1 });
        this.adding.set(false);
        this.snackBar.open('Item adicionado!', 'OK', { duration: 2000 });
      });
  }

  removeItem(item: InvoiceItem): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Remover Item',
        message: `Remover "${item.productDescription}" da nota?`,
        confirmLabel: 'Remover',
        confirmColor: 'warn',
        icon: 'delete',
        iconColor: '#c62828'
      },
      width: '380px'
    });

    ref.afterClosed()
      .pipe(
        filter(ok => !!ok),
        tap(() => this.removing.set(item.id)),
        switchMap(() => this.invoiceService.removeItem(this.invoice()!.id, item.id).pipe(
          catchError(() => { this.removing.set(null); return EMPTY; })
        ))
      )
      .subscribe(updatedInvoice => {
        this.invoice.set(updatedInvoice);
        this.removing.set(null);
        this.snackBar.open('Item removido.', 'OK', { duration: 2000 });
      });
  }

  startEditing(item: InvoiceItem): void {
    this.editingItemId.set(item.id);
  }

  saveEditingById(item: InvoiceItem): void {
    const input = document.getElementById(`qty-input-${item.id}`) as HTMLInputElement | null;
    const newQty = input ? +input.value : item.quantity;
    this.saveEditing(item, newQty);
  }

  saveEditing(item: InvoiceItem, newQty: number): void {
    if (newQty < 1) return;
    this.invoiceService.updateItem(this.invoice()!.id, item.id, newQty)
      .pipe(catchError(() => { this.editingItemId.set(null); return EMPTY; }))
      .subscribe(updatedInvoice => {
        this.invoice.set(updatedInvoice);
        this.editingItemId.set(null);
        this.snackBar.open('Quantidade atualizada.', 'OK', { duration: 2000 });
      });
  }

  deleteInvoice(): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Excluir Nota Fiscal',
        message: `Excluir a Nota Nº ${this.invoice()!.number} permanentemente? Esta ação não pode ser desfeita.`,
        confirmLabel: 'Excluir',
        confirmColor: 'warn',
        icon: 'delete_forever',
        iconColor: '#b71c1c'
      },
      width: '420px',
      disableClose: true
    });

    ref.afterClosed()
      .pipe(
        filter(ok => !!ok),
        tap(() => this.deleting.set(true)),
        switchMap(() => this.invoiceService.delete(this.invoice()!.id).pipe(
          catchError(() => { this.deleting.set(false); return EMPTY; })
        ))
      )
      .subscribe(() => {
        this.snackBar.open('Nota excluída.', 'OK', { duration: 3000 });
        this.router.navigate(['/invoices']);
      });
  }

  print(): void {
    if (!this.invoice()) return;

    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Emitir Nota Fiscal',
        message: `Confirma a emissão da Nota Nº ${this.invoice()!.number}? O estoque será debitado e a nota será fechada permanentemente.`,
        confirmLabel: 'Emitir',
        confirmColor: 'warn',
        icon: 'print',
        iconColor: '#e65100'
      },
      width: '440px',
      disableClose: true
    });

    ref.afterClosed()
      .pipe(
        filter(confirmed => !!confirmed),
        tap(() => { this.printing.set(true); this.printError.set(''); }),
        switchMap(() => this.invoiceService.print(this.invoice()!.id).pipe(
          tap(() => this.printing.set(false)),
          catchError(err => {
            this.printing.set(false);
            const msg = err?.error?.detail ?? 'Erro ao imprimir nota. Verifique o serviço de estoque.';
            this.printError.set(msg);
            return EMPTY;
          })
        ))
      )
      .subscribe(updatedInvoice => {
        // Zera antes de trocar a nota: o "Resumo da emissão" só aparece quando status === 'Closed',
        // então é preciso já estar em 0 no mesmo tick em que o card é montado.
        this.emissionStep.set(0);
        this.invoice.set(updatedInvoice);
        this.snackBar.open(`Nota Nº ${updatedInvoice.number} emitida com sucesso!`, 'OK', { duration: 4000 });
        this.animateEmissionSummary();
      });
  }

  // Revela cada etapa do resumo (Criada → Emitida → Estoque debitado) em sequência,
  // em vez de tudo aparecer pronto de uma vez — dá a sensação de acompanhar o processamento em tempo real.
  // Não dispara mais impressão automática: o usuário decide quando imprimir (botão "Reimprimir"/"Visualizar NF").
  private animateEmissionSummary(): void {
    setTimeout(() => this.emissionStep.set(1), 350);
    setTimeout(() => this.emissionStep.set(2), 750);
    setTimeout(() => this.emissionStep.set(3), 1150);
  }

  viewInvoice(): void {
    window.open(`/invoices/${this.invoice()!.id}/print`, '_blank');
  }

  // Chamado pelo botão "Reimprimir" (nota já fechada): dispara a impressão e registra no histórico
  // de impressões. A emissão em si já foi registrada automaticamente pelo backend em PrintAsync.
  openPrintView(): void {
    window.print();

    if (this.invoice()) {
      this.invoiceService.logReprint(this.invoice()!.id).subscribe({ error: () => {} });
    }
  }

  back(): void {
    this.router.navigate(['/invoices']);
  }

  protocol(number: number): string {
    return '1262500' + number.toString().padStart(8, '0');
  }
}
