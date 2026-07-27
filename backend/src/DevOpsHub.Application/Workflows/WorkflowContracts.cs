namespace DevOpsHub.Application.Workflows;
public sealed record ApprovalRequestDto(Guid Id,string Title,string Type,string Requester,string Status,DateTime CreatedAt);
public sealed record CreateApprovalRequest(string Title,string Type,string Requester);
public interface IWorkflowService { IReadOnlyList<ApprovalRequestDto> GetAll(); ApprovalRequestDto Create(CreateApprovalRequest request); ApprovalRequestDto Decide(Guid id,string decision); }
