using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using APILibrary.Core.Attributes;
using APILibrary.Core.Extensions;
using APILibrary.Core.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APILibrary
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControllerBaseAPI<TModel, TContext> : ControllerBase where TModel : ModelBase where TContext : DbContext 
    {

        protected readonly DbContext _context;

        public ControllerBaseAPI(DbContext context) {
            this._context = context;
        }

        protected IEnumerable<dynamic> ToJsonList(IEnumerable<dynamic> tab)
        {
            var tabNew = tab.Select((x) =>
            {
                return ToJson(x);
            });
            return tabNew;
        }

        protected dynamic ToJson(dynamic item)
        {
            var expo = new ExpandoObject() as IDictionary<string, object>;

            var collectionType = typeof(TModel);

            IDictionary<string, object> dico = item as IDictionary<string, object>;
            if (dico != null)
            {
                foreach (var propDyn in dico)
                {
                    var propInTModel = collectionType.GetProperty(propDyn.Key, BindingFlags.Public |
                            BindingFlags.IgnoreCase | BindingFlags.Instance);

                    var isPresentAttribute = propInTModel.CustomAttributes
                    .Any(x => x.AttributeType == typeof(NotJsonAttribute));

                    if (!isPresentAttribute)
                        expo.Add(propDyn.Key, propDyn.Value);
                }
            }
            else
            {
                foreach (var prop in collectionType.GetProperties())
                {
                    var isPresentAttribute = prop.CustomAttributes
                    .Any(x => x.AttributeType == typeof(NotJsonAttribute));

                    if (!isPresentAttribute)
                        expo.Add(prop.Name, prop.GetValue(item));
                }
            }
            return expo;
        }

        // create
        [HttpPost]
        public async Task<ActionResult<TModel>> CreateItem([FromBody] TModel item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                return (item);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        // delete
        [HttpDelete("{id}")]
        public async Task<ActionResult<TModel>> DeleteAsync([FromRoute] int id)
        {
            TModel item = await _context.Set<TModel>().FindAsync(id);
            if (item == null) return NotFound();

            _context.Remove(item);
            int result = await _context.SaveChangesAsync();

            if (result != 0) { return NoContent(); } else { return NotFound(); }
        }
       

        // update
        [HttpPut("{id}")]
        public async Task<ActionResult<TModel>> UpdateItem([FromRoute] int id, [FromBody] TModel item)
        {
            bool result = await _context.Set<TModel>().AnyAsync(x => x.ID == id);
            if (!result) return NotFound(new { Message = "Not found." });

            if(ModelState.IsValid && result)
            {
                try
                {
                    _context.Update<TModel>(item);
                    await _context.SaveChangesAsync();
                    return Ok(item);
                } catch (Exception e)
                {
                    return BadRequest(new { e.Message });
                }
            }

            else
            {
                return BadRequest(ModelState);
            }
        }

        // get all by field
        [HttpGet]
        public virtual async Task<ActionResult<IEnumerable<dynamic>>> GetAllAsync([FromQuery] string fields,string asc,string desc)
        {
            var query = _context.Set<TModel>().AsQueryable();

            List<dynamic> finish = await query.ToListAsync<dynamic>();
            var qx = finish.AsQueryable();
     
            if (!string.IsNullOrWhiteSpace(asc))
            {
                var x = asc.Split(',');
               /* foreach (string element in x)
                {
                    finish = finish.OrderBy(p => p.GetType().GetProperty(element).GetValue(p)).ToList();
                }*/
                foreach (string element in x)
                 {
                     finish = query.OrderByx(element, false).ToList();
                 }
             }
             if (!string.IsNullOrWhiteSpace(desc))
             {
                 var x = desc.Split(',');
                 /*foreach (string element in x)
                 {
                     finish = finish.OrderByDescending(p => p.GetType().GetProperty(element).GetValue(p)).ToList();
                 }*/
                  foreach (string element in x) {
                     finish = query.OrderByx(element, true).ToList();
                    }

            }
             if (!string.IsNullOrWhiteSpace(fields))
             {
                 var tab = fields.Split(',');
                 // var results = await IQueryableExtensions.SelectDynamic<TModel>(query, tab).ToListAsync();
                  //finish = await query.SelectDynamic(tab).ToListAsync();

                 finish = finish.Select((x) => IQueryableExtensions.SelectObject(x, tab)).ToList();
             }

            return Ok(ToJsonList(finish));

        }

        // get fields by id
        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TModel>> GetById([FromRoute] int id, [FromQuery] string fields)
        {
            var query = _context.Set<TModel>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(fields))
            {
                var tab = new List<string>(fields.Split(','));
                if (!tab.Contains("id"))
                    tab.Add("id");
                var result = query.SelectModel(tab.ToArray()).SingleOrDefault(x => x.ID == id);
                if (result != null)
                {
                    var tabFields = fields.Split(',');
                    return Ok(IQueryableExtensions.SelectObject(result, tabFields));
                }
                else
                {
                    return NotFound(new { Message = $"ID {id} not found" });
                }
            }
            else
            {
                var result = query.SingleOrDefault(x => x.ID == id);
                if (result != null)
                {

                    return Ok(ToJson(result));
                }
                else
                {
                    return NotFound(new { Message = $"ID {id} not found" });
                }
            }
        }



    }
}
