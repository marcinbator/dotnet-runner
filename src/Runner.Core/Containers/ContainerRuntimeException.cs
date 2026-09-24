using System.Net;

namespace Runner.Core.Containers;

public sealed class ContainerRuntimeException(string message, HttpStatusCode? statusCode = null) : Exception(message)
{
  public HttpStatusCode? StatusCode { get; } = statusCode;
}
