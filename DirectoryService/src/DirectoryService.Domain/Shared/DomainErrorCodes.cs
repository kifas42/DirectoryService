using Shared;

namespace DirectoryService.Domain.Shared;

public static class DomainErrorCodes
{
    public static class Validation
    {
        public static readonly ErrorCode InvalidPostalCodeFormat = new("validation.postal_code.invalid_format");
        public static readonly ErrorCode InvalidIdentifierFormat = new("validation.identifier.invalid_format");
        public static readonly ErrorCode InvalidTimezone = new("validation.timezone.invalid_format");
    }

    public static class Department
    {
        public static readonly ErrorCode NotFound = new("department.not_found");
        public static readonly ErrorCode IdentifierConflict = new("department.identifier.conflict");
        public static readonly ErrorCode InvalidLocationReference = new("department.location.not_found");

        public static readonly ErrorCode SelfReferenceParent = new("department.parent.self_reference");
        public static readonly ErrorCode CyclicReference = new("department.hierarchy.cyclic_reference");

        public static readonly ErrorCode PathUpdateFailed = new("department.update.path_failed");
        public static readonly ErrorCode LockFailed = new("department.lock.failed");
        public static readonly ErrorCode HierarchyUpdateFailed = new("department.update.hierarchy_failed");

        public static readonly ErrorCode AlreadyDeleted = new("department.already_deleted");
    }

    public static class Location
    {
        public static readonly ErrorCode NotFound = new("location.not_found");
        public static readonly ErrorCode NameConflict = new("location.name.conflict");
        public static readonly ErrorCode AddressConflict = new("location.address.conflict");
        public static readonly ErrorCode OrphanDeleteFailed = new("location.delete.orphans_failed");
    }

    public static class Position
    {
        public static readonly ErrorCode NameConflict = new("position.name.conflict");
        public static readonly ErrorCode OrphanDeleteFailed = new("position.delete.orphans_failed");
    }
}