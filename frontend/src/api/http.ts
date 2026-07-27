import { z } from "zod";

const apiBaseUrl = import.meta.env.VITE_API_URL ?? "/api";

export async function request<T>(
  path: string,
  schema: z.ZodSchema<T>,
  init?: RequestInit
): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...init?.headers
    }
  });

  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new Error(problem?.title ?? `Request failed with ${response.status}`);
  }

  return schema.parse(await response.json());
}
