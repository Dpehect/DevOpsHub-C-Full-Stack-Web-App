import { z } from "zod";

export async function parseJson<T>(
  response: Response,
  schema: z.ZodSchema<T>
): Promise<T> {
  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new Error(problem?.title ?? `Request failed: ${response.status}`);
  }

  return schema.parse(await response.json());
}
