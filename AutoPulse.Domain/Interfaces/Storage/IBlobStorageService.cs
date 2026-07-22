namespace AutoPulse.Domain.Interfaces.Storage
{
    public interface IBlobStorageService
    {
        Task<string> GeneratePreSignedUploadUrlAsync(string fileName, string contentType, TimeSpan expiresIn);
    }
}
