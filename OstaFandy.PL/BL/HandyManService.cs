using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OstaFandy.DAL.Entities;
using OstaFandy.DAL.Repos;
using OstaFandy.DAL.Repos.IRepos;
using OstaFandy.PL.BL.IBL;
using OstaFandy.PL.DTOs;
using OstaFandy.PL.utils;

namespace OstaFandy.PL.BL
{
    public class HandyManService : IHandyManService
    {
        public readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HandyManService> _logger;
        private readonly IMapper _mapper;
        public HandyManService(IUnitOfWork unitOfWork, ILogger<HandyManService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public PaginationHelper<Handyman> GetAll(string searchString = "", int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                var data = _unitOfWork.HandyManRepo.GetAll(includeProperties: "User,Specialization,DefaultAddress,BlockDates,JobAssignments").ToList();

                if (data == null)
                {
                    return new PaginationHelper<Handyman>
                    {
                        Data = new List<Handyman>(),
                        CurrentPage = pageNumber,
                        TotalPages = 0,
                        TotalCount = 0,
                        SearchString = searchString
                    };
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchString))
                {
                    data = data.Where(h =>
                        h.User.FirstName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        h.User.LastName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        h.User.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        h.Specialization.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                return PaginationHelper<Handyman>.Create(data, pageNumber, pageSize, searchString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all handymen");
                return new PaginationHelper<Handyman>
                {
                    Data = new List<Handyman>(),
                    CurrentPage = pageNumber,
                    TotalPages = 0,
                    TotalCount = 0,
                    SearchString = searchString
                };
            }
        }
        public AdminHandyManDTO CreateHandyman([FromBody] CreateHandymanDTO createHandymanDto)
        {
            try
            {
                var existingUser = _unitOfWork.UserRepo.CheckUniqueOfEmailPhone(createHandymanDto.Phone, createHandymanDto.Email);

                if (existingUser == null)
                {
                    throw new InvalidOperationException("User with this email, phone, or national ID already exists");
                }
                var x = _unitOfWork.HandyManRepo.checkUniqueNationalId(createHandymanDto.NationalId);
                if (x == null)
                {
                    throw new InvalidOperationException("User with this national ID already exists");
                }
                // Create User entity
                //var user = _mapper.Map<User>(createHandymanDto);
                var user = new User()
                {
                    FirstName = createHandymanDto.FirstName,
                    LastName = createHandymanDto.LastName,
                    Email = createHandymanDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(createHandymanDto.Password),
                    Phone = createHandymanDto.Phone

                };
                _unitOfWork.UserRepo.Insert(user);
                _unitOfWork.Save();
                var handymanType = _unitOfWork.UserTypeRepo
                        .FirstOrDefault(t => t.TypeName == "Handyman") ?? throw new InvalidOperationException("UserType 'Handyman' not found");

                user.UserTypes.Add(handymanType);

                int newUserId = user.Id;
                var handyman = new Handyman
                {
                    UserId = newUserId,
                    SpecializationId = createHandymanDto.SpecializationId,
                    Latitude = createHandymanDto.Latitude,
                    Longitude = createHandymanDto.Longitude,
                    NationalId = createHandymanDto.NationalId,
                    NationalIdImg = createHandymanDto.NationalIdImg,
                    Img = createHandymanDto.Img,
                    ExperienceYears = createHandymanDto.ExperienceYears,
                    Status = createHandymanDto.Status
                };

                _unitOfWork.HandyManRepo.Insert(handyman);
                _unitOfWork.Save();
                // Hash password
                //user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(createHandymanDto.Password);



                // Create Handyman entity
                //var handyman = _mapper.Map<Handyman>(createHandymanDto);
                //handyman.UserId = user.Id; // This is the primary key for Handyman
                // Create default address if provided
                if (!string.IsNullOrEmpty(createHandymanDto.DefaultAddressPlace))
                {
                    var defaultAddress = new Address
                    {
                        UserId = user.Id,
                        Address1 = createHandymanDto.DefaultAddressPlace,
                        City = createHandymanDto.DefaultAddressCity ?? createHandymanDto.DefaultAddressPlace,
                        Latitude = createHandymanDto.DefaultAddressLatitude ?? 0,
                        Longitude = createHandymanDto.DefaultAddressLongitude ?? 0,
                        AddressType = createHandymanDto.AddressType,
                        IsDefault = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _unitOfWork.AddressRepo.Insert(defaultAddress);
                    _unitOfWork.Save(); // Save to get address.Id

                    // Update handyman with address ID
                    handyman.DefaultAddressId = defaultAddress.Id;
                    _unitOfWork.HandyManRepo.Update(handyman);
                    _unitOfWork.Save();
                }
                // Add handyman to database
                //_unitOfWork.HandyManRepo.Insert(handyman);
                //  _unitOfWork.Save();



                // FIXED: Get the created handyman using the correct ID
                var createdHandyman = _unitOfWork.HandyManRepo.GetById(user.Id); // user.Id is the UserId (PK) for Handyman

                // Map to DTO and return
                return _mapper.Map<AdminHandyManDTO>(createdHandyman);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating handyman: {Message}", ex.Message);
                throw;
            }
        }

        public AdminHandyManDTO GetById(int id)
        {
            try
            {
                // Get handyman by UserId (which is the primary key)
                var handyman = _unitOfWork.HandyManRepo.GetById(id);

                if (handyman == null)
                {
                    return null; // Return null so controller can return NotFound
                }

                // Map to DTO and return
                return _mapper.Map<AdminHandyManDTO>(handyman);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting handyman by id {Id}", id);
                throw;
            }
        }

        public bool DeleteHandyman(int id)
        {
            try
            {
                bool success = _unitOfWork.UserRepo.SoftDelete(id);
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting handyman with id {Id}", id);
                throw;
            }

        }

        public AdminHandyManDTO EditHandyman(EditHandymanDTO editHandymanDto)
        {
            try
            {
                //var handyman = _unitOfWork.HandyManRepo.GetById(editHandymanDto.UserId);
                var handyman = _unitOfWork.HandyManRepo.FirstOrDefault(a => a.UserId == editHandymanDto.UserId, includeProperties: "User,Specialization,DefaultAddress,BlockDates,JobAssignments");
                if (handyman == null)
                {
                    _logger.LogWarning(
                        "EditHandyman: no handyman found with UserId {UserId}",
                        editHandymanDto.UserId
                    );
                    return null;
                }
                _mapper.Map(editHandymanDto, handyman);
                if (!string.IsNullOrEmpty(editHandymanDto.DefaultAddressPlace))
                {
                    var defaultAddress = new Address
                    {
                        UserId = handyman.UserId,
                        Address1 = editHandymanDto.DefaultAddressPlace,
                        City = editHandymanDto.DefaultAddressCity,
                        Latitude = editHandymanDto.DefaultAddressLatitude ?? 0,
                        Longitude = editHandymanDto.DefaultAddressLongitude ?? 0,
                        AddressType = editHandymanDto.AddressType
                    };

                    _unitOfWork.AddressRepo.Insert(defaultAddress);
                    _unitOfWork.Save();
                }
                _unitOfWork.HandyManRepo.Update(handyman);
                _unitOfWork.Save();

                return _mapper.Map<AdminHandyManDTO>(handyman);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while editing handyman with UserId {UserId}",
                    editHandymanDto.UserId
                );
                throw;
            }
        }
    }
}

