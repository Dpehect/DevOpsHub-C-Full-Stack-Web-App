import { z } from "zod";

export const problemDetailsSchema = z.object({
  type: z.string().optional(),
  title: z.string().optional(),
  status: z.number().int().optional(),
  detail: z.string().optional(),
  instance: z.string().optional(),
  traceId: z.string().optional(),
  errors: z.record(z.array(z.string())).optional()
}).passthrough();

export type ProblemDetails = z.infer<typeof problemDetailsSchema>;
