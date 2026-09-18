import { computed, ref } from "vue";
import { platformFetch, sessionExpiryChangedEvent, sessionStateChangedEvent } from "@/services/http-client";

export const sessionRemainingSeconds = ref<number | null>(null);
export const sessionExpiryWarning = computed(() =>
  sessionRemainingSeconds.value !== null && sessionRemainingSeconds.value <= 120
);
let expiresAt: number | undefined;
let countdownId: number | undefined;
let statusCheckInProgress = false;
let lastStatusCheckAt = 0;
let sessionBlocked = false;

function updateExpiry(event: Event): void {
  expiresAt = (event as CustomEvent<number>).detail;
  sessionBlocked = false;
  updateCountdown();
}

function blockSession(): void {
  sessionBlocked = true;
  activityPending = false;
  sessionRemainingSeconds.value = null;
}

function updateCountdown(): void {
  if (sessionBlocked) return;
  if (expiresAt === undefined) {
    if (!statusCheckInProgress && Date.now() - lastStatusCheckAt >= 10000) {
      void checkSessionStatus();
    }
    return;
  }
  sessionRemainingSeconds.value = Math.max(0, Math.ceil((expiresAt - Date.now()) / 1000));
  if (sessionRemainingSeconds.value === 0 && !statusCheckInProgress && Date.now() - lastStatusCheckAt >= 10000) {
    void checkSessionStatus();
  }
}

async function checkSessionStatus(): Promise<void> {
  if (statusCheckInProgress) return;
  statusCheckInProgress = true;
  lastStatusCheckAt = Date.now();
  try {
    await platformFetch("/api/backoffice-session/status", {
      cache: "no-store",
      headers: { Accept: "application/json" }
    });
  }
  catch (error) {
    console.error("Unable to check the backoffice session.", error);
  }
  finally {
    statusCheckInProgress = false;
  }
}

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
  if (sessionBlocked || sessionExpiryWarning.value) return;
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
  if (sessionBlocked || sessionExpiryWarning.value || !activityPending || reportInProgress || document.visibilityState !== "visible") {
    return;
  }

  reportInProgress = true;
  try {
    if (await reportSessionActivity()) {
      activityPending = false;
    }
  }
  finally {
    // Throttle failed checks too; otherwise typing inside the re-login modal
    // could generate one unauthorized request for every key press.
    lastReportAt = Date.now();
    reportInProgress = false;
  }
}

function handleVisibilityChange(): void {
  if (document.visibilityState === "visible") {
    updateCountdown();
    void flushActivity();
  }
}

export function startSessionLifecycle(intervalSeconds: number): () => void {
  stopSessionLifecycle();
  sessionBlocked = false;
  window.addEventListener(sessionExpiryChangedEvent, updateExpiry);
  window.addEventListener(sessionStateChangedEvent, blockSession);
  countdownId = window.setInterval(updateCountdown, 1000);
  // Read the authoritative expiry without renewing an idle session.
  void checkSessionStatus();

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
  window.removeEventListener(sessionExpiryChangedEvent, updateExpiry);
  window.removeEventListener(sessionStateChangedEvent, blockSession);
  if (countdownId !== undefined) window.clearInterval(countdownId);
  countdownId = undefined;
  expiresAt = undefined;
  sessionRemainingSeconds.value = null;
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
