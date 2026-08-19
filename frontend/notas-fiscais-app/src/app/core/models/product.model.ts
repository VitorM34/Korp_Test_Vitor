export interface Product {
  id: string;
  code: string;
  description: string;
  balance: number;
}

export interface CreateProductRequest {
  code: string;
  description: string;
  balance: number;
}
