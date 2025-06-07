using AutoMapper;
using OstaFandy.DAL.Entities;
using OstaFandy.PL.DTOs;

namespace OstaFandy.PL.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //dd
            #region User
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<UserType, UserTypeDto>().ReverseMap();
            CreateMap<User, UserRegesterDto>().ReverseMap();
            CreateMap<User, UserLoginDto>().ReverseMap();
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

            {
                CreateMap<User, UserDto>().ReverseMap();
                CreateMap<UserType, UserTypeDto>().ReverseMap();
                CreateMap<Address, AddressDTO>().ReverseMap();
                CreateMap<JobAssignment, JobAssignmentDTO>().ReverseMap();

                CreateMap<BlockDate, AdminBlockDateDTO>()
                    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                    .ReverseMap();

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
            }
           
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
        }
    }
}
