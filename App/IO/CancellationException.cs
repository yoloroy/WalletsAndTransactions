namespace App.IO;

public sealed class CancellationException : Exception
{
    // ReSharper disable UnusedMember.Global
    public CancellationException() {}

    public CancellationException(string message) : base(message) {}

    public CancellationException(string message, Exception innerException) : base(message, innerException) {}
    // ReSharper restore UnusedMember.Global
}