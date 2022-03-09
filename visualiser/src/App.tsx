import React, { useState } from "react";
import { Search } from "./app/Search";
import { useTracker } from "./app/useTracker";

const urlSearchParams = new URLSearchParams(window.location.search);
const params = Object.fromEntries(urlSearchParams.entries());

function App() {
  const [isTopHidden, setIsTopHidden] = useState(false);
  const { ticks, tracker } = useTracker(params["caseId"]);

  const no = () => <span style={{ color: "red" }}>&#x25A1;</span>;

  const yes = () => <span style={{ color: "green" }}>&#x2588;</span>;

  const ticksContent = () => (
    <div
      style={{
        wordWrap: "break-word",
        width: 600,
        fontSize: 40,
      }}
    >
      {/* {Array.from(Array(ticks).keys()).map((i) => (i % 10 ? "." : "|"))} */}
      {ticks / 10}
    </div>
  );

  const trackerContent = () =>
    tracker && (
      <>
        <code>
          <div>
            {tracker.documents.map((document) => (
              <div key={document.documentId}>
                Doc id: {document.documentId} Pdf?:{" "}
                {document.pdfUrl ? yes() : no()}
                &nbsp;&nbsp; Analyzed?:
                {document.pageDetails?.every((item) => item.dimensions)
                  ? yes()
                  : no()}
              </div>
            ))}
          </div>
          <div>
            Is indexed and complete?: {tracker.isIndexed ? yes() : no()}
            &nbsp;&nbsp;&nbsp;
            {tracker.isIndexed && (
              <button onClick={() => setIsTopHidden(true)}>hide</button>
            )}
          </div>
        </code>
        <br />
      </>
    );

  const rawTrackerContent = () => (
    <div
      style={{
        backgroundColor: "#eeeeee",
        fontSize: 10,
        padding: 10,
        flexGrow: 1,
      }}
    >
      <pre>{JSON.stringify(tracker, null, 2)}</pre>
    </div>
  );

  const searchContent = () =>
    tracker?.isIndexed && <Search tracker={tracker} />;
  return (
    <>
      {!isTopHidden && (
        <div
          style={{
            fontFamily: "courier",
            fontSize: 20,
            padding: 30,
            display: "flex",
            height: 500,
            overflow: "scroll",
          }}
        >
          <div>
            {ticksContent()}
            {trackerContent()}
          </div>

          <div>{rawTrackerContent()}</div>
        </div>
      )}

      {searchContent()}
    </>
  );
}

export default App;
