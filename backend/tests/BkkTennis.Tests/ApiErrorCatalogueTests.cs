using System.Text.RegularExpressions;

namespace BkkTennis.Tests;

/// <summary>
/// Keeps the player app's `apiError` i18n catalogue (apps/player/src/i18n/{th,en}.ts) in step with the error codes the API can emit.
/// The API side is scanned from source (no hard-coded list), so adding a code without translating it fails here.
/// </summary>
public class ApiErrorCatalogueTests
{
    // ---- scanners (pure, unit-tested below) ----

    /// <summary>`new BookingException(<status>, "code", ...)`, tolerant of line breaks and any status expression.</summary>
    private static readonly Regex BookingExceptionRx = new(@"new\s+BookingException\(\s*[^,""]+,\s*""(?<c>[a-z][a-z0-9_]*)""", RegexOptions.Compiled);
    /// <summary>Anonymous error bodies: `new { code = "x", message = ... }` (also `Code =`-less variants with any spacing/newlines).</summary>
    private static readonly Regex AnonBodyRx = new(@"\bcode\s*=\s*""(?<c>[a-z][a-z0-9_]*)""", RegexOptions.Compiled);
    /// <summary>Switch arms of `ApiErrors.For`: `... => ("code", "message")`.</summary>
    private static readonly Regex ForArmRx = new(@"=>\s*\(\s*""(?<c>[a-z][a-z0-9_]*)""\s*,", RegexOptions.Compiled);

    public static SortedSet<string> ScanSource(string cs)
    {
        var set = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var rx in new[] { BookingExceptionRx, AnonBodyRx, ForArmRx })
            foreach (Match m in rx.Matches(cs)) set.Add(m.Groups["c"].Value);
        return set;
    }

    /// <summary>Entries of the top-level `apiError: { key: 'text', ... }` block of a th.ts / en.ts file.</summary>
    public static Dictionary<string, string> ParseCatalogue(string ts)
    {
        var start = ts.IndexOf("apiError: {", StringComparison.Ordinal);
        Assert.True(start >= 0, "no `apiError: {` block found");
        var bodyStart = start + "apiError: {".Length;
        var end = Regex.Match(ts[bodyStart..], @"\n  \},?").Index; // closing brace of the block (2-space indent)
        var body = ts.Substring(bodyStart, end);
        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Match m in Regex.Matches(body, @"^\s*(?<k>[A-Za-z_0-9]+)\s*:\s*(?<q>['""`])(?<v>(?:\\.|(?!\k<q>).)*)\k<q>", RegexOptions.Multiline))
            Assert.True(dict.TryAdd(m.Groups["k"].Value, m.Groups["v"].Value), $"duplicate catalogue key {m.Groups["k"].Value}");
        return dict;
    }

    // ---- repo access ----

    private static readonly string Root = FindRoot();

    private static string FindRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "CLAUDE.md"))) dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("repo root (CLAUDE.md) not found above " + AppContext.BaseDirectory);
    }

    private static SortedSet<string> ApiCodes()
    {
        var all = new SortedSet<string>(StringComparer.Ordinal);
        var files = Directory.GetFiles(Path.Combine(Root, "backend", "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));
        foreach (var f in files) all.UnionWith(ScanSource(File.ReadAllText(f)));
        return all;
    }

    private static Dictionary<string, string> Catalogue(string lang) =>
        ParseCatalogue(File.ReadAllText(Path.Combine(Root, "apps", "player", "src", "i18n", $"{lang}.ts")));

    // ---- the real checks ----

    [Fact]
    public void Scanner_finds_the_codes_it_should()
    {
        var codes = ApiCodes();
        Assert.True(codes.Count >= 50, $"scanner found only {codes.Count} codes - regexes probably broke");
        foreach (var known in new[] { "slot_taken", "otp_invalid", "validation_failed", "not_found", "rate_limited", "server_error", "bad_phone", "invalid_credentials", "price_rule_ambiguous", "block_conflict" })
            Assert.Contains(known, codes);
    }

    [Theory]
    [InlineData("th")]
    [InlineData("en")]
    public void Every_api_error_code_has_a_catalogue_entry(string lang)
    {
        var cat = Catalogue(lang);
        var missing = ApiCodes().Where(c => !cat.ContainsKey(c)).ToList();
        Assert.True(missing.Count == 0, $"apiError in {lang}.ts is missing: {string.Join(", ", missing)}");
    }

    [Theory]
    [InlineData("th")]
    [InlineData("en")]
    public void Catalogue_has_no_stale_keys(string lang)
    {
        var codes = ApiCodes();
        var stale = Catalogue(lang).Keys.Where(k => !codes.Contains(k)).ToList();
        Assert.True(stale.Count == 0, $"apiError in {lang}.ts has keys the API never emits: {string.Join(", ", stale)}");
    }

    [Fact]
    public void Th_and_en_have_the_same_keys()
    {
        var th = Catalogue("th").Keys.ToHashSet();
        var en = Catalogue("en").Keys.ToHashSet();
        Assert.True(th.SetEquals(en), $"only in th: [{string.Join(", ", th.Except(en))}] only in en: [{string.Join(", ", en.Except(th))}]");
        Assert.True(th.Count >= 50);
    }

    [Theory]
    [InlineData("th")]
    [InlineData("en")]
    public void Catalogue_texts_are_not_empty(string lang)
    {
        var empty = Catalogue(lang).Where(kv => string.IsNullOrWhiteSpace(kv.Value)).Select(kv => kv.Key).ToList();
        Assert.True(empty.Count == 0, $"empty texts in {lang}.ts: {string.Join(", ", empty)}");
    }

    [Fact]
    public void English_texts_have_no_thai_and_thai_texts_have_thai()
    {
        var thai = new Regex("[฀-๿]");
        Assert.All(Catalogue("en"), kv => Assert.False(thai.IsMatch(kv.Value), $"en.{kv.Key} contains Thai"));
        Assert.All(Catalogue("th"), kv => Assert.True(thai.IsMatch(kv.Value), $"th.{kv.Key} has no Thai text"));
    }

    // ---- scanner self-tests (prove the check can fail) ----

    [Fact]
    public void ScanSource_handles_multiline_calls_interpolation_and_anonymous_bodies()
    {
        const string sample = """
            throw new BookingException(StatusCodes.Status409Conflict, "slot_taken", "x");
            throw new BookingException(
                409,
                "multi_line",
                $"interpolated {a} message");
            return Results.BadRequest(new { code = "bad_phone", message = "y" });
            return Results.Json(new
            {
                code   =   "spaced_out",
                message = "z"
            });
            StatusCodes.Status404NotFound => ("not_found", "ไม่พบ"),
            var s = "not_a_code"; var code = RandomNumberGenerator.GetInt32(0, 5);
            """;
        Assert.Equal(new[] { "bad_phone", "multi_line", "not_found", "slot_taken", "spaced_out" }, ScanSource(sample).ToArray());
    }

    [Fact]
    public void A_code_missing_from_the_catalogue_is_detected()
    {
        var emitted = ScanSource("""throw new BookingException(409, "brand_new_code", "m");""");
        var cat = ParseCatalogue("export const th = {\n  apiError: {\n    slot_taken: 'x',\n  },\n  common: {}\n}");
        Assert.Equal(new[] { "brand_new_code" }, emitted.Where(c => !cat.ContainsKey(c)).ToArray());
        Assert.Equal(new[] { "slot_taken" }, cat.Keys.Where(k => !emitted.Contains(k)).ToArray());
    }

    [Fact]
    public void ParseCatalogue_reads_quotes_apostrophes_and_stops_at_block_end()
    {
        var cat = ParseCatalogue("x = {\n  apiError: {\n    a: 'It\\'s',\n    b: \"two\",\n    c: 'curly ’',\n  },\n  common: {\n    d: 'no',\n  },\n}");
        Assert.Equal(new[] { "a", "b", "c" }, cat.Keys.ToArray());
        Assert.Equal("two", cat["b"]);
    }
}
