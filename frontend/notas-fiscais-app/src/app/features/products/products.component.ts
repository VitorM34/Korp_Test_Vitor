import { Component, OnInit, ViewChild, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroupDirective, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { EMPTY, catchError } from 'rxjs';
import { ProductService } from '../../core/services/product.service';
import { Product } from '../../core/models/product.model';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss']
})
export class ProductsComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly fb = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);

  // Referência à diretiva do form: FormGroup.reset() sozinho não zera a flag "submitted"
  // da diretiva, que é o que o Angular Material usa para decidir se mostra o campo em erro.
  @ViewChild('formDirective') formDirective?: FormGroupDirective;

  readonly products = signal<Product[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly search = signal('');

  readonly productsWithBalance = computed(() => this.products().filter(p => p.balance > 0).length);
  readonly productsOutOfStock = computed(() => this.products().filter(p => p.balance === 0).length);

  readonly filteredProducts = computed(() => {
    const term = this.search().trim().toLowerCase();
    if (!term) return this.products();
    return this.products().filter(p =>
      p.code.toLowerCase().includes(term) ||
      p.description.toLowerCase().includes(term)
    );
  });

  readonly emptyList = computed(() => this.products().length === 0);
  readonly noSearchResults = computed(() =>
    !this.emptyList() && this.filteredProducts().length === 0
  );

  readonly form = this.fb.group({
    code:        ['', [Validators.required, Validators.maxLength(50)]],
    description: ['', [Validators.required, Validators.maxLength(200)]],
    balance:     [0,  [Validators.required, Validators.min(0)]]
  });

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading.set(true);
    this.productService.list()
      .pipe(
        catchError(() => {
          this.snackBar.open('Erro ao carregar produtos.', 'Fechar', { duration: 4000 });
          this.loading.set(false);
          return EMPTY;
        })
      )
      .subscribe(list => {
        this.products.set(list);
        this.loading.set(false);
      });
  }

  save(): void {
    if (this.form.invalid) return;
    this.saving.set(true);

    this.productService.create(this.form.value as any)
      .pipe(
        catchError(() => {
          this.saving.set(false);
          return EMPTY;
        })
      )
      .subscribe(created => {
        this.products.update(list =>
          [...list, created].sort((a, b) => a.code.localeCompare(b.code))
        );
        this.formDirective?.resetForm({ balance: 0 });
        this.saving.set(false);
        this.snackBar.open('Produto cadastrado com sucesso!', 'OK', { duration: 3000 });
      });
  }

  onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  focusRegistration(): void {
    document.getElementById('product-code')?.focus();
  }
}
