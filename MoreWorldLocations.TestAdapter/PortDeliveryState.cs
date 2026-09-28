using System;
using System.Reflection;
namespace MoreWorldLocations.TestAdapter;

// Older MWL stores this flag on Port; current MWL persists it on the port ZDO.
// A missing observation remains unknown rather than being treated as false.
internal static class PortDeliveryState
{
    internal static bool? Read(object port, FieldInfo? legacyField, Func<bool?> readSaved) =>
        legacyField != null ? legacyField.GetValue(port) is bool value ? (bool?)value : null : readSaved();

    // ZDO.GetBool returns the caller's default for an absent key, so reading with
    // both defaults distinguishes a stored value from absence.
    internal static bool? ReadSaved(Func<bool, bool> readWithDefault)
    {
        bool whenAbsentFalse = readWithDefault(false);
        bool whenAbsentTrue = readWithDefault(true);
        return whenAbsentFalse == whenAbsentTrue ? whenAbsentFalse : null;
    }
}
