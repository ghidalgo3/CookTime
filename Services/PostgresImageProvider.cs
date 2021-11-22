using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;

namespace babe_algorithms.Services;

public class PostgresImageProvider : IImageProvider
{
    public ApplicationDbContext Context { get; }

    public PostgresImageProvider(ApplicationDbContext context)
    {
        this.Context = context;
    }
    /// <summary>
    /// A match function used by the resolver to identify itself as the correct resolver to use.
    /// </summary>
    private Func<HttpContext, bool> _match;

    public ProcessingBehavior ProcessingBehavior => ProcessingBehavior.All;
    public Func<HttpContext, bool> Match
        {
            get => _match ?? IsValidRequest;
            set => _match = value;
        }


    public Task<IImageResolver> GetAsync(HttpContext context)
    {
        var value = context.Request.Path.Value;
        var id = value.Split("/")[2];
        var resolver = new PostgresImageResolver(
                this.Context,
                Guid.Parse(id)) as IImageResolver;
        return Task.FromResult(resolver);
    }

    public bool IsValidRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/image");
    }
}

public class PostgresImageResolver : IImageResolver
{
    public PostgresImageResolver(ApplicationDbContext context, Guid imageId)
    {
        this.ImageId = imageId;
        this.Context = context;
    }

    public Guid ImageId { get; }

    public ApplicationDbContext Context { get; }

    public async Task<ImageMetadata> GetMetaDataAsync()
    {
        var image = await this.Context.Images.FindAsync(this.ImageId);
        if (image != null)
        {
            var _image = await SixLabors.ImageSharp.Image.LoadAsync(
                new MemoryStream(image.Data), new JpegDecoder());
            return new ImageMetadata(DateTime.UtcNow, TimeSpan.FromHours(1), image.Data.Length);
        }
        else
        {
            return new ImageMetadata();
        }
    }

    public async Task<Stream> OpenReadAsync()
    {
        var image = await this.Context.Images.FindAsync(this.ImageId);
        if (image != null)
        {
            return new MemoryStream(image.Data);
        }
        else
        {
            return new MemoryStream();
        }
    }
}
