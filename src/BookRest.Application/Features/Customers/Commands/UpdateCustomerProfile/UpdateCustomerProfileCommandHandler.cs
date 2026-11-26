using BookRest.Application.Common.Errors;
using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Repositories;
using BookRest.Domain.Errors;
using BookRest.Domain.Shared;

namespace BookRest.Application.Features.Customers.Commands.UpdateCustomerProfile;

public class UpdateCustomerProfileCommandHandler : ICommandHandler<UpdateCustomerProfileCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ICustomerRepository _customerRepository;
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerProfileCommandHandler(
        ICurrentUserService currentUserService,
        ICustomerRepository customerRepository,
        IIdentityService identityService,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _customerRepository = customerRepository;
        _identityService = identityService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateCustomerProfileCommand request, CancellationToken cancellationToken)
    {
        // Provide heck for null even though Authorization middleware will fail in case user is not authenticated
        var applicationUserId = _currentUserService.ApplicationUserId;

        if (applicationUserId == null)
        {
            return Result.Failure(IdentityErrors.Unauthorized());
        }

        var customer = await _customerRepository.GetByApplicationUserIdAsync(applicationUserId.Value, cancellationToken);

        if (customer == null)
        {
            return Result.Failure(CustomerErrors.NotFoundById(applicationUserId.Value));
        }

        // In case the name is the same - update nothing
        if (customer.UserName == request.UserName)
        {
            return Result.Success();
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Update identity user first
            var setNameResult = await _identityService.SetUserNameAsync(applicationUserId.Value, request.UserName);

            if (setNameResult.IsFailure)
            {
                return setNameResult;
            }

            // Update customer entity
            customer.UpdateProfile(userName: request.UserName);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }, cancellationToken);
    }
}
