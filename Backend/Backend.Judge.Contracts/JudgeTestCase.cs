namespace Backend.Judge.Contracts;

public sealed record JudgeTestCase(
    int OrderIndex,
    string Input,
    string ExpectedOutput
);
