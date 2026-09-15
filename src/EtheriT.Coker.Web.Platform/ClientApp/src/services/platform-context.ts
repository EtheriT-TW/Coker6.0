import type { PlatformContext } from "@/types/platform-context";
import { platformFetch, setAntiforgeryToken } from "@/services/http-client";
import { setReauthenticationTicket } from "@/core/auth/reauthentication-ticket";

export async function getPlatformContext(): Promise<PlatformContext> {
  const response = await platformFetch("/api/platform-context", {
    headers: {
      Accept: "application/json"
    }
  });

  if (!response.ok) {
    throw new Error(`Platform context request failed: ${response.status}`);
  }

  const context = await response.json() as PlatformContext;
  setAntiforgeryToken(context.AntiforgeryToken);
  setReauthenticationTicket(context.ReauthenticationTicket);
  return context;
}
