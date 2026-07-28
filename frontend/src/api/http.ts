import { z } from "zod";
import { problemDetailsSchema } from "./problemDetails";

const apiBaseUrl = import.meta.env.VITE_API_URL ?? "/api";

export async function request<T>(
  path: string,
  schema: z.ZodSchema<T>,
  init?: RequestInit
): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    credentials: "include",
    headers: {
      Accept: "application/json",
      ...(init?.body ? { "Content-Type": "application/json" } : {}),
      ...init?.headers
    }
  });

  if (!response.ok) {
    const raw = await response.json().catch(() => null);
    const parsed = problemDetailsSchema.safeParse(raw);

    throw new Error(
      parsed.success
        ? parsed.data.title ?? `Request failed with ${response.status}`
        : `Request failed with ${response.status}`
    );
  }

  if (response.status === 204) {
    return schema.parse(undefined);
  }

  return schema.parse(await response.json());
}
