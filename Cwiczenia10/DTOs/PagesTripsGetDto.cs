namespace Cwiczenia10.DTOs;

public class PagesTripsGetDto
{
    public int PageNum { get; set; }
    public int PageSize { get; set; }
    public int AllPages { get; set; }
    public IEnumerable<TripGetDto> Trips { get; set; }
}
public class TripGetDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }
    public IEnumerable<CountryGetDto> Countries { get; set; }
    public IEnumerable<ClientGetDto> Clients { get; set; }
    
}

public class CountryGetDto
{
    public string Name { get; set; }
}

public class ClientGetDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}