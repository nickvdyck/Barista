using System;
using System.Threading.Tasks;
using Barista.MacOS.Models;

namespace Barista.MacOS.Services
{
    public interface ISettingsService
    {
        AppSettings GetSettings();
        Task Save(AppSettings settings);
    }
}
