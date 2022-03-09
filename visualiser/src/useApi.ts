import { stringify } from "querystring";
import { useEffect, useRef, useState } from "react";
import { Tracker } from "./types";
import { useClock } from "./useClock";

const COORDNINATOR_URL = process.env.REACT_APP_COORDINATOR!;

const resolveHttps = (url: string) =>
  COORDNINATOR_URL.startsWith("https://")
    ? url.replace("http://", "https://")
    : url;

const resolveUrl = (urlTemplate: string, caseId: string, force?: boolean) =>
  urlTemplate
    .replaceAll("{caseId}", caseId)
    .replaceAll("{force}", String(force));

const initiatePipeline = async (caseId: string, force: boolean) => {
  const koResponse = await fetch(resolveUrl(COORDNINATOR_URL, caseId, force));
  const koResponseContent = await koResponse.json();

  const statusQueryCallResponse = await fetch(
    resolveHttps(koResponseContent.statusQueryGetUri as string)
  );
  const statusQueryContent = await statusQueryCallResponse.json();

  return statusQueryContent.input.TrackerUrl;
};

export const useApi = () => {
  const tick = useClock();

  const [caseId, setCaseId] = useState<{ val: string; force: boolean }>();
  const [trackerUrl, setTrackerUrl] = useState<string>();
  const [tracker, setTracker] = useState<Tracker>();
  const [elapsedSeconds, setElapsedSeconds] = useState(0);

  const isInProgressRef = useRef<boolean>(false);

  useEffect(() => {
    if (isInProgressRef.current || !caseId) return;

    setTrackerUrl("");
    setElapsedSeconds(0);
    isInProgressRef.current = true;

    initiatePipeline(caseId.val, caseId.force).then((trackerUrl) => {
      setTrackerUrl(resolveUrl(trackerUrl, caseId.val));
    });
  }, [caseId]);

  useEffect(() => {
    if (!trackerUrl || !isInProgressRef.current) return;

    fetch(trackerUrl)
      .then((response) => response.json())
      .then((nextTracker: Tracker) => {
        setTracker(nextTracker);
        if (
          nextTracker.isIndexed &&
          /* avoid false positive from reading the historic state and finding isIndexed true */ elapsedSeconds >
            1
        ) {
          isInProgressRef.current = false;
        }
      });
  }, [trackerUrl, elapsedSeconds]);

  useEffect(() => {
    if (!isInProgressRef.current) return;

    setElapsedSeconds((elapsedSeconds) => elapsedSeconds + 1);
  }, [tick]);

  return { caseId, setCaseId, elapsedSeconds, tracker };
};
