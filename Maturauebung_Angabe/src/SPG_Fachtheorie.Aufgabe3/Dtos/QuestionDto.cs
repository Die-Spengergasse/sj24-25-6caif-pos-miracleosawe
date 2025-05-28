using SPG_Fachtheorie.Aufgabe2.Model;

namespace SPG_Fachtheorie.Aufgabe3.Dtos
{
    // TODO: Füge nötige Properties hinzu.
    public record QuestionDto(int Id, string Text, Exam Exam, List<PossibleAnswer> PossibleAnswers);
}
