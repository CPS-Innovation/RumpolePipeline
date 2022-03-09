export type Tracker = {
  isIndexed: boolean;
  transactionId: string;
  documents: {
    documentId: number;
    pdfUrl: string;
    pageDetails: {
      dimensions?: {
        height: number;
        width: number;
      };
    }[];
  }[];
};
