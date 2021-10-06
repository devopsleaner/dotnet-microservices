using System;
using System.Collections.Generic;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers
{
    [ApiController]
    [Route("api/c/platforms/{platformId}/[controller]")]
    public class CommandsController : ControllerBase
    {
        private readonly ICommandsRepo _repository;
        private readonly IMapper _mapper;
        public CommandsController(ICommandsRepo repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;

        }


        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId)
        {
            Console.WriteLine($"--> getting commands for platform {platformId}");

            if(_repository.PlatformExists(platformId) ==false){
                return NotFound();
            }
            var commands = _repository.GetCommandsForPlatform(platformId);

            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commands));

        }


        [HttpGet("{commandId}", Name ="GetCommandForPlatform")]
        public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId , int commandId)
        {
             Console.WriteLine($"--> getting commands for platform {platformId}, command{commandId}");
             if(_repository.PlatformExists(platformId) ==false)
             {
                return NotFound();
            }

             var command = _repository.GetCommand(platformId,commandId);

            if(command == null)
            {
                return NotFound();
            }
            
            return Ok(_mapper.Map<CommandReadDto>(command));
        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommandForPlatform(int platformId, CommandCreateDto commandDto)
        {
            Console.WriteLine($"--> creating command for platform {platformId}");
             if(_repository.PlatformExists(platformId) ==false)
             {
                return NotFound();
            }

            var command = _mapper.Map<Command>(commandDto);

            _repository.CreateCommand(platformId, command);

            _repository.SaveChanges();

            var commandreadDto = _mapper.Map<CommandReadDto>(command);

            return CreatedAtRoute(nameof(GetCommandForPlatform), new {platformId=platformId, commandId= commandreadDto.Id, commandreadDto});
        }

    }
}