// Application/Features/Preview/Queries/ServePreviewFileQuery.cs
using MediatR;

namespace Qtemplate.Application.Features.Preview.Queries;

public class ServePreviewFileQuery : IRequest<PreviewFileResult>
{
    public Guid TemplateId { get; init; }
    public string FilePath { get; init; } = string.Empty;
}

public class PreviewFileResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public int StatusCode { get; init; } = 200;
    public byte[]? FileBytes { get; init; }
    public string? MimeType { get; init; }

    public static PreviewFileResult Success(byte[] bytes, string mime) =>
        new() { IsSuccess = true, FileBytes = bytes, MimeType = mime };

    public static PreviewFileResult NotFound(string error) =>
        new() { IsSuccess = false, Error = error, StatusCode = 404 };

    public static PreviewFileResult BadRequest(string error) =>
        new() { IsSuccess = false, Error = error, StatusCode = 400 };
}