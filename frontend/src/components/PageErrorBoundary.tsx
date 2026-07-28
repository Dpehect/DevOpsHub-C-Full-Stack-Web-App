import React from "react";

type State = {
  failed: boolean;
};

export class PageErrorBoundary extends React.Component<
  React.PropsWithChildren,
  State
> {
  state: State = { failed: false };

  static getDerivedStateFromError(): State {
    return { failed: true };
  }

  componentDidCatch(error: Error, info: React.ErrorInfo): void {
    console.error("Page render failed", error, info);
  }

  render(): React.ReactNode {
    if (this.state.failed) {
      return (
        <section role="alert" className="page-error">
          <h2>This page could not be loaded</h2>
          <button
            type="button"
            onClick={() => this.setState({ failed: false })}
          >
            Try again
          </button>
        </section>
      );
    }

    return this.props.children;
  }
}
