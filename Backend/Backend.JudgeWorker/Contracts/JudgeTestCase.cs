namespace Backend.JudgeWorker.Contracts;

public sealed record JudgeTestCase(
    int OrderIndex,
    string Input,
    string ExpectedOutput
);
