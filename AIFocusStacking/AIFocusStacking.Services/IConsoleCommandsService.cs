using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIFocusStacking.Services
{
    public interface IConsoleCommandsService
    {
        ServiceResult RunModel();
        ServiceResult ClearOutputDirectory();
    }
}
