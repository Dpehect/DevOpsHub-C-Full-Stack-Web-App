import type { ReactNode } from "react";
import { Skeleton } from "./Skeleton";

type AsyncStateProps<T> = {
  data: T | undefined;
  isLoading: boolean;
  error: unknown;
  children: (data: T) => ReactNode;
  loadingRows?: number;
};

export function AsyncState<T>({
  data,
  isLoading,
  error,
  children,
  loadingRows = 4
}: AsyncStateProps<T>) {
  if (isLoading) {
    return (
      <section aria-busy="true" aria-label="Loading">
        {Array.from({ length: loadingRows }).map((_, index) => (
          <Skeleton key={index} height={72} className="skeleton-row" />
        ))}
      </section>
    );
  }

  if (error) {
    return (
      <section role="alert">
        <h2>Request failed</h2>
        <p>{error instanceof Error ? error.message : "Unexpected error"}</p>
      </section>
    );
  }

  if (data === undefined) {
    return null;
  }

  return children(data);
}
