using Enexure.MicroBus;
using DevCongress.Jobs.Core.Domain.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DevCongress.Jobs.Core.Features.Role.List
{
    internal class ListRolesQueryHandler : IQueryHandler<ListRolesQuery, ListRolesQueryResult>
    {
        private readonly IRoleRepository _roleRepository;

        public ListRolesQueryHandler(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<ListRolesQueryResult> Handle(ListRolesQuery query)
        {
            var limit = query.Limit;
            var roleCount = await _roleRepository.Count(
                                      query.Filter
                                      , query.CreateDateFilter
                                      , query.UpdateDateFilter
                                      , query.DeleteDateFilter
                                      , query.IncludeTrashed
                                    ).ConfigureAwait(false);

            var pageCount = limit == 0 ? 1 : Math.Max((int)Math.Ceiling((double)roleCount / limit), 1);
            var page = Math.Min(query.Page, pageCount);

            var offset = (page - 1) * limit;

            var roles = await _roleRepository.List(
                                  offset
                                  , limit
                                  , query.Filter
                                  , query.CreateDateFilter
                                  , query.UpdateDateFilter
                                  , query.DeleteDateFilter
                                  , query.SortBy
                                  , query.IncludeTrashed
                                ).ConfigureAwait(false);

            return Plutonium.Reactor.Lib.Results.Results<ListRolesQueryResult>.Ok()
                    .WithSuccess($"Retrieved {roles.Count()} out of {roleCount} {(roleCount == 1 ? "role" : "roles")}")
                    .With(res =>
                    {
                        res.Limit = limit;
                        res.TotalCount = roleCount;
                        res.Page = page;
                        res.TotalPageCount = pageCount;
                        res.Roles = roles.ToArray();
                        res.IncludeTrashed = query.IncludeTrashed;
                    });
        }
    }
}