using Enexure.MicroBus;
using DevCongress.Jobs.Core.Domain.Repository;
using Plutonium.Reactor.Services.Auth.User;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DevCongress.Jobs.Core.Features.User.List
{
  internal class ListUsersQueryHandler : IQueryHandler<ListUsersQuery, ListUsersQueryResult>
  {
    private readonly IAuthenticatedUserProvider _userProvider;
    private readonly IUserRepository _userRepository;

    public ListUsersQueryHandler(
        IAuthenticatedUserProvider userProvider,
        IUserRepository userRepository)
    {
      _userProvider = userProvider;
      _userRepository = userRepository;
    }

    public async Task<ListUsersQueryResult> Handle(ListUsersQuery query)
    {
      var tenantId = _userProvider.GetUser().HasRoles("user.list.su") ? 0 : _userProvider.GetUser().TenantId;

      var limit = query.Limit;
      var userCount = await _userRepository.Count(
                                tenantId
                                , query.Filter
                                , query.CreateDateFilter
                                , query.UpdateDateFilter
                                , query.DeleteDateFilter
                                , query.IncludeTrashed
                              ).ConfigureAwait(false);

      var pageCount = limit == 0 ? 1 : Math.Max((int)Math.Ceiling((double)userCount / limit), 1);
      var page = Math.Min(query.Page, pageCount);

      var offset = (page - 1) * limit;

      var users = await _userRepository.List(
                            tenantId
                            , offset
                            , limit
                            , query.Filter
                            , query.CreateDateFilter
                            , query.UpdateDateFilter
                            , query.DeleteDateFilter
                            , query.SortBy
                            , query.IncludeTrashed
                          ).ConfigureAwait(false);

      return Plutonium.Reactor.Lib.Results.Results<ListUsersQueryResult>.Ok()
              .WithSuccess($"Retrieved {users.Count()} out of {userCount} {(userCount == 1 ? "user" : "users")}")
              .With(res =>
              {
                res.Limit = limit;
                res.TotalCount = userCount;
                res.Page = page;
                res.TotalPageCount = pageCount;
                res.Users = users.ToArray();
                res.IncludeTrashed = query.IncludeTrashed;
              });
    }
  }
}
