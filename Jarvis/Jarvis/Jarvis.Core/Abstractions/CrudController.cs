// using Infrastructure.Database.Models;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Jarvis.Core.Services;
// using System;
// using System.Collections.Generic;
// using System.Text;
// using System.Threading.Tasks;

// namespace Jarvis.Core.Abstractions
// {
//     [Authorize]
//     public abstract class CrudController<TModel, TEntity, TKey> : ControllerBase where TEntity : CrudEntity<TKey>
//     {
//         private readonly ICrudService<TModel, TEntity, TKey> _service;

//         public CrudController(ICrudService<TModel, TEntity, TKey> service)
//         {
//             _service = service;
//         }

//         [HttpGet]
//         [Authorize("*_Read")]
//         public IActionResult Get([FromQuery]Paging paging)
//         {
//             if (!ModelState.IsValid)
//                 return BadRequest(ModelState);

//             var paged = _service.Query(paging);
//             return Ok(paged);
//         }

//         [HttpGet("{id}")]
//         [Authorize("*_Read")]
//         public IActionResult Get([FromRoute]TKey id)
//         {
//             var model = _service.Query(id);
//             return Ok(model);
//         }

//         [Authorize("*_Create")]
//         [HttpPost]
//         public IActionResult Create([FromBody]TModel model)
//         {
//             //if (!ModelState.IsValid)
//             //    return BadRequest(ModelState);

//             _service.Create(model);
//             return Ok();
//         }

//         [Authorize("*_Update")]
//         [HttpPut("{id}")]
//         public IActionResult Update([FromRoute] TKey id, [FromBody]TModel model)
//         {
//             //if (!ModelState.IsValid)
//             //    return BadRequest(ModelState);

//             _service.Update(id, model);
//             return Ok();
//         }

//         [Authorize("*_Delete")]
//         [HttpDelete("{id}")]
//         public IActionResult Delete([FromRoute]TKey id)
//         {
//             _service.Delete(id);
//             return Ok();
//         }
//     }

// }
