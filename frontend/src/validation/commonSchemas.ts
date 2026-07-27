import { z } from "zod";

export const guidSchema = z.string().uuid();

export const paginationSchema = z.object({
  page: z.coerce.number().int().min(1).default(1),
  pageSize: z.coerce.number().int().min(1).max(100).default(25)
});

export const searchSchema = z.object({
  query: z.string().trim().max(200).optional()
});

export const markdownSchema = z.object({
  title: z.string().trim().min(1).max(200),
  content: z.string().max(100_000)
});
