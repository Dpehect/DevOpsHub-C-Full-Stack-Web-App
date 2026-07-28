import { useMutation, useQueryClient } from "@tanstack/react-query";
import { z } from "zod";
import { request } from "../api/http";

const pipelineRunSchema = z.object({
  id: z.string().uuid(),
  status: z.string(),
  branch: z.string(),
  startedAt: z.string()
});

type TriggerPipelineInput = {
  pipelineId: string;
  branch: string;
  commitSha: string;
};

export function useTriggerPipeline() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ pipelineId, branch, commitSha }: TriggerPipelineInput) =>
      request(
        `/pipelines/${pipelineId}/runs`,
        pipelineRunSchema,
        {
          method: "POST",
          body: JSON.stringify({ branch, commitSha })
        }
      ),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["pipelines"]
      });
    }
  });
}
