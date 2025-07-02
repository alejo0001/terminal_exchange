using System.Collections.Generic;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;

namespace CrmAPI.Infrastructure.Services;

public class IsDefaultService : IIsDefaultService
{
    public Task SetIsDefault<T>(List<T> list)
    {
        string property = "IsDefault";
        if (list.Count == 1)
        {
            list[0].GetType().GetProperty(property)?.SetValue(list[0], true);
        }
        else 
        {
            bool hasDefault = false;
                
            foreach (var item in list)
            {
                if ((bool)item.GetType().GetProperty(property)?.GetValue(item)!)
                {
                    if (!hasDefault)
                    {
                        hasDefault = true;
                    }
                    else
                    {
                        item.GetType().GetProperty(property)?.SetValue(item, false);
                    }
                }
            }
                
            if (!hasDefault)
            {
                list[0].GetType().GetProperty(property)?.SetValue(list[0], true);
            }
        }
        return Task.CompletedTask;
    }
}