import type { CSSProperties } from "react";

type SkeletonProps = {
  width?: string | number;
  height?: string | number;
  radius?: string | number;
  className?: string;
};

export function Skeleton({
  width = "100%",
  height = 16,
  radius = 8,
  className
}: SkeletonProps) {
  const style: CSSProperties = { width, height, borderRadius: radius };

  return (
    <span
      aria-hidden="true"
      className={["skeleton", className].filter(Boolean).join(" ")}
      style={style}
    />
  );
}
