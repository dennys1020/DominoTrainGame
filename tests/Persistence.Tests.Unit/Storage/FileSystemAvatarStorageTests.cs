using System;
using System.IO;
using NUnit.Framework;
using DominoTrainGame.Server.Persistence.Storage;

namespace DominoTrainGame.Server.Persistence.Tests.Unit.Storage;

[TestFixture]
public sealed class FileSystemAvatarStorageTests
{
    private const int GeneratedFileNameLength = 36;
    private const string GuidFolderFormat = "N";
    private const string PngFileExtension = ".png";
    private const string MissingFileName = "0123456789abcdef0123456789abcdef.png";
    private const string UnsupportedFileName = "0123456789abcdef0123456789abcdef.gif";
    private const string TraversalFileName = @"..\..\..\0123456789abcdef0123456.png";
    private const string EmptyText = "";
    private const string WhitespaceText = "   ";

    private static readonly byte[] _avatarImage;

    private string _avatarFolderPath;
    private FileSystemAvatarStorage _avatarStorage;
    private string _avatarFileName;
    private byte[] _imageBytes;

    static FileSystemAvatarStorageTests()
    {
        _avatarImage = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00 };
    }

    [SetUp]
    public void SetUp()
    {
        string folderName = Guid.NewGuid().ToString(GuidFolderFormat);
        _avatarFolderPath = Path.Combine(Path.GetTempPath(), folderName);
        CreateAvatarStorage();
        _avatarFileName = MissingFileName;
        _imageBytes = _avatarImage;
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_avatarFolderPath))
        {
            Directory.Delete(_avatarFolderPath, true);
        }
    }

    [TestCase(EmptyText)]
    [TestCase(WhitespaceText)]
    [TestCase(null)]
    public void Test_Constructor_BlankFolderPath_ThrowsArgumentException(string blankFolderPath)
    {
        _avatarFolderPath = blankFolderPath;

        Assert.Throws<ArgumentException>(CreateAvatarStorage);
    }

    [Test]
    public void Test_SaveAvatarImage_ValidImage_ReturnsGeneratedFileName()
    {
        string avatarFileName = _avatarStorage.SaveAvatarImage(_avatarImage, PngFileExtension);

        Assert.That(avatarFileName, Has.Length.EqualTo(GeneratedFileNameLength));
    }

    [Test]
    public void Test_SaveAvatarImage_ValidImage_WritesFileInAvatarFolder()
    {
        string avatarFileName = _avatarStorage.SaveAvatarImage(_avatarImage, PngFileExtension);

        bool fileExists = File.Exists(Path.Combine(_avatarFolderPath, avatarFileName));

        Assert.That(fileExists, Is.True);
    }

    [Test]
    public void Test_SaveAvatarImage_NullImage_ThrowsArgumentNullException()
    {
        _imageBytes = null;

        Assert.Throws<ArgumentNullException>(SaveArrangedImage);
    }

    [Test]
    public void Test_LoadAvatarImage_SavedImage_ReturnsSameBytes()
    {
        string avatarFileName = _avatarStorage.SaveAvatarImage(_avatarImage, PngFileExtension);

        byte[] loadedImage = _avatarStorage.LoadAvatarImage(avatarFileName);

        Assert.That(loadedImage, Is.EqualTo(_avatarImage));
    }

    [Test]
    public void Test_LoadAvatarImage_MissingFile_ReturnsEmptyImage()
    {
        byte[] loadedImage = _avatarStorage.LoadAvatarImage(MissingFileName);

        Assert.That(loadedImage, Is.Empty);
    }

    [TestCase(UnsupportedFileName)]
    [TestCase(TraversalFileName)]
    [TestCase(null)]
    public void Test_LoadAvatarImage_FileNameWithoutGeneratedFormat_ThrowsArgumentException(string fileName)
    {
        _avatarFileName = fileName;

        Assert.Throws<ArgumentException>(LoadArrangedImage);
    }

    [Test]
    public void Test_DeleteAvatarImage_SavedImage_RemovesFile()
    {
        string avatarFileName = _avatarStorage.SaveAvatarImage(_avatarImage, PngFileExtension);
        _avatarStorage.DeleteAvatarImage(avatarFileName);

        bool fileExists = File.Exists(Path.Combine(_avatarFolderPath, avatarFileName));

        Assert.That(fileExists, Is.False);
    }

    [Test]
    public void Test_DeleteAvatarImage_MissingFile_DoesNotThrow()
    {
        Assert.DoesNotThrow(DeleteArrangedImage);
    }

    [Test]
    public void Test_DeleteAvatarImage_FileNameWithoutGeneratedFormat_ThrowsArgumentException()
    {
        _avatarFileName = TraversalFileName;

        Assert.Throws<ArgumentException>(DeleteArrangedImage);
    }

    private void CreateAvatarStorage()
    {
        _avatarStorage = new FileSystemAvatarStorage(_avatarFolderPath);
    }

    private void SaveArrangedImage()
    {
        _avatarStorage.SaveAvatarImage(_imageBytes, PngFileExtension);
    }

    private void LoadArrangedImage()
    {
        _avatarStorage.LoadAvatarImage(_avatarFileName);
    }

    private void DeleteArrangedImage()
    {
        _avatarStorage.DeleteAvatarImage(_avatarFileName);
    }
}
