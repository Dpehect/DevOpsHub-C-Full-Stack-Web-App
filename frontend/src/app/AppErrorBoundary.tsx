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

  componentDidCatch(error: Error, info: React.ErrorInfo) {
    console.error("Unhandled frontend error", error, info);
  }

  render() {
    if (this.state.hasError) {
      return (
        <main role="alert" className="app-error">
          <h1>Something went wrong</h1>
          <p>The application could not render this page.</p>
          <button onClick={() => window.location.reload()}>Reload</button>
        </main>
      );
    }

    return this.props.children;
  }
}
