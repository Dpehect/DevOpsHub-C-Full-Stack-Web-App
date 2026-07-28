import { useQuery } from "@tanstack/react-query";
import { z } from "zod";
import { request } from "../api/http";

const incidentSchema = z.object({
  id: z.string().uuid(),
  title: z.string(),
  severity: z.string(),
  status: z.string(),
  createdAt: z.string()
});

const incidentsSchema = z.array(incidentSchema);

export function useIncidents(workspaceId: string) {
  return useQuery({
    queryKey: ["workspaces", workspaceId, "incidents"],
    queryFn: () =>
      request(
        `/incidents/dashboard/${workspaceId}`,
        incidentsSchema
      ),
    enabled: Boolean(workspaceId)
  });
}
