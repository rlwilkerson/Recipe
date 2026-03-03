using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.Tests.TestHelpers;

public static class DbContextHelper
{
    public static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
