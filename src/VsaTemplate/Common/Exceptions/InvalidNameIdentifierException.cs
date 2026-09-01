namespace VsaTemplate.Common.Exceptions;

public sealed class InvalidNameIdentifierException(string value)
    : Exception($"Invalid name identifier: {value}.");
