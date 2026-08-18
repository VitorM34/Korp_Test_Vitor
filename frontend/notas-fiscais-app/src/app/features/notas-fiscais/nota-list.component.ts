import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { Subject, EMPTY } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
import { NotaFiscalService } from '../../core/services/nota-fiscal.service';
import { NotaFiscal } from '../../core/models/nota-fiscal.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nota-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTableModule,
    MatButtonModule,
    MatChipsModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatIconModule
  ],
  templateUrl: './nota-list.component.html',
  styleUrls: ['./nota-list.component.scss']
})
export class NotaListComponent implements OnInit, OnDestroy {
  notas: NotaFiscal[] = [];
  displayedColumns = ['numeracao', 'status', 'dataCriacao', 'dataImpressao', 'acoes'];
  carregando = false;
  criando = false;

  private destroy$ = new Subject<void>();

  constructor(
    private notaService: NotaFiscalService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.carregarNotas();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  carregarNotas(): void {
    this.carregando = true;
    this.notaService.listar()
      .pipe(
        takeUntil(this.destroy$),
        catchError(() => {
          this.snackBar.open('Erro ao carregar notas fiscais.', 'Fechar', { duration: 4000 });
          this.carregando = false;
          return EMPTY;
        })
      )
      .subscribe(notas => {
        this.notas = notas;
        this.carregando = false;
      });
  }

  criarNota(): void {
    this.criando = true;
    this.notaService.criar()
      .pipe(
        takeUntil(this.destroy$),
        catchError(err => {
          this.snackBar.open('Erro ao criar nota fiscal.', 'Fechar', { duration: 4000 });
          this.criando = false;
          return EMPTY;
        })
      )
      .subscribe(nota => {
        this.criando = false;
        this.router.navigate(['/notas-fiscais', nota.id]);
      });
  }

  abrirDetalhe(id: string): void {
    this.router.navigate(['/notas-fiscais', id]);
  }
}
