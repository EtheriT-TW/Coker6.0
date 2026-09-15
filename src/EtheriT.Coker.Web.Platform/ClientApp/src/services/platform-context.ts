import type { PlatformContext } from "@/types/platform-context";
import { platformFetch } from "@/services/http-client";

export async function getPlatformContext(): Promise<PlatformContext> {
  const response = await platformFetch("/api/platform-context", {
    headers: {
      Accept: "application/json"
    }
  });

  if (!response.ok) {
    throw new Error(`Platform context request failed: ${response.status}`);
  }

  return response.json() as Promise<PlatformContext>;
}
