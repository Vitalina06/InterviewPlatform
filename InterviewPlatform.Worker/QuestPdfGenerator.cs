using InterviewPlatform.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace InterviewPlatform.Worker;

public class QuestPdfGenerator : IPdfGenerator
{
    private readonly string _font;

    public QuestPdfGenerator(IConfiguration config)
    {
        _font = config["Pdf:FontFamily"]!;
    }

    public byte[] Generate(DocumentType type, DocumentData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontFamily(_font).FontSize(12));

                page.Header().Text(GetTitle(type)).FontSize(20).Bold();

                page.Content().PaddingVertical(15).Column(column =>
                {
                    column.Spacing(8);

                    switch (type)
                    {
                        case DocumentType.InterviewProtocol:
                            BuildProtocol(column, data);
                            break;
                        case DocumentType.Invitation:
                            BuildInvitation(column, data);
                            break;
                        case DocumentType.Rejection:
                            BuildRejection(column, data);
                            break;
                        case DocumentType.CandidateCard:
                            BuildCandidateCard(column, data);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type));
                    }
                });

                page.Footer().AlignCenter().Text($"Платформа для технических собеседований, {DateTime.Now:dd.MM.yyyy}");
            });
        });

        return document.GeneratePdf();
    }

    private static string GetTitle(DocumentType type)
    {
        return type switch
        {
            DocumentType.InterviewProtocol => "Протокол собеседования",
            DocumentType.Invitation => "Приглашение на собеседование",
            DocumentType.Rejection => "Ответ по результатам собеседования",
            DocumentType.CandidateCard => "Карточка кандидата",
            _ => ""
        };
    }

    private static void BuildProtocol(ColumnDescriptor column, DocumentData data)
    {
        Line(column, "Кандидат", data.CandidateName);
        Line(column, "Вакансия", data.VacancyTitle);
        Line(column, "Дата и время", FormatDate(data.ScheduledAt));
        Line(column, "Решение", StatusToText(data.Status));

        column.Item().PaddingTop(5).Text("Этапы").Bold();
        foreach (var stage in data.Stages)
        {
            column.Item().Text("• " + stage);
        }

        column.Item().PaddingTop(5).Text("Матрица компетенций").Bold();
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.ConstantColumn(60);
                columns.RelativeColumn(3);
            });

            table.Cell().Border(1).Padding(4).Text("Компетенция").Bold();
            table.Cell().Border(1).Padding(4).Text("Оценка").Bold();
            table.Cell().Border(1).Padding(4).Text("Комментарий").Bold();

            foreach (var rating in data.Ratings)
            {
                table.Cell().Border(1).Padding(4).Text(rating.Competency);
                table.Cell().Border(1).Padding(4).Text(rating.Score.ToString());
                table.Cell().Border(1).Padding(4).Text(rating.Comment);
            }
        });

        if (data.Ratings.Count > 0)
        {
            Line(column, "Средняя оценка", data.Ratings.Average(r => r.Score).ToString("0.0"));
        }

        if (!string.IsNullOrWhiteSpace(data.InterviewComment))
        {
            Line(column, "Комментарий", data.InterviewComment);
        }
    }

    private static void BuildInvitation(ColumnDescriptor column, DocumentData data)
    {
        column.Item().Text($"Уважаемый(ая) {data.CandidateName}!");
        column.Item().Text($"Приглашаем вас на техническое собеседование на позицию «{data.VacancyTitle}».");
        Line(column, "Дата и время", FormatDate(data.ScheduledAt));

        if (data.Stages.Count > 0)
        {
            column.Item().Text("Этапы собеседования:").Bold();
            foreach (var stage in data.Stages)
            {
                column.Item().Text("• " + stage);
            }
        }

        column.Item().PaddingTop(15).Text("С уважением, отдел кадров.");
    }

    private static void BuildRejection(ColumnDescriptor column, DocumentData data)
    {
        column.Item().Text($"Уважаемый(ая) {data.CandidateName}!");
        column.Item().Text($"Благодарим вас за интерес к позиции «{data.VacancyTitle}» и время, уделённое собеседованию.");
        column.Item().Text("По итогам рассмотрения мы приняли решение не продолжать сотрудничество на данном этапе.");
        column.Item().Text("Желаем вам успехов в дальнейшем поиске работы!");
        column.Item().PaddingTop(15).Text("С уважением, отдел кадров.");
    }

    private static void BuildCandidateCard(ColumnDescriptor column, DocumentData data)
    {
        Line(column, "ФИО", data.CandidateName);
        Line(column, "Телефон", data.Phone);
        Line(column, "Город", data.City);

        column.Item().PaddingTop(5).Text("Навыки").Bold();
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.ConstantColumn(60);
            });

            table.Cell().Border(1).Padding(4).Text("Навык").Bold();
            table.Cell().Border(1).Padding(4).Text("Оценка").Bold();

            foreach (var skill in data.Skills)
            {
                table.Cell().Border(1).Padding(4).Text(skill.Name);
                table.Cell().Border(1).Padding(4).Text(skill.Rating.ToString());
            }
        });

        column.Item().PaddingTop(5).Text("Собеседования").Bold();
        if (data.Interviews.Count == 0)
        {
            column.Item().Text("Собеседований пока нет");
        }

        foreach (var interview in data.Interviews)
        {
            column.Item().Text("• " + interview);
        }
    }

    private static void Line(ColumnDescriptor column, string label, string value)
    {
        column.Item().Text(text =>
        {
            text.Span(label + ": ").Bold();
            text.Span(value);
        });
    }

    private static string FormatDate(DateTime? date)
    {
        return date == null ? "" : date.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
    }

    private static string StatusToText(string status)
    {
        return status switch
        {
            "Accepted" => "Принять",
            "Rejected" => "Отклонить",
            "NextStage" => "На следующий этап",
            _ => "Ожидает решения"
        };
    }
}
