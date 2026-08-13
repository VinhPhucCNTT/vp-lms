namespace Backend.Api.Core.Types;

public enum QuestionType
{
    MultipleChoice,
    MultipleSelect,
    TrueFalse,
    ShortAnswer,
    DragAndDrop,
    Matching,
    Ordering,
    Coding
}

public class MultipleChoiceQuestion
{
    public List<QuestionOption> Options { get; set; } = [];
}

public class MultipleSelectQuestion
{
    public List<QuestionOption> Options { get; set; } = [];
}

public class TrueFalseQuestion
{
    public bool IsCorrect { get; set; }
}

public class ShortAnswerQuestion
{
    public List<string> AcceptedAnswers { get; set; } = [];
    public bool IsCaseSensitive { get; set; } = false;
}

public class DragAndDropQuestion
{
    public List<DragItem> Items { get; set; } = [];
    public List<DropZone> Zones { get; set; } = [];
}

public class CodingQuestion
{
    public string ProblemStatement { get; set; } = string.Empty;

    public List<string> AllowedLanguages { get; set; } = [];

    public string? StarterCode { get; set; }

    public string? InputDescription { get; set; }
    public string? OutputDescription { get; set; }

    public List<SampleCase> SampleCases { get; set; } = [];
}

public class QuestionOption
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = "";
    public bool IsCorrect { get; set; }
}

public class DragItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public string CorrectZoneId { get; set; } = string.Empty;
}

public class DropZone
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Label { get; set; } = string.Empty;
}

public class SampleCase
{
    public string Input { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
}
