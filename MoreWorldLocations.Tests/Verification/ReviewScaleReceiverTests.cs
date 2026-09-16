using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.ServerOnly.Verification;
using UnityEngine;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

// ZNetView.Awake reads s_scaleHash only inside the RECEIVER's
// if (m_syncInitialScale). Changing the template's sender flag cannot enable it.
public class ReviewScaleReceiverTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void ConvertedScaleRequiresAStockReceiverThatReadsIt(bool receiverSyncs, bool expectedApproval)
    {
        var root = new GameObject("ReviewScaledSite");
        GameObject emitted = root.Child("wood_floor");
        emitted.AddComponent<ZNetView>().m_persistent = true;
        emitted.transform.localScale = new Vector3(1f, 1f, 1.05f);
        emitted.Child("mesh").AddComponent<BoxCollider>();

        var stock = new GameObject("wood_floor");
        ZNetView receiver = stock.AddComponent<ZNetView>();
        receiver.m_persistent = true;
        receiver.m_syncInitialScale = receiverSyncs;
        stock.Child("mesh").AddComponent<BoxCollider>();

        // Imported from the review unchanged except this line. The review asked
        // for the conversion to run only where the stock receiver can consume
        // it, so Apply now takes the stock lookup and converts nothing when the
        // receiver ignores scale: 1 where it reads one, 0 where it does not.
        // Both approval assertions below — the finding itself — are untouched.
        Assert.Equal(receiverSyncs ? 1 : 0,
            ScaleSyncConversion.Apply(root, name => name == "wood_floor" ? stock : null));
        Assert.Equal(receiverSyncs, receiver.m_syncInitialScale);
        TemplateEvaluation result = TemplatePolicy.Evaluate(
            TemplateFactsExtractor.Extract(root.name, "Meadows", root,
                stockPrefabOf: name => name == "wood_floor" ? stock : null),
            VerificationData.StockPrefabs, Fixtures.ExcludedPacks);
        Assert.Equal(expectedApproval, result.Approved);
    }
}
