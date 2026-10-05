// Builds one row of a Gherkin data table as a Field -> Value dictionary,
// e.g. Row(("Field", "Given Name"), ("Value", "John")).

namespace EmergencyCareSystem.SeleniumTests.Support;

public static class Rows
{
    public static Dictionary<string, string> Row(params (string Key, string Value)[] pairs) =>
        pairs.ToDictionary(p => p.Key, p => p.Value);
}
