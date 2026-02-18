using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Dto.Auth;
using ProjectManager.Models;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AuthService(
            ApplicationDbContext context,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IMapper mapper)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        public async Task<UserResponseDto> Register(UserRegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email.ToLower()))
                throw new Exception("Пользователь с таким email уже существует");

            _passwordHasher.CreatePasswordHash(registerDto.Password,
                out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Email = registerDto.Email.ToLower(),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedAt = DateTime.UtcNow
            };

            var roleName = registerDto.RoleName ?? "Member";
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
                role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Member");

            user.UserRoles = new List<UserRole>
            {
                new UserRole { Role = role }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.CreateToken(user);

            var userDto = _mapper.Map<UserResponseDto>(user);
            userDto.Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            userDto.Token = token;

            return userDto;
        }

        public async Task<UserResponseDto> Login(UserLoginDto loginDto)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email.ToLower());

            if (user == null)
                throw new Exception("Неверный email или пароль");  

            if (!_passwordHasher.VerifyPasswordHash(loginDto.Password,
                user.PasswordHash, user.PasswordSalt))
                throw new Exception("Неверный email или пароль");

            var token = _tokenService.CreateToken(user);

            var userDto = _mapper.Map<UserResponseDto>(user);
            userDto.Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            userDto.Token = token;

            return userDto;
        }

        public async Task<UserResponseDto> GetCurrentUser(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("Пользователь не найден");

            var userDto = _mapper.Map<UserResponseDto>(user);
            userDto.Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            return userDto;
        }
    }
}
