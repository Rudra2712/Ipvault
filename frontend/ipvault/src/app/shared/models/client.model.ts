export interface Client {
  clientId: number;
  clientName: string;
  email: string;
  phone: string;
  company: string;
  address: string;
  createdAt: string;
  updatedAt?: string;
  patentsCount: number;
}

export interface CreateClientRequest {
  clientName: string;
  email: string;
  phone: string;
  company: string;
  address: string;
}

export interface UpdateClientRequest {
  clientName: string;
  email: string;
  phone: string;
  company: string;
  address: string;
}
