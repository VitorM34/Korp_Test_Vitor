import { Pipe, PipeTransform } from '@angular/core';

const LABELS: Record<string, string> = {
  Open: 'Aberta',
  Closed: 'Fechada',
};

@Pipe({
  name: 'invoiceStatus',
  standalone: true,
})
export class InvoiceStatusPipe implements PipeTransform {
  transform(status: string | null | undefined): string {
    if (!status) return '';
    return LABELS[status] ?? status;
  }
}
