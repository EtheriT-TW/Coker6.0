import { getPlatformContext } from "@/services/platform-context";
import { platformFetch } from "@/services/http-client";
import { getReauthenticationTicket } from "@/core/auth/reauthentication-ticket";

interface ReauthenticationResponse {
  Success: boolean;
  Error?: string;
}

const pendingSaveChecks = new Set<(authenticated: boolean) => void>();

function waitForReauthentication(): Promise<boolean> {
  return new Promise(resolve => pendingSaveChecks.add(resolve));
}

export function completeReauthentication(authenticated: boolean): void {
  for (const resolve of pendingSaveChecks) resolve(authenticated);
  pendingSaveChecks.clear();
}

export async function ensureAuthenticatedForSave(): Promise<boolean> {
  const response = await platformFetch("/api/backoffice-session/activity", {
    method: "POST",
    headers: {
      Accept: "application/json",
      "X-Requested-With": "XMLHttpRequest"
    }
  });

  if (response.ok) return true;
  if (response.status === 401) return waitForReauthentication();
  if (response.status === 403) return false;

  throw new Error(`無法驗證登入狀態 (${response.status})`);
}

export async function reauthenticate(
  password: string
): Promise<{ success: boolean; error?: string }> {
  try {
    const response = await platformFetch("/api/reauthentication", {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json; charset=utf-8",
        "X-Requested-With": "XMLHttpRequest"
      },
      body: JSON.stringify({
        Ticket: getReauthenticationTicket(),
        Password: password
      })
    });

    if (response.status === 429) {
      return { success: false, error: "嘗試次數過多，請稍候一分鐘再試。" };
    }
    if (!response.ok) {
      return { success: false, error: `重新登入失敗 (${response.status})` };
    }

    const result = await response.json() as ReauthenticationResponse;
    if (!result.Success) {
      return { success: false, error: result.Error ?? "密碼不正確。" };
    }

    // Obtain an anti-forgery token bound to the newly issued identity before
    // resuming the original save operation.
    await getPlatformContext();
    completeReauthentication(true);
    return { success: true };
  }
  catch (error) {
    console.error(error);
    return { success: false, error: "目前無法連線至登入服務，請稍後再試。" };
  }
}
