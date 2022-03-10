import { Tracker } from "./types";

type Props = {
  tracker: Tracker | undefined;
};

export const RawResults: React.FC<Props> = ({ tracker }) => {
  return <pre style={{ margin: 15 }}>{JSON.stringify(tracker, null, 1)}</pre>;
};
