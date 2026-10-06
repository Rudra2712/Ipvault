export interface PatentDocument {
  documentId: number;
  patentId: number;
  documentName: string;
  documentType: string;
  filePath: string;
  fileSize: number;
  uploadedAt: string;
  uploadedBy: string;
}
