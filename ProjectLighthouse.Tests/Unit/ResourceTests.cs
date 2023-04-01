using System;
using System.IO;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Types.Resources;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests.Unit;

[Trait("Category", "Unit")]
public class ResourceTests
{
    [Fact]
    public void ShouldNotDeleteResourceFolder()
    {
        FileHelper.EnsureDirectoryCreated(FileHelper.ResourcePath);
        Assert.True(Directory.Exists(FileHelper.ResourcePath));
        FileHelper.DeleteResource(FileHelper.ResourcePath);
        Assert.True(Directory.Exists(FileHelper.ResourcePath));
    }

    [Fact]
    public void ShouldNotDeleteImagesFolder()
    {
        FileHelper.EnsureDirectoryCreated(FileHelper.ImagePath);
        Assert.True(Directory.Exists(FileHelper.ImagePath));
        FileHelper.DeleteResource(FileHelper.ImagePath);
        Assert.True(Directory.Exists(FileHelper.ImagePath));
    }

    [Fact]
    public void ShouldNotRecursivelyTraverseImage()
    {
        string path = Path.Combine(FileHelper.ImagePath, $"..{Path.DirectorySeparatorChar}appsettings.json");
        FileHelper.DeleteResource(path);
        Assert.True(File.Exists(Path.Combine(FileHelper.ImagePath, $"..{Path.DirectorySeparatorChar}appsettings.json")));
    }

    [Fact]
    public void ShouldNotRecursivelyTraverseResource()
    {
        string path = Path.Combine(FileHelper.ResourcePath, $"..{Path.DirectorySeparatorChar}appsettings.json");
        FileHelper.DeleteResource(path);
        Assert.True(File.Exists(Path.Combine(FileHelper.ResourcePath, $"..{Path.DirectorySeparatorChar}appsettings.json")));
    }

    [Fact]
    public async void ShouldDeleteResourceAndImage()
    {
        FileHelper.EnsureDirectoryCreated(FileHelper.ResourcePath);
        FileHelper.EnsureDirectoryCreated(FileHelper.ImagePath);
        string? hash = await FileHelper.ParseBase64Image("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8r8NQDwAFCQGsNA7jBAAAAABJRU5ErkJggg==");
        LbpFile? file = LbpFile.FromHash("ed4e2857a2e315e4487ea976d1b398f57b863ff4");
        Assert.True(file != null);
        // Convert resource to png
        FileHelper.LbpFileToPNG(file);

        Assert.True(hash != null);
        Assert.True(hash.Equals("ed4e2857a2e315e4487ea976d1b398f57b863ff4", StringComparison.InvariantCultureIgnoreCase));
        Assert.True(File.Exists(Path.Combine(FileHelper.ResourcePath, hash)));
        Assert.True(File.Exists(Path.Combine(FileHelper.ImagePath, $"{hash}.png")));

        FileHelper.DeleteResource(hash);
        Assert.False(File.Exists(Path.Combine(FileHelper.ResourcePath, hash)));
        Assert.False(File.Exists(Path.Combine(FileHelper.ImagePath, $"{hash}.png")));
    }
}