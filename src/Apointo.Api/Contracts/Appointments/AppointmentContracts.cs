using System;
using System.Collections.Generic;

namespace Apointo.Api.Contracts.Appointments;

// Find Available Slots
public sealed record FindAvailableSlotsRequest(
    IReadOnlyCollection<string> ServiceIds,
    string? PreferredStaffId,
    DateTime StartDate,
    DateTime EndDate);

public sealed record AvailableSlotResponse(
    DateTime StartTime,
    DateTime EndTime,
    string StaffId,
    string StaffName,
    bool IsAvailable);

public sealed record FindAvailableSlotsResponse(
    DateTime SearchDate,
    int TotalDurationInMinutes,
    IReadOnlyCollection<AvailableSlotResponse> AvailableSlots);

// Create Appointment
public sealed record CreateAppointmentRequest(
    string StaffId,
    DateTime StartTimeUtc,
    IReadOnlyCollection<string> ServiceIds,
    string? Notes);

public sealed record AppointmentServiceResponse(
    string ServiceId,
    string ServiceName,
    decimal Price,
    int DurationInMinutes);

public sealed record AppointmentResponse(
    string Id,
    string CustomerId,
    string StaffId,
    string StaffName,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalPrice,
    string Status,
    string? Notes,
    IReadOnlyCollection<AppointmentServiceResponse> Services);

// Get Customer Appointments
public sealed record GetCustomerAppointmentsRequest(
    bool IncludePast = false,
    int PageNumber = 1,
    int PageSize = 10);

// Cancel Appointment
public sealed record CancelAppointmentRequest(
    string? CancellationReason);

// Update Appointment
public sealed record UpdateAppointmentRequest(
    DateTime? NewStartTimeUtc = null,
    DateTime? NewEndTimeUtc = null,
    string? NewStaffId = null,
    string? Notes = null,
    string? Status = null);

// Calendar View
public sealed record GetCalendarAppointmentsRequest(
    DateTime StartDate,
    DateTime EndDate,
    IReadOnlyCollection<string>? StaffIds = null);

public sealed record CalendarAppointmentResponse(
    string Id,
    string Title,
    DateTime StartTime,
    DateTime EndTime,
    string StaffId,
    string StaffName,
    string CustomerId,
    string CustomerName,
    string Status,
    decimal TotalPrice,
    string? Notes,
    IReadOnlyCollection<string> ServiceNames,
    string? ColorHex);

public sealed record StaffCalendarInfoResponse(
    string Id,
    string Name,
    string? ColorHex);

public sealed record CalendarViewResponse(
    DateTime StartDate,
    DateTime EndDate,
    IReadOnlyCollection<CalendarAppointmentResponse> Appointments,
    IReadOnlyCollection<StaffCalendarInfoResponse> Staff);