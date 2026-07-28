import { useQuery } from "@tanstack/react-query";
import { z } from "zod";
import { request } from "../api/http";

const projectSchema = z.object({
  id: z.string().uuid(),
  name: z.string(),
  description: z.string().nullable().optional()
});

const projectsSchema = z.array(projectSchema);

export function useProjects(workspaceId: string) {
  return useQuery({
    queryKey: ["workspaces", workspaceId, "projects"],
    queryFn: () =>
      request(
        `/workspaces/${workspaceId}/projects`,
        projectsSchema
      ),
    enabled: Boolean(workspaceId)
  });
}
