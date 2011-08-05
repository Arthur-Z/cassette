﻿using System.IO;
using Moq;
using Xunit;

namespace Cassette
{
    public class ParseCoffeeScriptReferences_Tests
    {
        [Fact]
        public void ProcessAddsReferencesToCoffeeScriptAssetInModule()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("c:\\asset.coffee");

            var coffeeScriptSource = @"
# reference ""another1.js""
# reference 'another2.coffee'
# reference ""/another3.coffee""

class Foo
";
            asset.Setup(a => a.OpenStream())
                 .Returns(CreateStream(coffeeScriptSource));
            var module = new Module("c:\\");
            module.Assets.Add(asset.Object);

            var processor = new ParseCoffeeScriptReferences();
            processor.Process(module);

            asset.Verify(a => a.AddReference("another1.js"));
            asset.Verify(a => a.AddReference("another2.coffee"));
            asset.Verify(a => a.AddReference("/another3.coffee"));
        }

        Stream CreateStream(string text)
        {
            var source = new MemoryStream();
            var writer = new StreamWriter(source);
            writer.Write(text);
            writer.Flush();
            source.Position = 0;
            return source;
        }
    }
}
