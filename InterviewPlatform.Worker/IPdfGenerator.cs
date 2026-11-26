using InterviewPlatform.Shared;

namespace InterviewPlatform.Worker;

public interface IPdfGenerator
{
    byte[] Generate(DocumentType type, DocumentData data);
}
