export type SearchResults = {
  value: SearchResult[];
};

export type SearchResult = {
  id: string;
  documentId: number;
  pageIndex: number;
  lineIndex: number;
  text: string;
  boundingBox: number[];
};
