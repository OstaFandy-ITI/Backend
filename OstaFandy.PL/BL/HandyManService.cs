﻿using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OstaFandy.DAL.Entities;
using OstaFandy.DAL.Repos;
using OstaFandy.DAL.Repos.IRepos;
using OstaFandy.PL.BL.IBL;
using OstaFandy.PL.DTOs;
using OstaFandy.PL.General;
using OstaFandy.PL.utils;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;

namespace OstaFandy.PL.BL
{
    public class HandyManService : IHandyManService
    {
        public readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<HandyManService> _logger;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;
        public HandyManService(IUnitOfWork unitOfWork, ILogger<HandyManService> logger, IMapper mapper, ICloudinaryService cloudinaryService)
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
                }
                else if (isActive == false)
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
                var handyman = _unitOfWork.HandyManRepo.FirstOrDefault(h=>h.UserId==id, "Specialization,User");

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
                var jobAssignments = _unitOfWork.JobAssignmentRepo.FirstOrDefault(
                            j => j.Handyman.UserId == id &&
                                 j.Booking.PreferredDate > DateTime.Now,
                            includeProperties: "Handyman,Booking,Quotes");
                if (jobAssignments != null)
                {
                    _logger.LogWarning("Cannot delete handyman with active job assignments or bookings.");
                    return false;
                }
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

                //if (!string.IsNullOrEmpty(editHandymanDto.DefaultAddressPlace))
                //{
                //    var defaultAddress = new Address
                //    {
                //        UserId = handyman.UserId,
                //        Address1 = editHandymanDto.DefaultAddressPlace,
                //        City = editHandymanDto.DefaultAddressCity,
                //        Latitude = editHandymanDto.DefaultAddressLatitude ?? 0,
                //        Longitude = editHandymanDto.DefaultAddressLongitude ?? 0,
                //        AddressType = editHandymanDto.AddressType
                //    };

                //    _unitOfWork.AddressRepo.Insert(defaultAddress);
                //    _unitOfWork.Save();
                //}
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
        //public AdminHandyManDTO EditHandyman(EditHandymanDTO editHandymanDto)
        //{
        //    try
        //    {
        //        var handyman = _unitOfWork.HandyManRepo.FirstOrDefault(
        //            a => a.UserId == editHandymanDto.UserId,
        //            includeProperties: "User,Specialization,DefaultAddress,BlockDates,JobAssignments"
        //        );

        //        if (handyman == null)
        //        {
        //            _logger.LogWarning("EditHandyman: no handyman found with UserId {UserId}", editHandymanDto.UserId);
        //            return null;
        //        }

        //        // Update HandyMan properties (excluding Status)
        //        if (!string.IsNullOrEmpty(editHandymanDto.NationalId))
        //            handyman.NationalId = editHandymanDto.NationalId;

        //        if (!string.IsNullOrEmpty(editHandymanDto.NationalIdImg))
        //            handyman.NationalIdImg = editHandymanDto.NationalIdImg;

        //        if (!string.IsNullOrEmpty(editHandymanDto.Img))
        //            handyman.Img = editHandymanDto.Img;

        //        if (editHandymanDto.ExperienceYears != null)
        //            handyman.ExperienceYears = editHandymanDto.ExperienceYears;

        //        if (editHandymanDto.SpecializationId != null)
        //            handyman.SpecializationId = editHandymanDto.SpecializationId;

        //        if (editHandymanDto.Latitude.HasValue)
        //            handyman.Latitude = editHandymanDto.Latitude.Value;

        //        if (editHandymanDto.Longitude.HasValue)
        //            handyman.Longitude = editHandymanDto.Longitude.Value;



        //        handyman.UpdatedAt = DateTime.UtcNow;

        //        // Update User properties ONLY (including IsActive based on Status)
        //        if (handyman.User != null)
        //        {
        //            if (!string.IsNullOrEmpty(editHandymanDto.FirstName))
        //                handyman.User.FirstName = editHandymanDto.FirstName;

        //            if (!string.IsNullOrEmpty(editHandymanDto.LastName))
        //                handyman.User.LastName = editHandymanDto.LastName;

        //            if (!string.IsNullOrEmpty(editHandymanDto.Email))
        //                handyman.User.Email = editHandymanDto.Email;

        //            if (!string.IsNullOrEmpty(editHandymanDto.Phone))
        //                handyman.User.Phone = editHandymanDto.Phone;

        //            // Map Status to User.IsActive ONLY
        //            if (!string.IsNullOrEmpty(editHandymanDto.Status))
        //            {
        //                handyman.User.IsActive = editHandymanDto.Status == "Active";
        //            }

        //            handyman.User.UpdatedAt = DateTime.UtcNow;
        //        }

        //        // DO NOT update handyman.Status - leave it as is

        //        _unitOfWork.HandyManRepo.Update(handyman);
        //        _unitOfWork.Save();

        //        // Manual mapping for return DTO
        //        return MapToAdminHandyManDTO(handyman);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while editing handyman with UserId {UserId}", editHandymanDto.UserId);
        //        throw;
        //    }
        //}

        //// Helper method to manually map to AdminHandyManDTO
        //private AdminHandyManDTO MapToAdminHandyManDTO(Handyman handyman)
        //{
        //    return new AdminHandyManDTO
        //    {
        //        UserId = handyman.UserId,
        //        FirstName = handyman.User?.FirstName,
        //        LastName = handyman.User?.LastName,
        //        Email = handyman.User?.Email,
        //        Phone = handyman.User?.Phone,
        //        IsActive = handyman.User?.IsActive ?? false,

        //        // Show Status based on User.IsActive, NOT HandyMan.Status
        //        Status = handyman.User?.IsActive == true ? "Active" : "Inactive",

        //        NationalId = handyman.NationalId,
        //        NationalIdImg = handyman.NationalIdImg,
        //        Img = handyman.Img,
        //        ExperienceYears = handyman.ExperienceYears,
        //        //SpecializationId = handyman.SpecializationId,
        //        //SpecializationName = handyman.Specialization?.Name,
        //        Latitude = handyman.Latitude,
        //        Longitude = handyman.Longitude,
        //        //DefaultAddressId = handyman.DefaultAddressId,
        //        CreatedAt = handyman.CreatedAt,
        //        UpdatedAt = handyman.UpdatedAt
        //        // Add other properties as needed
        //    };
        //}

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
                user.EmailConfirmed = true;
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

        public HandyManStatsDto? GetHandyManStats(int handymanId)
        {
            try
            {
                var handyman = _unitOfWork.HandyManRepo.FirstOrDefault(h => h.UserId == handymanId, "JobAssignments.Booking,JobAssignments.Quotes");
                if (handyman == null)
                {
                    _logger.LogWarning($"Handyman with ID {handymanId} not found");
                    return null;
                }
                return (new HandyManStatsDto
                {
                    TodayJobs = handyman.JobAssignments
                        .Count(j => j.IsActive && j.Status == "Assigned" && j.AssignedAt.Date == DateTime.UtcNow.Date),

                    PendingQuotes = handyman.JobAssignments
                         .SelectMany(j => j.Quotes)
                         .Count(q => q.Status == QuotesStatus.Pending),

                    CompletedJobs = handyman.JobAssignments
                         .Count(j => j.Booking != null && j.Booking.Status == BookingStatus.Completed)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting total jobs for handyman with ID {handymanId}");
                return null;
            }
        }

        public async Task<HandymanProfileDto> GetHandymanProfile(int userId)
        {
            try
            {
                var handyman = _unitOfWork.HandyManRepo.FirstOrDefault(
                    h => h.UserId == userId,
                    includeProperties: "User,Specialization,DefaultAddress"
                );

                if (handyman == null)
                {
                    _logger.LogWarning($"Handyman with ID {userId} not found for profile retrieval.");
                    return null;
                }

                return new HandymanProfileDto
                {
                    UserId = handyman.UserId,
                    FirstName = handyman.User?.FirstName,
                    LastName = handyman.User?.LastName,
                    Email = handyman.User?.Email,
                    Phone = handyman.User?.Phone,
                    ProfilePictureUrl = handyman.Img,
                    SpecializationName = handyman.Specialization?.Name,
                    ExperienceYears = handyman.ExperienceYears,
                    Status = handyman.Status,
                    DefaultAddress = handyman.DefaultAddress?.Address1,
                    City = handyman.DefaultAddress?.City,
                    Latitude = handyman.DefaultAddress?.Latitude ?? 0,
                    Longitude = handyman.DefaultAddress?.Longitude ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting handyman profile for UserId: {userId}");
                throw;
            }
        }

        public async Task<bool> UpdateHandymanProfilePhoto(int userId, string profilePhotoUrl)
        {
            try
            {
                var handyman = _unitOfWork.HandyManRepo.FirstOrDefault(h => h.UserId == userId);

                if (handyman == null)
                {
                    _logger.LogWarning($"Handyman with UserId {userId} not found for profile photo update.");
                    return false;
                }

                handyman.Img = profilePhotoUrl;

                _unitOfWork.HandyManRepo.Update(handyman);
                var result = await _unitOfWork.SaveAsync();

                if (result > 0)
                {
                    _logger.LogInformation($"Successfully updated profile photo for handyman with UserId {userId}.");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Failed to save profile photo update for handyman with UserId {userId}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating profile photo for handyman with UserId: {userId}");
                return false;
            }
        }

        public async Task<HandymanProfileDto> UpdateHandymanProfile(int userId, UpdateHandymanProfileDto updateDto)
        {
            using var transaction = await _unitOfWork.BeginTransactionasync();

            try
            {
                var handyman = _unitOfWork.HandyManRepo.FirstOrDefault(
                    h => h.UserId == userId,
                    includeProperties: "User,Specialization,DefaultAddress"
                );

                if (handyman == null)
                {
                    _logger.LogWarning($"Handyman with UserId {userId} not found for profile update.");
                    return null;
                }

                // Update User information
                var user = handyman.User;
                bool userUpdated = false;

                if (!string.IsNullOrWhiteSpace(updateDto.FirstName) && updateDto.FirstName != user.FirstName)
                {
                    user.FirstName = updateDto.FirstName.Trim();
                    userUpdated = true;
                }

                if (!string.IsNullOrWhiteSpace(updateDto.LastName) && updateDto.LastName != user.LastName)
                {
                    user.LastName = updateDto.LastName.Trim();
                    userUpdated = true;
                }

                if (!string.IsNullOrWhiteSpace(updateDto.Phone) && updateDto.Phone != user.Phone)
                {
                    // Check if phone number already exists for another user
                    var existingUserWithPhone = _unitOfWork.UserRepo.FirstOrDefault(u => u.Phone == updateDto.Phone && u.Id != userId);
                    if (existingUserWithPhone != null)
                    {
                        throw new InvalidOperationException("Phone number already exists for another user");
                    }

                    user.Phone = updateDto.Phone.Trim();
                    userUpdated = true;
                }

                if (userUpdated)
                {
                    user.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.UserRepo.Update(user);
                }

                // Update Handyman information
                bool handymanUpdated = false;

                if (updateDto.ExperienceYears.HasValue && updateDto.ExperienceYears != handyman.ExperienceYears)
                {
                    handyman.ExperienceYears = updateDto.ExperienceYears.Value;
                    handymanUpdated = true;
                }

                if (handymanUpdated)
                {
                    _unitOfWork.HandyManRepo.Update(handyman);
                }

                var result = await _unitOfWork.SaveAsync();

                if (result > 0)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation($"Successfully updated handyman profile for UserId {userId}.");

                    // Return updated profile
                    return await GetHandymanProfile(userId);
                }
                else
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning($"No changes made to handyman profile for UserId {userId}.");
                    return await GetHandymanProfile(userId);
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error updating handyman profile for UserId: {userId}");
                throw;
            }
        }

    }
}

