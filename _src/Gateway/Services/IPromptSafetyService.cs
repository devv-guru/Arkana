using System.Text.Json;
using System.Text.RegularExpressions;

namespace Gateway.Services;

public interface IPromptSafetyService
{
    PromptSafetyResult AnalyzeContent(string content);
}

public record PromptSafetyResult(
    bool IsSafe,
    double RiskScore,
    string[] DetectedPatterns,
    string[] BlockingReasons
);

public class PromptSafetyService : IPromptSafetyService
{
    private readonly ILogger<PromptSafetyService> _logger;
    private const double DefaultRiskThreshold = 0.7;

    private static readonly Dictionary<string, (Regex Pattern, double Weight)> DetectionPatterns = new()
    {
        ["instruction_override"] = (
            new(@"\b(ignore|forget|disregard|override)\s+(previous|all|earlier|above)\s+(instructions?|prompts?|rules?|commands?)", 
                RegexOptions.IgnoreCase | RegexOptions.Compiled), 
            0.8
        ),
        ["role_manipulation"] = (
            new(@"\b(you\s+are\s+now|act\s+as|pretend\s+to\s+be|roleplay\s+as|become\s+a)\s+", 
                RegexOptions.IgnoreCase | RegexOptions.Compiled), 
            0.7
        ),
        ["system_extraction"] = (
            new(@"\b(what\s+(is|are)\s+your\s+(instructions?|system\s+prompt|guidelines?|rules?)|show\s+me\s+your\s+prompt)", 
                RegexOptions.IgnoreCase | RegexOptions.Compiled), 
            0.9
        ),
        ["jailbreak_attempt"] = (
            new(@"\b(DAN|jailbreak|bypass|unrestricted|uncensored|unlimited)\b", 
                RegexOptions.IgnoreCase | RegexOptions.Compiled), 
            0.8
        ),
        ["delimiter_attack"] = (
            new(@"[""'`]{3,}|^\s*[-=]{5,}|^\s*[#*]{3,}|---+|===+", 
                RegexOptions.Multiline | RegexOptions.Compiled), 
            0.6
        ),
        ["encoding_attempt"] = (
            new(@"\\x[0-9a-f]{2}|%[0-9a-f]{2}|&#\d+;|\\u[0-9a-f]{4}", 
                RegexOptions.IgnoreCase | RegexOptions.Compiled), 
            0.5
        )
    };

    public PromptSafetyService(ILogger<PromptSafetyService> logger)
    {
        _logger = logger;
    }

    public PromptSafetyResult AnalyzeContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new PromptSafetyResult(true, 0.0, [], []);

        var detectedPatterns = new List<string>();
        var totalRisk = 0.0;

        // Pattern-based detection
        foreach (var (patternName, (pattern, weight)) in DetectionPatterns)
        {
            var matches = pattern.Matches(content);
            if (matches.Count > 0)
            {
                detectedPatterns.Add(patternName);
                var patternRisk = weight * Math.Min(matches.Count, 3) / 3.0; // Cap at 3 matches
                totalRisk += patternRisk;
                
                _logger.LogDebug("Detected pattern {Pattern} with {Count} matches, risk: {Risk:F2}", 
                    patternName, matches.Count, patternRisk);
            }
        }

        // Length-based risk analysis
        var lengthRisk = AnalyzeLengthRisk(content);
        totalRisk += lengthRisk;

        // Entropy analysis (randomness detection)
        var entropyRisk = AnalyzeEntropyRisk(content);
        totalRisk += entropyRisk;

        // Final risk calculation
        var finalRiskScore = Math.Min(totalRisk, 1.0);
        var isSafe = finalRiskScore < DefaultRiskThreshold;

        var blockingReasons = new List<string>();
        if (!isSafe)
        {
            blockingReasons.Add($"Risk score {finalRiskScore:F2} exceeds threshold {DefaultRiskThreshold:F2}");
            blockingReasons.AddRange(detectedPatterns.Select(p => $"Pattern: {p}"));
        }

        _logger.LogInformation("Prompt safety analysis: Risk={Risk:F2}, Safe={Safe}, Patterns=[{Patterns}]",
            finalRiskScore, isSafe, string.Join(", ", detectedPatterns));

        return new PromptSafetyResult(
            isSafe,
            finalRiskScore,
            detectedPatterns.ToArray(),
            blockingReasons.ToArray()
        );
    }

    private static double AnalyzeLengthRisk(string content)
    {
        return content.Length switch
        {
            > 50000 => 0.4,
            > 20000 => 0.3,
            > 10000 => 0.2,
            > 5000 => 0.1,
            _ => 0.0
        };
    }

    private static double AnalyzeEntropyRisk(string content)
    {
        if (content.Length < 10) return 0.0;

        var entropy = CalculateEntropy(content);
        return entropy switch
        {
            > 5.0 => 0.3,
            > 4.5 => 0.2,
            > 4.0 => 0.1,
            _ => 0.0
        };
    }

    private static double CalculateEntropy(string text)
    {
        var frequency = text.GroupBy(c => c)
                           .ToDictionary(g => g.Key, g => (double)g.Count() / text.Length);
        
        return -frequency.Values.Sum(p => p * Math.Log2(p));
    }
}