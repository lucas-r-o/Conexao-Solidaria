using Models.Dtos;
using Models.DTOs;

namespace ApplicationServices.Interfaces;

public interface IUserService
{
    Task<Result<string>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken);
    Task<Result<string>> RegisterAsync(RegistrationDto user, CancellationToken cancellationToken);
    Task<Result<string>> RegisterManagerAsync(RegistrationDto user, CancellationToken cancellationToken);
    Task<bool> TryDonationAsync(DonationDto donation, CancellationToken cancellationToken = default);
}