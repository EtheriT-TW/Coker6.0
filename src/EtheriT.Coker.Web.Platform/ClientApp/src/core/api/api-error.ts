export type ApiFieldErrors = Record<string, string[]>;

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly fieldErrors: ApiFieldErrors = {},
    public readonly responseBody: unknown = null
  ) {
    super(message);
    this.name = "ApiError";
  }
}
