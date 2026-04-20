namespace HabitDev.DTOs.Common;

public class HateoasResponse<T>
{
    public T Data { get; set; }
    public List<LinkDto> Links { get; set; } = [];

}
  
