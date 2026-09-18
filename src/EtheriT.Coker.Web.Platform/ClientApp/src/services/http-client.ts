export const sessionStateChangedEvent = "coker:backoffice-session-state";
export const sessionExpiryChangedEvent = "coker:backoffice-session-expiry";

export type SessionState = "expired" | "forbidden";

let antiforgeryToken = "";

export function setAntiforgeryToken(token: string): void {
  antiforgeryToken = token;
}

function reportAuthenticationFailure(status: number): void {
  const state: SessionState | null = status === 401
    ? "expired"
    : status === 403
      ? "forbidden"
      : null;

  if (state) {
    window.dispatchEvent(new CustomEvent<SessionState>(sessionStateChangedEvent, {
      detail: state
    }));
  }
}

export async function platformFetch(
  input: RequestInfo | URL,
  init: RequestInit = {}
): Promise<Response> {
  const headers = new Headers(init.headers);
  const method = (init.method ?? "GET").toUpperCase();
  if (antiforgeryToken && !["GET", "HEAD", "OPTIONS", "TRACE"].includes(method)) {
    headers.set("X-XSRF-TOKEN", antiforgeryToken);
  }

  const response = await fetch(input, {
    ...init,
    headers,
    credentials: "same-origin"
  });

  reportAuthenticationFailure(response.status);
  const expiresAt = Number(response.headers.get("X-Session-Expires-At"));
  const serverTime = Number(response.headers.get("X-Session-Server-Time"));
  if (response.ok && expiresAt > 0 && serverTime > 0) {
    window.dispatchEvent(new CustomEvent<number>(sessionExpiryChangedEvent, {
      detail: Date.now() + expiresAt - serverTime
    }));
  }
  return response;
}
