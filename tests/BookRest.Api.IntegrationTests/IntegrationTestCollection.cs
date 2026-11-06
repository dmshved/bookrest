namespace BookRest.Api.IntegrationTests;

[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<BookRestWebApplicationFactory>
{
    public const string Name = "Integration";
}
