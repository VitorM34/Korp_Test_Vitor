export interface InvoiceItem {
  id: string;
  productId: string;
  productDescription: string;
  quantity: number;
}

export interface Invoice {
  id: string;
  number: number;
  status: 'Open' | 'Closed';
  createdDate: string;
  printedDate?: string;
  items: InvoiceItem[];
}

export interface AddItemRequest {
  productId: string;
  productDescription: string;
  quantity: number;
}

export interface PrintLogEntry {
  id: string;
  invoiceId: string;
  invoiceNumber: number;
  printedDate: string;
}
