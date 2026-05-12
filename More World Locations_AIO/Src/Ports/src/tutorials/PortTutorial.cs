using System.Collections.Generic;

namespace More_World_Locations_AIO.tutorials;

public class PortTutorial
{
    internal static readonly List<PortTutorial> tutorials = new();
    public readonly string text;
    public readonly string label;

    private PortTutorial(string label, string text)
    {
        this.label = label;
        this.text = text;
        tutorials.Add(this);
    }
    
    public static void Setup()
    {
        tutorials.Clear();
        new PortTutorial(LocalKeys.TutorialIntroduction, LocalKeys.TutorialTextIntroduction);
        new PortTutorial(LocalKeys.Port, LocalKeys.TutorialTextPort);
        new PortTutorial(LocalKeys.Manifest, LocalKeys.TutorialTextManifest);
        new PortTutorial(LocalKeys.ShipmentSingular, LocalKeys.TutorialTextShipment);
        new PortTutorial(LocalKeys.DeliverySingular, LocalKeys.TutorialTextDelivery);
        new PortTutorial(LocalKeys.Teleport, LocalKeys.TutorialTextTeleport);
    }
}
