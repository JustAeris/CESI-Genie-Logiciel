using System.Text.Json;
using System.Xml.Linq;
using EasyLog;

namespace EasySave.Tests;

[Collection("Singletons")]
public class LogEntryTests
{
    private static LogEntry MakeEntry(long encryptionTime) => new()
    {
        Name = "job",
        FileSource = @"C:\src\file.txt",
        FileTarget = @"C:\dst\file.txt",
        FileSize = 512,
        FileTransferTime = 10.0,
        Timestamp = "2026-01-01 12:00:00",
        EncryptionTime = encryptionTime
    };

    // --- JSON ---

    [Theory]
    [InlineData(0)]
    [InlineData(150)]
    [InlineData(-1)]
    public void JsonSerializer_EncryptionTime_RoundTrips(long value)
    {
        var entry = MakeEntry(value);
        var serializer = new JsonLogSerializer();

        var json = serializer.Serialize([entry]);
        var result = serializer.Deserialize(json).First();

        Assert.Equal(value, result.EncryptionTime);
    }

    [Fact]
    public void JsonSerializer_EncryptionTime_Zero_PresentInJson()
    {
        var json = new JsonLogSerializer().Serialize([MakeEntry(0)]);
        Assert.Contains("EncryptionTime", json);
    }

    [Fact]
    public void JsonSerializer_AllFields_Persisted()
    {
        var entry = MakeEntry(99);
        var json = new JsonLogSerializer().Serialize([entry]);
        var result = new JsonLogSerializer().Deserialize(json).First();

        Assert.Equal("job", result.Name);
        Assert.Equal(512, result.FileSize);
        Assert.Equal(10.0, result.FileTransferTime);
        Assert.Equal("2026-01-01 12:00:00", result.Timestamp);
        Assert.Equal(99L, result.EncryptionTime);
    }

    // --- XML ---

    [Theory]
    [InlineData(0)]
    [InlineData(150)]
    [InlineData(-1)]
    public void XmlSerializer_EncryptionTime_RoundTrips(long value)
    {
        var entry = MakeEntry(value);
        var serializer = new XmlLogSerializer();

        var xml = serializer.Serialize([entry]);
        var result = serializer.Deserialize(xml).First();

        Assert.Equal(value, result.EncryptionTime);
    }

    [Fact]
    public void XmlSerializer_ProducesLogEntryElement()
    {
        var xml = new XmlLogSerializer().Serialize([MakeEntry(0)]);
        var doc = XDocument.Parse(xml);

        Assert.Equal("Logs", doc.Root?.Name.LocalName);
        Assert.Single(doc.Root!.Elements("LogEntry"));
    }

    [Fact]
    public void XmlSerializer_AllFields_Persisted()
    {
        var entry = MakeEntry(42);
        var xml = new XmlLogSerializer().Serialize([entry]);
        var result = new XmlLogSerializer().Deserialize(xml).First();

        Assert.Equal("job", result.Name);
        Assert.Equal(512, result.FileSize);
        Assert.Equal(10.0, result.FileTransferTime);
        Assert.Equal("2026-01-01 12:00:00", result.Timestamp);
        Assert.Equal(42L, result.EncryptionTime);
    }
}
