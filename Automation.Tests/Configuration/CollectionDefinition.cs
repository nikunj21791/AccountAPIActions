using Automation.Base.Core;
using Xunit;

namespace Automation.Tests.Configuration
{
    [CollectionDefinition("XUnit Collection")]
    public class CollectionDefinition : ICollectionFixture<OneTimeSetup>
    {
        // A class with no code, only used to define the collection
    }
}
