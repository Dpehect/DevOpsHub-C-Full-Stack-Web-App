import {
  useMutation,
  useQueryClient,
  type QueryKey,
  type UseMutationOptions
} from "@tanstack/react-query";

type MutationOptions<TData, TVariables> = UseMutationOptions<
  TData,
  Error,
  TVariables
> & {
  invalidate?: QueryKey[];
};

export function useApiMutation<TData, TVariables>({
  invalidate = [],
  ...options
}: MutationOptions<TData, TVariables>) {
  const queryClient = useQueryClient();

  return useMutation({
    ...options,
    onSuccess: async (data, variables, context) => {
      await Promise.all(
        invalidate.map(queryKey =>
          queryClient.invalidateQueries({ queryKey })
        )
      );

      await options.onSuccess?.(data, variables, context);
    }
  });
}
