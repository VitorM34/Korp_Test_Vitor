import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const message = error.error?.detail
        ?? error.error?.title
        ?? getDefaultMessage(error.status);

      snackBar.open(message, 'Fechar', {
        duration: 6000,
        verticalPosition: 'top',
        horizontalPosition: 'center',
        panelClass: 'snack-erro'
      });

      return throwError(() => error);
    })
  );
};

function getDefaultMessage(status: number): string {
  switch (status) {
    case 0:    return 'Serviço indisponível. Verifique a conexão.';
    case 400:  return 'Requisição inválida.';
    case 404:  return 'Recurso não encontrado.';
    case 409:  return 'Conflito de concorrência. Tente novamente.';
    case 422:  return 'Dados inválidos.';
    case 503:  return 'Serviço de estoque indisponível.';
    default:   return 'Ocorreu um erro inesperado.';
  }
}
