namespace Shared;

public static class SharedErrorCodes
{
    public static class Validation
    {
        public static readonly ErrorCode Required = new("validation.required");
        public static readonly ErrorCode InvalidFormat = new("validation.invalid_format");
        public static readonly ErrorCode Duplicate = new("validation.duplicate");
        public static readonly ErrorCode InvalidRequest = new("validation.request.invalid");

        public static readonly ErrorCode StringTooShort = new("validation.string.too_short");
        public static readonly ErrorCode StringTooLong = new("validation.string.too_long");
        public static readonly ErrorCode StringLengthOutOfBounds = new("validation.string.length_out_of_bounds");
    }

    public static class System
    {
        public static readonly ErrorCode UnexpectedError = new("system.unexpected_error");

        public static readonly ErrorCode OperationCanceled = new("system.operation.canceled");
        public static readonly ErrorCode ExternalServiceTimeout = new("system.external_service.timeout");

        public static class Database
        {
            public static readonly ErrorCode OperationFailed = new("system.database.operation_failed");

            public static readonly ErrorCode TransactionFailed = new("system.database.transaction_failed");

            public static readonly ErrorCode SaveChangesFailed = new("system.database.save_failed");
        }
    }
}