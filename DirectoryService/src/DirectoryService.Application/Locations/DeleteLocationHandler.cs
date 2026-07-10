using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;

namespace DirectoryService.Application.Locations;

public record DeleteLocationCommand(Guid Id) : ICommand;

public class DeleteLocationHandler
{
    
}