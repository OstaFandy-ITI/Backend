using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OstaFandy.DAL.Entities;
using OstaFandy.DAL.Repos;
using OstaFandy.DAL.Repos.IRepos;
using OstaFandy.PL.BL.IBL;
using OstaFandy.PL.DTOs;
using OstaFandy.PL.General;
using OstaFandy.PL.utils;

namespace OstaFandy.PL.BL
{
    public class HandyManService : IHandyManService
    {
        public readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HandyManService> _logger;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;
        public HandyManService(IUnitOfWork unitOfWork, ILogger<HandyManService> logger, IMapper mapper,ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
        }

        public PaginationHelper<Handyman> GetAll(string searchString = "", int pageNumber = 1, int pageSize = 5, bool? isActive = null)
        {
            try
            {
                var data = _unitOfWork.HandyManRepo.GetAll(a => a.Status == "Approved", includeProperties: "User,Specialization,DefaultAddress,BlockDates,JobAssignments").ToList();

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
 
                if (isActive == true)
                {
                data = data.Where(a => a.User.IsActive == true).ToList();
                }else if(isActive == false)
                {
                    data = data.Where(a => a.User.IsActive == false).ToList();
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


        public List<AdminHandyManDTO> GetAllPendingHandymen()
        {
            try
            {
                var pendingHandymen = _unitOfWork.HandyManRepo.GetAll(h => h.Status == "Pending", "User,Specialization,DefaultAddress,BlockDates,JobAssignments").ToList();
                return _mapper.Map<List<AdminHandyManDTO>>(pendingHandymen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all pending handymen");
                return new List<AdminHandyManDTO>();
            }
        }
        public async Task<bool> UpdateHandymanStatusById(int userId, string status)
        {
            try
            {
                var handyman = _unitOfWork.HandyManRepo.GetById(userId);
                if (handyman == null)
                {
                    _logger.LogWarning($"Handyman with ID {userId} not found");
                    return false;
                }
                handyman.Status = status;
                _unitOfWork.HandyManRepo.Update(handyman);
                _unitOfWork.Save();
                _logger.LogInformation($"Handyman {userId} status updated to {status}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating handyman status for UserId: {userId}");
                return false;
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
                //handyman.UserId = user.Id; 
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
                    _unitOfWork.Save();  
                    handyman.DefaultAddressId = defaultAddress.Id;
                    _unitOfWork.HandyManRepo.Update(handyman);
                    _unitOfWork.Save();
                } 

 
                var createdHandyman = _unitOfWork.HandyManRepo.GetById(user.Id); 
 
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
                 var handyman = _unitOfWork.HandyManRepo.GetById(id);

                if (handyman == null)
                {
                    return null; 
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

        public async Task<int> CreateHandyManApplicationAsync(HandyManApplicationDto handymandto)
        {
            if (handymandto == null)
                return 0; // invalid input

            using var transaction = await _unitOfWork.BeginTransactionasync();

            try
            {
                var existingUser = _unitOfWork.UserRepo.CheckUniqueOfEmailPhone(handymandto.Email, handymandto.Phone);
                if (!existingUser)
                    return -1; // existing user

                if (handymandto.Password != handymandto.ConfirmPassword)
                    return -2; // passwords do not match

                // User
                var user = _mapper.Map<User>(handymandto);
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(handymandto.Password);
                user.CreatedAt = DateTime.Now;
                user.UpdatedAt = DateTime.Now;
                user.UserTypes.Add(_unitOfWork.UserTypeRepo.FirstOrDefault(s => s.TypeName == General.UserType.Handyman));
                _unitOfWork.UserRepo.Insert(user);
                await _unitOfWork.SaveAsync();

                // Address
                var address = _mapper.Map<Address>(handymandto);
                address.UserId = user.Id;
                _unitOfWork.AddressRepo.Insert(address);
                await _unitOfWork.SaveAsync();

                // Upload images
                var imgUrl = await _cloudinaryService.UploadImageAsync(handymandto.Img);
                var nationalIdImgUrl = await _cloudinaryService.UploadImageAsync(handymandto.NationalIdImg);

                // Handyman
                var handyman = _mapper.Map<Handyman>(handymandto);
                handyman.UserId = user.Id;
                handyman.Img = imgUrl;
                handyman.NationalIdImg = nationalIdImgUrl;
                handyman.DefaultAddressId = address.Id;
                handyman.Status = HandymenStatus.Pending;

                _unitOfWork.HandyManRepo.Insert(handyman);

                var res = await _unitOfWork.SaveAsync();


                if (res > 0)
                {
                    await transaction.CommitAsync();
 
                    return 1;
 
                    return user.Id;
                 }
                else
                {
                    await transaction.RollbackAsync();
                    return 0;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while creating handyman application");
                return 0;
            }
        }

    }
}

