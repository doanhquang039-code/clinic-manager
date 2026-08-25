using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MyMvcApp.Services
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file);
    }
}
