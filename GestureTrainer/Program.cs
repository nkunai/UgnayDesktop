using System.Globalization;
using System.Text.Json;

namespace GestureTrainer;

internal static class Program
{
    private const double TrainFraction = 0.8;

    public static int Main(string[] args)
    {
        var options = TrainerOptions.FromArgs(args);

        Console.WriteLine("=== Gesture Trainer ===");
        Console.WriteLine($"Dataset: {Path.GetFullPath(options.DatasetPath)}");

        if (!File.Exists(options.DatasetPath))
        {
            Console.WriteLine("Dataset file not found.");
            return 1;
        }

        var mode = options.Mode == TrainingMode.Auto
            ? DatasetInspector.DetectMode(options.DatasetPath)
            : options.Mode;

        var modelPath = options.ResolveModelPath(mode);
        var reportPath = options.ResolveReportPath(mode);

        Console.WriteLine($"Mode:    {mode}");
        Console.WriteLine($"Model:   {Path.GetFullPath(modelPath)}");
        Console.WriteLine($"Report:  {Path.GetFullPath(reportPath)}");

        var rows = mode == TrainingMode.Motion
            ? MotionDatasetLoader.Load(options.DatasetPath)
            : StaticDatasetLoader.Load(options.DatasetPath);

        var usableRows = rows
            .Where(r => string.Equals(r.CurationAction, "keep", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (usableRows.Count == 0)
        {
            Console.WriteLine("No usable rows found. Check curation_action values.");
            return 1;
        }

        var labelCounts = usableRows
            .GroupBy(r => r.Label)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var keptLabels = labelCounts
            .Where(x => x.Value >= options.MinSamplesPerLabel)
            .Select(x => x.Key)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        usableRows = usableRows
            .Where(r => keptLabels.Contains(r.Label))
            .ToList();

        if (keptLabels.Count < 2)
        {
            Console.WriteLine("Need at least 2 labels with enough samples to train.");
            return 1;
        }

        var featureNames = usableRows[0].FeatureNames;
        var featureCount = featureNames.Count;

        var split = DataSplitter.StratifiedSplit(usableRows, TrainFraction);
        if (split.Train.Count == 0 || split.Test.Count == 0)
        {
            Console.WriteLine("Dataset split failed. Add more samples per label.");
            return 1;
        }

        var scaler = StandardScaler.Fit(split.Train, featureCount);
        var model = NearestCentroidModel.Train(split.Train, scaler, featureNames);
        var evaluation = Evaluator.Evaluate(model, split.Test);

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(modelPath)) ?? Directory.GetCurrentDirectory());
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(reportPath)) ?? Directory.GetCurrentDirectory());

        ModelSerializer.Save(modelPath, model);
        File.WriteAllText(reportPath, ReportBuilder.Build(mode, model, split, evaluation));

        Console.WriteLine();
        Console.WriteLine($"Labels used: {model.Labels.Count}");
        Console.WriteLine($"Train {mode.GetItemLabel()}: {split.Train.Count}");
        Console.WriteLine($"Test {mode.GetItemLabel()}:  {split.Test.Count}");
        Console.WriteLine($"Accuracy:         {evaluation.Accuracy:P2}");
        Console.WriteLine();
        Console.WriteLine("Label counts:");
        foreach (var pair in labelCounts.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            var suffix = keptLabels.Contains(pair.Key) ? string.Empty : " (excluded)";
            Console.WriteLine($"- {pair.Key}: {pair.Value}{suffix}");
        }

        return 0;
    }
}

internal enum TrainingMode
{
    Auto,
    Static,
    Motion
}

internal static class TrainingModeExtensions
{
    public static string GetItemLabel(this TrainingMode mode) => mode switch
    {
        TrainingMode.Motion => "sequences",
        _ => "rows"
    };
}

internal sealed class TrainerOptions
{
    public required string DatasetPath { get; init; }
    public string? ModelPathOverride { get; init; }
    public string? ReportPathOverride { get; init; }
    public TrainingMode Mode { get; init; }
    public int MinSamplesPerLabel { get; init; }

    public string ResolveModelPath(TrainingMode mode)
        => !string.IsNullOrWhiteSpace(ModelPathOverride)
            ? ModelPathOverride
            : mode == TrainingMode.Motion
                ? @"GestureTrainer\artifacts\gesture_motion_model.json"
                : @"GestureTrainer\artifacts\gesture_static_model.json";

    public string ResolveReportPath(TrainingMode mode)
        => !string.IsNullOrWhiteSpace(ReportPathOverride)
            ? ReportPathOverride
            : mode == TrainingMode.Motion
                ? @"GestureTrainer\artifacts\motion_training_report.txt"
                : @"GestureTrainer\artifacts\training_report.txt";

    public static TrainerOptions FromArgs(string[] args)
    {
        var map = args
            .Select(x => x.Split('=', 2))
            .Where(x => x.Length == 2)
            .ToDictionary(x => x[0].TrimStart('-').ToLowerInvariant(), x => x[1]);

        return new TrainerOptions
        {
            DatasetPath = map.TryGetValue("dataset", out var dataset)
                ? dataset
                : @"GestureCollector\bin\Debug\net10.0\dataset.csv",
            ModelPathOverride = map.TryGetValue("model", out var model) ? model : null,
            ReportPathOverride = map.TryGetValue("report", out var report) ? report : null,
            Mode = map.TryGetValue("mode", out var mode)
                ? ParseMode(mode)
                : TrainingMode.Auto,
            MinSamplesPerLabel = map.TryGetValue("minsamples", out var minSamples) && int.TryParse(minSamples, out var value)
                ? Math.Max(2, value)
                : 3
        };
    }

    private static TrainingMode ParseMode(string value)
        => value.Trim().ToLowerInvariant() switch
        {
            "static" => TrainingMode.Static,
            "motion" => TrainingMode.Motion,
            "moving" => TrainingMode.Motion,
            _ => TrainingMode.Auto
        };
}

internal sealed class DatasetRow
{
    public required string Label { get; init; }
    public required List<string> FeatureNames { get; init; }
    public required double[] Features { get; init; }
    public required string CurationAction { get; init; }
}

internal static class DatasetInspector
{
    public static TrainingMode DetectMode(string path)
    {
        using var reader = new StreamReader(path);
        var headerLine = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            throw new InvalidOperationException("Dataset is empty.");
        }

        var headers = headerLine.Split(',').Select(h => h.Trim().Trim('"')).ToArray();
        var hasSequenceColumns = headers.Any(h => string.Equals(h, "sequence_id", StringComparison.OrdinalIgnoreCase))
            && headers.Any(h => string.Equals(h, "frame_index", StringComparison.OrdinalIgnoreCase));

        return hasSequenceColumns ? TrainingMode.Motion : TrainingMode.Static;
    }
}

internal static class StaticDatasetLoader
{
    private static readonly HashSet<string> IgnoredColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "label",
        "capture_mode",
        "hand_presence",
        "curation_action",
        "timestamp_unix_ms"
    };

    public static List<DatasetRow> Load(string path)
    {
        var lines = File.ReadAllLines(path);
        if (lines.Length < 2)
        {
            throw new InvalidOperationException("Dataset is empty.");
        }

        var headers = lines[0].Split(',').Select(h => h.Trim().Trim('"')).ToArray();
        var featureIndexes = new List<int>();
        var featureNames = new List<string>();
        var labelIndex = Array.FindIndex(headers, h => string.Equals(h, "label", StringComparison.OrdinalIgnoreCase));
        var curationIndex = Array.FindIndex(headers, h => string.Equals(h, "curation_action", StringComparison.OrdinalIgnoreCase));

        if (labelIndex < 0)
        {
            throw new InvalidOperationException("Dataset is missing the label column.");
        }

        for (var i = 0; i < headers.Length; i++)
        {
            var header = headers[i].Trim().Trim('"');
            if (IgnoredColumns.Contains(header))
            {
                continue;
            }

            featureIndexes.Add(i);
            featureNames.Add(header);
        }

        var rows = new List<DatasetRow>();
        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(',');
            if (parts.Length != headers.Length)
            {
                continue;
            }

            var label = parts[labelIndex].Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(label))
            {
                continue;
            }

            var curation = curationIndex >= 0 ? parts[curationIndex].Trim().Trim('"') : "keep";
            if (string.IsNullOrWhiteSpace(curation))
            {
                curation = "keep";
            }

            rows.Add(new DatasetRow
            {
                Label = label,
                FeatureNames = featureNames,
                Features = ParseFeatureValues(parts, featureIndexes),
                CurationAction = curation
            });
        }

        return rows;
    }

    private static double[] ParseFeatureValues(string[] parts, List<int> featureIndexes)
    {
        var values = new double[featureIndexes.Count];
        for (var j = 0; j < featureIndexes.Count; j++)
        {
            var raw = parts[featureIndexes[j]].Trim().Trim('"');
            values[j] = double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0d;
        }

        return values;
    }
}

internal static class MotionDatasetLoader
{
    private static readonly HashSet<string> IgnoredColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "sequence_id",
        "frame_index",
        "label",
        "capture_mode",
        "hand_presence",
        "curation_action",
        "timestamp_unix_ms"
    };

    private static readonly string[] AggregateSuffixes =
    {
        "_start",
        "_end",
        "_mean",
        "_min",
        "_max",
        "_delta",
        "_std"
    };

    public static List<DatasetRow> Load(string path)
    {
        using var reader = new StreamReader(path);
        var headerLine = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            throw new InvalidOperationException("Dataset is empty.");
        }

        var headers = headerLine.Split(',').Select(h => h.Trim().Trim('"')).ToArray();
        var sequenceIndex = Array.FindIndex(headers, h => string.Equals(h, "sequence_id", StringComparison.OrdinalIgnoreCase));
        var labelIndex = Array.FindIndex(headers, h => string.Equals(h, "label", StringComparison.OrdinalIgnoreCase));
        var curationIndex = Array.FindIndex(headers, h => string.Equals(h, "curation_action", StringComparison.OrdinalIgnoreCase));

        if (sequenceIndex < 0 || labelIndex < 0)
        {
            throw new InvalidOperationException("Motion dataset is missing sequence metadata.");
        }

        var featureIndexes = new List<int>();
        var baseFeatureNames = new List<string>();

        for (var i = 0; i < headers.Length; i++)
        {
            var header = headers[i].Trim().Trim('"');
            if (IgnoredColumns.Contains(header))
            {
                continue;
            }

            featureIndexes.Add(i);
            baseFeatureNames.Add(header);
        }

        var aggregateFeatureNames = new List<string>(baseFeatureNames.Count * AggregateSuffixes.Length);
        foreach (var baseName in baseFeatureNames)
        {
            foreach (var suffix in AggregateSuffixes)
            {
                aggregateFeatureNames.Add(baseName + suffix);
            }
        }

        var sequences = new Dictionary<string, MotionSequence>(StringComparer.OrdinalIgnoreCase);
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(',');
            if (parts.Length != headers.Length)
            {
                continue;
            }

            var label = parts[labelIndex].Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(label))
            {
                continue;
            }

            var sequenceId = parts[sequenceIndex].Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(sequenceId))
            {
                continue;
            }

            var curation = curationIndex >= 0 ? parts[curationIndex].Trim().Trim('"') : "keep";
            if (string.IsNullOrWhiteSpace(curation))
            {
                curation = "keep";
            }

            var key = $"{label}::{sequenceId}";
            if (!sequences.TryGetValue(key, out var sequence))
            {
                sequence = new MotionSequence(label, curation);
                sequences[key] = sequence;
            }

            sequence.Frames.Add(ParseFeatureValues(parts, featureIndexes));
        }

        return sequences.Values
            .Where(s => s.Frames.Count > 0)
            .Select(s => new DatasetRow
            {
                Label = s.Label,
                CurationAction = s.CurationAction,
                FeatureNames = aggregateFeatureNames,
                Features = BuildAggregateFeatures(s.Frames)
            })
            .ToList();
    }

    private static double[] ParseFeatureValues(string[] parts, List<int> featureIndexes)
    {
        var values = new double[featureIndexes.Count];
        for (var j = 0; j < featureIndexes.Count; j++)
        {
            var raw = parts[featureIndexes[j]].Trim().Trim('"');
            values[j] = double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0d;
        }

        return values;
    }

    private static double[] BuildAggregateFeatures(List<double[]> frames)
    {
        var featureCount = frames[0].Length;
        var output = new double[featureCount * AggregateSuffixes.Length];
        var offset = 0;

        for (var i = 0; i < featureCount; i++)
        {
            var values = frames.Select(frame => frame[i]).ToArray();
            var start = values[0];
            var end = values[^1];
            var mean = values.Average();
            var min = values.Min();
            var max = values.Max();
            var delta = end - start;
            var variance = values.Select(v => Math.Pow(v - mean, 2)).Average();
            var std = Math.Sqrt(variance);

            output[offset++] = start;
            output[offset++] = end;
            output[offset++] = mean;
            output[offset++] = min;
            output[offset++] = max;
            output[offset++] = delta;
            output[offset++] = std;
        }

        return output;
    }

    private sealed class MotionSequence(string label, string curationAction)
    {
        public string Label { get; } = label;
        public string CurationAction { get; } = curationAction;
        public List<double[]> Frames { get; } = [];
    }
}

internal static class DataSplitter
{
    public static (List<DatasetRow> Train, List<DatasetRow> Test) StratifiedSplit(List<DatasetRow> rows, double trainFraction)
    {
        var train = new List<DatasetRow>();
        var test = new List<DatasetRow>();
        var seed = 12345;

        foreach (var group in rows.GroupBy(r => r.Label).OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
        {
            var shuffled = group
                .OrderBy(_ => Random.Shared.Next(seed++))
                .ToList();

            var trainCount = (int)Math.Floor(shuffled.Count * trainFraction);
            trainCount = Math.Clamp(trainCount, 1, shuffled.Count - 1);

            train.AddRange(shuffled.Take(trainCount));
            test.AddRange(shuffled.Skip(trainCount));
        }

        return (train, test);
    }
}

internal sealed class StandardScaler
{
    public required double[] Means { get; init; }
    public required double[] Stds { get; init; }

    public static StandardScaler Fit(List<DatasetRow> rows, int featureCount)
    {
        var means = new double[featureCount];
        var stds = new double[featureCount];

        for (var i = 0; i < featureCount; i++)
        {
            means[i] = rows.Average(r => r.Features[i]);
            var variance = rows.Average(r => Math.Pow(r.Features[i] - means[i], 2));
            stds[i] = Math.Sqrt(variance);
            if (stds[i] < 1e-9)
            {
                stds[i] = 1d;
            }
        }

        return new StandardScaler
        {
            Means = means,
            Stds = stds
        };
    }

    public double[] Transform(double[] input)
    {
        var output = new double[input.Length];
        for (var i = 0; i < input.Length; i++)
        {
            output[i] = (input[i] - Means[i]) / Stds[i];
        }

        return output;
    }
}

internal sealed class NearestCentroidModel
{
    public required List<string> Labels { get; init; }
    public required List<string> FeatureNames { get; init; }
    public required StandardScaler Scaler { get; init; }
    public required Dictionary<string, double[]> Centroids { get; init; }

    public static NearestCentroidModel Train(List<DatasetRow> rows, StandardScaler scaler, List<string> featureNames)
    {
        var centroids = new Dictionary<string, double[]>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in rows.GroupBy(r => r.Label).OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
        {
            var vectors = group.Select(x => scaler.Transform(x.Features)).ToList();
            var centroid = new double[featureNames.Count];

            for (var i = 0; i < centroid.Length; i++)
            {
                centroid[i] = vectors.Average(v => v[i]);
            }

            centroids[group.Key] = centroid;
        }

        return new NearestCentroidModel
        {
            Labels = centroids.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList(),
            FeatureNames = featureNames,
            Scaler = scaler,
            Centroids = centroids
        };
    }

    public string Predict(double[] rawFeatures)
    {
        var features = Scaler.Transform(rawFeatures);
        var bestLabel = string.Empty;
        var bestDistance = double.MaxValue;

        foreach (var pair in Centroids)
        {
            var distance = 0d;
            for (var i = 0; i < features.Length; i++)
            {
                var diff = features[i] - pair.Value[i];
                distance += diff * diff;
            }

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestLabel = pair.Key;
            }
        }

        return bestLabel;
    }
}

internal sealed class EvaluationResult
{
    public required double Accuracy { get; init; }
    public required Dictionary<string, Dictionary<string, int>> Confusion { get; init; }
}

internal static class Evaluator
{
    public static EvaluationResult Evaluate(NearestCentroidModel model, List<DatasetRow> testRows)
    {
        var correct = 0;
        var confusion = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in testRows)
        {
            var predicted = model.Predict(row.Features);
            if (string.Equals(predicted, row.Label, StringComparison.OrdinalIgnoreCase))
            {
                correct++;
            }

            if (!confusion.TryGetValue(row.Label, out var predictions))
            {
                predictions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                confusion[row.Label] = predictions;
            }

            predictions[predicted] = predictions.TryGetValue(predicted, out var count)
                ? count + 1
                : 1;
        }

        return new EvaluationResult
        {
            Accuracy = testRows.Count == 0 ? 0d : (double)correct / testRows.Count,
            Confusion = confusion
        };
    }
}

internal static class ModelSerializer
{
    public static void Save(string path, NearestCentroidModel model)
    {
        var payload = new SerializableModel
        {
            Labels = model.Labels,
            FeatureNames = model.FeatureNames,
            Means = model.Scaler.Means,
            Stds = model.Scaler.Stds,
            Centroids = model.Centroids
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(path, json);
    }

    private sealed class SerializableModel
    {
        public required List<string> Labels { get; init; }
        public required List<string> FeatureNames { get; init; }
        public required double[] Means { get; init; }
        public required double[] Stds { get; init; }
        public required Dictionary<string, double[]> Centroids { get; init; }
    }
}

internal static class ReportBuilder
{
    public static string Build(TrainingMode mode, NearestCentroidModel model, (List<DatasetRow> Train, List<DatasetRow> Test) split, EvaluationResult evaluation)
    {
        var title = mode == TrainingMode.Motion ? "Gesture motion training report" : "Gesture training report";
        var underline = new string('=', title.Length);
        var itemLabel = mode.GetItemLabel();
        var lines = new List<string>
        {
            title,
            underline,
            string.Empty,
            $"Labels: {model.Labels.Count}",
            $"Train {itemLabel}: {split.Train.Count}",
            $"Test {itemLabel}: {split.Test.Count}",
            $"Accuracy: {evaluation.Accuracy:P2}",
            string.Empty,
            "Labels used:"
        };

        foreach (var label in model.Labels)
        {
            lines.Add($"- {label}");
        }

        lines.Add(string.Empty);
        lines.Add("Confusion summary:");
        foreach (var actual in evaluation.Confusion.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            var summary = string.Join(", ",
                actual.Value
                    .OrderByDescending(x => x.Value)
                    .ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(x => $"{x.Key}:{x.Value}"));

            lines.Add($"- {actual.Key} -> {summary}");
        }

        return string.Join(Environment.NewLine, lines);
    }
}



