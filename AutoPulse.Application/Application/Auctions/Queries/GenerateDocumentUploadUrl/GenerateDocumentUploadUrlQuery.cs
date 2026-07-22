using MediatR;

namespace AutoPulse.Application.Application.Auctions.Queries.GenerateDocumentUploadUrl
{
    public record GenerateDocumentUploadUrlQuery(
        string FileName,
        string ContentType
    ) : IRequest<PreSignedUrlResponseDto>;

    public record PreSignedUrlResponseDto(
        string UploadUrl,
        string StorageKey
    );
}
