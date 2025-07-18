﻿using AutoMapper;
using OstaFandy.DAL.Entities;
using OstaFandy.PL.DTOs;
using OstaFandy.PL.General;

namespace OstaFandy.PL.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {


            #region User
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<DAL.Entities.UserType, UserTypeDto>().ReverseMap();
            CreateMap<User, UserRegesterDto>().ReverseMap();
            CreateMap<User, UserLoginDto>().ReverseMap();
            #endregion

            #region Address
            CreateMap<Address, AddressDTO>()
                    .ForSourceMember(src => src.Location, opt => opt.DoNotValidate());

            CreateMap<AddressDTO, Address>()
                .ForMember(dest => dest.Location, opt => opt.Ignore());

            CreateMap<CreateAddressDTO, Address>().ReverseMap();
            #endregion

            #region JobAssignment
            CreateMap<JobAssignment, JobAssignmentDTO>().ReverseMap();
            #endregion

            #region BlockDate
            CreateMap<BlockDate, AdminBlockDateDTO>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ReverseMap();
            #endregion

            #region Handyman
            CreateMap<Handyman, AdminHandyManDTO>()
                .AfterMap((src, dest) =>
                {
                    if (src.User != null)
                    {
                        dest.FirstName = src.User.FirstName;
                        dest.LastName = src.User.LastName;
                        dest.Email = src.User.Email;
                        dest.Phone = src.User.Phone;
                        dest.IsActive = src.User.IsActive;
                        dest.CreatedAt = src.User.CreatedAt;
                    }

                    dest.UpdatedAt = src.UpdatedAt;
                    dest.Latitude = src.Latitude;
                    dest.Longitude = src.Longitude;
                    dest.NationalId = src.NationalId;
                    dest.NationalIdImg = src.NationalIdImg;
                    dest.Img = src.Img;
                    dest.ExperienceYears = src.ExperienceYears;
                    dest.Status = src.Status;
                    dest.SpecializationCategory = src.Specialization?.Name;
                    dest.DefaultAddressPlace = src.DefaultAddress?.City;

                    dest.AdminBlockDateDTO = src.BlockDates?.Select(bd => new AdminBlockDateDTO
                    {
                        Id = bd.Id,
                        UserId = bd.UserId,
                        Reason = bd.Reason,
                        StartDate = bd.StartDate,
                        EndDate = bd.EndDate,
                        IsActive = bd.IsActive,
                        CreatedAt = bd.CreatedAt,
                        UpdatedAt = bd.UpdatedAt
                    }).ToList();
                });

            CreateMap<EditHandymanDTO, Handyman>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    if (dest.User != null)
                    {
                        dest.User.FirstName = src.FirstName;
                        dest.User.LastName = src.LastName;
                        dest.User.Email = src.Email;
                        dest.User.Phone = src.Phone;
                        dest.User.IsActive = src.Status == "Active";
                        dest.User.UpdatedAt = DateTime.UtcNow;
                    }
                })
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.User.IsActive ? "Active" : "Inactive"));
            #endregion

            #region Client
            CreateMap<User, AdminDisplayClientDTO>()
                .AfterMap((src, dest) =>
                {
                    if (src.Client != null)
                    {
                        dest.ClientUserId = src.Client.UserId;
                        dest.DefaultAddressId = src.Client.DefaultAddressId;

                        if (src.Client.DefaultAddress != null)
                        {
                            dest.DefaultAddress = new AddressDTO
                            {
                                Id = src.Client.DefaultAddress.Id,
                                Address1 = src.Client.DefaultAddress.Address1,
                                City = src.Client.DefaultAddress.City,
                                Latitude = src.Client.DefaultAddress.Latitude,
                                Longitude = src.Client.DefaultAddress.Longitude,
                                AddressType = src.Client.DefaultAddress.AddressType,
                                IsDefault = src.Client.DefaultAddress.IsDefault,
                                IsActive = src.Client.DefaultAddress.IsActive,
                                CreatedAt = src.Client.DefaultAddress.CreatedAt
                            };
                        }

                        dest.Addresses = src.Addresses?
                            .Where(a => a.IsActive)
                            .Select(address => new AddressDTO
                            {
                                Id = address.Id,
                                Address1 = address.Address1,
                                City = address.City,
                                Latitude = address.Latitude,
                                Longitude = address.Longitude,
                                AddressType = address.AddressType,
                                IsDefault = address.IsDefault,
                                IsActive = address.IsActive,
                                CreatedAt = address.CreatedAt
                            }).ToList() ?? new List<AddressDTO>();

                        var bookings = src.Client.Bookings ?? new List<Booking>();
                        dest.TotalBookings = bookings.Count;
                        dest.ActiveBookings = bookings.Count(b => b.IsActive &&
                            (b.Status == "Pending" || b.Status == "Confirmed" || b.Status == "InProgress"));
                        dest.TotalSpent = bookings
                            .Where(b => b.TotalPrice.HasValue)
                            .Sum(b => b.TotalPrice ?? 0m);
                    }
                });

            CreateMap<AdminEditClientDTO, User>()
                .AfterMap((src, dest) =>
                {
                    if (dest.Client != null)
                    {
                        dest.Client.DefaultAddressId = src.DefaultAddressId;
                        dest.UpdatedAt = DateTime.UtcNow;
                    }
                })
                .ReverseMap();
            #endregion

            #region Payment
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src =>
                    $"{src.Booking.Client.User.FirstName} {src.Booking.Client.User.LastName}"))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.CreatedAt));

            CreateMap<Payment, PaymentDetailsDto>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src =>
                    $"{src.Booking.Client.User.FirstName} {src.Booking.Client.User.LastName}"))
                .ForMember(dest => dest.Receipt, opt => opt.MapFrom(src => src.ReceiptUrl))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.CreatedAt));
            #endregion

            #region Booking
            CreateMap<Booking, BookingViewDto>()
           .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => $"{src.Client.User.FirstName} {src.Client.User.LastName}"))
           .ForMember(dest => dest.HandymanName, opt => opt.MapFrom(src =>
               src.JobAssignment != null && src.JobAssignment.Handyman != null
                   ? $"{src.JobAssignment.Handyman.User.FirstName} {src.JobAssignment.Handyman.User.LastName}"
                   : string.Empty))
           .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src =>
               src.BookingServices.FirstOrDefault().Service.Category.Name))
           .ForMember(dest => dest.ServiceNames, opt => opt.MapFrom(src =>
               src.BookingServices.Select(bs => bs.Service.Name).ToList()))
           .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Address.Latitude))
           .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Address.Longitude));


            //create bokking mapping
            CreateMap<CreateBookingDTO, Booking>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => BookingStatus.Confirmed))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());


            CreateMap<CreateBookingDTO, Payment>()
                .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.PaymentStatus))
                .ForMember(dest => dest.PaymentIntentId, opt => opt.MapFrom(src => src.PaymentIntentId))
                .ForMember(dest => dest.ReceiptUrl, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.BookingId, opt => opt.Ignore());

            CreateMap<CreateBookingDTO, JobAssignment>()
                .ForMember(dest => dest.HandymanId, opt => opt.MapFrom(src => src.HandymanId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Assigned"))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.AssignedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.BookingId, opt => opt.Ignore());

            #endregion

            #region 
            // Category
            CreateMap<Category, CategoryDTO>().ReverseMap();
            CreateMap<CategoryCreateDTO, Category>();

            // Service
            CreateMap<Service, ServiceDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

            CreateMap<ServiceCreateDTO, Service>();
            CreateMap<ServiceUpdateDTO, Service>();
            #endregion


            #region feedback admin page
            CreateMap<Review, OrderFeedbackDto>()
                .AfterMap((src, dest) =>
                {
                    dest.BookingId = src.BookingId;
                    dest.HandymanName = $"{src.Booking.JobAssignment.Handyman.User.FirstName}  {src.Booking.JobAssignment.Handyman.User.LastName}";
                    dest.HandymanSpecialty = src.Booking.JobAssignment.Handyman.Specialization?.Name;
                    dest.ClientName = $"{src.Booking.Client.User.FirstName} {src.Booking.Client.User.LastName}";
                    dest.ServiceName = src.Booking.JobAssignment.Handyman.Specialization.Name;
                    dest.Rating = src.Rating;
                    dest.Comment = src.Comment;
                    dest.CompletedAt = src.Booking.JobAssignment.CompletedAt;
                    dest.ReviewCreatedAt = src.CreatedAt;

                })
                .ReverseMap();
            #endregion

            #region Dashboard
            CreateMap<Booking, DashboardDTO>()
            .AfterMap((src, dest) =>
            {
                dest.Service = src.BookingServices != null && src.BookingServices.Any()
                   ? string.Join(", ", src.BookingServices.Select(bs => bs.Service.Name))
                   : string.Empty;

                dest.location = src.Address?.City ?? "";

                dest.client = src.Client?.User != null
                    ? $"{src.Client.User.FirstName} {src.Client.User.LastName}" : "";

                dest.handyman = src.JobAssignment?.Handyman?.User != null
                    ? $"{src.JobAssignment.Handyman.User.FirstName} {src.JobAssignment.Handyman.User.LastName}"
                    : "Not Assigned";

                dest.review = src.Reviews != null && src.Reviews.Any()
                    ? src.Reviews.Average(r => r.Rating).ToString("F1")
                    : "No Review";

                dest.Revenue = src.TotalPrice ?? 0;
            });
            #endregion

            #region Handyman application
            CreateMap<HandyManApplicationDto, User>()
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
           .ReverseMap();

            CreateMap<HandyManApplicationDto, Handyman>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => HandymenStatus.Pending))
                .ForMember(dest => dest.DefaultAddressId, opt => opt.Ignore());

            CreateMap<HandyManApplicationDto, Address>()
                .ForMember(dest => dest.Address1, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude))
                .ForMember(dest => dest.AddressType, opt => opt.MapFrom(src => src.AddressType))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
            #endregion


            #region service catalog

            // Category Mappings
            CreateMap<Category, CategoryDTO>().ReverseMap();
            CreateMap<CategoryCreateDTO, Category>();

            // Service Mappings
            CreateMap<Service, ServiceDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
            CreateMap<ServiceDTO, Service>();
            CreateMap<ServiceCreateDTO, Service>();
            CreateMap<ServiceUpdateDTO, Service>();

            #endregion


            #region handyman page job tap
            //public int jobassingmentid { get; set; }
            //public string clientname { get; set; }
            //public int clientnumber { get; set; }
            //public string address { get; set; }
            //public string status { get; set; }

            CreateMap<JobAssignment, HandymanJobsDTO>()
                .AfterMap((src, dest) =>
                {
                    dest.JobAssignmentId = src.Id;
                    dest.ClientName = $"{src.Booking.Client.User.FirstName} {src.Booking.Client.User.LastName}";
                    dest.ClientNumber = src.Booking.Client.User.Phone;
                    dest.Address = src.Booking.Address?.Address1 ?? "No Address";
                    dest.Status = src.Status;
                })
                .ReverseMap();
            #endregion


            #region ClientProfile
            

            CreateMap<Client, ClientProfileDTO>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User != null ? src.User.FirstName : null))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User != null ? src.User.LastName : null))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User != null ? src.User.Phone : null))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.User != null ? src.User.IsActive : false))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.User != null ? src.User.CreatedAt : DateTime.MinValue))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.User != null ? src.User.UpdatedAt : DateTime.MinValue))
                .ForMember(dest => dest.DefaultAddress, opt => opt.MapFrom(src => src.DefaultAddress));

            CreateMap<Address, ClientDefaultAddressDTO>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address1))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude))
                .ForMember(dest => dest.AddressType, opt => opt.MapFrom(src => src.AddressType));

            CreateMap<Address, ClientAddressDTO>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address1))
                .ForMember(dest => dest.IsDefault, opt => opt.Ignore());

            CreateMap<Booking, ClientOrderDTO>()
                .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.HandymanName, opt => opt.MapFrom(src =>
                    src.JobAssignment != null && src.JobAssignment.Handyman != null && src.JobAssignment.Handyman.User != null
                        ? $"{src.JobAssignment.Handyman.User.FirstName} {src.JobAssignment.Handyman.User.LastName}"
                        : "Not Assigned"))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.BookingServices))
                .ForMember(dest => dest.Payment, opt => opt.MapFrom(src => src.Payments.FirstOrDefault()))
                .ForMember(dest => dest.Review, opt => opt.MapFrom(src => src.Reviews.FirstOrDefault()));

            CreateMap<Address, ClientOrderAddressDTO>()
                .ForMember(dest => dest.FullAddress, opt => opt.MapFrom(src => src.Address1));

            CreateMap<BookingService, ClientOrderServiceDTO>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.Name))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Service.Category.Name))
                .ForMember(dest => dest.FixedPrice, opt => opt.MapFrom(src => src.Service.FixedPrice))
                .ForMember(dest => dest.EstimatedMinutes, opt => opt.MapFrom(src => src.Service.EstimatedMinutes));

            CreateMap<Payment, ClientPaymentDTO>()
                .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.CreatedAt));

            CreateMap<Review, ClientReviewDTO>()
                .ForMember(dest => dest.ReviewDate, opt => opt.MapFrom(src => src.CreatedAt));

            CreateMap<Quote, ClientQuoteDTO>()
                .ForMember(dest => dest.QuoteId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.JobAssignment.BookingId))
                .ForMember(dest => dest.HandymanName, opt => opt.MapFrom(src =>
                    src.JobAssignment.Handyman != null && src.JobAssignment.Handyman.User != null
                        ? $"{src.JobAssignment.Handyman.User.FirstName} {src.JobAssignment.Handyman.User.LastName}"
                        : "Unknown"))
                .ForMember(dest => dest.handymanId,
                         opt => opt.MapFrom(src => src.JobAssignment.HandymanId))
                .ForMember(dest => dest.addressId,
                         opt => opt.MapFrom(src => src.JobAssignment.Booking.AddressId))
                .ForMember(dest => dest.BookingDate, opt => opt.MapFrom(src => src.JobAssignment.Booking.PreferredDate))
                .ForMember(dest => dest.Services, opt => opt.MapFrom(src =>
                    src.JobAssignment.Booking.BookingServices.Select(bs => bs.Service.Name).ToList()))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src =>
                    src.JobAssignment.Booking.BookingServices.FirstOrDefault().Service.Category.Name ?? "Unknown"));
            CreateMap<UpdateClientProfileDTO, User>()
                 .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateClientProfileDTO, User>()
                 .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateClientAddressDTO, Address>()
                .ForMember(dest => dest.Address1, opt => opt.MapFrom(src => src.Address1))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            #endregion


            #region made all quotes for handyman
            CreateMap<Quote, AllQuotes>()
                .AfterMap((src, dest) =>
                {
                    dest.JobAssignmentId = src.JobAssignmentId;
                    dest.price = src.Price;
                    dest.estimatedMinutes = src.EstimatedMinutes;
                    dest.notes = src.Notes;
                    dest.status = src.Status;
                    dest.createdAt = src.CreatedAt;
                })
                .ReverseMap();
            #endregion

            #region review
            CreateMap<Review, ReviewResponseDTO>();
            CreateMap<CreateReviewDTO, Review>();
            #endregion
        }
    }
}