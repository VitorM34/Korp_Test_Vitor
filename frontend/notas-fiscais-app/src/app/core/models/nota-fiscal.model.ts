export interface ItemNotaFiscal {
  id: string;
  produtoId: string;
  descricaoProduto: string;
  quantidade: number;
}

export interface NotaFiscal {
  id: string;
  numeracao: number;
  status: 'Aberta' | 'Fechada';
  dataCriacao: string;
  dataImpressao?: string;
  itens: ItemNotaFiscal[];
}

export interface AdicionarItemRequest {
  produtoId: string;
  descricaoProduto: string;
  quantidade: number;
}
