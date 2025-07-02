using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrmAPI.Application.Common.Interfaces;

public interface IIsDefaultService
{
    Task SetIsDefault<T>(List<T> list);
}