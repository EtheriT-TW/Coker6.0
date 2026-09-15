export const sessionStateChangedEvent = "coker:backoffice-session-state";

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
  return response;
}
