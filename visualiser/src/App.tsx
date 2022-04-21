import { Input } from "./Input";
import { RawResults } from "./RawResults";
import { Results } from "./Results";
import { useApi } from "./useApi";

const App: React.FC = () => {
  const { setCaseId, tracker } = useApi();

  return (
    <div style={{ display: "flex", margin: 20 }}>
      <div>
        <Input setCaseId={setCaseId} />
        <br />
        <br />

        <Results tracker={tracker} />
      </div>

      <div>
        <RawResults tracker={tracker} />
      </div>
    </div>
  );
};

export default App;
