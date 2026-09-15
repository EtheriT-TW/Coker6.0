import { api } from "@/core/api/api-client";

export async function recordPlatformLocation(path: string): Promise<void> {
  try {
    await api.post<void>("/api/navigation-preference", { Path: path });
  }
  catch (error) {
    // Navigation tracking must never block normal page usage.
    console.warn("Unable to record the last Platform location.", error);
  }
}
