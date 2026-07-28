import { z } from "zod";

export const uuidParamSchema = z.object({
  id: z.string().uuid()
}).strict();

export const workspaceParamSchema = z.object({
  workspaceId: z.string().uuid()
}).strict();

export const paginationQuerySchema = z.object({
  page: z.coerce.number().int().min(1).default(1),
  pageSize: z.coerce.number().int().min(1).max(100).default(25)
}).strict();

export const searchQuerySchema = z.object({
  query: z.string().trim().max(200).optional()
}).strict();

export const createProjectSchema = z.object({
  name: z.string().trim().min(1).max(120),
  description: z.string().trim().max(2_000).optional()
}).strict();

export const createTaskSchema = z.object({
  title: z.string().trim().min(1).max(200),
  description: z.string().trim().max(5_000).optional(),
  priority: z.enum(["Low", "Medium", "High", "Critical"]),
  assigneeId: z.string().uuid().nullable().optional()
}).strict();

export const triggerPipelineSchema = z.object({
  branch: z.string().trim().min(1).max(255),
  commitSha: z.string().trim().regex(/^[a-f0-9]{7,40}$/i)
}).strict();

export const createIncidentSchema = z.object({
  title: z.string().trim().min(1).max(200),
  description: z.string().trim().min(1).max(10_000),
  severity: z.enum(["Low", "Medium", "High", "Critical"]),
  serviceId: z.string().uuid()
}).strict();
