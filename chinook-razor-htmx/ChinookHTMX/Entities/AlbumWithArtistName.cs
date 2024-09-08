namespace ChinookHTMX.Entities;

public partial class AlbumWithArtistName
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int ArtistId { get; set; }

    public string? Name { get; set; }
}