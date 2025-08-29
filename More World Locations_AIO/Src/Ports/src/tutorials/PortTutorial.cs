using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using More_World_Locations_AIO;

namespace More_World_Locations_AIO.tutorials;

public class PortTutorial
{
    private static readonly StringBuilder sb = new StringBuilder();

    internal static readonly List<PortTutorial> tutorials = new();
    public readonly string text;
    public readonly string label;
    
    private PortTutorial(string label, string resource) : this(label, LoadMarkdownFromAssembly(resource))
    {
    }

    private PortTutorial(string label, List<string> lines)
    {
        this.label = label;
        sb.Clear();

        foreach (string? line in lines)
        {
            sb.Append(line);
            sb.Append('\n');
        }
        text = sb.ToString();
        tutorials.Add(this);
    }
    
    public static void Setup()
    {
        PortTutorial portTab = new PortTutorial("Port", "port.md");
        PortTutorial manifestTab = new PortTutorial("Manifest", "manifest.md");
        PortTutorial shipmentTab = new PortTutorial("Shipment",  "shipment.md");
        PortTutorial deliveryTab = new PortTutorial("Delivery",  "delivery.md");
        PortTutorial teleportTab = new PortTutorial("Teleport", "teleport.md");
    }

    private static List<string> LoadMarkdownFromAssembly(string resourceName, string folder = "Src.Ports.src.tutorials")
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string path = $"{More_World_Locations_AIOPlugin.ModName}.{folder}.{resourceName}";
        using Stream? stream = assembly.GetManifestResourceStream(path);
        if (stream == null)
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found in assembly '{assembly.FullName}'.");

        using StreamReader reader = new StreamReader(stream);
        List<string> lines = new List<string>();
        while (!reader.EndOfStream)
        {
            lines.Add(reader.ReadLine() ?? string.Empty);
        }
        return lines;
    }
}