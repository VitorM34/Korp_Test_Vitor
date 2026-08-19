import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product, CreateProductRequest } from '../models/product.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly baseUrl = `${environment.inventoryApiUrl}/api/products`;

  constructor(private http: HttpClient) {}

  list(): Observable<Product[]> {
    return this.http.get<Product[]>(this.baseUrl);
  }

  create(request: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.baseUrl, request);
  }
}
