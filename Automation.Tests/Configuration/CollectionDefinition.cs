using CommonSpirit.Automation.Base.Core;
using Xunit;

namespace CommonSpirit.Automation.PCDE.CollectionDefinition
{
    [CollectionDefinition("XUnit Collection")]
    public class CollectionDefinition : ICollectionFixture<OneTimeSetup>
    {
        // A class with no code, only used to define the collection
    }
}
