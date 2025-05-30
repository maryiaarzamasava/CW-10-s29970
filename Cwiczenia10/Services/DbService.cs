using Cwiczenia10.Data;
using Cwiczenia10.DTOs;
using Cwiczenia10.Exceptions;
using Cwiczenia10.Models;
using Microsoft.EntityFrameworkCore;

namespace Cwiczenia10.Services;

public interface IDbService
{
    public Task<PagesTripsGetDto> GetTripsDetailsAsync(int? page = null, int? pageSize = null);
    public Task RemoveClientAsync(int clientId);
    public Task<ClientTripGetDto> CreateClientTripAsync(int idTrip, ClientTripCreateDto clientTripData);
    public Task<ClientTripGetDto> GetClientTripDetailsByIdAsync(int idClient, int idTrip);
}
public class DbService(MasterContext data) : IDbService
{
    public async Task<PagesTripsGetDto> GetTripsDetailsAsync(int? page = null, int? pageSize = null)
    {
        var trips = data.Trips
            .OrderByDescending(t => t.DateFrom)
            .Select(t => new TripGetDto
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.IdCountries.Select(c => new CountryGetDto
                {
                    Name = c.Name,
                }).ToList(),
                Clients = t.ClientTrips.Select(cl => new ClientGetDto
                {
                    FirstName = cl.IdClientNavigation.FirstName,
                    LastName = cl.IdClientNavigation.LastName,
                })
            });

        int totalTrips = await trips.CountAsync();

        if (page == null || pageSize == null)
        {
            return new PagesTripsGetDto
            {
                PageNum = 1,
                PageSize = 10,
                AllPages = (int)Math.Ceiling((double)totalTrips / 10),
                Trips = await trips.ToListAsync()
            };
        }

        int totalPages = (int)Math.Ceiling((double)totalTrips / pageSize.Value);

        var pagedTrips = await trips
            .Skip((page.Value - 1) * pageSize.Value)
            .Take(pageSize.Value)
            .ToListAsync();

        return new PagesTripsGetDto
        {
            PageNum = page.Value,
            PageSize = pageSize.Value,
            AllPages = totalPages,
            Trips = pagedTrips
        };
    }

    public async Task RemoveClientAsync(int clientId)
    {
        var clientTrips = await data.ClientTrips.FirstOrDefaultAsync(ct => ct.IdClient == clientId);
        if (clientTrips != null)
        {
            throw new ClientHasTripsException($"Client with id: {clientId} is registered for trip");
        }
        
        var affectedRows = await data.Clients.Where(c => c.IdClient == clientId).ExecuteDeleteAsync();
        if (affectedRows == 0)
        {
            throw new NotFoundException($"Client with id: {clientId} not found");
        }
    }

    public async Task<ClientTripGetDto> CreateClientTripAsync(int idTrip, ClientTripCreateDto clientTripData)
    {
        var client = await data.Clients.FirstOrDefaultAsync(c=>c.Pesel == clientTripData.Pesel);
        if (client == null)
        {
            throw new NotFoundException($"Client with pesel: {clientTripData.Pesel} not found");
        }
        
        var trip = await data.Trips.FirstOrDefaultAsync(t => t.IdTrip == idTrip);
        if (trip == null)
        {
            throw new NotFoundException($"Trip with id: {idTrip} not found");
        }
        
        if (trip.DateFrom < DateTime.Now)
        {
            throw new TripAlreadyHappenedException($"Trip with id: {trip.IdTrip} has already happened");
        }
        
        var isAssigned = await data.ClientTrips.FirstOrDefaultAsync(ct=>ct.IdClient == client.IdClient && ct.IdTrip == trip.IdTrip );
        if (isAssigned != null)
        {
            throw new ClientAlreadyOnTripException($"Client with pesel: {client.Pesel} is already on trip with id: {idTrip}");
        }

        var clientTrip = new ClientTrip
        {
            IdClient = client.IdClient,
            IdTrip = trip.IdTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = clientTripData.PaymentDate
        };
        await data.ClientTrips.AddAsync(clientTrip);
        await data.SaveChangesAsync();

        return new ClientTripGetDto
        {
            IdClient = client.IdClient,
            IdTrip = trip.IdTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = clientTripData.PaymentDate
        };
    }

    public async Task<ClientTripGetDto> GetClientTripDetailsByIdAsync(int idClient, int idTrip)
    {
        var result = await data.ClientTrips.Select(ct => new ClientTripGetDto
        {
            IdClient = idClient,
            IdTrip = idTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = ct.PaymentDate
        }).FirstOrDefaultAsync(ct => ct.IdClient == idClient && ct.IdTrip == idTrip);
        return result ?? throw new NotFoundException($"Client with id: {idClient} is not registered for trip with id: {idTrip}");
    }
}