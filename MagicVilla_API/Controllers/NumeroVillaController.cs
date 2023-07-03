using AutoMapper;
using MagicVilla_API.Datos;
using MagicVilla_API.Modelos;
using MagicVilla_API.Modelos.Dto;
using MagicVilla_API.Repositorios.IRepositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.RegularExpressions;

namespace MagicVilla_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NumeroVillaController : ControllerBase
    {
        private readonly ILogger<NumeroVillaController> _logger;
        private readonly INumeroVillaRepositorio _numeroVillarepo;
        private readonly IVillaRepositorio _villaRepo;
        private readonly IMapper _mapper;
        protected APIResponse _apiResponse;

        public NumeroVillaController(ILogger<NumeroVillaController> logger,INumeroVillaRepositorio numeroVillarepo, IMapper mapper, IVillaRepositorio villaRepo)
        {
            _logger = logger;
            _numeroVillarepo = numeroVillarepo;
            _villaRepo = villaRepo;
            _mapper = mapper;
            _apiResponse = new APIResponse();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetNumeroVillas()
        {
            try
            {
                _logger.LogInformation("Obtener el listado de NumeroVillas.");

                IEnumerable<NumeroVilla> NumeroVillaList = await _numeroVillarepo.ObtenerTodos();

                _apiResponse.Resultado = _mapper.Map<IEnumerable<NumeroVillaDto>>(NumeroVillaList);
                _apiResponse.statusCode = HttpStatusCode.OK;

                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _apiResponse.IsExitoso = false;
                _apiResponse.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _apiResponse;
            
        }

        [HttpGet("id:int", Name = "GetNumeroVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<APIResponse>> GetNumeroVilla(int numVilla)
        {
            try
            {
                if (numVilla == 0)
                {
                    _logger.LogError("Error el numero de NumeroVilla no puede ser 0.");
                    _apiResponse.statusCode=HttpStatusCode.BadRequest;
                    _apiResponse.IsExitoso = false;
                    return BadRequest(_apiResponse);
                }
                var NumeroVilla = await _numeroVillarepo.Obtener(v => v.VillaNo == numVilla);
                if (NumeroVilla == null)
                {
                    _apiResponse.statusCode = HttpStatusCode.NotFound;
                    _apiResponse.IsExitoso = false;
                    return NotFound(_apiResponse);
                }

                _apiResponse.Resultado = _mapper.Map<NumeroVillaDto>(NumeroVilla);
                _apiResponse.statusCode = HttpStatusCode.OK;

                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _apiResponse.IsExitoso = false;
                _apiResponse.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _apiResponse;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> CrearNumeroVilla([FromBody] NumeroVillaCreateDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _apiResponse.statusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsExitoso = false;
                    _apiResponse.Resultado = ModelState;
                    return BadRequest(_apiResponse);
                }
                if (await _numeroVillarepo.Obtener(v => v.VillaNo == createDto.VillaNo) != null)
                {
                    ModelState.AddModelError("NombreExiste", "La NumeroVilla con ese nombre ya existe!");
                    _apiResponse.Resultado = ModelState;
                    _apiResponse.ErrorMessages = new List<string>() { "La NumeroVilla con ese numero ya existe!" };
                    _apiResponse.statusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsExitoso = false;
                    return BadRequest(_apiResponse);
                }
                if(await _villaRepo.Obtener(v => v.Id == createDto.VillaId) == null)
                {
                    ModelState.AddModelError("ClaveForanea", "El id de la villa no existe.");
                    _apiResponse.Resultado = ModelState;
                    _apiResponse.ErrorMessages = new List<string>() { "El id de la villa no existe." };
                    _apiResponse.statusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsExitoso = false;
                    return BadRequest(_apiResponse);
                }

                if (createDto == null)
                {
                    return BadRequest(createDto);
                }

                NumeroVilla modelo = _mapper.Map<NumeroVilla>(createDto);

                modelo.FechaActualizacion = DateTime.Now;
                modelo.FechaCreacion = DateTime.Now;

                await _numeroVillarepo.Crear(modelo);
                _apiResponse.Resultado = modelo;
                _apiResponse.statusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetNumeroVilla", new { id = modelo.VillaNo }, _apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _apiResponse.IsExitoso = false;
                _apiResponse.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _apiResponse;
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id == 0)
                {
                    _apiResponse.statusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsExitoso=false;
                    return BadRequest(_apiResponse);
                }

                var NumeroVilla = await _numeroVillarepo.Obtener(NumeroVilla => NumeroVilla.VillaNo == id);
                if (NumeroVilla == null)
                {
                    _apiResponse.statusCode = HttpStatusCode.NotFound;
                    _apiResponse.IsExitoso = false;
                    return NotFound(_apiResponse);
                }

                await _numeroVillarepo.Remover(NumeroVilla);
                _apiResponse.statusCode = HttpStatusCode.NoContent;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _apiResponse.IsExitoso = false;
                _apiResponse.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return BadRequest(_apiResponse);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateNumeroVilla(int id, [FromBody] NumeroVillaUpdateDto updateDto)
        {
            try
            {
                if (updateDto == null || id != updateDto.VillaNo)
                {
                    _apiResponse.statusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsExitoso = false;
                    return BadRequest(_apiResponse);
                }

                if(await _villaRepo.Obtener(v=>v.Id==updateDto.VillaId) == null)
                {
                    ModelState.AddModelError("ClaveForanea","Id Villa invalido no existe");
                    return BadRequest(ModelState);
                }

                NumeroVilla modelo = _mapper.Map<NumeroVilla>(updateDto);

                modelo.FechaActualizacion = DateTime.Now;

                await _numeroVillarepo.Actualizar(modelo);

                _apiResponse.Resultado = modelo;
                _apiResponse.statusCode = HttpStatusCode.NoContent;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _apiResponse.IsExitoso = false;
                _apiResponse.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return BadRequest(_apiResponse);
        }
        
    }
}
