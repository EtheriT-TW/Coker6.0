import { platformFetch } from "@/services/http-client";

const activityEvents: Array<keyof WindowEventMap> = [
  "pointerdown",
  "keydown",
  "input",
  "change",
  "touchstart"
];

let activityPending = false;
let intervalId: number | undefined;
let intervalMilliseconds = 5 * 60 * 1000;
let lastReportAt = Date.now();
let reportInProgress = false;

function markActivity(): void {
  activityPending = true;

  // If the user returns shortly before expiry, renew immediately instead of
  // waiting for the next timer tick.
  if (Date.now() - lastReportAt >= intervalMilliseconds) {
    void flushActivity();
  }
}

export async function reportSessionActivity(): Promise<boolean> {
  try {
    const response = await platformFetch("/api/backoffice-session/activity", {
      method: "POST",
      headers: {
        Accept: "application/json",
        "X-Requested-With": "XMLHttpRequest"
      }
    });

    return response.ok;
  }
  catch (error) {
    console.error("Unable to refresh the backoffice session.", error);
    return false;
  }
}

async function flushActivity(): Promise<void> {
  if (!activityPending || reportInProgress || document.visibilityState !== "visible") {
    return;
  }

  reportInProgress = true;
  try {
    if (await reportSessionActivity()) {
      activityPending = false;
      lastReportAt = Date.now();
    }
  }
  finally {
    reportInProgress = false;
  }
}

function handleVisibilityChange(): void {
  if (document.visibilityState === "visible") {
    void flushActivity();
  }
}

export function startSessionLifecycle(intervalSeconds: number): () => void {
  stopSessionLifecycle();

  for (const eventName of activityEvents) {
    window.addEventListener(eventName, markActivity, { passive: true });
  }
  document.addEventListener("visibilitychange", handleVisibilityChange);

  intervalMilliseconds = Math.max(60, intervalSeconds) * 1000;
  lastReportAt = Date.now();
  intervalId = window.setInterval(() => void flushActivity(), intervalMilliseconds);

  return stopSessionLifecycle;
}

export function stopSessionLifecycle(): void {
  if (intervalId !== undefined) {
    window.clearInterval(intervalId);
    intervalId = undefined;
  }

  for (const eventName of activityEvents) {
    window.removeEventListener(eventName, markActivity);
  }
  document.removeEventListener("visibilitychange", handleVisibilityChange);
  activityPending = false;
  reportInProgress = false;
}
