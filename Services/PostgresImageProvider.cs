using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Providers;
using SixLabors.ImageSharp.Web.Resolvers;

namespace babe_algorithms.Services;

public class PostgresImageProvider : IImageProvider
{
    /// <summary>
    /// You can't pass in the AppDbContext directly here because we need that to be allocated
    /// per-request. If you try to re-use the same appdbcontext for everything,
    /// a memory leak will occur.
    /// </summary>
    /// <param name="sp"></param>
    public PostgresImageProvider(IServiceProvider sp)
    {
        this.ServiceProvider = sp;
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

    public IServiceProvider ServiceProvider { get; }

    public Task<IImageResolver> GetAsync(HttpContext context)
    {
        var appDbContext = this.ServiceProvider.CreateAsyncScope().ServiceProvider.GetService<ApplicationDbContext>();
        var value = context.Request.Path.Value;
        var id = value.Split("/")[2];
        var resolver = new PostgresImageResolver(
                appDbContext,
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
            // var _image = await SixLabors.ImageSharp.Image.LoadAsync(
            //     new MemoryStream(image.Data), new JpegDecoder());
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
