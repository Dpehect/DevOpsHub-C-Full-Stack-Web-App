import React from "react";

type State = { hasError: boolean };

export class AppErrorBoundary extends React.Component<
  React.PropsWithChildren,
  State
> {
  state: State = { hasError: false };

  static getDerivedStateFromError(): State {
    return { hasError: true };
  }

  componentDidCatch(error: Error, info: React.ErrorInfo): void {
    console.error("Unhandled render error", error, info);
  }

  render(): React.ReactNode {
    if (this.state.hasError) {
      return (
        <main role="alert" className="app-error-boundary">
          <h1>Something went wrong</h1>
          <p>The page could not be rendered safely.</p>
          <button type="button" onClick={() => window.location.reload()}>
            Reload
          </button>
        </main>
      );
    }

    return this.props.children;
  }
}
