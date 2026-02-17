using System.Collections.Generic;

namespace PhoronixResultViewer.Models;

public readonly record struct Result(double Performance, double? PowerConsumption, string System);

public readonly record struct Test(string Title, string Identifier, string Description, PerformanceClass PerformanceClass, string Scale);

public readonly record struct TestResults(string Id, Test Test, Result Result);

public readonly record struct TestSuite(string Name, List<string> TestNames);

public readonly record struct TestSuiteGroup(TestSuite Key, List<CalculatedResults> Results, int TotalTests);
