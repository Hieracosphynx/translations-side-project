using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Translations.Models;

namespace Translation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalizedTextEntriesController : ControllerBase 
    {
        private readonly LocalizedTextContext _context;

        public LocalizedTextEntriesController(LocalizedTextContext context)
        {
            _context = context;
        }

        // GET: api/LocalizedTextEntries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocalizedText>>> GetLocalizedTextEntries()
        {
            return await _context.LocalizedTextEntries.ToListAsync();
        }

        // GET: api/LocalizedTextEntries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LocalizedText>> GetLocalizedText(long id)
        {
            var localizedText = await _context.LocalizedTextEntries.FindAsync(id);

            if (localizedText == null)
            {
                return NotFound();
            }

            return localizedText;
        }

        // PUT: api/LocalizedTextEntries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocalizedText(string id, LocalizedText localizedText)
        {
            if (id != localizedText.Id)
            {
                return BadRequest();
            }

            _context.Entry(localizedText).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocalizedTextExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/LocalizedTextEntries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LocalizedText>> PostLocalizedText(LocalizedText localizedText)
        {
            _context.LocalizedTextEntries.Add(localizedText);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLocalizedText), new { id = localizedText.Id }, localizedText);
        }

        // DELETE: api/LocalizedTextEntries/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocalizedText(string id)
        {
            var localizedText = await _context.LocalizedTextEntries.FindAsync(id);
            if (localizedText == null)
            {
                return NotFound();
            }

            _context.LocalizedTextEntries.Remove(localizedText);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LocalizedTextExists(string id)
        {
            return _context.LocalizedTextEntries.Any(e => e.Id == id);
        }
    }
}
