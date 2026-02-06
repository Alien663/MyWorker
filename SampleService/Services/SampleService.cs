using SampleService1.Interfaces;
using SampleService1.Entities;

namespace SampleService1.Services;

public class SampleService: ISampleService
{
    private readonly SampleContext _dbContext;

    public SampleService(SampleContext dbContext)
    {
        _dbContext = dbContext;
    }
}
