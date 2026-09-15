export const sessionStateChangedEvent = "coker:backoffice-session-state";

export type SessionState = "expired" | "forbidden";

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
  const response = await fetch(input, {
    ...init,
    credentials: "same-origin"
  });

  reportAuthenticationFailure(response.status);
  return response;
}
