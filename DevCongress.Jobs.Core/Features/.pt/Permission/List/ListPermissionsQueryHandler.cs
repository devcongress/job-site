using Enexure.MicroBus;
using DevCongress.Jobs.Core.Domain.Repository;
using System.Linq;
using System.Threading.Tasks;

namespace DevCongress.Jobs.Core.Features.Permission.List
{
    internal class ListPermissionsQueryHandler : IQueryHandler<ListPermissionsQuery, ListPermissionsQueryResult>
    {
        private readonly IPermissionRepository _permissionRepository;

        public ListPermissionsQueryHandler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<ListPermissionsQueryResult> Handle(ListPermissionsQuery query)
        {
            var permissions = await _permissionRepository.List(query.SortBy).ConfigureAwait(false);
            var permissionCount = permissions.Count();

            return Plutonium.Reactor.Lib.Results.Results<ListPermissionsQueryResult>.Ok()
                    .WithSuccess($"Retrieved {permissionCount} {(permissionCount == 1 ? "permission" : "permissions")}")
                    .With(res =>
                    {
                        res.Permissions = permissions.ToArray();
                    });
        }
    }
}