import { useEffect, useState } from "react";
import Redaction from "./Redaction";
import { SearchResult, SearchResults } from "./SearchResults";
import { Tracker } from "./tracker";

type Props = {
  tracker: Tracker;
};

export const Search: React.FC<Props> = ({ tracker }) => {
  const [searchTerm, setSearchTerm] = useState("");
  const [results, setResults] = useState<SearchResults | undefined>();

  const [selectedSearchResult, setSelectedSearchResult] =
    useState<SearchResult>();

  console.log(selectedSearchResult);

  useEffect(() => {
    (async () => {
      if (!searchTerm) return;

      const searchResponse = await fetch(
        process.env.REACT_APP_SEARCH_INDEX! + searchTerm
      );

      const results = (await searchResponse.json()) as SearchResults;
      console.log(results);
      setResults(results);
    })();
  }, [searchTerm]);

  const searchContent = () => (
    <>
      {" "}
      <input onChange={(ev) => setSearchTerm(ev.target.value)} />
      <br />
      <ul style={{ width: 500 }}>
        {results &&
          results.value.map((item) => (
            <li>
              <div>
                Doc: {item.documentId}, page:{item.pageIndex}, line:{" "}
                {item.lineIndex}:&nbsp;&nbsp;&nbsp;
                <a href="#" onClick={() => setSelectedSearchResult(item)}>
                  <em>{item.text}</em>
                </a>
              </div>
            </li>
          ))}
      </ul>
    </>
  );

  const displayContent = () => {
    const docUrl = tracker.documents.find(
      (item) => item.documentId === selectedSearchResult?.documentId
    )?.pdfUrl;

    return (
      docUrl && (
        <div>
          <button onClick={() => setSelectedSearchResult(undefined)}>
            Back
          </button>
          <Redaction pdfUrl={docUrl} />
        </div>
      )
    );
  };

  return (
    <div style={{ margin: 30 }}>
      {selectedSearchResult ? displayContent() : searchContent()}
    </div>
  );
};
