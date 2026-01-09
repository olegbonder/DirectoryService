export type ApiError = {
  code: string;
  message: string;
  invalidField?: string | null;
  type: ErrorType;
};

export type ErrorType =
  | "validation"
  | "notFound"
  | "conflict"
  | "unauthorized"
  | "forbidden"
  | "serverError";

export class EnvelopeError extends Error {
  public readonly apiErrors: ApiError[];

  constructor(apiErrors: ApiError[]) {
    const firstMessage = apiErrors[0]?.message || "Неизвестная ошибка";

    super(firstMessage);
    this.apiErrors = apiErrors;

    Object.setPrototypeOf(this, EnvelopeError.prototype);
  }

  get errors(): ApiError[] {
    return this.apiErrors;
  }

  get firstMessage(): string {
    return this.apiErrors[0]?.message || "Неизвестная ошибка";
  }

  getAllMessages(): string {
    return this.apiErrors.map((error) => error.message).join(", ");
  }
}

export function isEnvelopeError(error: unknown): error is EnvelopeError {
  return error instanceof EnvelopeError;
}
