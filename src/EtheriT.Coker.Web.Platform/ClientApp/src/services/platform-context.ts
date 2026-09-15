import type { PlatformContext } from "@/types/platform-context";

export async function getPlatformContext(): Promise<PlatformContext> {
  const response = await fetch("/api/platform-context", {
    credentials: "same-origin",
    headers: {
      Accept: "application/json"
    }
  });

  if (!response.ok) {
    throw new Error(`Platform context request failed: ${response.status}`);
  }

  return response.json() as Promise<PlatformContext>;
}
