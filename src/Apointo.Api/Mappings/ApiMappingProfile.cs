using AutoMapper;
using Apointo.Api.Contracts.BusinessSettings;
using Apointo.Api.Contracts.Services;
using Apointo.Api.Contracts.Staff;
using Apointo.Application.Businesses.Commands.UpdateBusinessSettings;
using Apointo.Application.Businesses.Dtos;
using Apointo.Application.ServiceCatalog.Dtos;
using Apointo.Application.Staff.Dtos;

namespace Apointo.Api.Mappings;

public sealed class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        CreateMap<BusinessAddressDto, BusinessAddressResponse>();
        CreateMap<BusinessOperatingHourDto, BusinessOperatingHourResponse>();
        CreateMap<BusinessSettingsDto, BusinessSettingsResponse>();

        CreateMap<BusinessAddressRequest, BusinessAddressDto>();
        CreateMap<BusinessOperatingHourRequest, BusinessOperatingHourInput>();
        CreateMap<UpdateBusinessSettingsRequest, UpdateBusinessSettingsCommand>();

        CreateMap<StaffSummaryDto, StaffSummaryResponse>();
        CreateMap<StaffScheduleDto, StaffScheduleResponse>();
        CreateMap<StaffAvailabilityOverrideDto, StaffAvailabilityOverrideResponse>();
        CreateMap<StaffDetailDto, StaffResponse>();

        CreateMap<ServiceCategoryDto, ServiceCategoryResponse>();
        CreateMap<ServiceDto, ServiceResponse>();
    }
}
