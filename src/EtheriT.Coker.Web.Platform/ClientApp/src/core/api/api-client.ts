import { ApiError, type ApiFieldErrors } from "@/core/api/api-error";
import { platformFetch } from "@/services/http-client";

export interface ApiRequestContext {
  url: string;
  method: string;
  requestId: string;
}

export interface ApiHooks {
  beforeRequest?(context: ApiRequestContext): void | Promise<void>;
  afterResponse?(context: ApiRequestContext, response: Response): void | Promise<void>;
  onError?(context: ApiRequestContext, error: unknown): void | Promise<void>;
}

export interface ApiRequestOptions
  extends Omit<RequestInit, "method" | "body"> {
  query?: Record<string, unknown>;
  body?: unknown;
  timeoutMs?: number;
  acceptFailureEnvelope?: boolean;
}

const hooks = new Set<ApiHooks>();

export function registerApiHooks(apiHooks: ApiHooks): () => void {
  hooks.add(apiHooks);
  return () => hooks.delete(apiHooks);
}

function createUrl(path: string, query?: Record<string, unknown>): string {
  const url = new URL(path, window.location.origin);
  if (url.origin !== window.location.origin || !url.pathname.startsWith("/api/")) {
    throw new Error(`Platform API 僅允許呼叫同來源 /api/*：${path}`);
  }

  for (const [key, rawValue] of Object.entries(query ?? {})) {
    if (rawValue === null || rawValue === undefined || rawValue === "") continue;
    const values = Array.isArray(rawValue) ? rawValue : [rawValue];
    for (const value of values) url.searchParams.append(key, String(value));
  }

  return `${url.pathname}${url.search}`;
}

async function parseBody(response: Response): Promise<unknown> {
  if (response.status === 204) return null;

  const text = await response.text();
  if (!text) return null;

  const contentType = response.headers.get("content-type") ?? "";
  if (!contentType.includes("json")) return text;

  try {
    return JSON.parse(text) as unknown;
  }
  catch {
    return text;
  }
}

function readFieldErrors(body: unknown): ApiFieldErrors {
  if (!body || typeof body !== "object") return {};
  const source = (body as { Errors?: unknown; errors?: unknown }).Errors ??
    (body as { errors?: unknown }).errors;
  if (!source || typeof source !== "object") return {};

  const output: ApiFieldErrors = {};
  for (const [field, messages] of Object.entries(source)) {
    output[field] = Array.isArray(messages)
      ? messages.map(String)
      : [String(messages)];
  }
  return output;
}

function readMessage(body: unknown, fallback: string): string {
  if (!body || typeof body !== "object") return fallback;
  const value = body as Record<string, unknown>;
  return String(value.Error ?? value.error ?? value.Message ?? value.message ?? value.Title ?? value.title ?? fallback);
}

function isFailureEnvelope(body: unknown): boolean {
  if (!body || typeof body !== "object") return false;
  const value = body as Record<string, unknown>;
  return value.Success === false || value.success === false;
}

async function request<T>(
  method: string,
  path: string,
  options: ApiRequestOptions = {}
): Promise<T> {
  const {
    query,
    body: requestBody,
    timeoutMs = 30_000,
    acceptFailureEnvelope = false,
    ...requestInit
  } = options;
  const requestId = crypto.randomUUID();
  const url = createUrl(path, query);
  const context: ApiRequestContext = { url, method, requestId };
  const headers = new Headers(options.headers);
  headers.set("Accept", "application/json");
  headers.set("X-Requested-With", "XMLHttpRequest");
  headers.set("X-Coker-Request-Id", requestId);

  let body: BodyInit | undefined;
  if (requestBody instanceof FormData || requestBody instanceof URLSearchParams) {
    body = requestBody;
  }
  else if (requestBody !== undefined) {
    headers.set("Content-Type", "application/json; charset=utf-8");
    body = JSON.stringify(requestBody);
  }

  const controller = new AbortController();
  const timeoutId = window.setTimeout(
    () => controller.abort(new DOMException("API request timed out", "TimeoutError")),
    timeoutMs
  );
  const abortFromCaller = () => controller.abort(options.signal?.reason);
  options.signal?.addEventListener("abort", abortFromCaller, { once: true });

  try {
    for (const hook of hooks) await hook.beforeRequest?.(context);

    const response = await platformFetch(url, {
      ...requestInit,
      method,
      headers,
      body,
      signal: controller.signal
    } as RequestInit);
    for (const hook of hooks) await hook.afterResponse?.(context, response);

    const responseBody = await parseBody(response);
    if (!response.ok ||
        (!acceptFailureEnvelope && isFailureEnvelope(responseBody))) {
      throw new ApiError(
        readMessage(responseBody, `API 呼叫失敗 (${response.status})`),
        response.status,
        readFieldErrors(responseBody),
        responseBody
      );
    }

    return responseBody as T;
  }
  catch (error) {
    for (const hook of hooks) await hook.onError?.(context, error);
    throw error;
  }
  finally {
    window.clearTimeout(timeoutId);
    options.signal?.removeEventListener("abort", abortFromCaller);
  }
}

export const api = {
  get: <T>(path: string, options?: ApiRequestOptions) => request<T>("GET", path, options),
  post: <T>(path: string, body?: unknown, options?: ApiRequestOptions) =>
    request<T>("POST", path, { ...options, body }),
  put: <T>(path: string, body?: unknown, options?: ApiRequestOptions) =>
    request<T>("PUT", path, { ...options, body }),
  patch: <T>(path: string, body?: unknown, options?: ApiRequestOptions) =>
    request<T>("PATCH", path, { ...options, body }),
  delete: <T>(path: string, options?: ApiRequestOptions) => request<T>("DELETE", path, options)
};
