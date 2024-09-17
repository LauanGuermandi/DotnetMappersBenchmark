using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using BenchmarkDotNetVisualizer;
using Bogus;
using Mapster;

namespace DotnetMapperBenchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class BenchmarkContainer
{
    private BookDto _bookDto;
    private IMapper _autoMapper;
    private MapperlyMapper _mapperlyMapper;

    [GlobalSetup]
    public void Setup()
    {
        var faker = new Faker();
        _bookDto = new BookDto
        {
            Title = faker.Lorem.Sentence(),
            Author = new AuthorDto { Name = faker.Name.FullName() },
            PublishedDate = faker.Date.Past().ToString("yyyy-MM-dd"),
            ISBN = faker.Random.AlphaNumeric(13),
            Pages = faker.Random.Int(100, 1000),
            Publisher = faker.Company.CompanyName(),
            Genre = faker.Lorem.Word(),
            Price = faker.Finance.Amount(10, 100),
            IsEbook = faker.Random.Bool(),
            Language = faker.Locale,
            Rating = faker.Random.Double(0, 5)
        };

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BookDto, Book>();
            cfg.CreateMap<AuthorDto, Author>();
            cfg.CreateMap<Book, BookDto>();
        });

        _autoMapper = mapperConfig.CreateMapper();

        _mapperlyMapper = new MapperlyMapper();
    }

    [Benchmark]
    public void AgileMapper() => AgileObjects.AgileMapper.Mapper.Map(_bookDto).ToANew<Book>();

    [Benchmark]
    public void AutoMapper() => _autoMapper.Map<Book>(_bookDto);

    [Benchmark]
    public void ManualMapping() => _bookDto.Map();

    [Benchmark]
    public void Mapster() => _bookDto.Adapt<Book>();

    [Benchmark]
    public void Mapperly() => _mapperlyMapper.Map(_bookDto);
}

public class MapperlyMapper
{
    public Book Map(BookDto dto) => new Book
    {
        Title = dto.Title,
        Author = new Author { Name = dto.Author.Name },
        PublishedDate = dto.PublishedDate,
        ISBN = dto.ISBN,
        Pages = dto.Pages,
        Publisher = dto.Publisher,
        Genre = dto.Genre,
        Price = dto.Price,
        IsEbook = dto.IsEbook,
        Language = dto.Language,
        Rating = dto.Rating
    };
}

public class BookDto
{
    public string Title { get; set; }
    public AuthorDto Author { get; set; }
    public string PublishedDate { get; set; }
    public string ISBN { get; set; }
    public int Pages { get; set; }
    public string Publisher { get; set; }
    public string Genre { get; set; }
    public decimal Price { get; set; }
    public bool IsEbook { get; set; }
    public string Language { get; set; }
    public double Rating { get; set; }

    public Book Map() => new Book
    {
        Title = Title,
        Author = new Author { Name = Author.Name },
        PublishedDate = PublishedDate,
        ISBN = ISBN,
        Pages = Pages,
        Publisher = Publisher,
        Genre = Genre,
        Price = Price,
        IsEbook = IsEbook,
        Language = Language,
        Rating = Rating
    };
}

public class AuthorDto
{
    public string Name { get; set; }
}

public class Book
{
    public string Title { get; set; }
    public Author Author { get; set; }
    public string PublishedDate { get; set; }
    public string ISBN { get; set; }
    public int Pages { get; set; }
    public string Publisher { get; set; }
    public string Genre { get; set; }
    public decimal Price { get; set; }
    public bool IsEbook { get; set; }
    public string Language { get; set; }
    public double Rating { get; set; }
}

public class Author
{
    public string Name { get; set; }
}

public static class DirectoryHelper
{
    public static string GetPathRelativeToProjectDirectory(string fileName) => Path.Combine(AppContext.BaseDirectory, fileName);
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var config = DefaultConfig.Instance.WithOptions(ConfigOptions.DisableOptimizationsValidator);
        var summary1 = BenchmarkRunner.Run<BenchmarkContainer>(config);

        await summary1.SaveAsImageAsync(
            path: DirectoryHelper.GetPathRelativeToProjectDirectory("Benchmark.png"),
            options: new ReportHtmlOptions
            {
                Title = "Mappers battle Benchmark",
                HighlightGroups = true
            });
    }
}
