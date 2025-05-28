using SPG_Fachtheorie.Aufgabe2.Model;

namespace SPG_Fachtheorie.Aufgabe3.Dtos
{
    // TODO: Füge nötige Properties hinzu.
    public record ExamDto(int Id, string Name, int FailTreshold, bool Visible, List<Question> Questions);
}
