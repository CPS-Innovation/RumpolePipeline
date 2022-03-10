import { useEffect, useState } from "react";

export const useClock = () => {
  const [count, setCount] = useState(true);

  useEffect(() => {
    setInterval(() => {
      setCount((count) => !count);
    }, 1000);
  }, []);

  return +count;
};
