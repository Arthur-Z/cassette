using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class CassetteSettings_Tests
    {
        [Fact]
        public void ConstructorAppliesConfigurations()
        {
            var configA = new ConfigA();
            var configB = new ConfigB();
            
            var settings = new CassetteSettings(new IConfiguration<CassetteSettings>[] { configA, configB });

            configA.AssertWasCalled(settings);
            configB.AssertWasCalled(settings);
        }

        [Fact]
        public void ConstructorAppliesConfigurationsInOrderDefinedbyAttribute()
        {
            var configA = new ConfigA();
            var configB = new ConfigB();
            var settings = new CassetteSettings(new IConfiguration<CassetteSettings>[] { configB, configA });
         
            // Each config appends a letter to Version so we can test the order that they were called.
            settings.Version.ShouldEqual("AB");
        }

        [Fact]
        public void ConstructorAssignsPercompiledManifestFileToBeNonExistentInsteadOfNull()
        {
            var settings = new CassetteSettings(new IConfiguration<CassetteSettings>[0]);
            settings.PrecompiledManifestFile.ShouldBeType<NonExistentFile>();
        }

        [Fact]
        public void ConstructorHonoursPrecompiledManifestFileSetByConfiguration()
        {
            var file = Mock.Of<IFile>();
            var config = new Mock<IConfiguration<CassetteSettings>>();
            config
                .Setup(c => c.Configure(It.IsAny<CassetteSettings>()))
                .Callback<CassetteSettings>(s => s.PrecompiledManifestFile = file);

            var settings = new CassetteSettings(new[] { config.Object });
            settings.PrecompiledManifestFile.ShouldBeSameAs(file);
        }

        [ConfigurationOrder(1)]
        class ConfigA : TestableConfiguration
        {
            public override void Configure(CassetteSettings settings)
            {
                base.Configure(settings);
                settings.Version += "A";
            }
        }

        [ConfigurationOrder(2)]
        class ConfigB : TestableConfiguration
        {
            public override void Configure(CassetteSettings settings)
            {
                base.Configure(settings);
                settings.Version += "B";
            }
        }

        abstract class TestableConfiguration : IConfiguration<CassetteSettings>
        {
            CassetteSettings calledWithSettings;

            public virtual void Configure(CassetteSettings settings)
            {
                calledWithSettings = settings;
            }

            public void AssertWasCalled(CassetteSettings settings)
            {
                calledWithSettings.ShouldBeSameAs(settings);
            }
        }
    }
}