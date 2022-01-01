using System;

namespace LeagueToolkit.Helpers.Exceptions;

public class InvalidFileSignatureException : Exception
{
    public InvalidFileSignatureException() : base("Invalid file signature")
    {
    }

    public InvalidFileSignatureException(string message) : base("Invalid file signature: " + message)
    {
    }
}

public class UnsupportedFileVersionException : Exception
{
    public UnsupportedFileVersionException() : base("Unsupported file Version")
    {
    }
}