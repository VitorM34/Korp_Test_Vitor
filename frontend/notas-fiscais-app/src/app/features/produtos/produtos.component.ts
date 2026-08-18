import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { Subject, switchMap, EMPTY } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
import { ProdutoService } from '../../core/services/produto.service';
import { Produto } from '../../core/models/produto.model';

@Component({
  selector: 'app-produtos',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatIconModule
  ],
  templateUrl: './produtos.component.html',
  styleUrls: ['./produtos.component.scss']
})
export class ProdutosComponent implements OnInit, OnDestroy {
  produtos: Produto[] = [];
  displayedColumns = ['codigo', 'descricao', 'saldo'];
  form!: FormGroup;
  salvando = false;
  carregando = false;

  private destroy$ = new Subject<void>();

  constructor(
    private produtoService: ProdutoService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      codigo: ['', [Validators.required, Validators.maxLength(50)]],
      descricao: ['', [Validators.required, Validators.maxLength(200)]],
      saldo: [0, [Validators.required, Validators.min(0)]]
    });

    this.carregarProdutos();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  carregarProdutos(): void {
    this.carregando = true;
    this.produtoService.listar()
      .pipe(
        takeUntil(this.destroy$),
        catchError(err => {
          this.snackBar.open('Erro ao carregar produtos.', 'Fechar', { duration: 4000 });
          this.carregando = false;
          return EMPTY;
        })
      )
      .subscribe(produtos => {
        this.produtos = produtos;
        this.carregando = false;
      });
  }

  salvar(): void {
    if (this.form.invalid) return;

    this.salvando = true;
    this.produtoService.criar(this.form.value)
      .pipe(
        takeUntil(this.destroy$),
        catchError(err => {
          const msg = err?.error?.detail ?? 'Erro ao salvar produto.';
          this.snackBar.open(msg, 'Fechar', { duration: 5000 });
          this.salvando = false;
          return EMPTY;
        })
      )
      .subscribe(novo => {
        this.produtos = [...this.produtos, novo];
        this.form.reset({ saldo: 0 });
        this.salvando = false;
        this.snackBar.open('Produto cadastrado com sucesso!', 'OK', { duration: 3000 });
      });
  }
}
