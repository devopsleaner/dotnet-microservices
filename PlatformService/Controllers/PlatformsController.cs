using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Platformservice.AsyncDataServices;
using Platformservice.Data;
using Platformservice.Dtos;
using Platformservice.Models;
using PlatformService.SyncDataServices.Http;

namespace Platformservice.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo _repository;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _client;
        private IMessageBusClient _messageBusClient;

        public PlatformsController(IPlatformRepo repository, IMapper mapper, ICommandDataClient client , IMessageBusClient messageBusClient)
        {
            _repository = repository;
            _mapper = mapper;
            _client = client;
            _messageBusClient = messageBusClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            Console.WriteLine("Getting platforms");

            var platformItems = _repository.GetAllPlatforms();

            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platformItems));

        }

        [HttpGet("{id}", Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int id)
        {
            var platformItem = _repository.GetPlatformById(id);

            if (platformItem != null)
            {
                return Ok(_mapper.Map<PlatformReadDto>(platformItem));
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platformCreateDto)
        {
            var platformModel = _mapper.Map<Platform>(platformCreateDto);
            _repository.CreatePlatform(platformModel);
            _repository.SaveChanges();

            var platformReadDto = _mapper.Map<PlatformReadDto>(platformModel);

            //send sync message
            try
            {
                await _client.SendPlatformToCommand(platformReadDto);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception in Platform - Createplatform send sync failed. ex = {ex.Message} ");
            }

            //send async message

            try
            {
                var platformPublishDto = _mapper.Map<PlatformPublishDto>(platformReadDto);
                
                platformPublishDto.Event = "Platform_Published";
                
                _messageBusClient.PublishNewPlatform(platformPublishDto);
            }
            catch(Exception ex)
            {
                Console.WriteLine($" Createplatform send async to messagebus failed. ex = {ex.Message} ");
            }

            return CreatedAtRoute(nameof(GetPlatformById), new { Id = platformReadDto.Id }, platformReadDto);
        }
    }
}