using OstaFandy.DAL.Entities;
using OstaFandy.PL.DTOs;
using OstaFandy.PL.utils;

namespace OstaFandy.PL.BL.IBL
{
    public interface IHandyManService
    {
        PaginationHelper<Handyman> GetAll(string searchString = "", int pageNumber = 1, int pageSize = 5);
 
        AdminHandyManDTO CreateHandyman(CreateHandymanDTO createHandymanDto);
        AdminHandyManDTO GetById(int id);
        bool DeleteHandyman(int id);
        AdminHandyManDTO EditHandyman(EditHandymanDTO editHandymanDto);
 
    }
}

