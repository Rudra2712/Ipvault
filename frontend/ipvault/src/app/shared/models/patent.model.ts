import { PatentDocument } from './document.model';

export interface PatentStatus {
  statusId: number;
  statusName: string;
  description: string;
  patentsCount?: number;
}

export interface PatentListItem {
  patentId: number;
  patentNumber: string;
  title: string;
  filingDate: string;
  grantDate?: string;
  expiryDate?: string;
  statusId: number;
  statusName: string;
  clientId: number;
  clientName: string;
  primaryInventorName: string;
  inventorsCount: number;
  documentsCount: number;
}

export interface PatentInventor {
  inventorId: number;
  fullName: string;
  email: string;
  organization: string;
  isPrimaryInventor: boolean;
}

export interface PatentDetail {
  patentId: number;
  patentNumber: string;
  title: string;
  abstract: string;
  filingDate: string;
  publicationDate?: string;
  grantDate?: string;
  expiryDate?: string;
  statusId: number;
  statusName: string;
  statusDescription: string;
  clientId: number;
  clientName: string;
  clientCompany: string;
  clientEmail: string;
  clientPhone: string;
  createdAt: string;
  updatedAt?: string;
  inventors: PatentInventor[];
  documents: PatentDocument[];
}

export interface CreatePatentRequest {
  patentNumber: string;
  title: string;
  abstract: string;
  filingDate: string;
  publicationDate?: string | null;
  grantDate?: string | null;
  expiryDate?: string | null;
  statusId: number;
  clientId: number;
  inventorIds: number[];
  primaryInventorId?: number | null;
}

export interface UpdatePatentRequest {
  patentNumber: string;
  title: string;
  abstract: string;
  filingDate: string;
  publicationDate?: string | null;
  grantDate?: string | null;
  expiryDate?: string | null;
  statusId: number;
  clientId: number;
}

export interface PatentFilter {
  search?: string;
  status?: string;
  statusId?: number;
  clientId?: number;
  year?: number;
  page?: number;
  pageSize?: number;
}
