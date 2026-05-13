using EasySave.CryptoSoft;

namespace EasySave.Tests;

public class XorEncryptorTests : IDisposable
{
    private readonly string _tempFile = Path.Combine(Path.GetTempPath(), $"cryptosoft_test_{Guid.NewGuid()}.bin");

    public void Dispose()
    {
        if (File.Exists(_tempFile)) File.Delete(_tempFile);
    }

    // Encrypting a file changes its content
    [Fact]
    public void EncryptFile_ChangesContent()
    {
        var original = new byte[] { 0x00, 0x01, 0xFF, 0xAB };
        File.WriteAllBytes(_tempFile, original);

        XorEncryptor.EncryptFile(_tempFile);

        var encrypted = File.ReadAllBytes(_tempFile);
        Assert.NotEqual(original, encrypted);
    }

    // Applying twice restores the original (XOR is symmetric)
    [Fact]
    public void EncryptFile_AppliedTwice_RestoresOriginal()
    {
        var original = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello"
        File.WriteAllBytes(_tempFile, original);

        XorEncryptor.EncryptFile(_tempFile);
        XorEncryptor.EncryptFile(_tempFile);

        var restored = File.ReadAllBytes(_tempFile);
        Assert.Equal(original, restored);
    }

    // Returns true on a valid file
    [Fact]
    public void EncryptFile_ValidFile_ReturnsTrue()
    {
        File.WriteAllBytes(_tempFile, new byte[] { 0x42 });

        bool result = XorEncryptor.EncryptFile(_tempFile);

        Assert.True(result);
    }

    // Returns false on a non-existent file
    [Fact]
    public void EncryptFile_NonExistentFile_ReturnsFalse()
    {
        bool result = XorEncryptor.EncryptFile("nonexistent_file_that_does_not_exist.bin");

        Assert.False(result);
    }

    // Empty file is handled without error
    [Fact]
    public void EncryptFile_EmptyFile_ReturnsTrueAndRemainsEmpty()
    {
        File.WriteAllBytes(_tempFile, []);

        bool result = XorEncryptor.EncryptFile(_tempFile);

        Assert.True(result);
        Assert.Empty(File.ReadAllBytes(_tempFile));
    }

    // Each byte is XOR'd with 0x55
    [Fact]
    public void EncryptFile_CorrectlyAppliesXorKey()
    {
        var original = new byte[] { 0x00, 0x55, 0xFF };
        File.WriteAllBytes(_tempFile, original);

        XorEncryptor.EncryptFile(_tempFile);

        var encrypted = File.ReadAllBytes(_tempFile);
        Assert.Equal(new byte[] { 0x55, 0x00, 0xAA }, encrypted);
    }
}
