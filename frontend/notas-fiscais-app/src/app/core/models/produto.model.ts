export interface Produto {
  id: string;
  codigo: string;
  descricao: string;
  saldo: number;
}

export interface CriarProdutoRequest {
  codigo: string;
  descricao: string;
  saldo: number;
}
