import { useQuery } from "@tanstack/react-query";
import { z } from "zod";
import { request } from "../api/http";

const userSchema = z.object({
  id: z.string().uuid(),
  email: z.string().email(),
  displayName: z.string(),
  role: z.string()
});

export function useCurrentUser() {
  return useQuery({
    queryKey: ["auth", "me"],
    queryFn: () => request("/auth/me", userSchema)
  });
}
