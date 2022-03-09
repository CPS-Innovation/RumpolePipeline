import type { IHighlight } from "../react-pdf-highlighter";

interface Props {
  highlights: Array<IHighlight>;
  resetHighlights: () => void;
  //toggleDocument: () => void;
  undoRedaction: (id: string) => void;
}

const updateHash = (highlight: IHighlight) => {
  document.location.hash = `highlight-${highlight.id}`;
};

export function Sidebar({
  highlights,
  //toggleDocument,
  resetHighlights,
  undoRedaction,
}: Props) {
  const handleUndoClick = (highlight: IHighlight) => {
    undoRedaction(highlight.id);
  };

  return (
    <div className="sidebar" style={{ width: "25vw" }}>
      <div style={{ padding: "1rem" }}>
        <h2>Redaction Types</h2>
        <ul>
          <li>
            <strong>Text selection</strong> - highlight text and select "Click
            to redact"
          </li>
          <li>
            <strong>Area selection</strong>- hold Alt (Windows) or ⌥ Option
            (Mac), then click and drag.
          </li>
        </ul>
      </div>

      <ul className="sidebar__highlights">
        {highlights.map((highlight, index) => (
          <li key={index} className="sidebar__highlight">
            <div>
              {highlight.content.text ? (
                <blockquote style={{ marginTop: "0.5rem" }}>
                  {`${
                    highlight.content.text.length > 90
                      ? highlight.content.text.slice(0, 90).trim() + "..."
                      : highlight.content.text
                  }`}
                </blockquote>
              ) : null}
              {highlight.content.image ? (
                <div
                  className="highlight__image"
                  style={{ marginTop: "0.5rem" }}
                >
                  <img src={highlight.content.image} alt={"Screenshot"} />
                </div>
              ) : null}
            </div>
            <div className="highlight__location">
              Page {highlight.position.pageNumber}
              &nbsp;&nbsp;&nbsp;&nbsp;
              <button
                onClick={() => {
                  updateHash(highlight);
                }}
              >
                Go to redaction
              </button>
              &nbsp;&nbsp;&nbsp;&nbsp;
              <button onClick={(ev) => handleUndoClick(highlight)}>
                Undo redaction
              </button>
            </div>
          </li>
        ))}
      </ul>
      {/* <div style={{ padding: "1rem" }}>
        <button onClick={toggleDocument}>Toggle PDF document</button>
      </div> */}
      {highlights.length > 0 ? (
        <div style={{ padding: "1rem" }}>
          <button onClick={resetHighlights}>Clear all redactions</button>
        </div>
      ) : null}
    </div>
  );
}
