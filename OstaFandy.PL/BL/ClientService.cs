using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OstaFandy.DAL.Entities;
using OstaFandy.DAL.Repos.IRepos;
using OstaFandy.PL.BL.IBL;
using OstaFandy.PL.DTOs;
using OstaFandy.PL.utils;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace OstaFandy.PL.BL
{
    public class ClientService : IClientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ClientService> _logger;

        public ClientService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ClientService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }
        public PaginationHelper<AdminDisplayClientDTO> GetAll(string searchString = "", int pageNumber = 1, int pageSize = 5, bool? isActive = null)
        {
             Expression<Func<User, bool>> filter = u => u.Client != null;

            if (!string.IsNullOrEmpty(searchString) && isActive.HasValue)
            {
                var searchLower = searchString.ToLower();
                filter = u => u.Client != null &&
                             u.IsActive == isActive.Value &&
                             (u.FirstName.ToLower().Contains(searchLower) ||
                              u.LastName.ToLower().Contains(searchLower) ||
                              u.Email.ToLower().Contains(searchLower) ||
                              u.Phone.Contains(searchString));
            }
            else if (!string.IsNullOrEmpty(searchString))
            {
                var searchLower = searchString.ToLower();
                filter = u => u.Client != null &&
                             (u.FirstName.ToLower().Contains(searchLower) ||
                              u.LastName.ToLower().Contains(searchLower) ||
                              u.Email.ToLower().Contains(searchLower) ||
                              u.Phone.Contains(searchString));
            }
            else if (isActive.HasValue)
            {
                filter = u => u.Client != null && u.IsActive == isActive.Value;
            }

            var users = _unitOfWork.UserRepo.GetAll(filter, "Client,Client.DefaultAddress,Addresses,Client.Bookings,Client.Bookings.Payments").ToList();
                var clientDTOs = _mapper.Map<List<AdminDisplayClientDTO>>(users);
                return PaginationHelper<AdminDisplayClientDTO>.Create(clientDTOs, pageNumber, pageSize, searchString);
        }

        public AdminDisplayClientDTO GetById(int id)
        {
            var user = _unitOfWork.UserRepo.GetAll(u => u.Id == id && u.Client != null,
                "Client,Client.DefaultAddress,Addresses,Client.Bookings,Client.Bookings.Payments").FirstOrDefault();

            if (user == null)
                return null;

            return _mapper.Map<AdminDisplayClientDTO>(user);
        }

        public AdminEditClientDTO EditClientDTO(AdminEditClientDTO editClientDto)
        {
            try
            {

                var client = _unitOfWork.UserRepo.FirstOrDefault(a => a.Id == editClientDto.Id, "Client,Addresses");
                if (client == null)
                {
                    throw new KeyNotFoundException("Client not found");
                }
                _mapper.Map(editClientDto, client);
                _unitOfWork.UserRepo.Update(client);
                _unitOfWork.Save();
                return editClientDto;
            }
            catch (Exception ex)
            {
                // Log the exception
                throw new Exception("An error occurred while processing the client data", ex);
            }
           
        }

        public bool DeleteClient(int id)
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
    }
}
