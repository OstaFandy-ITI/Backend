using AutoMapper;
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
            CreateMap<Address, AddressDTO>().ReverseMap();
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
                .AfterMap((src, dest) =>
                {
                    if (dest.User != null)
                    {
                        dest.User.FirstName = src.FirstName;
                        dest.User.LastName = src.LastName;
                        dest.User.Email = src.Email;
                        dest.User.Phone = src.Phone;
                        dest.User.UpdatedAt = DateTime.UtcNow;
                    }
                })
                .ReverseMap();
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
               src.BookingServices.Select(bs => bs.Service.Name).ToList()));
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

 
        }
    }
}