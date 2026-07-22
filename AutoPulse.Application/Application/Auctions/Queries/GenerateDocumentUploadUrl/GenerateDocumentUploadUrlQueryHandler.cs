using AutoPulse.Domain.Interfaces.Storage;
using MediatR;

namespace AutoPulse.Application.Application.Auctions.Queries.GenerateDocumentUploadUrl
{
    public class GenerateDocumentUploadUrlQueryHandler : IRequestHandler<GenerateDocumentUploadUrlQuery, PreSignedUrlResponseDto>
    {
        private readonly IBlobStorageService _blobStorageService;

        public GenerateDocumentUploadUrlQueryHandler(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        public async Task<PreSignedUrlResponseDto> Handle(GenerateDocumentUploadUrlQuery request, CancellationToken cancellationToken)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{request.FileName}";
            var uploadUrl = await _blobStorageService.GeneratePreSignedUploadUrlAsync(
                uniqueFileName,
                request.ContentType,
                TimeSpan.FromMinutes(15)
            );

            return new PreSignedUrlResponseDto(uploadUrl, uniqueFileName);
        }
    }
}
