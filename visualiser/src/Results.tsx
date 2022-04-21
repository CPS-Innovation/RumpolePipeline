import { Tracker } from "./types";

type Props = {
  tracker: Tracker | undefined;
};

const Val: React.FC<{ val: any }> = ({ val }) => {
  return !!val ? (
    <span style={{ color: "green" }}>&#x2588;</span>
  ) : (
    <span style={{ color: "red" }}>&#x25A1;</span>
  );
};

export const Results: React.FC<Props> = ({ tracker }) => {
  if (!tracker) return null;

  return (
    <table>
      <thead>
        <tr>
          <th>Doc id</th>
          <th>Pdf ready?</th>
          <th>Search Ready?</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        {tracker.documents.map((document) => (
          <tr key={document.documentId}>
            <td>{document.documentId}</td>
            <td>
              <Val val={document.pdfUrl} />
            </td>
            <td>
              <Val
                val={document.pageDetails?.every((item) => item.dimensions)}
              />
            </td>
            <td>
              {document.pdfUrl && (
                <a href={document.pdfUrl} target="_blank" rel="noreferrer">
                  Open Pdf
                </a>
              )}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
};
