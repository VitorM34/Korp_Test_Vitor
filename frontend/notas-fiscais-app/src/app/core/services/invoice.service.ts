import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Invoice, AddItemRequest, PrintLogEntry } from '../models/invoice.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class InvoiceService {
  private readonly baseUrl = `${environment.billingApiUrl}/api/invoices`;

  constructor(private http: HttpClient) {}

  list(): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(this.baseUrl);
  }

  getById(id: string): Observable<Invoice> {
    return this.http.get<Invoice>(`${this.baseUrl}/${id}`);
  }

  create(): Observable<Invoice> {
    return this.http.post<Invoice>(this.baseUrl, {});
  }

  delete(invoiceId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${invoiceId}`);
  }

  addItem(invoiceId: string, request: AddItemRequest): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.baseUrl}/${invoiceId}/items`, request);
  }

  removeItem(invoiceId: string, itemId: string): Observable<Invoice> {
    return this.http.delete<Invoice>(`${this.baseUrl}/${invoiceId}/items/${itemId}`);
  }

  updateItem(invoiceId: string, itemId: string, quantity: number): Observable<Invoice> {
    return this.http.patch<Invoice>(`${this.baseUrl}/${invoiceId}/items/${itemId}`, { quantity });
  }

  print(invoiceId: string): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.baseUrl}/${invoiceId}/print`, {});
  }

  getPrintLog(): Observable<PrintLogEntry[]> {
    return this.http.get<PrintLogEntry[]>(`${this.baseUrl}/print-log`);
  }

  logReprint(invoiceId: string): Observable<PrintLogEntry> {
    return this.http.post<PrintLogEntry>(`${this.baseUrl}/${invoiceId}/reprint-log`, {});
  }
}
