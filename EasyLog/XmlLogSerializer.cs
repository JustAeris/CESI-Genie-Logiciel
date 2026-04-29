using System.Xml.Linq;

namespace EasyLog;

// Concrete Strategy (GoF) for XML serialization.
public class XmlLogSerializer : ILogSerializer
{
    public string FileExtension => ".xml";

    public string Serialize(IEnumerable<LogEntry> entries)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("Logs",
                entries.Select(e => new XElement("LogEntry",
                    new XElement("Name", e.Name),
                    new XElement("FileSource", e.FileSource),
                    new XElement("FileTarget", e.FileTarget),
                    new XElement("FileSize", e.FileSize),
                    new XElement("FileTransferTime", e.FileTransferTime),
                    new XElement("time", e.Timestamp),
                    new XElement("EncryptionTime", e.EncryptionTime)
                ))
            )
        );
        return doc.ToString();
    }

    public IEnumerable<LogEntry> Deserialize(string raw)
    {
        var doc = XDocument.Parse(raw);
        return doc.Root?.Elements("LogEntry").Select(e => new LogEntry
        {
            Name = (string?)e.Element("Name") ?? "",
            FileSource = (string?)e.Element("FileSource") ?? "",
            FileTarget = (string?)e.Element("FileTarget") ?? "",
            FileSize = (long?)e.Element("FileSize") ?? 0,
            FileTransferTime = (double?)e.Element("FileTransferTime") ?? 0,
            Timestamp = (string?)e.Element("time") ?? "",
            EncryptionTime = (long?)e.Element("EncryptionTime") ?? 0
        }) ?? [];
    }
}
