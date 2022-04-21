import { useRef } from "react";

type Props = {
  setCaseId: (caseId: { val: string; force: boolean }) => void;
};

export const Input: React.FC<Props> = ({ setCaseId }) => {
  const inputRef = useRef<HTMLInputElement>(null);
  const checkBoxRef = useRef<HTMLInputElement>(null);

  return (
    <div style={{ margin: 5 }}>
      <label>
        <span>Case Id:</span>
        <input ref={inputRef}></input>
      </label>
      <br />
      <label>
        <span>Force:</span>
        <input type="checkbox" ref={checkBoxRef} />
      </label>
      <br />
      <br />
      <button
        onClick={() =>
          setCaseId({
            val: inputRef.current?.value || "",
            force: checkBoxRef.current?.checked || false,
          })
        }
      >
        Go
      </button>
    </div>
  );
};
