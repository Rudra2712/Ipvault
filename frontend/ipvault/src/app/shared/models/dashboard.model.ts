export interface StatusCount {
  statusName: string;
  count: number;
}

export interface ClientPatentCount {
  clientName: string;
  count: number;
}

export interface YearlyFilingCount {
  year: number;
  count: number;
}

export interface UpcomingExpiry {
  patentId: number;
  patentNumber: string;
  title: string;
  expiryDate?: string;
  clientName: string;
  daysRemaining: number;
}

export interface DashboardSummary {
  totalPatents: number;
  grantedPatents: number;
  pendingPatents: number;
  expiredPatents: number;
  totalClients: number;
  totalInventors: number;
  patentCountByStatus: StatusCount[];
  patentCountByClient: ClientPatentCount[];
  patentsFiledPerYear: YearlyFilingCount[];
  upcomingExpiries: UpcomingExpiry[];
}
