using Microsoft.AspNetCore.JsonPatch;

namespace SteamApp.Infrastructure.DTOs.Product
{
    public class UpdateProductDto
    {
        public long Id { get; set; }

        public JsonPatchDocument<ProductForPatchDto> PatchDoc { get; set; }
    }
}
