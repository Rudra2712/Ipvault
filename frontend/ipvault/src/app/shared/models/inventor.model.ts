export interface Inventor {
  inventorId: number;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone: string;
  organization: string;
  createdAt: string;
  patentsCount: number;
}

export interface CreateInventorRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  organization: string;
}

export interface UpdateInventorRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  organization: string;
}
