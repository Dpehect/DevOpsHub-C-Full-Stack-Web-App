import { z } from "zod";

export const idSchema = z.string().uuid();

export const paginationSchema = z.object({
  page: z.coerce.number().int().min(1).default(1),
  pageSize: z.coerce.number().int().min(1).max(100).default(25)
}).strict();

export const searchSchema = z.object({
  query: z.string().trim().max(200).optional()
}).strict();

export const taskSchema = z.object({
  title: z.string().trim().min(1).max(200),
  description: z.string().trim().max(5_000).optional(),
  priority: z.enum(["Low", "Medium", "High", "Critical"]),
  assigneeId: z.string().uuid().nullable().optional()
}).strict();

export const incidentSchema = z.object({
  title: z.string().trim().min(1).max(200),
  description: z.string().trim().min(1).max(10_000),
  severity: z.enum(["Low", "Medium", "High", "Critical"]),
  serviceId: z.string().uuid()
}).strict();

export const pipelineSchema = z.object({
  name: z.string().trim().min(1).max(120),
  repositoryId: z.string().uuid(),
  branch: z.string().trim().min(1).max(255)
}).strict();

export const wikiSchema = z.object({
  title: z.string().trim().min(1).max(200),
  content: z.string().max(100_000),
  category: z.string().trim().max(100).optional()
}).strict();
