using ApplicationLayer.Features.LoginAuditLogs.Dtos;
using ApplicationLayer.Interfaces;
using DomainLayer.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Features.LoginAuditLogs.Queries
{
    public class GetLoginAuditLogsQueryHandler : IRequestHandler<GetLoginAuditLogsQuery, OperationResult<List<LoginAuditLogDto>>>
    {
        private readonly ILoginAuditRepository _repo;

        public GetLoginAuditLogsQueryHandler(ILoginAuditRepository repo)
        {
            _repo = repo;
        }

        public async Task<OperationResult<List<LoginAuditLogDto>>> Handle(
            GetLoginAuditLogsQuery request,
            CancellationToken cancellationToken)
        {
            var logs = await _repo.GetRecentAsync();

            var result = logs.Select(l => new LoginAuditLogDto(
                l.UserId,
                l.Identifier,
                l.Role,
                l.Success,
                l.FailureReason,
                l.CreatedAt
            )).ToList();

            return OperationResult<List<LoginAuditLogDto>>.Success(result);
        }
    }
}
