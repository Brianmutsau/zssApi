namespace WebApplication1.DTOS

    {
        public record BookCreateDto(string Title, string? Description, decimal Price, int CategoryId);
        public record BookReadDto(int Id, string Title, string? Description, decimal Price, int CategoryId, string CategoryTitle);
    }


