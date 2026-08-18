import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { Subject, EMPTY } from 'rxjs';
import { takeUntil, catchError, tap, switchMap } from 'rxjs/operators';
import { NotaFiscalService } from '../../core/services/nota-fiscal.service';
import { ProdutoService } from '../../core/services/produto.service';
import { NotaFiscal } from '../../core/models/nota-fiscal.model';
import { Produto } from '../../core/models/produto.model';

@Component({
  selector: 'app-nota-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatIconModule,
    MatDividerModule
  ],
  templateUrl: './nota-detail.component.html',
  styleUrls: ['./nota-detail.component.scss']
})
export class NotaDetailComponent implements OnInit, OnDestroy {
  nota: NotaFiscal | null = null;
  produtos: Produto[] = [];
  displayedColumns = ['descricaoProduto', 'quantidade'];
  itemForm!: FormGroup;
  adicionando = false;
  imprimindo = false;
  carregando = true;
  erroImpressao = '';

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private notaService: NotaFiscalService,
    private produtoService: ProdutoService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.itemForm = this.fb.group({
      produtoId: ['', Validators.required],
      quantidade: [1, [Validators.required, Validators.min(1)]]
    });

    const id = this.route.snapshot.paramMap.get('id')!;
    this.carregarDados(id);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  carregarDados(id: string): void {
    this.carregando = true;
    this.notaService.obterPorId(id)
      .pipe(
        takeUntil(this.destroy$),
        tap(nota => {
          this.nota = nota;
          this.carregando = false;
        }),
        switchMap(() => this.produtoService.listar()),
        catchError(() => {
          this.snackBar.open('Erro ao carregar dados.', 'Fechar', { duration: 4000 });
          this.carregando = false;
          return EMPTY;
        })
      )
      .subscribe(produtos => {
        this.produtos = produtos;
      });
  }

  get produtoSelecionado(): Produto | undefined {
    const id = this.itemForm.get('produtoId')?.value;
    return this.produtos.find(p => p.id === id);
  }

  adicionarItem(): void {
    if (this.itemForm.invalid || !this.nota) return;

    const produto = this.produtoSelecionado;
    if (!produto) return;

    this.adicionando = true;
    const request = {
      produtoId: produto.id,
      descricaoProduto: produto.descricao,
      quantidade: this.itemForm.get('quantidade')!.value
    };

    this.notaService.adicionarItem(this.nota.id, request)
      .pipe(
        takeUntil(this.destroy$),
        catchError(err => {
          const msg = err?.error?.detail ?? 'Erro ao adicionar item.';
          this.snackBar.open(msg, 'Fechar', { duration: 5000 });
          this.adicionando = false;
          return EMPTY;
        })
      )
      .subscribe(notaAtualizada => {
        this.nota = notaAtualizada;
        this.itemForm.patchValue({ quantidade: 1 });
        this.adicionando = false;
        this.snackBar.open('Item adicionado!', 'OK', { duration: 2000 });
      });
  }

  imprimir(): void {
    if (!this.nota) return;
    this.imprimindo = true;
    this.erroImpressao = '';

    this.notaService.imprimir(this.nota.id)
      .pipe(
        tap(() => { this.imprimindo = false; }),
        catchError(err => {
          this.imprimindo = false;
          this.erroImpressao = err?.error?.detail ?? 'Erro ao imprimir nota. Verifique o serviço de estoque.';
          this.snackBar.open(this.erroImpressao, 'Fechar', { duration: 8000, panelClass: 'snack-erro' });
          return EMPTY;
        })
      )
      .subscribe(notaAtualizada => {
        this.nota = notaAtualizada;
        this.snackBar.open(`Nota Nº ${notaAtualizada.numeracao} impressa com sucesso!`, 'OK', { duration: 4000 });
      });
  }

  voltar(): void {
    this.router.navigate(['/notas-fiscais']);
  }
}
