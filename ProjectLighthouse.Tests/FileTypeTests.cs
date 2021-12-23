using System;
using System.IO;
using System.Text;
using LBPUnion.ProjectLighthouse.Types.Files;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests
{
    public class FileTypeTests
    {
        [Fact]
        public void ShouldRecognizeLevel()
        {
            LbpFile file = new(File.ReadAllBytes("ExampleFiles/TestLevel.lvl"));
            Assert.True(file.FileType == LbpFileType.Level);
        }

        [Fact]
        public void ShouldRecognizeScript()
        {
            LbpFile file = new(File.ReadAllBytes("ExampleFiles/TestScript.ff"));
            Assert.True(file.FileType == LbpFileType.Script);
        }

        [Fact]
        public void ShouldRecognizeTexture()
        {
            LbpFile file = new(File.ReadAllBytes("ExampleFiles/TestTexture.tex"));
            Assert.True(file.FileType == LbpFileType.Texture);
        }

        [Fact]
        public void ShouldRecognizeFileArchive()
        {
            LbpFile file = new(File.ReadAllBytes("ExampleFiles/TestFarc.farc"));
            Assert.True(file.FileType == LbpFileType.FileArchive);
        }

        [Fact]
        public void ShouldNotRecognizeFileArchiveAsScript()
        {
            LbpFile file = new(File.ReadAllBytes("ExampleFiles/TestFarc.farc"));
            Assert.False(file.FileType == LbpFileType.Script);
            Assert.True(file.FileType == LbpFileType.FileArchive);
        }

        [Fact]
        public void ShouldRecognizeNothingAsUnknown()
        {
            LbpFile file = new(Array.Empty<byte>());
            Assert.True(file.FileType == LbpFileType.Unknown);
        }

        [Fact]
        public void ShouldRecognizeGarbageAsUnknown()
        {
            LbpFile file = new(Encoding.ASCII.GetBytes("free pc only $900"));
            Assert.True(file.FileType == LbpFileType.Unknown);
        }
    }
}